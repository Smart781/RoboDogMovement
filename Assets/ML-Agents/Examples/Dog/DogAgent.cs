using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgentsExamples;
using System.Collections;
using System;
using System.Linq;
using MBaske;
using Unity.Sentis.Layers;
using UnityEngine.WSA;

public class DogAgent : Agent
{
    //public List<Rigidbody> upperLimbs;
    //public List<Rigidbody> lowerLimbs;

    public List<Bone> m_Bones;
    public Body m_Body;

    public Matrix4x4 Matrix => Matrix4x4.TRS(m_Body.WorldPosition, m_Body.AvgWorldRotationXZ, Vector3.one);
    public Vector3 LocalVelocity => m_Body.AvgLocalVelocityXZ;
    public Vector3 WorldVelocity => m_Body.AvgWorldVelocityXZ;


    public List<string> groundContacts;
    public List<string> groundContactsPrev;

    [Header("Target To Walk Towards")]
    public Transform TargetPrefab; //Target prefab to use in Dynamic envs
    private Transform m_Target; //Target the agent will walk towards during training.

    //public ArticulationBody body;

    public float fallThreshold;
    public float heightThreshold;

    public Transform Cube;
    public Transform arrow;

    //private Vector3 initialPosition;
    //private Vector3 initialRotation;

    private float targetDist;

    public float targetEpsilon = 0.5f;

    private float startDistance = 0f;

    protected enum Activation
    {
        Immediately, ImmediatelyRotate, WhenGrounded
    }
    [SerializeField]
    protected Activation m_Activation;

    [Header("Награды")]
    [Header(" ")]
    [Header("Падение")]
    public float notFallingRewardMultiplier = 0.0001f;
    public float fallingReward = -1f;
    [Header("Наклон относительно земли")]
    public float standingReward = 1f;
    public float notStandingReward = -0.5f;
    [Header("Высота")]
    public float heightRewardMultiplier = 0.0001f;
    public float negativeHeightRewardMultiplier = -0.1f;
    [Header("Дистанция")]
    public float distanceRewardMultiplier = 0.05f;
    public float negativeDistanceRewardMultiplier = -0.01f;
    [Header("Поворот к цели")]
    public float orientationRewardMultiplier = 0.0005f;
    public float negativeOrientationRewardMultiplier = -0.001f;
    [Header("Достижение цели")]
    public float targetReward = 10f;
    [Header("Соприкосновение с землей")]
    [Header(" ")]
    [Header("Соприкосновение с землей 3 ноги (разные)")]
    public float stepRewardThreeDiffernt = 0.05f;
    [Header("Соприкосновение с землей 3 ноги (одинаковые)")]
    public float stepRewardThreeSame = -0.05f;
    [Header("Соприкосновение с землей 4 ноги")]
    public float stepRewardFour = 0.0001f;
    [Header("Соприкосновение с землей < 3 ног")]
    public float stepReward = -0.1f;

    /// <summary>
    /// Сброс положения
    /// </summary>
    public void ResetAgent()
    {
        foreach (var bone in m_Bones)
        {
            bone.ManagedReset();
        }
        targetDist = Vector3.Distance(m_Body.ManagedReset(m_Activation == Activation.ImmediatelyRotate), m_Target.transform.position);
    }

    /// <summary>
    /// Метод, вызываемый при старте
    /// </summary>
    public override void Initialize()
    {
        m_Target = TargetPrefab;
        targetDist = Vector3.Distance(m_Body.transform.position, m_Target.transform.position);

        foreach (var bone in m_Bones)
        {
            bone.Initialize();
        }
        m_Body.Initialize();
    }

    /// <summary>
    /// Метод, вызываемый при начале новой эпохи обучения
    /// </summary>
    public override void OnEpisodeBegin()
    {
        ResetAgent();
        StopAllCoroutines();
        StartCoroutine(EndTimer());

        var diff = m_Target.transform.position - Cube.position;
        startDistance = diff.magnitude;
    }

    /// <summary>
    /// Случайный набор действия для обучения (+ подсчет наград)
    /// </summary>
    /// <param name="vectorAction"></param>
    public override void OnActionReceived(ActionBuffers  vectorAction)
    {
        var dt = Time.fixedDeltaTime;
        var actions = vectorAction.ContinuousActions;

        for (int i = 0; i < m_Bones.Count; i++)
        {
            m_Bones[i].ManagedUpdate(actions[i], dt);
        }
        m_Body.ManagedUpdate();
    }

    /// <summary>
    /// Сбор информации с "датчиков"
    /// </summary>
    /// <param name="sensor"></param>
    public override void CollectObservations(VectorSensor sensor)
    {

        sensor.AddObservation(Cube.transform.forward.y);
        sensor.AddObservation(Cube.transform.position);
        sensor.AddObservation(Cube.transform.rotation);

        sensor.AddObservation(m_Body.Gyro);
        sensor.AddObservation(m_Body.LocalVelocity.normalized);
        sensor.AddObservation(m_Body.LocalAngularVelocity.normalized);

        for (int i = 0; i < m_Bones.Count; i++)
        {
            sensor.AddObservation(m_Bones[i].NormAngle);
            sensor.AddObservation(m_Bones[i].NormLocalVelocity);
        }

        for (int i = 0; i < 8; i++)
        {
            sensor.AddObservation(m_Bones[i].GetComponent<GroundContact>().touchingGround);
        }

        sensor.AddObservation(m_Body.GetComponent<GroundContact>().touchingGround);
    }

