using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PaladinControl : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private float speed;

    // Update is called once per frame
    void Update()
    {
        speed = Input.GetAxis("Vertical");

        ApplyAnimation(Input.GetKeyDown(KeyCode.Space));
    }

    private void ApplyAnimation(bool attack)
    {
        if (attack) animator.SetTrigger("Attack");
        animator.SetFloat("Speed", speed);
    }
}
