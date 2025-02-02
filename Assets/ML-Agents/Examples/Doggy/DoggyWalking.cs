using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoggyWalking : MonoBehaviour
{

    public ArticulationBody[] legs;

    public float[] angel_LX_front;
    public float[] angel_RX_front;

    public float[] angel_LX_left;
    public float[] angel_RX_left;

    public float[] angel_LX_right;
    public float[] angel_RX_right;

    public float[] angles_LF;
    public float[] angles_RF;
    public float[] angles_LB;
    public float[] angles_RB;

    public float time_delay;

    public float servoSpeed;

    bool isWalk = false;

    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            if (!isWalk)
            {
                StartCoroutine(Walking(legs[0], legs[2], angel_LX_front));
                StartCoroutine(Walking(legs[1], legs[3], angel_RX_front));

                StartCoroutine(Walking(legs[4], legs[8], angles_LF));
                StartCoroutine(Walking(legs[5], legs[9], angles_RF));
                StartCoroutine(Walking(legs[6], legs[10], angles_LB));
                StartCoroutine(Walking(legs[7], legs[11], angles_RB));
            }
        }
        else if (Input.GetKey(KeyCode.A))
        {
            if (!isWalk)
            {
                StartCoroutine(Walking(legs[0], legs[2], angel_LX_left));
                StartCoroutine(Walking(legs[1], legs[3], angel_RX_left));

                StartCoroutine(Walking(legs[4], legs[8], angles_LF));
                StartCoroutine(Walking(legs[5], legs[9], angles_RF));
                StartCoroutine(Walking(legs[6], legs[10], angles_LB));
                StartCoroutine(Walking(legs[7], legs[11], angles_RB));
            }
        }
        else if(Input.GetKey(KeyCode.D))
        {
            if (!isWalk)
            {
                StartCoroutine(Walking(legs[0], legs[2], angel_LX_right));
                StartCoroutine(Walking(legs[1], legs[3], angel_RX_right));

                StartCoroutine(Walking(legs[4], legs[8], angles_LF));
                StartCoroutine(Walking(legs[5], legs[9], angles_RF));
                StartCoroutine(Walking(legs[6], legs[10], angles_LB));
                StartCoroutine(Walking(legs[7], legs[11], angles_RB));
            }
        }
    }

    void MoveLeg(ArticulationBody leg, float targetAngle)
    {
        leg.GetComponent<Leg>().MoveLeg(targetAngle, servoSpeed);
    }

    IEnumerator Walking(ArticulationBody upper_limb, ArticulationBody lower_limb, float[] angles)
    {
        isWalk = true;

        MoveLeg(upper_limb, angles[0]);
        MoveLeg(lower_limb, angles[1]);
        yield return new WaitForSeconds(time_delay);

        MoveLeg(upper_limb, angles[2]);
        MoveLeg(lower_limb, angles[3]);
        yield return new WaitForSeconds(time_delay);

        MoveLeg(upper_limb, angles[4]);
        MoveLeg(lower_limb, angles[5]);
        yield return new WaitForSeconds(time_delay);

        MoveLeg(upper_limb, angles[6]);
        MoveLeg(lower_limb, angles[7]);
        yield return new WaitForSeconds(time_delay);

        //MoveLeg(upper_limb, angles[8]);
        //MoveLeg(lower_limb, angles[9]);
        //yield return new WaitForSeconds(time_delay);

        isWalk = false;

        MoveLeg(upper_limb, 0);
        MoveLeg(lower_limb, 0);
        yield return new WaitForSeconds(time_delay);
    }
}
