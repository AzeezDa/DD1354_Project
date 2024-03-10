using UnityEngine;

public class Wall : Body
{
    [SerializeField]
    Transform hook;

    public Transform Hook {get; private set;}
    
    void Awake() {
        Position = transform.position;
    }
}
