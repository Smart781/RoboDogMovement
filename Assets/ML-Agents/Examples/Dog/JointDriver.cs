using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JointDriver : MonoBehaviour
{
    [Header("Joint Drive Settings")]
    [Space(10)]
    public float maxJointSpring;
    public float jointDampen;
    public float maxJointForceLimit;
    public bool isRight;

    private ConfigurableJoint joint;

    public void Start()
    {
        joint = GetComponent<ConfigurableJoint>();
    }
    public void Rotate(float x, float y, float z)
    {
        if (isRight)
        {
            SetRotR(x, y, z);
        }
        else
        {
            SetRotL(x, y, z);
        }
    }

    private void SetRotR(float x, float y, float z)
    {
        x = (x + 1f) * 0.5f;
        y = (y + 1f) * 0.5f;
        z = (z + 1f) * 0.5f;

        var xRot = Mathf.Lerp(joint.lowAngularXLimit.limit, joint.highAngularXLimit.limit, x);
        var yRot = Mathf.Lerp(-joint.angularYLimit.limit, joint.angularYLimit.limit, y);
        var zRot = Mathf.Lerp(-joint.angularZLimit.limit, joint.angularZLimit.limit, z);

        joint.targetRotation = Quaternion.Euler(xRot, yRot, zRot);
    }
    private void SetRotL(float x, float y, float z)
    {
        x = (x + 1f) * 0.5f;
        y = (y + 1f) * 0.5f;
        z = (z + 1f) * 0.5f;

        var xRot = Mathf.Lerp(joint.lowAngularXLimit.limit, joint.highAngularXLimit.limit, x);
        var yRot = Mathf.Lerp(joint.angularYLimit.limit, -joint.angularYLimit.limit, y);
        var zRot = Mathf.Lerp(joint.angularZLimit.limit, -joint.angularZLimit.limit, z);

        joint.targetRotation = Quaternion.Euler(xRot, yRot, zRot);
    }

    void FixedUpdate()
    {
        SetJointStrength(0);
    }

    public void SetJointStrength(float strength)
    {
        var rawVal = (strength + 1f) * 0.5f * maxJointForceLimit;
        var jd = new JointDrive
        {
            positionSpring = maxJointSpring,
            positionDamper = jointDampen,
            maximumForce = rawVal
        };
        joint.slerpDrive = jd;
    }
}
