using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour {

    [SerializeField] float moveSpeed = 6;
    [SerializeField] float jumpHeight = 4;
    [SerializeField] float accelerationTimeAirborne = 0.2f;
    [SerializeField] float accelerationTimeGrounded = 0.1f;
    [SerializeField] float timeToJumpApex = 0.4f;

    [Header("Debug")]
    [SerializeField] float jumpVelocity;
    [SerializeField] float gravity;

    [SerializeField] float velocitySmoothingX;

    Vector3 velocity;

    Controller2D controller;

    void Start() {
        controller = GetComponent<Controller2D>();
        gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
    }

    private void Update() {
        if (controller.collisions.above || controller.collisions.below) {
            velocity.y = 0;
        }

        Vector2 input = new Vector2(CrossPlatformInputManager.GetAxisRaw("Horizontal"), CrossPlatformInputManager.GetAxisRaw("Vertical"));

        if (CrossPlatformInputManager.GetButtonDown("Jump") && controller.collisions.below) {
            velocity.y = jumpVelocity;
        }

        float targetVelocityX = input.x * moveSpeed;
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocitySmoothingX, (controller.collisions.below ? accelerationTimeGrounded : accelerationTimeAirborne));
        velocity.y += gravity * Time.deltaTime;

        if (!controller.useRigidbody) {
            var adjustedVelocity = velocity * Time.deltaTime;
            controller.Move(adjustedVelocity);
        }
    }

    void FixedUpdate() {
        if (controller.useRigidbody) {
            var adjustedVelocity = velocity * Time.deltaTime;
            controller.Move(adjustedVelocity);
        }
    }
}