    /// <summary>
    /// Награда, если робот не упал
    /// </summary>
    public void RewardForNotFalling()
    {
        if (!RobotHasFallen())
        {
            AddReward(notFallingRewardMultiplier);
        }
        else
        {
            AddReward(fallingReward);
            Debug.Log("End");
            EndEpisode();
        }
    }

    /// <summary>
    /// Награда за высоту
    /// </summary>
    public void GetHeightRewad()
    {
        if (Cube.transform.position.y < heightThreshold)
        {
            AddReward(Math.Abs(Cube.transform.position.y - heightThreshold) * negativeHeightRewardMultiplier);
        }
        else
        {
            AddReward(heightRewardMultiplier);
        }
    }

    /// <summary>
    /// Подсчет расстояния до цели
    /// </summary>
    public void FixedUpdate()
    {
        /*var d = Vector3.Distance(m_Body.transform.position, m_Target.transform.position);
        if (targetDist > d)
        {
            AddReward((Mathf.Abs(d - targetDist) * distanceRewardMultiplier));
            //Debug.Log($"Current {d} Target {targetDist} Reward {(Mathf.Abs(d - targetDist) * distanceRewardMultiplier)}");
            targetDist = d;
        }
        else
        {
            AddReward((Mathf.Abs(d - targetDist) * negativeDistanceRewardMultiplier));
            //Debug.Log($"Current {d} Target {targetDist} Reward {(Mathf.Abs(d - targetDist) * negativeDistanceRewardMultiplier)}");
        }*/
        GetHeightRewad();
        RewardForNotFalling();
        RewardForSteps();

        var diff = m_Target.transform.position - Cube.position;

        float m = diff.magnitude;
        if (startDistance > targetEpsilon)
        {
            AddReward(Mathf.Max(0, startDistance - m));
            Debug.Log($"Moving to target reward: {Mathf.Max(0, startDistance - m)})");
        }
        else
        {
            return;
        }
        UpdateOrientation(Cube, m_Target);
    }
    /// <summary>
    /// Награда за поворот к цели
    /// </summary>
    /// <param name="rootBP"></param>
    /// <param name="target"></param>
    public void UpdateOrientation(Transform rootBP, Transform target)
    {
        var dirVector = target.position - rootBP.position;
        dirVector.y = 0;
        var lookRot =
            dirVector == Vector3.zero
                ? Quaternion.identity
                : Quaternion.LookRotation(dirVector);

        arrow.SetPositionAndRotation(rootBP.position, lookRot);
        if(Vector3.Dot(arrow.forward, rootBP.TransformDirection(Vector3.up)) > 0.9f)
        {
            AddReward(orientationRewardMultiplier);
        }
        else
        {
            AddReward(Math.Abs(Vector3.Dot(arrow.forward, rootBP.TransformDirection(Vector3.up)) - 0.9f) * negativeOrientationRewardMultiplier);
        }
    }

    /// <summary>
    /// Награда за стойку на 3/4 лапах
    /// </summary>
    public void RewardForSteps()
    {
        /*int k = 0;

        for (int i = 4; i < 8; i++)
        {
            if (m_Bones[i].GetComponent<GroundContact>().touchingGround == true)
            {
                k++;
            }
            
        }
        if (k >= 3)
        {
            if (k == 3)
            {
                //Обновляем массив ног, которые касаются земли
                groundContacts.Clear();
                for (int i = 4; i < 8; i++)
                {
                    if (m_Bones[i].GetComponent<GroundContact>().touchingGround == true)
                    {
                        groundContacts.Add(m_Bones[i].name);
                    }
                }
                //Сравниваем массив ног, которые касаются земли
                if ((groundContacts.Count > 0) && (groundContactsPrev.Count > 0))
                {
                    bool compare = groundContactsPrev.SequenceEqual(groundContacts);

                    if (!compare)
                    {
                        AddReward(stepRewardThreeDiffernt);
                        Debug.Log("Different");
                    }
                    else
                    {
                        Debug.Log("Same");
                        AddReward(stepRewardThreeSame);
                    }
                }
                //Обновляем массив ног, которые касаются земли
                groundContactsPrev.Clear();
                for (int i = 4; i < 8; i++)
                {
                    if (m_Bones[i].GetComponent<GroundContact>().touchingGround == true)
                    {
                        groundContactsPrev.Add(m_Bones[i].name);
                    }
                }

            }
            else if (k == 4)
            {
                AddReward(stepRewardFour);
            }
            GetHeightRewad();
            SetRewardForRot();
        }
        else
        {
            AddReward(stepReward);
        }*/
    }

    /// <summary>
    /// Стоит ли собака прямо
    /// </summary>
    public void SetRewardForRot()
    {
        if (Cube.transform.forward.y > 0.95f)
        {
            AddReward(standingReward);
        }
        else
        {
            AddReward(Mathf.Abs(Cube.transform.forward.y - 0.95f) * notStandingReward);
        }
    }

    private bool RobotHasFallen()
    {
        if (Cube.transform.forward.y < fallThreshold)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Достижение цели
    /// </summary>
    public void TouchedTarget()
    {
        AddReward(targetReward);
        Debug.Log("TARGET!");
        EndEpisode();
    }

    /// <summary>
    /// Сброс положения собаки по времени
    /// </summary>
    /// <returns></returns>
    IEnumerator EndTimer()
    {
        yield return new WaitForSeconds(20f); //lol
        EndEpisode();
    }
}
