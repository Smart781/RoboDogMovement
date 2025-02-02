using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DogController : MonoBehaviour
{
    public ArticulationBody[] upperXLegs;
    public ArticulationBody[] upperZLegs;
    public ArticulationBody[] lowerLegs;

    [Header("Движение ног в Play Mode")]
    public float upperXLegsTargetAngle = 0f;
    public float upperZLegsTargetAngle = 0f;
    public float lowerLegsTargetAngle = 0f;

    [Header("Изменение ограничений поворта ног\n" +
        "Для получения текущих данных: GetLimits\n" +
        "Для установки новых: ChangeLimits")]
    public float upperXLegsLowerLimit = -60f;
    public float upperXLegsUpperLimit = 30f;

    public float upperZLegsLowerLimit = -90f;
    public float upperZLegsUpperLimit = 50f;

    public float lowerLegsLegsLowerLimit = -90f;
    public float lowerLegsLegsUpperLimit = 50f;

    [Header("Servo speed")]
    public float servoSpeed;

    public ArticulationBody body;
    public Vector3 defPos;
    public Quaternion defRot;

    void Start()
    {
        defPos = body.transform.position;
        defRot = body.transform.rotation;
    }

    //[ContextMenu("MoveAll")]
    public void FixedUpdate()
    {
        for (int i = 0; i < lowerLegs.Length; i++)
        {
            MoveLeg(lowerLegs[i], lowerLegsTargetAngle);
        }
        for (int i = 0; i < upperXLegs.Length; i++)
        {
            MoveLeg(upperXLegs[i], upperXLegsTargetAngle);
        }
        for (int i = 0; i < upperZLegs.Length; i++)
        {
            MoveLeg(upperZLegs[i], upperZLegsTargetAngle);
        }
    }

    void MoveLeg(ArticulationBody leg, float targetAngle)
    {
        leg.GetComponent<Leg>().MoveLeg(targetAngle, servoSpeed);
    }

    [ContextMenu("ChangeLimits")]
    public void StartChangeLimits()
    {
        for (int i = 0; i < upperXLegs.Length; i++)
        {
            ChangeLimits(upperXLegs[i], upperXLegsLowerLimit, upperXLegsUpperLimit);

        }
        for (int i = 0; i < upperZLegs.Length; i++)
        {
            ChangeLimits(upperZLegs[i], upperZLegsLowerLimit, upperZLegsUpperLimit);
        }
        for (int i = 0; i < lowerLegs.Length; i++)
        {
            ChangeLimits(lowerLegs[i], lowerLegsLegsLowerLimit, lowerLegsLegsUpperLimit);
        }
    }
    [ContextMenu("GetLimits")]
    public void GetLimits()
    {
        ArticulationDrive upperXLegsDrive = upperXLegs[0].xDrive;
        upperXLegsLowerLimit = upperXLegsDrive.lowerLimit;
        upperXLegsUpperLimit = upperXLegsDrive.upperLimit;

        ArticulationDrive upperZLegsDrive = upperZLegs[0].xDrive;
        upperZLegsLowerLimit = upperZLegsDrive.lowerLimit;
        upperZLegsUpperLimit = upperZLegsDrive.upperLimit;

        ArticulationDrive lowerLegsDrive = lowerLegs[0].xDrive;
        lowerLegsLegsLowerLimit = lowerLegsDrive.lowerLimit;
        lowerLegsLegsUpperLimit = lowerLegsDrive.upperLimit;
}
    void ChangeLimits(ArticulationBody leg, float lowerLimit, float upperLimit)
    {
        // Создайте новый объект ArticulationDrive
        ArticulationDrive drive = leg.xDrive;

        // Установите новые значения lowerLimit и upperLimit
        drive.lowerLimit = lowerLimit;
        drive.upperLimit = upperLimit;

        // Примените новый привод к ArticulationBody
        leg.xDrive = drive;
    }

    [ContextMenu("Reset")]
    public void ResetDog()
    {
        body.TeleportRoot(defPos, defRot);
        body.velocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;

        for (int i = 0; i < lowerLegs.Length; i++)
        {
            MoveLeg(lowerLegs[i], 0);
            MoveLeg(upperZLegs[i], 0);
            MoveLeg(upperXLegs[i], 0);
        }
    }
}

