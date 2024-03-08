using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField]
    List<Transform> waypoints;
    [SerializeField]
    public float moveSpeed = 2f;
    [SerializeField]
    private float waitTime = 5f;
    private float currentWaitTime = 0f;
    private int currentWP = 0;
    [SerializeField]
    PlatformMovementType movementType;
    enum PlatformMovementType
    {
        Lerp,
        Linear,
    }
    void Start()
    {
        if(waypoints == null)
        {
            this.enabled = false;
            return;
        }
    }
    void FixedUpdate()
    {
        if(currentWaitTime > 0)
        {
            currentWaitTime -= Time.fixedDeltaTime;
            return;
        }
        switch(movementType)
        {
            case PlatformMovementType.Lerp:
            {
                transform.position = Vector3.Lerp(transform.position, new Vector3(waypoints[currentWP].position.x, waypoints[currentWP].position.y, transform.position.z), Time.fixedDeltaTime * moveSpeed);
                break;
            }
            case PlatformMovementType.Linear:
            {
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(waypoints[currentWP].position.x, waypoints[currentWP].position.y, transform.position.z), Time.fixedDeltaTime * moveSpeed);
                break;
            }
        }
        if(Vector2.Distance(waypoints[currentWP].position, transform.position) < 0.1f)
        {
            currentWP++;
            currentWaitTime = waitTime;
            if(currentWP >= waypoints.Count)
            {
                currentWP = 0;
            }
        }
    }
}
