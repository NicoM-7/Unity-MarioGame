using System.Collections;
using UnityEngine;

public class RotatingBlock : Block
{
    public float rotationDuration = 5f;

    private Collider2D blockCollider;

    private Animator animator;

    private bool isRotating = false;

    protected override void Start()
    {
        base.Start();
        blockCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
    }

    protected override void OnHit()
    {
        if (!isRotating)
        {
            StartCoroutine(RotationCoroutine());
        }
    }

    private IEnumerator RotationCoroutine()
    {
        isRotating = true;
        animator.SetBool("isRotating", true);
        yield return new WaitForSeconds(0.3f);
        blockCollider.enabled = false;
        
        yield return new WaitForSeconds(rotationDuration - 0.3f);

        animator.SetBool("isRotating", false);
        blockCollider.enabled = true;
        isRotating = false;
    }
}
