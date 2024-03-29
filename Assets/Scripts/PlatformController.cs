﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlatformController : RaycastController {

    [SerializeField] public bool useRigidbody = false;
    [SerializeField] LayerMask passengerMask;
    [SerializeField] Vector3 move;

    List<PassengerMovement> passengerMovements;
    Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();

    Rigidbody2D myRigidbody;

    protected override void Start() {
        base.Start();
        myRigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update() {
        if (!useRigidbody) {
            UpdateRaycastOrigins();

            Vector3 velocity = move * Time.deltaTime;

            CalculatePassengerMovement(velocity);

            MovePassengers(true);
            transform.Translate(velocity);
            MovePassengers(false);
        }
    }

    void FixedUpdate() {
        if (useRigidbody) {
            UpdateRaycastOrigins();

            Vector3 velocity = move * Time.deltaTime;

            CalculatePassengerMovement(velocity);

            MovePassengers(true);
            myRigidbody.MovePosition(myRigidbody.position + new Vector2(velocity.x, velocity.y));
            MovePassengers(false);
        }
    }

    void MovePassengers(bool beforeMovePlatform) {
        foreach (PassengerMovement passenger in passengerMovements) {
            if (!passengerDictionary.ContainsKey(passenger.transform)) {
                passengerDictionary.Add(passenger.transform, passenger.transform.GetComponent<Controller2D>());
            }

            if (passenger.moveBeforePlatform == beforeMovePlatform) {
                passengerDictionary[passenger.transform].Move(passenger.velocity, passenger.standingOnPlatform);
            }
        }
    }

    void CalculatePassengerMovement(Vector3 velocity) {
        HashSet<Transform> movedPassengers = new HashSet<Transform>();
        passengerMovements = new List<PassengerMovement>();

        float directionX = Mathf.Sign(velocity.x);
        float directionY = Mathf.Sign(velocity.y);

        // Vertically moving platform
        if (velocity.y != 0) {
            float rayLength = Mathf.Abs(velocity.y) + SKIN_WIDTH;

            for (int i = 0; i < verticalRayCount; i++) {
                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);
                Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.green);

                if (hit) {
                    if (!movedPassengers.Contains(hit.transform)) {
                        movedPassengers.Add(hit.transform);
                        float pushX = (directionY == 1) ? velocity.x : 0;
                        float pushY = velocity.y - (hit.distance - SKIN_WIDTH) * directionY;
                        var pushVector = new Vector3(pushX, pushY);
                        passengerMovements.Add(new PassengerMovement(hit.transform, pushVector, directionY == 1, true));
                    }
                }
            }
        }

        // Horizontally moving platform
        if (velocity.x != 0) {
            float rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;

            for (int i = 0; i < horizontalRayCount; i++) {
                Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (horizontalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);
                Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);
                if (hit) {
                    if (!movedPassengers.Contains(hit.transform)) {
                        movedPassengers.Add(hit.transform);
                        float pushX = velocity.x - (hit.distance - SKIN_WIDTH) * directionX;
                        float pushY = -SKIN_WIDTH;
                        
                        passengerMovements.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
                    }
                }
            }
        }

        // Passenger on top of a horizontally or downward moving platform
        if (directionY == -1 || velocity.y == 0 && velocity.x != 0) {
            float rayLength = SKIN_WIDTH * 2;

            for (int i = 0; i < verticalRayCount; i++) {
                Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);
                Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);
                if (hit) {
                    if (!movedPassengers.Contains(hit.transform)) {
                        movedPassengers.Add(hit.transform);
                        float pushX = velocity.x;
                        float pushY = velocity.y;

                        passengerMovements.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
                    }
                }
            }
        }
    }

    struct PassengerMovement {
        public Transform transform;
        public Vector3 velocity;
        public bool standingOnPlatform;
        public bool moveBeforePlatform;

        public PassengerMovement(Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform) {
            transform = _transform;
            velocity = _velocity;
            standingOnPlatform = _standingOnPlatform;
            moveBeforePlatform = _moveBeforePlatform;
        }
    }

}