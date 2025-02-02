using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections;
using System;
using Random = UnityEngine.Random;
using UnityEngine.InputSystem;

public class DoggyAgent : Agent
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

    [Header("Сенсоры")]
    public Unity.MLAgentsExamples.GroundContact[] groundContacts;

    private float distToTarget = 0f;

    //private Oscillator m_Oscillator;

    private float lastUpdateTime = 0f; // Время последнего обновления
    private int currentStep = 0; // Текущий этап движений
    private bool change = false;

    public override void Initialize()
    {
        distToTarget = Vector3.Distance(body.transform.position, cube.transform.position);
        defRot = body.transform.rotation;
        defPos = body.transform.position;

        //m_Oscillator = GetComponent<Oscillator>(); ***
        //m_Oscillator.ManagedReset(); ***
    }

    public void ResetDog()
    {
        Quaternion newRot = Quaternion.Euler(-90, 0, Random.Range(0f, 360f));


        body.TeleportRoot(defPos, newRot);
        //body.TeleportRoot(defPos, defRot); ***
        body.velocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;

        for (int i = 0; i < 12; i++)
        {
            //MoveLeg(legs[i], Random.Range(legs[i].xDrive.lowerLimit, legs[i].xDrive.upperLimit));
            MoveLeg(legs[i], 0);
        }

        change = true;

        // MoveLeg(legs[8], 90);
        // MoveLeg(legs[11], 90);
    }

    public override void OnEpisodeBegin()
    {
        ResetDog();
        //m_Oscillator.ManagedReset(); ***

        //cube.transform.position = new Vector3(5, 0.21f, Random.Range(-2f, 2f));
        cube.transform.position = new Vector3(Random.Range(-7.5f, 7.5f), 0.21f, Random.Range(-7.5f, 7.5f));
        //cube.transform.position = new Vector3(5f, 0.21f, 0); ***

        //cube.transform.position = new Vector3(8f, 0.26f, 0f);
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

        foreach(var groundContact in groundContacts)
        {
            sensor.AddObservation(groundContact.touchingGround);
        }
    }

    public override void OnActionReceived(ActionBuffers vectorAction)
    {
        var actions = vectorAction.ContinuousActions;

        for (int i = 0; i < 12; i++)
        {
            float angle = Mathf.Lerp(legs[i].xDrive.lowerLimit, legs[i].xDrive.upperLimit, (actions[i] + 1) * 0.5f);
            MoveLeg(legs[i], angle);
            // if ((i % 4) != 0 && (i % 4) != 3) {
            //     if (i % 4 == 1) {
            //         float angle = Mathf.Lerp(legs[i].xDrive.lowerLimit, legs[i].xDrive.upperLimit, (actions[i] + 1) * 0.5f);
            //         MoveLeg(legs[i], angle);
            //     }
            //     else {
            //         float angle = Mathf.Lerp(legs[i - 1].xDrive.lowerLimit, legs[i - 1].xDrive.upperLimit, (actions[i - 1] + 1) * 0.5f);
            //         MoveLeg(legs[i], angle);
            //     }
            // }
        }
        
        // MoveLeg(legs[8], 90);
        // MoveLeg(legs[11], 90);

        //m_Oscillator.ManagedUpdate(); ***

        float currentDistanceToTarget = Vector3.Distance(body.transform.position, cube.transform.position);
        float distanceReward = distToTarget - currentDistanceToTarget;
        //AddReward(distanceReward);
        distToTarget = currentDistanceToTarget;

        if (currentDistanceToTarget < 1f)
        {
            AddReward(100.0f);
            EndEpisode();
        }

        if (currentDistanceToTarget > 11f)
        {
            AddReward(-100.0f);
            EndEpisode();
        }

        // if (distanceReward < 0)
        // {
        //     AddReward(-0.01f);
        // }

        // if (body.velocity.magnitude < 0.1f)
        // {
        //     AddReward(-0.01f);
        // }
    }

    private void ApplySinMovement_1(ActionSegment<float> actions, int[] indices)
    {
        float time = Time.time;
        foreach (var index in indices)
        {
            actions[index] = Mathf.Sin(Mathf.PI / 12);
        }
    }

    private void ApplySinMovement_2(ActionSegment<float> actions, int[] indices)
    {
        float time = Time.time;
        foreach (var index in indices)
        {
            //actions[index] = Mathf.Sin(Mathf.PI / 15.12f);
            //actions[index] = Mathf.Sin(Mathf.PI / 15);
            actions[index] = 0.5f;
        }
    }

    private void ApplySinMovement_3(ActionSegment<float> actions, int[] indices)
    {
        float time = Time.time;
        foreach (var index in indices)
        {
            //actions[index] = Mathf.Sin(Mathf.PI / 15.12f);
            //actions[index] = -Mathf.Sin(Mathf.PI / 20);
            actions[index] = 0.5f;
        }
    }

    private void ApplySinMovement1(ActionSegment<float> actions, int[] indices)
    {
        float time = Time.time;
        foreach (var index in indices)
        {
            actions[index] = 1f;
        }
    }

    private void ApplySinMovement2(ActionSegment<float> actions, int[] indices)
    {
        float time = Time.time;
        foreach (var index in indices)
        {
            actions[index] = 1f;
        }
    }

    private void ApplySinMovement3(ActionSegment<float> actions, int[] indices)
    {
        float time = Time.time;
        foreach (var index in indices)
        {
            actions[index] = 0.2f;
        }
    }

    private void ApplySinMovement4(ActionSegment<float> actions, int[] indices)
    {
        float time = Time.time;
        foreach (var index in indices)
        {
            actions[index] = 0.5f;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;

        if (change) {
            Debug.Log("YES");
            ApplySinMovement_1(continuousActions, new[] { 0, 1, 2, 3 });
            ApplySinMovement_2(continuousActions, new[] { 4, 5, 6, 7 });
            ApplySinMovement_3(continuousActions, new[] { 8, 9, 10, 11 });
            //change = false;
        }

        // Период между обновлениями в секундах
        const float stepDuration = 0.05f;
        float currentTime = Time.time;

        // Проверяем, прошло ли достаточно времени для следующего этапа
        if (currentTime - lastUpdateTime >= stepDuration)
        {
            lastUpdateTime = currentTime;

            // Обновляем текущий этап (циклическое переключение между 0 и 1)
            currentStep = (currentStep + 1) % 8;
        }

        // Настройка движений в зависимости от этапа
        if (currentStep == 0)
        {
            // Движение первой группы лап
            ApplySinMovement1(continuousActions, new[] { 4, 7 });
        }
        else if (currentStep == 1)
        {
            // Движение второй группы лап
            ApplySinMovement2(continuousActions, new[] { 8, 11 });
        }
        else if (currentStep == 2)
        {
            // Движение второй группы лап
            ApplySinMovement3(continuousActions, new[] { 4, 7 });
        }
        else if (currentStep == 3)
        {
            // Движение второй группы лап
            ApplySinMovement4(continuousActions, new[] { 8, 11 });
        }
        else if (currentStep == 4)
        {
            // Движение первой группы лап
            ApplySinMovement1(continuousActions, new[] { 5, 6 });
        }
        else if (currentStep == 5)
        {
            // Движение второй группы лап
            ApplySinMovement2(continuousActions, new[] { 9, 10 });
        }
        else if (currentStep == 6)
        {
            // Движение второй группы лап
            ApplySinMovement3(continuousActions, new[] { 5, 6 });
        }
        else if (currentStep == 7)
        {
            // Движение второй группы лап
            ApplySinMovement4(continuousActions, new[] { 9, 10 });
        }

        // ApplySinMovement_1(continuousActions, new[] { 0, 1, 2, 3 });
        // ApplySinMovement_2(continuousActions, new[] { 4, 5, 6, 7 });
        // ApplySinMovement_3(continuousActions, new[] { 8, 9, 10, 11 });
        // MoveLeg(legs[11], 90);
        //MoveLeg(legs[8], 90);
        // MoveLeg(legs[11], 90);

        // continuousActions[8] = Input.GetAxisRaw("Horizontal");
        // continuousActions[11] = Input.GetAxisRaw("Vertical");
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
                //AddReward(1f);
                body.AddForce(2f * strenghtMove * (cube.transform.position - body.transform.position).normalized);
                for (int i = 0; i < 12; i++)
                {
                    legs[i].AddForce((cube.transform.position - body.transform.position).normalized * strenghtMove / 10f);
                }
            }
            else
            {
                //AddReward(-0.001f);
            }
        }
        // if (Math.Abs(-0.03 - body.transform.position.z) >= 0.02) {
        //     AddReward(-100f);
        //     EndEpisode();
        // }
        // if (Math.Abs(-0.03 - body.transform.position.z) <= 0.03) {
        //     AddReward(1f);
        // }
        Debug.DrawRay(body.transform.position, body.transform.right, Color.white);
    }

    void MoveLeg(ArticulationBody leg, float targetAngle)
    {
        leg.GetComponent<Leg>().MoveLeg(targetAngle, servoSpeed);
    }
}
