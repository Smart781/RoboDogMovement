using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DrawCenterOfMass : MonoBehaviour
{
    public bool isOn;
    public Color rayColor;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //OnDrawGizmos();
        
    }
    void OnDrawGizmos()
    {
        if (isOn)
        {
            ArticulationBody rb = gameObject.GetComponent<ArticulationBody>();

            //Debug.Log(rb.centerOfMass);

            // ��������� ����������� ��������� ������ �����
            Vector3 globalCenterOfMass = transform.TransformPoint(rb.centerOfMass);
            Vector3 globalPosition = transform.TransformPoint(rb.transform.position);

            // Debug.Log(globalCenterOfMass);

            Debug.Log(globalPosition);

            // ��� �� ������ �����
            Debug.DrawRay(globalCenterOfMass, Vector3.up, Color.green);
        }
        else {
            //Debug.Log("NO");
        }
    }
}
