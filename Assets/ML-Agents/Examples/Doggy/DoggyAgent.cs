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

    private float lastUpdateForwardTime = 0f;
    private int currentForwardStep = 0;
    private float lastUpdateRightTime = 0f;
    private int currentRightStep = 0;
    private float lastUpdateLeftTime = 0f;
    private int currentLeftStep = 0;
    //private bool change = false;
    private float ang = 10f;

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

        //change = true;
        
        for (int i = 0; i < 12; i++)
        {
            float angle = 0f;
            if (i < 4) {
                //angle = Mathf.Lerp(legs[i].xDrive.lowerLimit, legs[i].xDrive.upperLimit, (Mathf.Sin(Mathf.PI / 12) + 1) * 0.5f);
            }
            else if (i < 8) {
                angle = Mathf.Lerp(legs[i].xDrive.lowerLimit, legs[i].xDrive.upperLimit, (0.5f + 1) * 0.5f);
            }
            else {
                angle = Mathf.Lerp(legs[i].xDrive.lowerLimit, legs[i].xDrive.upperLimit, (0.5f + 1) * 0.5f);
            }
            MoveLeg(legs[i], angle);
        }

        lastUpdateForwardTime = 0f;
        currentForwardStep = 0;
        lastUpdateRightTime = 0f;
        currentRightStep = 0;
        lastUpdateLeftTime = 0f;
        currentLeftStep = 0;


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
            // float angle = Mathf.Lerp(legs[i].xDrive.lowerLimit, legs[i].xDrive.upperLimit, (actions[i] + 1) * 0.5f);
            // MoveLeg(legs[i], angle);
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

    private void ApplySinMovement_1(int[] indices)
    {
        float time = Time.time;
        foreach (var index in indices)
        {
            float action = Mathf.Sin(Mathf.PI / 12);;
            float angle = Mathf.Lerp(legs[index].xDrive.lowerLimit, legs[index].xDrive.upperLimit, (action + 1) * 0.5f);
            MoveLeg(legs[index], angle);
        }
    }

    private void ApplySinMovement_2(int[] indices)
    {
        float time = Time.time;
        foreach (var index in indices)
        {
            //actions[index] = Mathf.Sin(Mathf.PI / 15.12f);
            //actions[index] = Mathf.Sin(Mathf.PI / 15);
            float action = 0.5f;
            float angle = Mathf.Lerp(legs[index].xDrive.lowerLimit, legs[index].xDrive.upperLimit, (action + 1) * 0.5f);
            MoveLeg(legs[index], angle);
        }
    }

    private void ApplySinMovement_3(int[] indices)
    {
        float time = Time.time;
        foreach (var index in indices)
        {
            //actions[index] = Mathf.Sin(Mathf.PI / 15.12f);
            //actions[index] = -Mathf.Sin(Mathf.PI / 20);
            float action = 0.5f;
            float angle = Mathf.Lerp(legs[index].xDrive.lowerLimit, legs[index].xDrive.upperLimit, (action + 1) * 0.5f);
            MoveLeg(legs[index], angle);
        }
    }

    private void ApplySinMovement1(int[] indices)
    {
        float time = Time.time;
        foreach (var index in indices)
        {
            float action = 1f;
            float angle = Mathf.Lerp(legs[index].xDrive.lowerLimit, legs[index].xDrive.upperLimit, (action + 1) * 0.5f);
            MoveLeg(legs[index], angle);
        }
    }

    private void ApplySinMovement2(int[] indices)
    {
        float time = Time.time;
        foreach (var index in indices)
        {
            float action = 1f;
            float angle = Mathf.Lerp(legs[index].xDrive.lowerLimit, legs[index].xDrive.upperLimit, (action + 1) * 0.5f);
            MoveLeg(legs[index], angle);
        }
    }

    private void ApplySinMovement3(int[] indices)
    {
        float time = Time.time;
        foreach (var index in indices)
        {
            float action = 0.1f;
            float angle = Mathf.Lerp(legs[index].xDrive.lowerLimit, legs[index].xDrive.upperLimit, (action + 1) * 0.5f);
            //Debug.Log(angle);
            MoveLeg(legs[index], angle);
            // if (time < 4) {
            //     Debug.Log("YEST");
            //     MoveLeg(legs[index], angle);
            // }
            // else {
            //     Debug.Log("NO");
            //     MoveLeg(legs[index], 2 * angle);
            // }
        }
    }

    private void ApplySinMovement4(int[] indices)
    {
        float time = Time.time;
        foreach (var index in indices)
        {
            float action = 0.5f;
            float angle = Mathf.Lerp(legs[index].xDrive.lowerLimit, legs[index].xDrive.upperLimit, (action + 1) * 0.5f);
            MoveLeg(legs[index], angle);
        }
    }

    private void ApplySinMovement5(int[] indices)
    {
        float time = Time.time;
        foreach (var index in indices)
        {
            float action = 0.8f;
            float angle = Mathf.Lerp(legs[index].xDrive.lowerLimit, legs[index].xDrive.upperLimit, (action + 1) * 0.5f);
            MoveLeg(legs[index], angle);
        }
    }

    private void MoveForward(float speed)
    {
        // Период между обновлениями в секундах
        float stepDuration = speed;
        float currentTime = Time.time;

        // Проверяем, прошло ли достаточно времени для следующего этапа
        if (currentTime - lastUpdateForwardTime >= stepDuration)
        {
            lastUpdateForwardTime = currentTime;

            // Обновляем текущий этап (циклическое переключение между 0 и 1)
            currentForwardStep = (currentForwardStep + 1) % 8;
        }

        ApplySinMovement_2(new[] { 4, 5, 6, 7 });
        ApplySinMovement_3(new[] { 8, 9, 10, 11 });

        if (true) {
            // Настройка движений в зависимости от этапа
            if (currentForwardStep == 0)
            {
                // Движение первой группы лап
                ApplySinMovement1(new[] { 4, 7 });
            }
            else if (currentForwardStep == 1)
            {
                // Движение второй группы лап
                ApplySinMovement2(new[] { 8, 11 });
                if (currentTime > 4) {
                    ApplySinMovement_2(new[] { 5, 6 });
                }
            }
            else if (currentForwardStep == 2)
            {
                // Движение второй группы лап
                ApplySinMovement3(new[] { 4, 7 });
            }
            else if (currentForwardStep == 3)
            {
                // Движение второй группы лап
                ApplySinMovement4(new[] { 8, 11 });
            }
            else if (currentForwardStep == 4)
            {
                // Движение первой группы лап
                ApplySinMovement1(new[] { 5, 6 });
            }
            else if (currentForwardStep == 5)
            {
                // Движение второй группы лап
                ApplySinMovement2(new[] { 9, 10 });
                if (currentTime > 4) {
                    ApplySinMovement_2(new[] { 4, 7 });
                }
            }
            else if (currentForwardStep == 6)
            {
                // Движение второй группы лап
                ApplySinMovement3(new[] { 5, 6 });
            }
            else if (currentForwardStep == 7)
            {
                // Движение второй группы лап
                ApplySinMovement4(new[] { 9, 10 });
            }
        }
    } 

    private void MoveRight(float speed)
    {
        // Период между обновлениями в секундах
        float stepDuration = speed;
        float currentTime = Time.time;

        // Проверяем, прошло ли достаточно времени для следующего этапа
        if (currentTime - lastUpdateRightTime >= stepDuration)
        {
            lastUpdateRightTime = currentTime;

            // Обновляем текущий этап (циклическое переключение между 0 и 1)
            currentRightStep = (currentRightStep + 1) % 7;
            // ang += 15f;
            // if (ang >= 60) {
            //     ang = 0f;
            // }
        }

        //ApplySinMovement_2(new[] { 4, 5, 6, 7 });
        //ApplySinMovement_3(new[] { 8, 9, 10, 11 });

        if (true) {
            // Настройка движений в зависимости от этапа
            if (currentRightStep == 0)
            {
                // Движение первой группы лап
                ApplySinMovement1(new[] { 4, 7 });
            }
            else if (currentRightStep == 1)
            {
                // Движение второй группы лап
                MoveLeg(legs[0], ang);
                MoveLeg(legs[3], -ang);
            }
            else if (currentRightStep == 2)
            {
                // Движение второй группы лап
                MoveLeg(legs[4], 15);
                MoveLeg(legs[7], 15);
            }
            else if (currentRightStep == 3)
            {
                // Движение второй группы лап
                ApplySinMovement1(new[] { 5, 6 });
            }
            else if (currentRightStep == 4)
            {
                // Движение первой группы лап
                MoveLeg(legs[1], -ang);
                MoveLeg(legs[3], -ang);
            }
            else if (currentRightStep == 5)
            {
                // Движение второй группы лап
                MoveLeg(legs[5], 15);
                MoveLeg(legs[6], 15);
            }
            else if (currentRightStep == 6)
            {
                // Движение второй группы лап
                MoveLeg(legs[1], 0);
                MoveLeg(legs[0], 0);
                MoveLeg(legs[2], 0);
                MoveLeg(legs[3], 0);
            }
            else if (currentRightStep == 7)
            {
                // Движение второй группы лап
                ApplySinMovement4(new[] { 9, 10 });
            }
        }
    } 

    private void MoveLeft(float speed)
    {
        // Период между обновлениями в секундах
        float stepDuration = speed;
        float currentTime = Time.time;

        // Проверяем, прошло ли достаточно времени для следующего этапа
        if (currentTime - lastUpdateLeftTime >= stepDuration)
        {
            lastUpdateLeftTime = currentTime;

            // Обновляем текущий этап (циклическое переключение между 0 и 1)
            currentLeftStep = (currentLeftStep + 1) % 7;
            // ang += 15f;
            // if (ang >= 60) {
            //     ang = 0f;
            // }
        }

        //ApplySinMovement_2(new[] { 4, 5, 6, 7 });
        //ApplySinMovement_3(new[] { 8, 9, 10, 11 });

        if (true) {
            // Настройка движений в зависимости от этапа
            if (currentLeftStep == 0)
            {
                // Движение первой группы лап
                ApplySinMovement1(new[] { 5, 6 });
            }
            else if (currentLeftStep == 1)
            {
                // Движение второй группы лап
                MoveLeg(legs[1], ang);
                MoveLeg(legs[2], -ang);
            }
            else if (currentLeftStep == 2)
            {
                // Движение второй группы лап
                MoveLeg(legs[5], 15);
                MoveLeg(legs[6], 15);
            }
            else if (currentLeftStep == 3)
            {
                // Движение второй группы лап
                ApplySinMovement1(new[] { 4, 7 });
            }
            else if (currentLeftStep == 4)
            {
                // Движение первой группы лап
                MoveLeg(legs[0], -ang);
                MoveLeg(legs[2], -ang);
            }
            else if (currentLeftStep == 5)
            {
                // Движение второй группы лап
                MoveLeg(legs[4], 15);
                MoveLeg(legs[7], 15);
            }
            else if (currentLeftStep == 6)
            {
                // Движение второй группы лап
                MoveLeg(legs[0], 0);
                MoveLeg(legs[1], 0);
                MoveLeg(legs[3], 0);
                MoveLeg(legs[2], 0);
            }
            else if (currentLeftStep == 7)
            {
                // Движение второй группы лап
                ApplySinMovement4(new[] { 9, 10 });
            }
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;

        //MoveForward(0.001f);
        MoveLeft(0.1f);

        // MoveLeg(legs[0], 15);
        // MoveLeg(legs[3], -15);

        // if (change) {
        //     Debug.Log("YES");
        //     ApplySinMovement_1(continuousActions, new[] { 0, 1, 2, 3 });
        //     ApplySinMovement_2(continuousActions, new[] { 4, 5, 6, 7 });
        //     ApplySinMovement_3(continuousActions, new[] { 8, 9, 10, 11 });
        //     //change = false;
        // }

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
        // body.AddForce((cube.transform.position - body.transform.position).normalized * strenghtMove);
        // for (int i = 0; i < 12; i++)
        // {
        //    legs[i].AddForce((cube.transform.position - body.transform.position).normalized * strenghtMove / 20f);
        // }

        RaycastHit hit;
        if (Physics.Raycast(body.transform.position, body.transform.right, out hit))
        {
            if (hit.collider.gameObject == cube)
            {
                //AddReward(1f);
                // body.AddForce(2f * strenghtMove * (cube.transform.position - body.transform.position).normalized);
                // for (int i = 0; i < 12; i++)
                // {
                //     legs[i].AddForce((cube.transform.position - body.transform.position).normalized * strenghtMove / 10f);
                // }
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