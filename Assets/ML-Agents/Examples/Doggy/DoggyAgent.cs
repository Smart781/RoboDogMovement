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

    public GameObject foot;

    public GameObject footLF;
    public GameObject footRF;
    public GameObject footLB;
    public GameObject footRB;


    private float distToTarget = 0f;

    //private Oscillator m_Oscillator;

    private float lastUpdateForwardTime = 0f;
    private int currentForwardStep = 0;
    private float lastUpdateRightTime = 0f;
    private int currentRightStep = 0;
    private float lastUpdateLeftTime = 0f;
    private int currentLeftStep = 0;
    float dfoot = 0.0f;
    float ufoot = 0.0f;
    //private bool change = false;
    private float ang = 10f;
    // private int pred_ind = -1;
    //private float pred_speed = 1f;
    // private bool compl = false;
    // float func_time = 0.0f;
    private Vector3[] startPosition = new Vector3[4];
    private Vector3[] endPosition = new Vector3[4];
    Vector4 UpFoot = new Vector4(0.5f, 0.5f, 0.5f, 0.5f);
    Vector4 DownFoot = new Vector4(0.5f, 0.5f, 0.5f, 0.5f);
    Vector4 results = new Vector4(0, 0, 0, 0);
    Vector4 change_results = new Vector4(0, 0, 0, 0);
    float[] prev_actions = new float[12];
    // private bool flag = false;

    private float FootFlag1 = -1;
    private float len = 0.02f;

    [Header("Параметры походки")]
    public float gaitFrequency = 1f;
    public float legSwingAngle = 30f;
    public float legLiftAngle = 15f;
    private float gaitPhase = 0f;

    [Header("Speed Control")]
    public float maxSpeed = 5f;
    public float minFrequency = 0.8f;
    public float maxFrequency = 3f;
    public float minSwingAngle = 25f;
    public float maxSwingAngle = 50f;
    public float minLiftAngle = 10f;
    public float maxLiftAngle = 25f;
    public float pushForceMultiplier = 3f;

    private Vector3 lastPosition;
    private float currentSpeed;

    private bool button = true;
    float button_time = 0.0f;

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
        // Quaternion newRot = Quaternion.Euler(-90, 0, Random.Range(0f, 360f));


        // body.TeleportRoot(defPos, newRot);
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

        //FootPos = transform.TransformPoint(foot.transform.position) + foot.transform.right * 0.5f;

        // Vector3 startPosition = foot.transform.position;

        // Vector3 direction = footLF.transform.right.normalized; // Нормализуем, чтобы длина была 1

        // float rayLength = 0.05f;

        // Debug.DrawRay(startPosition[0], direction * rayLength, Color.black);

        // endPosition = startPosition + direction * rayLength;

        // Debug.Log("Конечная точка луча: " + endPosition);

        // Debug.Log("Конечная точка вертикального луча: " + verticalEndPosition);


        // MoveLeg(legs[8], 90);
        // MoveLeg(legs[11], 90);
    }

    public override void OnEpisodeBegin()
    {
        ResetDog();
        //m_Oscillator.ManagedReset(); ***

        //cube.transform.position = new Vector3(5, 0.21f, Random.Range(-2f, 2f));
        //cube.transform.position = new Vector3(Random.Range(-7.5f, 7.5f), 0.21f, Random.Range(-7.5f, 7.5f));
        //cube.transform.position = new Vector3(5f, 0.21f, 0); ***

        //cube.transform.position = new Vector3(8f, 0.26f, 0f);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(body.transform.position);
        sensor.AddObservation(body.velocity);
        sensor.AddObservation(body.angularVelocity);
        sensor.AddObservation(body.transform.right);

        sensor.AddObservation(cube.transform.position);

        Vector3 relativePosition = cube.transform.position - body.transform.position;
        sensor.AddObservation(relativePosition);

        Vector3 toCube = (cube.transform.position - body.transform.position).normalized;
        float angleToCube = Vector3.SignedAngle(body.transform.right, toCube, Vector3.up);
        sensor.AddObservation(angleToCube);

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

        // float val = -1f;
        // int ind = -1;
        // // float func_time = 0.0f;

        // for (int i = 0; i < 3; i++)
        // {
        //     float current_action = ((actions[i] * 1f) + 1) / 2;
        //     if (current_action > val) {
        //         val = current_action;
        //         ind = i;
        //     }
        // }

        // if (compl) {
        //     // Debug.Log(pred_ind);
        //     var articulationBody = body.GetComponent<ArticulationBody>();
            
        //     bool res = false;
        //     if (pred_ind == 0) {
        //         // res = MoveForward(0.01f);
        //         // AddReward(0.01f);
        //         if (articulationBody != null)
        //         {
        //             articulationBody.centerOfMass = new Vector3(0.3f, 0, 0);
        //         }
        //         F_TROT(func_time, 0.5f, -60f, 30f, 2f, -35f, 35f, 1.8f);
        //         if (Time.time - func_time >= 4f) {
        //             res = true;
        //         }
        //     }
        //     else if (pred_ind == 1) {
        //         if (articulationBody != null)
        //         {
        //             articulationBody.centerOfMass = new Vector3(0.1f, 0, 0);
        //         }
        //         res = MoveRight(0.1f);
        //     }
        //     else {
        //         if (articulationBody != null)
        //         {
        //             articulationBody.centerOfMass = new Vector3(0.1f, 0, 0);
        //         }
        //         res = MoveLeft(0.1f);
        //     }
        //     if (res) {
        //         compl = false;
        //     }
        // }
        // else {
        //     if (val >= 0.5) {
        //         //pred_speed = ((actions[3 + ind] * 1f) + 1) / 2;
        //         for (int i = 0; i < 12; i++) {
        //             MoveLeg(legs[i], 0);
        //         }
        //         pred_ind = ind;
        //         compl = true;
        //         func_time = Time.time;
        //     }
        // }

        //Debug.Log(foot.transform.position);
        // Debug.Log(foot.transform.right);
        // Debug.Log(foot.transform.position);
        //Debug.Log(FootPos);
        //Debug.DrawRay(foot.transform.position, foot.transform.right, Color.black);

        float time = Time.time;

        // if (time > 0.1f) {
        //     flag = true;
        // }

        //Debug.Log(time);

        // Debug.DrawRay(footLF.transform.position, footLF.transform.right.normalized * len, Color.black);


        // if (!flag) {
        //     startPosition = foot.transform.position;
        //     endPosition = foot.transform.position + foot.transform.right.normalized * len;
        // }

        Debug.DrawRay(startPosition[0], Vector3.up * 1f, Color.green);


        // Vector3 verticalStartPosition = endPosition;

        // Vector3 verticalDirection = Vector3.up;

        // float verticalRayLength = 1f;

        Debug.DrawRay(endPosition[0], Vector3.up * 1f, Color.red);

        // Vector3 verticalEndPosition = verticalStartPosition + verticalDirection * verticalRayLength;

        // UpdateSpeedMeasurement();

        // TROT(3f);


        // for (int i = 4; i < 12; i++)
        // {
        //     if (i % 4 == 2 || i % 4 == 3) {
        //         continue;
        //     }
        //     float angle = Mathf.Lerp(legs[i].xDrive.lowerLimit, legs[i].xDrive.upperLimit, (actions[i] + 1) * 0.5f);
        //     if (prev_actions[i] != actions[i]) {
        //         if (i % 4 == 0) {
        //             MoveLeg(legs[i], angle);
        //             MoveLeg(legs[i + 3], angle);
        //         }
        //         else {
        //             MoveLeg(legs[i], angle);
        //             MoveLeg(legs[i + 1], angle);
        //         }
        //         //Debug.Log(i);
        //         //MoveLeg(legs[check_ind], angle);
        //     }
        //     prev_actions[i] = actions[i];
        //     // if ((i % 4) != 0 && (i % 4) != 3) {
        //     //     if (i % 4 == 1) {
        //     //         float angle = Mathf.Lerp(legs[i].xDrive.lowerLimit, legs[i].xDrive.upperLimit, (actions[i] + 1) * 0.5f);
        //     //         MoveLeg(legs[i], angle);
        //     //     }
        //     //     else {
        //     //         float angle = Mathf.Lerp(legs[i - 1].xDrive.lowerLimit, legs[i - 1].xDrive.upperLimit, (actions[i - 1] + 1) * 0.5f);
        //     //         MoveLeg(legs[i], angle);
        //     //     }
        //     // }
        // }
        
        // MoveLeg(legs[8], 90);
        // MoveLeg(legs[11], 90);

        //m_Oscillator.ManagedUpdate(); ***

        float currentDistanceToTarget = Vector3.Distance(body.transform.position, cube.transform.position);
        float distanceReward = distToTarget - currentDistanceToTarget;
        AddReward(distanceReward);
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

        // if (Time.time > 0.1f) {
        //     Debug.Log("YES");
        //     foot.transform.RotateAround(endPosition, Vector3.up, 30 * Time.deltaTime);
        // }

        // if ((Math.Abs(0.17 - body.transform.position.y) >= 0.025f) || !button) {
        //     position();
        // }
    }

    private void position() {
        // Debug.Log("Position");
        if (button) {
            MoveLeg(legs[4], 0);
            MoveLeg(legs[5], 0);
            MoveLeg(legs[6], 0);
            MoveLeg(legs[7], 0);

            MoveLeg(legs[8], 90);
            MoveLeg(legs[9], 90);
            MoveLeg(legs[10], 90);
            MoveLeg(legs[11], 90);

            button = false;
            button_time = Time.time;
        }

        else if (Time.time - button_time >= 2.0f) {
            float angle = 90 * (1 - (Time.time - button_time - 2.0f) / 5);
            if (angle <= 0) {
                MoveLeg(legs[8], 0);
                MoveLeg(legs[9], 0);
                MoveLeg(legs[10], 0);
                MoveLeg(legs[11], 0);
                if (Time.time - button_time >= 8.0f) {
                    button = true;
                }
            }
            else {
                MoveLeg(legs[8], angle);
                MoveLeg(legs[9], angle);
                MoveLeg(legs[10], angle);
                MoveLeg(legs[11], angle);
            }
        }

    }

    private void change_positionLF()
    {
        float time = Time.time;

        if (time > 0.1f) {
            Debug.Log("YES");
            startPosition[0] = footLF.transform.position;
            endPosition[0] = footLF.transform.position + footLF.transform.right.normalized * len;
        }
        else{
            Debug.Log("NO");
        }
    }

    private void change_positionRF()
    {
        float time = Time.time;

        if (time > 0.1f) {
            startPosition[1] = footRF.transform.position;
            endPosition[1] = footRF.transform.position + footRF.transform.right.normalized * len;
        }
    }

    private void change_positionLB()
    {
        float time = Time.time;

        if (time > 0.1f) {
            startPosition[2] = footLB.transform.position;
            endPosition[2] = footLB.transform.position + footLB.transform.right.normalized * len;
        }
    }

    private void change_positionRB()
    {
        float time = Time.time;

        if (time > 0.1f) {
            startPosition[3] = footRB.transform.position;
            endPosition[3] = footRB.transform.position + footRB.transform.right.normalized * len;
        }
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

    private bool MoveForward(float speed)
    {
        float stepDuration = speed;
        float currentTime = Time.time;

        if (currentTime - lastUpdateForwardTime >= stepDuration)
        {
            lastUpdateForwardTime = currentTime;

            currentForwardStep = (currentForwardStep + 1) % 8;
        }

        // ApplySinMovement_2(new[] { 4, 5, 6, 7 });
        // ApplySinMovement_3(new[] { 8, 9, 10, 11 });

        if (currentForwardStep == 0)
        {
            ApplySinMovement1(new[] { 4, 7 });
        }
        else if (currentForwardStep == 1)
        {
            ApplySinMovement2(new[] { 8, 11 });
            if (currentTime > 4) {
                ApplySinMovement_2(new[] { 5, 6 });
            }
        }
        else if (currentForwardStep == 2)
        {
            ApplySinMovement3(new[] { 4, 7 });
        }
        else if (currentForwardStep == 3)
        {
            ApplySinMovement4(new[] { 8, 11 });
        }
        else if (currentForwardStep == 4)
        {
            ApplySinMovement1(new[] { 5, 6 });
        }
        else if (currentForwardStep == 5)
        {
            ApplySinMovement2(new[] { 9, 10 });
            if (currentTime > 4) {
                ApplySinMovement_2(new[] { 4, 7 });
            }
        }
        else if (currentForwardStep == 6)
        {
            ApplySinMovement3(new[] { 5, 6 });
        }
        else if (currentForwardStep == 7)
        {
            ApplySinMovement4(new[] { 9, 10 });
        }

        if (currentForwardStep == 7) {
            ApplySinMovement_2(new[] { 4, 5, 6, 7 });
            ApplySinMovement_3(new[] { 8, 9, 10, 11 });
            return true;
        }
        else {
            return false;
        }
    } 

    private bool MoveRight(float speed)
    {
        float stepDuration = speed;
        float currentTime = Time.time;

        if (currentTime - lastUpdateRightTime >= stepDuration)
        {
            lastUpdateRightTime = currentTime;

            currentRightStep = (currentRightStep + 1) % 7;
            // ang += 15f;
            // if (ang >= 60) {
            //     ang = 0f;
            // }
        }

        //ApplySinMovement_2(new[] { 4, 5, 6, 7 });
        //ApplySinMovement_3(new[] { 8, 9, 10, 11 });

        if (true) {
            if (currentRightStep == 0)
            {
                ApplySinMovement1(new[] { 4, 7 });
            }
            else if (currentRightStep == 1)
            {
                MoveLeg(legs[0], ang);
                MoveLeg(legs[3], -ang);
            }
            else if (currentRightStep == 2)
            {
                MoveLeg(legs[4], 15);
                MoveLeg(legs[7], 15);
            }
            else if (currentRightStep == 3)
            {
                ApplySinMovement1(new[] { 5, 6 });
            }
            else if (currentRightStep == 4)
            {
                MoveLeg(legs[1], -ang);
                MoveLeg(legs[3], -ang);
            }
            else if (currentRightStep == 5)
            {
                MoveLeg(legs[5], 15);
                MoveLeg(legs[6], 15);
            }
            else if (currentRightStep == 6)
            {
                MoveLeg(legs[1], 0);
                MoveLeg(legs[0], 0);
                MoveLeg(legs[2], 0);
                MoveLeg(legs[3], 0);
            }
        }

        if (currentRightStep == 6) {
            Debug.Log("KKK");
            return true;
        }
        else {
            return false;
        }
    } 

    private bool MoveLeft(float speed)
    {
        float stepDuration = speed;
        float currentTime = Time.time;

        if (currentTime - lastUpdateLeftTime >= stepDuration)
        {
            lastUpdateLeftTime = currentTime;

            currentLeftStep = (currentLeftStep + 1) % 7;
            // ang += 15f;
            // if (ang >= 60) {
            //     ang = 0f;
            // }
        }

        //ApplySinMovement_2(new[] { 4, 5, 6, 7 });
        //ApplySinMovement_3(new[] { 8, 9, 10, 11 });

        if (true) {
            if (currentLeftStep == 0)
            {
                ApplySinMovement1(new[] { 5, 6 });
            }
            else if (currentLeftStep == 1)
            {
                MoveLeg(legs[1], ang);
                MoveLeg(legs[2], -ang);
            }
            else if (currentLeftStep == 2)
            {
                MoveLeg(legs[5], 15);
                MoveLeg(legs[6], 15);
            }
            else if (currentLeftStep == 3)
            {
                ApplySinMovement1(new[] { 4, 7 });
            }
            else if (currentLeftStep == 4)
            {
                MoveLeg(legs[0], -ang);
                MoveLeg(legs[2], -ang);
            }
            else if (currentLeftStep == 5)
            {
                MoveLeg(legs[4], 15);
                MoveLeg(legs[7], 15);
            }
            else if (currentLeftStep == 6)
            {
                MoveLeg(legs[0], 0);
                MoveLeg(legs[1], 0);
                MoveLeg(legs[3], 0);
                MoveLeg(legs[2], 0);
            }
        }

        if (currentLeftStep == 6) {
            Debug.Log("QQQ");
            return true;
        }
        else {
            return false;
        }
    }

   private bool MoveSinBackward(float speed)
    {
        float stepDuration = speed;
        float currentTime = Time.time;

        if (currentTime - lastUpdateForwardTime >= stepDuration)
        {
            lastUpdateForwardTime = currentTime;

            currentForwardStep = (currentForwardStep + 1) % 8;
        }

        float sinValue = Mathf.Sin(currentForwardStep * Mathf.PI / 4); 
        float cosValue = Mathf.Cos(currentForwardStep * Mathf.PI / 4); 

        MoveLeg(legs[4], sinValue * 30); 
        MoveLeg(legs[5], -sinValue * 30); 
        MoveLeg(legs[6], cosValue * 30); 
        MoveLeg(legs[7], -cosValue * 30); 

        MoveLeg(legs[8], sinValue * 45); 
        MoveLeg(legs[9], -sinValue * 45); 
        MoveLeg(legs[10], cosValue * 45);
        MoveLeg(legs[11], -cosValue * 45); 

        return currentForwardStep == 7;
    }

    private bool MoveSinForward(float speed)
    {
        float stepDuration = speed;
        float currentTime = Time.time;

        if (currentTime - lastUpdateForwardTime >= stepDuration)
        {
            lastUpdateForwardTime = currentTime;

            currentForwardStep = (currentForwardStep + 1) % 8;
        }

        float sinValue = Mathf.Sin(currentForwardStep * Mathf.PI / 4); 
        float cosValue = Mathf.Cos(currentForwardStep * Mathf.PI / 4); 

        MoveLeg(legs[4], -sinValue * 60); 
        // MoveLeg(legs[5], -cosValue * 90); 
        // MoveLeg(legs[6], -cosValue * 90); 
        // MoveLeg(legs[7], -sinValue * 90); 

        MoveLeg(legs[8], sinValue * 45); 
        // MoveLeg(legs[9], cosValue * 30); 
        // MoveLeg(legs[10], cosValue * 30); 
        // MoveLeg(legs[11], sinValue * 30); 

        return currentForwardStep == 7;
    }

    private bool MoveSinRight(float speed)
    {
        float stepDuration = speed;
        float currentTime = Time.time;
        float sinValue = Mathf.Sin(currentTime * Mathf.PI * 2 / stepDuration) * 15;

        if (currentTime - lastUpdateRightTime >= stepDuration)
        {
            lastUpdateRightTime = currentTime;
            currentRightStep = (currentRightStep + 1) % 7;
        }

        if (currentRightStep == 0)
        {
            ApplySinMovement1(new[] { 4, 7 });
        }
        else if (currentRightStep == 1)
        {
            MoveLeg(legs[0], sinValue);
            MoveLeg(legs[3], -sinValue);
        }
        else if (currentRightStep == 2)
        {
            MoveLeg(legs[4], 15);
            MoveLeg(legs[7], 15);
        }
        else if (currentRightStep == 3)
        {
            ApplySinMovement1(new[] { 5, 6 });
        }
        else if (currentRightStep == 4)
        {
            MoveLeg(legs[1], -sinValue);
            MoveLeg(legs[3], -sinValue);
        }
        else if (currentRightStep == 5)
        {
            MoveLeg(legs[5], 15);
            MoveLeg(legs[6], 15);
        }
        else if (currentRightStep == 6)
        {
            ResetLegs();
        }

        return currentRightStep == 6;
    }

    private bool MoveSinLeft(float speed)
    {
        float stepDuration = speed;
        float currentTime = Time.time;
        float sinValue = Mathf.Sin(currentTime * Mathf.PI * 2 / stepDuration) * 15;

        if (currentTime - lastUpdateLeftTime >= stepDuration)
        {
            lastUpdateLeftTime = currentTime;
            currentLeftStep = (currentLeftStep + 1) % 7;
        }

        if (currentLeftStep == 0)
        {
            ApplySinMovement1(new[] { 5, 6 });
        }
        else if (currentLeftStep == 1)
        {
            MoveLeg(legs[1], sinValue);
            MoveLeg(legs[2], -sinValue);
        }
        else if (currentLeftStep == 2)
        {
            MoveLeg(legs[5], 15);
            MoveLeg(legs[6], 15);
        }
        else if (currentLeftStep == 3)
        {
            ApplySinMovement1(new[] { 4, 7 });
        }
        else if (currentLeftStep == 4)
        {
            MoveLeg(legs[0], -sinValue);
            MoveLeg(legs[2], -sinValue);
        }
        else if (currentLeftStep == 5)
        {
            MoveLeg(legs[4], 15);
            MoveLeg(legs[7], 15);
        }
        else if (currentLeftStep == 6)
        {
            ResetLegs();
        }

        return currentLeftStep == 6;
    }

    private void ResetLegs()
    {
        for (int i = 0; i < legs.Length; i++)
        {
            MoveLeg(legs[i], 0);
        }
    }

    private float distance(GameObject foot, int ind)
    {
        return (foot.transform.position.x - endPosition[ind].x) * (foot.transform.position.x - endPosition[ind].x) + (foot.transform.position.y - endPosition[ind].y) * (foot.transform.position.y - endPosition[ind].y) + (foot.transform.position.z - endPosition[ind].z) * (foot.transform.position.z - endPosition[ind].z);
    }

    private void MoveImproveSinForward(float r, float speed)
    {
        if (results[0] == 1) {
            if (results[1] == 1) {
                if (results[2] == 1) {
                    if (results[3] == 1) {
                        results[0] = 0;
                        results[1] = 0;
                        results[2] = 0;
                        results[3] = 0;
                        change_results[0] = 0;
                        change_results[1] = 0;
                        change_results[2] = 0;
                        change_results[3] = 0;
                    }
                    else {
                        if (change_results[3] == 0) {
                            change_positionRB();
                            change_results[3] = 1;
                        }
                        MoveRB(r, speed);
                    }
                }
                else {
                    if (change_results[2] == 0) {
                        change_positionLB();
                        change_results[2] = 1;
                    }
                    MoveLB(r, speed);
                }
            }
            else {
                if (change_results[1] == 0) {
                    change_positionRF();
                    change_results[1] = 1;
                }
                MoveRF(r, speed);
            }
        }
        else {
            if (change_results[0] == 0) {
                change_positionLF();
                change_results[0] = 1;
            }
            MoveLF(r, speed);
        }
    }

    private void MoveLF(float r, float speed)
    {
        float dist = distance(footLF, 0);
        float cur_dist = (footLF.transform.position.x - startPosition[0].x) * (footLF.transform.position.x - startPosition[0].x) + (footLF.transform.position.y - startPosition[0].y) * (footLF.transform.position.y - startPosition[0].y) + (footLF.transform.position.z - startPosition[0].z) * (footLF.transform.position.z - startPosition[0].z);
        // Debug.Log(Math.Abs(0.017 - foot.transform.position.y));
        //Debug.Log(Math.Abs(cur_dist - 4 * r * r));
        // flag = true;
        //Debug.Log(dist - r * r);

        //Debug.Log(startPosition);


        if (Time.time > 0.1f) {
            if (Math.Abs(dist - r * r) < 1e-4) {
                if ((Math.Abs(cur_dist - 4 * r * r) < 1e-4) && (Math.Abs(0.017 - footLF.transform.position.y) < 2e-3)) {
                    Debug.Log("YES1");
                    results[0] = 1;
                    return;
                }
                else {
                    FootFlag1 = 1;
                    UpFoot[0] -= speed;
                    MoveLeg(legs[4], UpFoot[0]);
                    //MoveLeg(legs[6], UpFoot);
                }
            }
            else if (dist < r * r) {
                FootFlag1 = 2;
                DownFoot[0] += speed;
                MoveLeg(legs[8], DownFoot[0]);
                //MoveLeg(legs[10], DownFoot);
            }
            else {
                if (FootFlag1 == 2) {
                    FootFlag1 = 1;
                    UpFoot[0] -= speed;
                    MoveLeg(legs[4], UpFoot[0]);
                    //MoveLeg(legs[6], UpFoot);
                }
                else {
                    FootFlag1 = 3;
                    DownFoot[0] -= speed;
                    MoveLeg(legs[8], DownFoot[0]);
                    //MoveLeg(legs[10], DownFoot);
                }
            }
        }
    }

    private void MoveRF(float r, float speed)
    {
        float dist = distance(footRF, 1);
        float cur_dist = (footRF.transform.position.x - startPosition[1].x) * (footRF.transform.position.x - startPosition[1].x) + (footRF.transform.position.y - startPosition[1].y) * (footRF.transform.position.y - startPosition[1].y) + (footRF.transform.position.z - startPosition[1].z) * (footRF.transform.position.z - startPosition[1].z);
        // Debug.Log(Math.Abs(0.017 - foot.transform.position.y));
        //Debug.Log(Math.Abs(cur_dist - 4 * r * r));
        // flag = true;
        //Debug.Log(dist - r * r);

        //Debug.Log(startPosition);

        if (Time.time > 0.1f) {
            if (Math.Abs(dist - r * r) < 1e-4) {
                if ((Math.Abs(cur_dist - 4 * r * r) < 1e-4) && (Math.Abs(0.017 - footRF.transform.position.y) < 2e-3)) {
                    Debug.Log("YES1");
                    results[1] = 1;
                    return;
                }
                else {
                    FootFlag1 = 1;
                    UpFoot[1] -= speed;
                    MoveLeg(legs[5], UpFoot[1]);
                    //MoveLeg(legs[6], UpFoot);
                }
            }
            else if (dist < r * r) {
                FootFlag1 = 2;
                DownFoot[1] += speed;
                MoveLeg(legs[9], DownFoot[1]);
                //MoveLeg(legs[10], DownFoot);
            }
            else {
                if (FootFlag1 == 2) {
                    FootFlag1 = 1;
                    UpFoot[1] -= speed;
                    MoveLeg(legs[5], UpFoot[1]);
                    //MoveLeg(legs[6], UpFoot);
                }
                else {
                    FootFlag1 = 3;
                    DownFoot[1] -= speed;
                    MoveLeg(legs[9], DownFoot[1]);
                    //MoveLeg(legs[10], DownFoot);
                }
            }
        }
    }

    private void MoveLB(float r, float speed)
    {
        float dist = distance(footLB, 2);
        float cur_dist = (footLB.transform.position.x - startPosition[2].x) * (footLB.transform.position.x - startPosition[2].x) + (footLB.transform.position.y - startPosition[2].y) * (footLB.transform.position.y - startPosition[2].y) + (footLB.transform.position.z - startPosition[2].z) * (footLB.transform.position.z - startPosition[2].z);
        //Debug.Log(foot.transform.position.y);
        Debug.Log(Math.Abs(cur_dist - 4 * r * r));
        // flag = true;
        //Debug.Log(dist - r * r);

        //Debug.Log(startPosition);

        if (Time.time > 0.1f) {
            if (Math.Abs(dist - r * r) < 1e-4) {
                if ((cur_dist >= 4 * r * r) && (Math.Abs(0.017 - footLB.transform.position.y) < 2e-3)) {
                    Debug.Log("YES1");
                    results[2] = 1;
                    return;
                }
                else {
                    FootFlag1 = 1;
                    UpFoot[2] -= speed;
                    MoveLeg(legs[6], UpFoot[2]);
                    //MoveLeg(legs[6], UpFoot);
                }
            }
            else if (dist < r * r) {
                FootFlag1 = 2;
                DownFoot[2] += speed;
                MoveLeg(legs[10], DownFoot[2]);
                //MoveLeg(legs[10], DownFoot);
            }
            else {
                if (FootFlag1 == 2) {
                    FootFlag1 = 1;
                    UpFoot[2] -= speed;
                    MoveLeg(legs[6], UpFoot[2]);
                    //MoveLeg(legs[6], UpFoot);
                }
                else {
                    FootFlag1 = 3;
                    DownFoot[2] -= speed;
                    MoveLeg(legs[10], DownFoot[2]);
                    //MoveLeg(legs[10], DownFoot);
                }
            }
        }
    }

    private void MoveRB(float r, float speed)
    {
        float dist = distance(footRB, 3);
        float cur_dist = (footRB.transform.position.x - startPosition[3].x) * (footRB.transform.position.x - startPosition[3].x) + (footRB.transform.position.y - startPosition[3].y) * (footRB.transform.position.y - startPosition[3].y) + (footRB.transform.position.z - startPosition[3].z) * (footRB.transform.position.z - startPosition[3].z);
        //Debug.Log(foot.transform.position.y);
        Debug.Log(Math.Abs(cur_dist - 4 * r * r));
        // flag = true;
        //Debug.Log(dist - r * r);

        //Debug.Log(startPosition);

        if (Time.time > 0.1f) {
            if (Math.Abs(dist - r * r) < 1e-4) {
                if ((cur_dist >= 4 * r * r) && (Math.Abs(0.017 - footRB.transform.position.y) < 2e-3)) {
                    Debug.Log("YES1");
                    results[3] = 1;
                    return;
                }
                else {
                    FootFlag1 = 1;
                    UpFoot[3] -= speed;
                    MoveLeg(legs[7], UpFoot[3]);
                    //MoveLeg(legs[6], UpFoot);
                }
            }
            else if (dist < r * r) {
                FootFlag1 = 2;
                DownFoot[3] += speed;
                MoveLeg(legs[11], DownFoot[3]);
                //MoveLeg(legs[10], DownFoot);
            }
            else {
                if (FootFlag1 == 2) {
                    FootFlag1 = 1;
                    UpFoot[3] -= speed;
                    MoveLeg(legs[7], UpFoot[3]);
                    //MoveLeg(legs[6], UpFoot);
                }
                else {
                    FootFlag1 = 3;
                    DownFoot[3] -= speed;
                    MoveLeg(legs[11], DownFoot[3]);
                    //MoveLeg(legs[10], DownFoot);
                }
            }
        }
    }


    private void TROT(float targetSpeed)
    {
        float speedFactor = Mathf.Clamp01(targetSpeed / maxSpeed);
        
        float dynamicFrequency = Mathf.Lerp(minFrequency, maxFrequency, speedFactor);
        float dynamicSwing = Mathf.Lerp(minSwingAngle, maxSwingAngle, speedFactor) * 0.7f;
        float dynamicLift = Mathf.Lerp(minLiftAngle, maxLiftAngle, speedFactor);
        
        gaitPhase += Time.fixedDeltaTime * dynamicFrequency * 2f * Mathf.PI;
        if (gaitPhase > 2f * Mathf.PI) gaitPhase -= 2f * Mathf.PI;

        float trotPhase1 = Mathf.Sin(gaitPhase);
        float trotPhase2 = Mathf.Sin(gaitPhase + Mathf.PI);

        MoveLeg(legs[4], trotPhase1 * dynamicSwing);
        MoveLeg(legs[5], trotPhase2 * dynamicSwing);
        MoveLeg(legs[6], trotPhase2 * dynamicSwing);
        MoveLeg(legs[7], trotPhase1 * dynamicSwing);

        float lift1 = Mathf.Max(0, trotPhase1) * dynamicLift;
        float lift2 = Mathf.Max(0, trotPhase2) * dynamicLift;
        
        MoveLeg(legs[8], lift1);
        MoveLeg(legs[9], lift2);
        MoveLeg(legs[10], lift2);
        MoveLeg(legs[11], lift1);

        if (trotPhase1 < -0.8f) 
        {
            float pushForce = speedFactor * pushForceMultiplier;
            body.AddForce(body.transform.right * pushForce, ForceMode.Impulse);
        }
    }

    void UpdateSpeedMeasurement()
    {
        currentSpeed = (body.transform.position - lastPosition).magnitude / Time.fixedDeltaTime;
        lastPosition = body.transform.position;
    }

    private void CPG_TROT(float targetSpeed)
    {
        //const int TROT_GAIT = 1;
        float[] trotOffset = {0.0f, Mathf.PI, Mathf.PI, 0.0f}; 
        //float trotBeta = 0.5f; // 50% времени в опоре
        //float trotTime = 0.5f; // Период цикла (сек)
        //float delta = 1.0f;    // Коэффициент связи
        
        float speedFactor = Mathf.Clamp01(targetSpeed / maxSpeed);
        
        float dynamicFrequency = Mathf.Lerp(1.0f, 3.0f, speedFactor);
        float dynamicAmplitude = Mathf.Lerp(0.5f, 1.5f, speedFactor);
        
        gaitPhase += Time.fixedDeltaTime * dynamicFrequency * 2f * Mathf.PI;
        if (gaitPhase > 2f * Mathf.PI) gaitPhase -= 2f * Mathf.PI;
        
        for (int i = 0; i < 4; i++)
        {
            float phase = gaitPhase + trotOffset[i];
            float swing = Mathf.Sin(phase) * dynamicAmplitude;
            float lift = Mathf.Max(0, Mathf.Sin(phase)) * dynamicAmplitude;
            
            int shoulderIndex = 4 + i;
            int kneeIndex = 8 + i;
            
            MoveLeg(legs[shoulderIndex], swing * maxSwingAngle);
            MoveLeg(legs[kneeIndex], lift * maxLiftAngle);
        }
        
        float pushPhase = gaitPhase + trotOffset[0]; 
        if (Mathf.Sin(pushPhase) < -0.8f)
        {
            float pushForce = speedFactor * pushForceMultiplier * 2f; 
            body.AddForce(body.transform.right * pushForce, ForceMode.Impulse);
            
            Vector3 torqueStabilization = new Vector3(
                0,
                0,
                -body.angularVelocity.z * 5f 
            );
            body.AddTorque(torqueStabilization);
        }
        
        // Vector3 comOffset = new Vector3(
        //     Mathf.Lerp(0.1f, 0.3f, speedFactor),
        //     0,
        //     0
        // );
        // body.centerOfMass = body.transform.InverseTransformPoint(
        //     body.worldCenterOfMass + body.transform.TransformDirection(comOffset)
        // );
    }

    private void CPG_WALK(float targetSpeed)
    {
        float speedFactor = Mathf.Clamp01(targetSpeed / maxSpeed);
        
        float dynamicFrequency = Mathf.Lerp(minFrequency, maxFrequency, speedFactor);
        float dynamicSwing = Mathf.Lerp(minSwingAngle, maxSwingAngle, speedFactor);
        float dynamicLift = Mathf.Lerp(minLiftAngle, maxLiftAngle, speedFactor);

        gaitPhase += Time.fixedDeltaTime * dynamicFrequency * 2f * Mathf.PI;
        if (gaitPhase > 2f * Mathf.PI) gaitPhase -= 2f * Mathf.PI;

        float[] walkOffsets = { 0f, Mathf.PI, Mathf.PI * 0.5f, Mathf.PI * 1.5f };

        for (int i = 0; i < 4; i++)
        {
            float phase = gaitPhase + walkOffsets[i];
            float swing = Mathf.Sin(phase) * dynamicSwing; 
            float lift = Mathf.Max(0, Mathf.Sin(phase)) * dynamicLift; 

            int shoulderIndex = 4 + i;
            int kneeIndex = 8 + i;     

            MoveLeg(legs[shoulderIndex], swing);
            MoveLeg(legs[kneeIndex], lift);

            if (Mathf.Sin(phase) < -0.8f)
            {
                float pushForce = speedFactor * pushForceMultiplier;
                body.AddForce(body.transform.right * pushForce, ForceMode.Impulse);
            }
        }
    }

    private void Walk() {
        float period = 0.5f; 
        int currentCycle = Mathf.FloorToInt(Time.time / period);

        float minValue1 = -50f;
        float maxValue1 = 30f;
        float oscillationSpeed1 = 2f;
        float amplitude1 = (maxValue1 - minValue1) / 2f;
        float offset1 = (maxValue1 + minValue1) / 2f;
        float normalizedTime1 = (Time.time % period) / period; 
        float sinValue1 = Mathf.Sin(normalizedTime1 * 2 * Mathf.PI * oscillationSpeed1); 
        float currentValue1 = amplitude1 * sinValue1 + offset1;

        float minValue2 = -25f;
        float maxValue2 = 35f;
        float oscillationSpeed2 = 1.8f;
        float amplitude2 = (maxValue2 - minValue2) / 2f;
        float offset2 = (maxValue2 + minValue2) / 2f;
        float normalizedTime2 = (Time.time % period) / period; 
        float sinValue2 = Mathf.Sin(normalizedTime2 * 2 * Mathf.PI * oscillationSpeed2); 
        float currentValue2 = amplitude2 * sinValue2 + offset2;

        float minValue3 = -10f;
        float maxValue3 = 10f;
        float oscillationSpeed3 = 0.9f;
        float amplitude3 = (maxValue3 - minValue3) / 2f;
        float offset3 = (maxValue3 + minValue3) / 2f;
        float normalizedTime3 = (Time.time % period) / period; 
        float sinValue3 = Mathf.Sin(normalizedTime3 * 2 * Mathf.PI * oscillationSpeed3); 
        float currentValue3 = amplitude3 * sinValue3 + offset3;

        if (Time.time % period < 0.1f) 
        {
            if (currentCycle % 2 == 0) {
                MoveLeg(legs[4], 0);
                MoveLeg(legs[7], 0);
                MoveLeg(legs[8], 0);
                MoveLeg(legs[11], 0);
            }

            else {
                MoveLeg(legs[5], 0);
                MoveLeg(legs[9], 0);
                MoveLeg(legs[6], 0);
                MoveLeg(legs[10], 0);
            }
        }
        else 
        {
            if (currentCycle % 2 != 0) {
                MoveLeg(legs[4], currentValue1);
                MoveLeg(legs[7], currentValue1);
                MoveLeg(legs[8], currentValue2);
                MoveLeg(legs[11], currentValue2);
            }
            else {
                MoveLeg(legs[5], currentValue1);
                MoveLeg(legs[6], currentValue1);
                MoveLeg(legs[9], currentValue2);
                MoveLeg(legs[10], currentValue2);
            }
        }
    }

    private void F_TROT(float start_time, float period, float minValue1, float maxValue1, float oscillationSpeed1, float minValue2, float maxValue2, float oscillationSpeed2) {
        int currentCycle = Mathf.FloorToInt((Time.time - start_time) / period);

        float amplitude1 = (maxValue1 - minValue1) / 2f;
        float offset1 = (maxValue1 + minValue1) / 2f;
        float normalizedTime1 = ((Time.time - start_time) % period) / period;
        float sinValue1 = Mathf.Sin(normalizedTime1 * 2 * Mathf.PI * oscillationSpeed1); 
        float currentValue1 = amplitude1 * sinValue1 + offset1;  

        float sinValue11 = Mathf.Sin((normalizedTime1 + 0.25f) * 2 * Mathf.PI * oscillationSpeed1);
        float currentValue11 = amplitude1 * sinValue11 + offset1;

        float amplitude2 = (maxValue2 - minValue2) / 2f;
        float offset2 = (maxValue2 + minValue2) / 2f;
        float normalizedTime2 = ((Time.time - start_time) % period) / period; 
        float sinValue2 = Mathf.Sin(normalizedTime2 * 2 * Mathf.PI * oscillationSpeed2); 
        float currentValue2 = amplitude2 * sinValue2 + offset2;

        float sinValue21 = Mathf.Sin((normalizedTime2 + 0.25f) * 2 * Mathf.PI * oscillationSpeed2);
        float currentValue21 = amplitude2 * sinValue21 + offset2;

        float minValue3 = -10f;
        float maxValue3 = 80f;
        float oscillationSpeed3 = 0.9f;
        float amplitude3 = (maxValue3 - minValue3) / 2f;
        float offset3 = (maxValue3 + minValue3) / 2f;
        float normalizedTime3 = ((Time.time - start_time) % period) / period; 
        float sinValue3 = Mathf.Sin((normalizedTime3 + (period / 2)) * 2 * Mathf.PI * oscillationSpeed3); 
        float currentValue3 = amplitude3 * sinValue3 + offset3;

        MoveLeg(legs[4], currentValue1);
        MoveLeg(legs[7], currentValue1);
        MoveLeg(legs[8], currentValue2);
        MoveLeg(legs[11], currentValue2);

        MoveLeg(legs[5], currentValue11);
        MoveLeg(legs[6], currentValue11);
        MoveLeg(legs[9], currentValue21);
        MoveLeg(legs[10], currentValue21);

        if (Time.time % period < 0.1f) 
        {
            // if (currentCycle % 2 == 0) {
            //     MoveLeg(legs[4], 0);
            //     MoveLeg(legs[7], 0);
            //     MoveLeg(legs[8], 0);
            //     MoveLeg(legs[11], 0);
            // }

            // else {
            //     MoveLeg(legs[5], 0);
            //     MoveLeg(legs[9], 0);
            //     MoveLeg(legs[6], 0);
            //     MoveLeg(legs[10], 0);
            // }
        }
        else 
        {
        }
    }


    private void F_Gallop(float period, float minValue1, float maxValue1, float oscillationSpeed1, float minValue2, float maxValue2, float oscillationSpeed2) {
        int currentCycle = Mathf.FloorToInt(Time.time / period);

        float amplitude1 = (maxValue1 - minValue1) / 2f;
        float offset1 = (maxValue1 + minValue1) / 2f;
        float normalizedTime1 = (Time.time % period) / period;
        float sinValue1 = Mathf.Sin(normalizedTime1 * 2 * Mathf.PI * oscillationSpeed1); 
        float currentValue1 = amplitude1 * sinValue1 + offset1;  

        float sinValue11 = Mathf.Sin((normalizedTime1 + 0.25f) * 2 * Mathf.PI * oscillationSpeed1);
        float currentValue11 = amplitude1 * sinValue11 + offset1;

        float amplitude2 = (maxValue2 - minValue2) / 2f;
        float offset2 = (maxValue2 + minValue2) / 2f;
        float normalizedTime2 = (Time.time % period) / period; 
        float sinValue2 = Mathf.Sin(normalizedTime2 * 2 * Mathf.PI * oscillationSpeed2); 
        float currentValue2 = amplitude2 * sinValue2 + offset2;

        float sinValue21 = Mathf.Sin((normalizedTime2 + 0.25f) * 2 * Mathf.PI * oscillationSpeed2);
        float currentValue21 = amplitude2 * sinValue21 + offset2;

        float minValue3 = -10f;
        float maxValue3 = 80f;
        float oscillationSpeed3 = 0.9f;
        float amplitude3 = (maxValue3 - minValue3) / 2f;
        float offset3 = (maxValue3 + minValue3) / 2f;
        float normalizedTime3 = (Time.time % period) / period; 
        float sinValue3 = Mathf.Sin((normalizedTime3 + (period / 2)) * 2 * Mathf.PI * oscillationSpeed3); 
        float currentValue3 = amplitude3 * sinValue3 + offset3;

        MoveLeg(legs[6], currentValue1);
        MoveLeg(legs[7], currentValue1);
        MoveLeg(legs[10], currentValue2);
        MoveLeg(legs[11], currentValue2);

        MoveLeg(legs[4], currentValue11);
        MoveLeg(legs[5], currentValue11);
        MoveLeg(legs[8], currentValue21);
        MoveLeg(legs[9], currentValue21);

        if (Time.time % period < 0.1f) 
        {
            // if (currentCycle % 2 == 0) {
            //     MoveLeg(legs[4], 0);
            //     MoveLeg(legs[7], 0);
            //     MoveLeg(legs[8], 0);
            //     MoveLeg(legs[11], 0);
            // }

            // else {
            //     MoveLeg(legs[5], 0);
            //     MoveLeg(legs[9], 0);
            //     MoveLeg(legs[6], 0);
            //     MoveLeg(legs[10], 0);
            // }
        }
        else 
        {
        }
    }

    


    private void MoveDistSinForward(float r)
    {  
        float dist = distance(foot, 0);
        float cur_dist = (foot.transform.position.x - startPosition[0].x) * (foot.transform.position.x - startPosition[0].x) + (foot.transform.position.y - startPosition[0].y) * (foot.transform.position.y - startPosition[0].y) + (foot.transform.position.z - startPosition[0].z) * (foot.transform.position.z - startPosition[0].z);
        // Debug.Log(Math.Abs(0.017 - foot.transform.position.y));
        //Debug.Log(Math.Abs(cur_dist - 4 * r * r));
        // flag = true;
        //Debug.Log(dist - r * r);

        //Debug.Log(startPosition);

        if (Time.time > 0.1f) {
            if ((Math.Abs(cur_dist - 4 * r * r) < 1e-4) && (Math.Abs(0.017 - foot.transform.position.y) < 2e-3)) {
                Debug.Log("YES");
            }
            else {
                float m_dist = Math.Abs(dist - r * r);
                int ind = -1;
                ufoot -= 1f;
                MoveLeg(legs[5], ufoot);
                if (Math.Abs(distance(foot, 0) - r * r) <= m_dist) {
                    ind = 1;
                    m_dist = Math.Abs(distance(foot, 0) - r * r);
                    Debug.Log($"ind: {ind}, m_dist: {m_dist}");
                    Debug.Log($"position: {foot.transform.position}");
                }
                ufoot += 2f;
                MoveLeg(legs[5], ufoot);
                if (Math.Abs(distance(foot, 0) - r * r)  <= m_dist) {
                    ind = 2;
                    m_dist = Math.Abs(distance(foot, 0) - r * r);
                    Debug.Log($"ind: {ind}, m_dist: {m_dist}");
                    Debug.Log($"position: {foot.transform.position}");
                }
                ufoot -= 1f;
                MoveLeg(legs[5], ufoot);


                dfoot -= 1f;
                MoveLeg(legs[9], dfoot);
                if (Math.Abs(distance(foot, 0) - r * r)  <= m_dist) {
                    ind = 3;
                    m_dist = Math.Abs(distance(foot, 0) - r * r);
                    Debug.Log($"ind: {ind}, m_dist: {m_dist}");
                    Debug.Log($"position: {foot.transform.position}");
                }
                dfoot += 2f;
                MoveLeg(legs[9], dfoot);
                if (Math.Abs(distance(foot, 0) - r * r)  <= m_dist) {
                    ind = 4;
                    m_dist = Math.Abs(distance(foot, 0) - r * r);
                    Debug.Log($"ind: {ind}, m_dist: {m_dist}");
                    // Debug.Log($"position: {foot.transform.position}");
                }
                dfoot -= 1f;
                MoveLeg(legs[9], dfoot);

                if (ind == 1) {
                    ufoot -= 1f;
                    MoveLeg(legs[5], ufoot);
                    Debug.Log($"Final ind: {ind}");
                    Debug.Log($"UpFoot: {ufoot}, DownFoot: {dfoot}");
                }
                else if (ind == 2) {
                    ufoot += 1f;
                    MoveLeg(legs[5], ufoot);
                    Debug.Log($"Final ind: {ind}");
                    Debug.Log($"UpFoot: {ufoot}, DownFoot: {dfoot}");
                }
                else if (ind == 3) {
                    dfoot -= 1f;
                    MoveLeg(legs[9], dfoot);
                    Debug.Log($"Final ind: {ind}");
                    Debug.Log($"UpFoot: {ufoot}, DownFoot: {dfoot}");
                }
                else if (ind == 4) {
                    dfoot += 1f;
                    MoveLeg(legs[9], dfoot);
                    Debug.Log($"Final ind: {ind}");
                    Debug.Log($"UpFoot: {ufoot}, DownFoot: {dfoot}");
                }
            }
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;

        Debug.Log(foot.transform.position.y);

        MoveImproveSinForward(len);

        if (change_results[0] == 0 && Time.time > 0.1) {
            change_positionLF();
            change_results[0] = 1;
        }

        if (Time.time > 0.1) {
            // MoveImproveSinForward(len, 0.5f);
            // change_positionLF();
            // MoveLF(len, 0.5f);
        }

        UpdateSpeedMeasurement();


        if (Time.time > 5.0f) {
            Debug.Log("Time1");
            position();
            Debug.Log("Time2");
        }

        Debug.Log(body.transform.position.y);

        if (button) {
            TROT(3f);
        }

        if (button) {
            // Move(0.5f, -50f, 30f, 2f, -25f, 35f, 1.8f); // Walk
            F_TROT(0.0f, 0.5f, -60f, 30f, 2f, -35f, 35f, 1.8f); // Trot
            // Walk();
        }


        Vector3 direction = footLF.transform.right.normalized; 
        float rayLength = 0.05f;

        Debug.DrawRay(startPosition[0], direction * rayLength, Color.black);


        float stepDuration = 1f;
        float currentTime = Time.time;
        float dif = 1f;

        if (currentTime - lastUpdateForwardTime >= stepDuration)
        {
            lastUpdateForwardTime = currentTime;

            currentForwardStep = (currentForwardStep + 1) % 4;
        }

        // ApplySinMovement_2(new[] { 4, 5, 6, 7 });
        // ApplySinMovement_3(new[] { 8, 9, 10, 11 });

        if (currentForwardStep == 0)
        {
            UpFoot -= dif;
            MoveLeg(legs[5], UpFoot);
            Debug.Log($"ind: {1}, position: {foot.transform.position}");
            UpFoot += dif;
            MoveLeg(legs[5], UpFoot);
        }

        else if (currentForwardStep == 1)
        {
            UpFoot += dif;
            MoveLeg(legs[5], UpFoot);
            Debug.Log($"ind: {2}, position: {foot.transform.position}");
            UpFoot -= dif;
            MoveLeg(legs[5], UpFoot);
        }

        else if (currentForwardStep == 2)
        {
            DownFoot += dif;
            MoveLeg(legs[9], DownFoot);
            Debug.Log($"ind: {3}, position: {foot.transform.position}");
            DownFoot -= dif;
            MoveLeg(legs[9], DownFoot);
        }

        else if (currentForwardStep == 3)
        {
            DownFoot -= dif;
            MoveLeg(legs[9], DownFoot);
            Debug.Log($"ind: {4}, position: {foot.transform.position}");
            DownFoot += dif;
            MoveLeg(legs[9], DownFoot);
        }

        MoveForward(0.01f);
        MoveLeft(0.1f);
        MoveSinForward(0.1f);

        MoveLeg(legs[0], 15);
        MoveLeg(legs[3], -15);
        

        if (change) {
            Debug.Log("YES");
            ApplySinMovement_1(continuousActions, new[] { 0, 1, 2, 3 });
            ApplySinMovement_2(continuousActions, new[] { 4, 5, 6, 7 });
            ApplySinMovement_3(continuousActions, new[] { 8, 9, 10, 11 });
            //change = false;
        }

        ApplySinMovement_1(new[] { 0, 1, 2, 3 });
        ApplySinMovement_2(new[] { 4, 5, 6, 7 });
        ApplySinMovement_3(new[] { 8, 10 });

        for (int i = 0; i < 12; i++)
        {
            //float angle = 0f;
            if (i < 4) {
                //angle = Mathf.Lerp(legs[i].xDrive.lowerLimit, legs[i].xDrive.upperLimit, (Mathf.Sin(Mathf.PI / 12) + 1) * 0.5f);
                continuousActions[i] = Mathf.Sin(Mathf.PI / 8);
            }
            else if (i < 8) {
                continuousActions[i] = 0.5f;
            }
            else {
                continuousActions[i] = 0.5f;
            }
        }
        MoveLeg(legs[11], 90);
        MoveLeg(legs[8], 90);
        MoveLeg(legs[11], 90);

        continuousActions[8] = 1;
        continuousActions[11] = 1;

        continuousActions[9] = Input.GetAxisRaw("Horizontal");
        continuousActions[11] = Input.GetAxisRaw("Vertical");
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
                AddReward(0.1f);
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