using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        
    }
    void OnDrawGizmos()
    {
        if (isOn)
        {
            ArticulationBody rb = gameObject.GetComponent<ArticulationBody>();

            // Получение глобального положения центра массы
            Vector3 globalCenterOfMass = transform.TransformPoint(rb.centerOfMass);

            // Луч из центра массы
            Debug.DrawRay(globalCenterOfMass, Vector3.up, rayColor);
        }
    }
}
