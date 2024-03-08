using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HangingArea : MonoBehaviour
{
    [SerializeField]
    Player player;
    [SerializeField]
    Player.LookAngle requiredDirection;
    [SerializeField]
    private bool isClimbPossible = false;
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

    void OnTriggerEnter(Collider other) 
    {
        if(other.gameObject.tag == "Player")
        {
            if(requiredDirection != player.GetPlayerLookVector())
            {
                return;
            }
            player.HangPlayer(isClimbPossible);
        }
    }
}
