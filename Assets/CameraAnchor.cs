using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAnchor : MonoBehaviour
{
    public float distanceToActivate = 2f;
    public float lerpSpeed = 4f;
    public Player.cameraProjection cameraProjection;
    public float orthographicSize = 5f;
    void Start()
    {  

    }
    void Update()
    {
        
    }
    public Vector3 GetAnchorPosition()
    {
        return transform.position;
    }
}
