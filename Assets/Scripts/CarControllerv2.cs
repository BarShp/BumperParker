﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarControllerv2 : MonoBehaviour
{
    private Rigidbody rb;
    private float steeringAngle;
    private float forwardVelocity;
    private float horizontalInput;
    private float verticalInput;
    private float brakeInput;

    public Transform wheels;
    [SerializeField] private float maxSteerAngle = 40;
    [SerializeField] private float acceleration = 50;
    [SerializeField] private float rotationSpeed = 5;
    [SerializeField] private float drag = 3;
    [SerializeField] private float brake = 3;
    [SerializeField] private float normalMultiplyer = 0.02f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        GetInput();
        steeringAngle = maxSteerAngle * horizontalInput;
        forwardVelocity = transform.InverseTransformDirection(rb.velocity).z;
        if (IsGrounded())
        {
            Accelerate();
            Steer();
            Drag();
        }

        Brake();
        UpdateWheelPoses();
    }

    private void OnCollisionEnter(Collision other)
    {
        // TODO: create public power
        float power = 30;

        var otherRb = other.gameObject.GetComponent<Rigidbody>();
        if (otherRb != null)
        {
            // otherRb.AddForce((otherRb.position - rb.position) * power, ForceMode.Impulse);
            otherRb.AddForceAtPosition(Vector3.one * power, (otherRb.position - rb.position), ForceMode.Impulse);
        }
    }

    public void GetInput()
    {
        horizontalInput = Input.GetAxis("HorizontalMovement");
        verticalInput = Input.GetAxis("Vertical");
        brakeInput = Input.GetAxis("Brake");
    }

    private void Steer()
    {
        var rotateMultiplier = Mathf.Clamp(forwardVelocity * 0.1f, -1, 1);
        rotateMultiplier *= Mathf.Lerp(1, 2, brakeInput);
        rb.MoveRotation(rb.rotation * Quaternion.Euler(Vector3.up * steeringAngle * rotationSpeed * rotateMultiplier * normalMultiplyer));
    }

    private void Accelerate()
    {
        // rb.AddForceAtPosition(Vector3.forward * verticalInput * acceleration * normalMultiplyer, Vector3.back, ForceMode.VelocityChange);
        rb.AddRelativeForce(Vector3.forward * verticalInput * acceleration * normalMultiplyer, ForceMode.VelocityChange);
    }

    private void Drag()
    {
        float dragForce = 1 - drag / 100;
        var vel = rb.velocity;
        vel.x *= dragForce;
        vel.z *= dragForce;
        rb.velocity = vel;
    }

    private void Brake()
    {
        float brakeForce = 1 - (brakeInput * brake) / 100;
        var vel = rb.velocity;
        vel.x *= brakeForce;
        vel.z *= brakeForce;
        rb.velocity = vel;
    }

    private void UpdateWheelPoses()
    {
        foreach (Transform wheel in wheels)
        {
            if (wheel.tag == "FrontWheel")
            {
                wheel.localEulerAngles = Vector3.up * steeringAngle;
            }
            // TODO: Allow wheel rotation with velocity even if not moving (upside down)
            wheel.GetChild(0).Rotate(Vector3.right * forwardVelocity * 2, Space.Self);
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(rb.position, -rb.transform.up, 0.5f);
    }
}