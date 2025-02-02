using MBaske;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoggyWalkingInstructions : MonoBehaviour
{
    [Header("Сервоприводы")]
    public ArticulationBody[] legs;

    [Header("Тело")]
    public ArticulationBody body;
    public float strenghtMove;

    [Header("Куб (цель)")]
    public GameObject cube;

    [Header("Скорость работы сервоприводов")]
    public float servoSpeed;

    [Header("Углы")]
    public float[] angles;

    [Header("Задержка между действиями")]
    public float timeDelay;
    public float timeDelayStep;


    bool isWalk = false;

    public bool isFirst;
    public int index;


    [ContextMenu("Start Walk")]
    public void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            MoveLeg(legs[i], angles[4]);
        }
        //StartCoroutine(Walk());
    }

    private void FixedUpdate()
    {
        for (int i = 0; i < 4; i++)
        {
            MoveLeg(legs[i], angles[4]);
        }
        
        Debug.DrawRay(body.transform.position, body.transform.right, Color.white);

        if (Input.GetKey(KeyCode.W))
        {
            body.AddForce((body.transform.right).normalized * strenghtMove);

            if (!isWalk)
            {
                StartCoroutine(WalkForward());
            }
        }
        else if (Input.GetKey(KeyCode.S))
        {
            body.AddForce((body.transform.right).normalized * strenghtMove * -1);

            if (!isWalk)
            {
                //StartCoroutine(WalkBack());
            }
        }
    }

    void MoveLeg(ArticulationBody leg, float targetAngle)
    {
        leg.GetComponent<Leg>().MoveLeg(targetAngle, servoSpeed);
    }

    IEnumerator WalkForward()
    {
        isWalk = true;
        if (isFirst)
        {
            MoveLeg(legs[8], angles[2]);
            MoveLeg(legs[4], angles[0]);

            MoveLeg(legs[11], angles[2]);
            MoveLeg(legs[7], angles[0]);
        }
        else
        {
            MoveLeg(legs[9], angles[3]);
            MoveLeg(legs[5], angles[1]);

            MoveLeg(legs[10], angles[3]);
            MoveLeg(legs[6], angles[1]);
        }
        
        yield return new WaitForSeconds(timeDelay);

        if (isFirst)
        {
            MoveLeg(legs[8], angles[3]);
            MoveLeg(legs[4], angles[1]);

            MoveLeg(legs[11], angles[3]);
            MoveLeg(legs[7], angles[1]);
        }
        else
        {
            MoveLeg(legs[9], angles[2]);
            MoveLeg(legs[5], angles[0]);

            MoveLeg(legs[10], angles[2]);
            MoveLeg(legs[6], angles[0]);
        }
        yield return new WaitForSeconds(timeDelay);

        isWalk = false;

        //
        if (isFirst)
        {
            MoveLeg(legs[4], 0);
            MoveLeg(legs[7], 0);
            MoveLeg(legs[8], 0);
            MoveLeg(legs[11], 0);
        }
        else
        {
            MoveLeg(legs[5], 0);
            MoveLeg(legs[6], 0);
            MoveLeg(legs[9], 0);
            MoveLeg(legs[10], 0);
        }
        yield return new WaitForSeconds(timeDelay);

        //

    }

    IEnumerator WalkBack()
    {
        isWalk = true;

        MoveLeg(legs[4], angles[0]);
        MoveLeg(legs[7], angles[0]);
        MoveLeg(legs[5], angles[1]);
        MoveLeg(legs[6], angles[1]);

        MoveLeg(legs[8], angles[3]);
        MoveLeg(legs[11], angles[2]);
        MoveLeg(legs[9], angles[2]);
        MoveLeg(legs[10], angles[2]);
        yield return new WaitForSeconds(timeDelay);

        MoveLeg(legs[4], angles[1]);
        MoveLeg(legs[7], angles[1]);
        MoveLeg(legs[5], angles[0]);
        MoveLeg(legs[6], angles[0]);

        MoveLeg(legs[8], angles[2]);
        MoveLeg(legs[11], angles[2]);
        MoveLeg(legs[9], angles[3]);
        MoveLeg(legs[10], angles[3]);
        yield return new WaitForSeconds(timeDelay);

        isWalk = false;

        //

        MoveLeg(legs[4], 0);
        MoveLeg(legs[7], 0);
        MoveLeg(legs[5], 0);
        MoveLeg(legs[6], 0);

        MoveLeg(legs[8], 0);
        MoveLeg(legs[11], 0);
        MoveLeg(legs[9], 0);
        MoveLeg(legs[10], 0);
        yield return new WaitForSeconds(timeDelay);

        //
    }
}
