using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oscillator : MonoBehaviour
{
    [Serializable]
    private struct OscLeg
    {
        public int Phase;
        public Leg Leg;
    }

    [Serializable]
    private struct OscGroup
    {
        public float Scale;
        public OscLeg[] Legs;
    }

    [Serializable]
    private struct OscMode
    {
        public float TargetAngle;
        public float MeasuredSpeed;
        public float Frequency;
        public OscGroup[] Groups;
    }

    [SerializeField]
    private OscMode[] m_Modes;

    [SerializeField, Tooltip("Set to -1 for cycling modes")]
    private int m_ModeIndex = -1;
    private bool m_CycleModes;

    private float m_Time;
    private const float c_PI2 = Mathf.PI * 2;

    private void Awake()
    {
        m_CycleModes = m_ModeIndex == -1;
    }

    public void ManagedReset()
    {
        m_Time = 0;

        if (m_CycleModes)
        {
            m_ModeIndex = ++m_ModeIndex % m_Modes.Length;
            //Debug.Log(m_ModeIndex);
        }
    }

    public void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            m_ModeIndex = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            m_ModeIndex = 0;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            m_ModeIndex = 3;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            m_ModeIndex = 2;
        }
    }

    public void ManagedUpdate()
    {
        var mode = m_Modes[m_ModeIndex];

        m_Time += Time.fixedDeltaTime * mode.Frequency;
        float cos = Mathf.Cos(m_Time * c_PI2);

        foreach (var group in mode.Groups)
        {
            float amp = cos * group.Scale;
            foreach (var leg in group.Legs)
            {
                // Рассчитываем целевой угол для каждой ноги
                float targetAngle = amp * leg.Phase;

                // Двигаем ногу, передавая целевой угол и скорость
                leg.Leg.MoveLeg(targetAngle, mode.MeasuredSpeed);
            }
        }
    }
}

