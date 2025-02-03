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
    //private int pred_ind = -1;
    //private float pred_speed = 1f;
    //private bool compl = false;
    private Vector3 endPosition;
    private Vector3 startPosition;
    private float UpFoot = 0.5f;
    private float DownFoot = 0.5f;
    private bool flag = false;
    private float FootFlag1 = -1;
    private float FootFlag2 = -1;
    private float len = 0.1f;

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

        //FootPos = transform.TransformPoint(foot.transform.position) + foot.transform.right * 0.5f;

        // Vector3 startPosition = foot.transform.position;

        // Vector3 direction = foot.transform.right.normalized; // Нормализуем, чтобы длина была 1

        // float rayLength = 0.05f;

        // Debug.DrawRay(startPosition, direction * rayLength, Color.black);

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

        float val = -1f;
        int ind = -1;

        for (int i = 0; i < 3; i++)
        {
            float current_action = ((actions[i] * 1f) + 1) / 2;
            if (current_action > val) {
                val = current_action;
                ind = i;
            }
        }

        if (compl) {
            Debug.Log(pred_ind);
            bool res = false;
            if (pred_ind == 0) {
                res = MoveForward(0.01f);
                // AddReward(0.01f);
            }
            else if (pred_ind == 1) {
                res = MoveRight(0.1f);
            }
            else {
                res = MoveLeft(0.1f);
            }
            if (res) {
                compl = false;
            }
        }
        else {
            if (val >= 0.5) {
                //pred_speed = ((actions[3 + ind] * 1f) + 1) / 2;
                pred_ind = ind;
                compl = true; 
            }
        }

        Debug.Log(foot.transform.position);
        Debug.Log(foot.transform.right);
        Debug.Log(foot.transform.position);
        Debug.Log(FootPos);
        Debug.DrawRay(foot.transform.position, foot.transform.right, Color.black);

        Debug.DrawRay(foot.transform.position, foot.transform.right.normalized * len, Color.black);


        if (!flag) {
            startPosition = foot.transform.position;
            endPosition = foot.transform.position + foot.transform.right.normalized * len;
        }

        Debug.DrawRay(startPosition, Vector3.up * 1f, Color.green);


        Vector3 verticalStartPosition = endPosition;

        Vector3 verticalDirection = Vector3.up;

        float verticalRayLength = 1f;

        Debug.DrawRay(verticalStartPosition, verticalDirection * verticalRayLength, Color.red);

        Vector3 verticalEndPosition = verticalStartPosition + verticalDirection * verticalRayLength;


        // for (int i = 0; i < 12; i++)
        // {
        //     float angle = Mathf.Lerp(legs[i].xDrive.lowerLimit, legs[i].xDrive.upperLimit, (actions[i] + 1) * 0.5f);
        //     MoveLeg(legs[i], angle);
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

        if (distanceReward < 0)
        {
            AddReward(-0.01f);
        }

        if (body.velocity.magnitude < 0.1f)
        {
            AddReward(-0.01f);
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
            return true;
        }
        else {
            return false;
        }
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
        MoveLeg(legs[5], -cosValue * 90);
        MoveLeg(legs[6], -cosValue * 90);
        MoveLeg(legs[7], -sinValue * 90);

        MoveLeg(legs[8], sinValue * 45);
        MoveLeg(legs[9], cosValue * 30);
        MoveLeg(legs[10], cosValue * 30);
        MoveLeg(legs[11], sinValue * 30);

        return currentForwardStep == 7;
    }

    private void MoveImproveSinForwardRight(float r)
    {
        float dist = (foot.transform.position.x - endPosition.x) * (foot.transform.position.x - endPosition.x) + (foot.transform.position.y - endPosition.y) * (foot.transform.position.y - endPosition.y) + (foot.transform.position.z - endPosition.z) * (foot.transform.position.z - endPosition.z);
        float cur_dist = (foot.transform.position.x - startPosition.x) * (foot.transform.position.x - startPosition.x) + (foot.transform.position.y - startPosition.y) * (foot.transform.position.y - startPosition.y) + (foot.transform.position.z - startPosition.z) * (foot.transform.position.z - startPosition.z);
        Debug.Log(Math.Abs(cur_dist - 4 * r * r));
        //Debug.Log(Math.Abs(dist - r * r));
        
        if (Math.Abs(dist - r * r) < 1e-8) {
            if (Math.Abs(cur_dist - 4 * r * r) < 1e-6) {
                Debug.Log("YES");
            }
            else {
                FootFlag2 = 1;
                UpFoot -= 0.1f;
                MoveLeg(legs[4], UpFoot);
                MoveLeg(legs[7], UpFoot);
            }
        }
        else if (dist < r * r) {
            FootFlag2 = 2;
            DownFoot += 0.1f;
            MoveLeg(legs[8], DownFoot);
            MoveLeg(legs[11], DownFoot);
        }
        else {
            if (FootFlag2 == 2) {
                FootFlag2 = 1;
                UpFoot -= 0.1f;
                MoveLeg(legs[4], UpFoot);
                MoveLeg(legs[7], UpFoot);
            }
            else {
                FootFlag2 = 3;
                DownFoot -= 0.1f;
                MoveLeg(legs[8], DownFoot);
                MoveLeg(legs[11], DownFoot);
            }
        }
    }

    private void MoveImproveSinForwardLeft(float r)
    {
        float dist = (foot.transform.position.x - endPosition.x) * (foot.transform.position.x - endPosition.x) + (foot.transform.position.y - endPosition.y) * (foot.transform.position.y - endPosition.y) + (foot.transform.position.z - endPosition.z) * (foot.transform.position.z - endPosition.z);
        float cur_dist = (foot.transform.position.x - startPosition.x) * (foot.transform.position.x - startPosition.x) + (foot.transform.position.y - startPosition.y) * (foot.transform.position.y - startPosition.y) + (foot.transform.position.z - startPosition.z) * (foot.transform.position.z - startPosition.z);
        Debug.Log(Math.Abs(cur_dist - 4 * r * r));
        //Debug.Log(Math.Abs(dist - r * r));
        
        if (Math.Abs(dist - r * r) < 1e-8) {
            if (Math.Abs(cur_dist - 4 * r * r) < 1e-6) {
                Debug.Log("YES");
            }
            else {
                FootFlag1 = 1;
                UpFoot -= 0.1f;
                MoveLeg(legs[5], UpFoot);
                MoveLeg(legs[6], UpFoot);
            }
        }
        else if (dist < r * r) {
            FootFlag1 = 2;
            DownFoot += 0.1f;
            MoveLeg(legs[9], DownFoot);
            MoveLeg(legs[10], DownFoot);
        }
        else {
            if (FootFlag1 == 2) {
                FootFlag1 = 1;
                UpFoot -= 0.1f;
                MoveLeg(legs[5], UpFoot);
                MoveLeg(legs[6], UpFoot);
            }
            else {
                FootFlag1 = 3;
                DownFoot -= 0.1f;
                MoveLeg(legs[9], DownFoot);
                MoveLeg(legs[10], DownFoot);
            }
        }
    }

    private bool MoveImproveSinForward(float speed)
    {
        float stepDuration = speed;
        float currentTime = Time.time;

        if (currentTime - lastUpdateLeftTime >= stepDuration)
        {
            lastUpdateLeftTime = currentTime;

            currentLeftStep = (currentLeftStep + 1) % 2;
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
                MoveImproveSinForwardLeft(len);
            }
            else if (currentLeftStep == 1)
            {
                MoveImproveSinForwardRight(len);
            }
        }
            
        if (currentLeftStep == 1) {
            return true;
        }
        else {
            return false;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;

        MoveImproveSinForward(len);

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
        // MoveLeg(legs[11], 90);
        //MoveLeg(legs[8], 90);
        // MoveLeg(legs[11], 90);

        // continuousActions[8] = 1;
        // continuousActions[11] = 1;

        // continuousActions[9] = Input.GetAxisRaw("Horizontal");
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
                AddReward(0.1f);
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
        if (Math.Abs(-0.03 - body.transform.position.z) >= 0.02) {
            AddReward(-100f);
            EndEpisode();
        }
        if (Math.Abs(-0.03 - body.transform.position.z) <= 0.03) {
            AddReward(1f);
        }
        Debug.DrawRay(body.transform.position, body.transform.right, Color.white);
    }

    void MoveLeg(ArticulationBody leg, float targetAngle)
    {
        leg.GetComponent<Leg>().MoveLeg(targetAngle, servoSpeed);
    }
}