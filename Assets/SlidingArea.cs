using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingArea : MonoBehaviour
{
    [SerializeField]
    Player player;
    [SerializeField]
    private float slidingSpeed = 20f;
    [SerializeField]
    private Player.LookAngle direction;
    [SerializeField]
    private Vector3 modelRotation;
    [SerializeField]
    private float gravity = 30f;
    [SerializeField]
    private bool slideAnimation = true;
    void Start()
    {
        try
        {
            if(player == null)
            {
                player = GameObject.Find("Player").GetComponent<Player>();
            }
        }
        catch
        {
            this.enabled = false;
        }
    }
    void OnTriggerStay(Collider other) 
    {
        if(other.gameObject.tag == "Player")
        {
            player.PlayerManualSliding(direction, slidingSpeed, gravity, modelRotation, slideAnimation);
        }
    }
    void OnTriggerExit(Collider other) 
    {
        if(other.gameObject.tag == "Player")
        {
            player.StopManualSliding();
        }
    }
}
