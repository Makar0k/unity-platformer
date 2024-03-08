using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fun : MonoBehaviour
{
    Transform[] kids;
    void Start()
    {
        kids = GetComponentsInChildren<Transform>();
        foreach ( var child in kids ) 
        {
            child.gameObject.AddComponent<CapsuleCollider>();
            child.gameObject.AddComponent<Rigidbody>();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
