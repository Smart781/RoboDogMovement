using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections;
using System;
using Random = UnityEngine.Random;
using UnityEngine.InputSystem;

public class DoggyImitationAgent : Agent
{
    [Header("Сервоприводы")]
    public ArticulationBody[] legs;

    [Header("Скорость работы сервоприводов")]
    public float servoSpeed;

    [Header("Тело")]
    public ArticulationBody body;
    private Vector3 defPos;
    private Quaternion defRot;
    public float strenghtMove;

    [Header("Куб (цель)")]
    public GameObject cube;

    private float distToTarget = 0f;

    [Header("Сенсоры")]
    public Unity.MLAgentsExamples.GroundContact[] groundContacts;

    private Oscillator m_Oscillator;

    public override void Initialize()
    {
        distToTarget = Vector3.Distance(body.transform.position, cube.transform.position);
        defRot = body.transform.rotation;
        defPos = body.transform.position;

        m_Oscillator = GetComponent<Oscillator>();
        m_Oscillator.ManagedReset();
    }

    public void ResetDog()
    {
        //Quaternion newRot = Quaternion.Euler(-90, 0, Random.Range(0f, 360f));


        body.TeleportRoot(defPos, defRot);
        body.velocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;

        for (int i = 0; i < 12; i++)
        {
            MoveLeg(legs[i], 0);
        }
    }

    public override void OnEpisodeBegin()
    {
        ResetDog();
        m_Oscillator.ManagedReset();

        cube.transform.position = new Vector3(4f, 0.21f, 0);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(body.transform.position);
        sensor.AddObservation(body.velocity);
        sensor.AddObservation(body.angularVelocity);
        sensor.AddObservation(body.transform.right);

        // Позиция куба
        sensor.AddObservation(cube.transform.position);

        // Относительное положение куба
        Vector3 relativePosition = cube.transform.position - body.transform.position;
        sensor.AddObservation(relativePosition);

        // Угловая позиция куба
        Vector3 toCube = (cube.transform.position - body.transform.position).normalized;
        float angleToCube = Vector3.SignedAngle(body.transform.right, toCube, Vector3.up);
        sensor.AddObservation(angleToCube);

        // Расстояние до куба
        float distanceToCube = Vector3.Distance(body.transform.position, cube.transform.position);
        sensor.AddObservation(distanceToCube);
        foreach (var leg in legs)
        {
            sensor.AddObservation(leg.xDrive.target);
            sensor.AddObservation(leg.velocity);
            sensor.AddObservation(leg.angularVelocity);
        }

        foreach (var groundContact in groundContacts)
        {
            sensor.AddObservation(groundContact.touchingGround);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        m_Oscillator.ManagedUpdate();

    }
    public override void OnActionReceived(ActionBuffers vectorAction)
    {
        var actions = vectorAction.ContinuousActions;
        for (int i = 0; i < 12; i++)
        {
            float angle = Mathf.Lerp(legs[i].xDrive.lowerLimit, legs[i].xDrive.upperLimit, (actions[i] + 1) * 0.5f);
            //MoveLeg(legs[i], angle);
        }

        float currentDistanceToTarget = Vector3.Distance(body.transform.position, cube.transform.position);
        float distanceReward = distToTarget - currentDistanceToTarget;
        AddReward(distanceReward);
        distToTarget = currentDistanceToTarget;

        if (currentDistanceToTarget < 1f)
        {
            AddReward(50.0f);
            EndEpisode();
        }

        if (distanceReward < 0)
        {
            AddReward(-0.001f);
        }

        if (body.velocity.magnitude < 0.1f)
        {
            AddReward(-0.001f);
        }
    }
    public void FixedUpdate()
    {
        body.AddForce((cube.transform.position - body.transform.position).normalized * strenghtMove);
        for (int i = 0; i < 12; i++)
        {
            legs[i].AddForce((cube.transform.position - body.transform.position).normalized * strenghtMove / 20f);
        }

        RaycastHit hit;
        if (Physics.Raycast(body.transform.position, body.transform.right, out hit))
        {
            if (hit.collider.gameObject == cube)
            {
                AddReward(0.001f);
                body.AddForce(2f * strenghtMove * (cube.transform.position - body.transform.position).normalized);
                for (int i = 0; i < 12; i++)
                {
                    legs[i].AddForce((cube.transform.position - body.transform.position).normalized * strenghtMove / 10f);
                }
            }
            else
            {
                AddReward(-0.001f);
            }
        }
        Debug.DrawRay(body.transform.position, body.transform.right, Color.white);
    }

    void MoveLeg(ArticulationBody leg, float targetAngle)
    {
        leg.GetComponent<Leg>().MoveLeg(targetAngle, servoSpeed);
    }
}
