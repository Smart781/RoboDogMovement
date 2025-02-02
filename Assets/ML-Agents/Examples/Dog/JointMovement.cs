using UnityEngine;

public class JointMovement : MonoBehaviour
{
    [Header("UR-UL-DR-DL")]
    public ConfigurableJoint[] joints;
    public float jointStrength = 0f;

    [Header("Set Rot UP")]
    public float targetXAngleUP = 45f; // Угол отклонения по оси X (предполагается, что углы задаются в градусах)
    public float targetYAngleUP = 0f;  // Угол отклонения по оси Y
    public float targetZAngleUP = 0f;  // Угол отклонения по оси Z

    [Header("Set Rot UP")]
    public float targetXAngleDown = 45f; // Угол отклонения по оси X (предполагается, что углы задаются в градусах)
    public float targetYAngleDown = 0f;  // Угол отклонения по оси Y
    public float targetZAngleDown = 0f;  // Угол отклонения по оси Z

    [Header("Current Joint Settings")]
    [Space(10)]
    public Vector3 currentEularJointRotation;

    [Header("Joint Drive Settings")]
    [Space(10)]
    public float maxJointSpring;
    public float jointDampen;
    public float maxJointForceLimit;

    [HideInInspector] public float currentStrength;
    public float currentXNormalizedRot;
    public float currentYNormalizedRot;
    public float currentZNormalizedRot;

    public void FixedUpdate()
    {
        Rotate();

        if (Input.GetKeyDown(KeyCode.Space))
        {
        }
    }
    public void Rotate()
    {
        // Отклонение сустава на определенный угол
        SetJointTargetRotation(targetXAngleUP, targetYAngleUP, targetZAngleUP, targetXAngleDown, targetYAngleDown, targetZAngleDown);

        // Фиксация сустава
        //float jointStrength = 0f; // Установите силу сустава на ноль, чтобы зафиксировать его
        SetJointStrength(jointStrength);
    }

    void SetRotR(float x, float y, float z, int i)
    {
        var joint = joints[i];
        x = (x + 1f) * 0.5f;
        y = (y + 1f) * 0.5f;
        z = (z + 1f) * 0.5f;

        var xRot = Mathf.Lerp(joint.lowAngularXLimit.limit, joint.highAngularXLimit.limit, x);
        var yRot = Mathf.Lerp(-joint.angularYLimit.limit, joint.angularYLimit.limit, y);
        var zRot = Mathf.Lerp(-joint.angularZLimit.limit, joint.angularZLimit.limit, z);

        joint.targetRotation = Quaternion.Euler(xRot, yRot, zRot);
    }
    void SetRotL(float x, float y, float z, int i)
    {
        var joint = joints[i];
        x = (x + 1f) * 0.5f;
        y = (y + 1f) * 0.5f;
        z = (z + 1f) * 0.5f;

        var xRot = Mathf.Lerp(joint.lowAngularXLimit.limit, joint.highAngularXLimit.limit, x);
        var yRot = Mathf.Lerp(joint.angularYLimit.limit, -joint.angularYLimit.limit, y);
        var zRot = Mathf.Lerp(joint.angularZLimit.limit, -joint.angularZLimit.limit, z);

        joint.targetRotation = Quaternion.Euler(xRot, yRot, zRot);
    }

    public void SetJointTargetRotation(float xU, float yU, float zU, float xD, float yD, float zD)
    {
        for (int i = 0; i < 2; i++)
        {
            SetRotR(xU, yU, zU, i);
        }

        for (int i = 2; i < 4; i++)
        {
            SetRotL(xU, yU, zU, i);
        }

        for (int i = 4; i < 6; i++)
        {
            SetRotR(xD, yD, zD, i);
        }

        for (int i = 6; i < 8; i++)
        {
            SetRotL(xD, yD, zD, i);
        }
    }

    public void SetJointStrength(float strength)
    {
        foreach (var joint in joints)
        {
            var rawVal = (strength + 1f) * 0.5f * maxJointForceLimit;
            var jd = new JointDrive
            {
                positionSpring = maxJointSpring,
                positionDamper = jointDampen,
                maximumForce = rawVal
            };
            joint.slerpDrive = jd;
            currentStrength = jd.maximumForce;
        }
    }

}
