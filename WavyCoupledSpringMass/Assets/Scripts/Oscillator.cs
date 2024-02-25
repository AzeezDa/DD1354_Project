using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oscillator : MonoBehaviour
{
    [SerializeField]
    Transform oscillator;

    [Header("X Oscillation")]
    [SerializeField]
    float xAmplitude = 0f;

    [SerializeField]
    float xFrequency = 0f;

    [SerializeField]
    [Range(0, 2 * Mathf.PI)]
    float xPhase = 0f;

    [Header("Y Oscillation")]
    [SerializeField]
    float yAmplitude = 0f;

    [SerializeField]
    float yFrequency = 0f;

    [SerializeField]
    [Range(0, 2 * Mathf.PI)]
    float yPhase = 0f;

    [Header("Z Oscillation")]
    [SerializeField]
    float zAmplitude = 0f;

    [SerializeField]
    float zFrequency = 0f;

    [SerializeField]
    [Range(0, 2 * Mathf.PI)]
    float zPhase = 0f;

    private Vector3 originalPosition;


    void Start()
    {
        originalPosition = oscillator.position;
    }

    void FixedUpdate()
    {
        float t = Time.fixedTime;
        const float PI2 = 2f * Mathf.PI;
        Vector3 deltaPosition = new(
        xAmplitude * Mathf.Cos(PI2 * xFrequency * t + xPhase),
        yAmplitude * Mathf.Cos(PI2 * yFrequency * t + yPhase),
        zAmplitude * Mathf.Cos(PI2 * zFrequency * t + zPhase)
        );

        transform.position = originalPosition + deltaPosition;
    }
}
