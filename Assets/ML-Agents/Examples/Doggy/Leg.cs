using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class Leg : MonoBehaviour
{
    private float speed = 0;
    private float integral = 0;

    float setpoint = 0;
    float actual = 0;
    float error = 0;

    ArticulationBody leg;

    public void Awake()
    {
        leg = GetComponent<ArticulationBody>();
    }

    public void FixedUpdate()
    {
        actual = leg.xDrive.target;

        error = setpoint - actual;

        integral += error * Time.fixedDeltaTime;

        float force = speed * integral;

        ArticulationDrive drive = leg.xDrive;
        drive.target = force;
        leg.xDrive = drive;
    }

    public void MoveLeg(float targetAngle, float servoSpeed)
    {
        speed = servoSpeed;
        setpoint = targetAngle;
    }
}

