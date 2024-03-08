using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterModel : MonoBehaviour
{
    Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        
    }
    public Transform GetTransform()
    {
        return this.transform;
    }
    public void ChangeAnimatorFloat(string name, float value)
    {
        animator.SetFloat(name, value);
    }
    public void ChangeAnimatorBool(string name, bool value)
    {
        animator.SetBool(name, value);
    }
    public void CallAnimatorTrigger(string name)
    {
        animator.SetTrigger(name);
    }
}
