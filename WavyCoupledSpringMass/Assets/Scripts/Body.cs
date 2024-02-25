using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Body : MonoBehaviour
{
    [SerializeField]
    Transform body;

    Vector3 PosXHook
    {
        get
        {
            Vector3 position = body.position;
            float xScale = body.localScale.x;
            position.x += xScale / 2f;

            return position;
        }
    }

    Vector3 NegXHook
    {
        get
        {
            Vector3 position = body.position;
            float xScale = body.localScale.x;
            position.x += xScale / 2f;

            return position;
        }
    }

    public Vector3 Velocity { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Acceleration { get; set; }

    void FixedUpdate() {
        transform.position = Position;
    }
}
