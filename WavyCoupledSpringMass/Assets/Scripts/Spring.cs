using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Spring : MonoBehaviour
{
    [SerializeField]
    Transform spring;

    [SerializeField]
    Transform connectPoint1;

    [SerializeField]
    Transform connectPoint2;

    void FixedUpdate()
    {
        Vector3 averagePosition = (connectPoint1.position + connectPoint2.position) / 2f;
        spring.transform.position = averagePosition;
        Vector3 springVector = connectPoint2.position - connectPoint1.position;
        Quaternion orientation = Quaternion.FromToRotation(Vector3.up, springVector);
        float length = springVector.magnitude;

        spring.rotation = orientation;

        Vector3 scale = spring.localScale;
        scale.y = length;

        spring.localScale = scale;
    }
    public void SetConnectionPoints(Transform point1, Transform point2)
    {
        connectPoint1 = point1;
        connectPoint2 = point2;
    }
}
