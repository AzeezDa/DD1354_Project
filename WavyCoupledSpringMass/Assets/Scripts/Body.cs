using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Body : MonoBehaviour
{
    [SerializeField]
    Transform body;

    public Vector3 Velocity { get; set; }
    public Vector3 Position { get; set; }

    void FixedUpdate() {
        transform.position = Position;
    }
}
