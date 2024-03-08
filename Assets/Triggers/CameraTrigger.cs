using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTrigger : MonoBehaviour, ITrigger
{
    // If True, Player's class variable called triggeredAnchor will be nulled, when player exiting Trigger;
    [SerializeField]
    private bool nullPlayerAnchorOnExit = true;
    [SerializeField]
    CameraAnchor anchor;
    private bool isTriggered = false;
    Player player;
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
    void OnTriggerEnter()
    {
        Trigger();
    }
    void OnTriggerExit()
    {

        Trigger();
    }
    public void Trigger()
    {
        if(isTriggered)
        {
            isTriggered = false;
            if(!nullPlayerAnchorOnExit) return;
            player.SetCameraAnchorTrigger(null);
        }
        else
        {
            isTriggered = true;
            if(anchor == null) return;
            player.SetCameraAnchorTrigger(anchor);
        }
    }
}
