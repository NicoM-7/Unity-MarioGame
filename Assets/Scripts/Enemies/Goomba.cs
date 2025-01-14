using UnityEngine;
using System.Collections;

public class GoombaEnemy : Enemy
{
    [Header("Goomba Custom Properties")]
    public float detectionRange = 15f; // Range to detect the player
    public float attackCooldown = 5f; // Cooldown between attacks

    [Header("Dash Attack Settings")]
    public float dashSpeed = 10f; // Speed during dash
    public float dashDuration = 1.5f; // Duration of the dash

    [Header("Jump Attack Settings")]
    public float jumpForce = 17.5f; // Jump force for jump attack

    public AudioClip stomp;

    public bool passive;
    public bool dashAttack;
    public bool jumpAttack;

    private Transform player; // Reference to the player
    private bool canAttack = true;

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        if(!dead && canMove) {
            if(!passive) {
                DetectPlayerAndAttack();
            }
        }
    }

    private void DetectPlayerAndAttack()
    {
        if (player != null && Vector2.Distance(transform.position, player.position) <= detectionRange)
        {
            if (canAttack)
            {
                PerformAttack();
            }
        }
    }

    private void PerformAttack()
    {
        isAttacking = true;
        canAttack = false;

        if (dashAttack)
        {
            animator.SetBool("dashAttack", true);
            StartCoroutine(DashAttack());
        }

        if(jumpAttack)
        {
            animator.SetBool("jumpAttack", true);
            StartCoroutine(JumpAttack());
        }

        Invoke(nameof(ResetAttack), attackCooldown);
    }

    private IEnumerator DashAttack()
    {
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        float elapsedTime = 0f;

        while (elapsedTime < dashDuration)
        {
            if(!canMove) {
                animator.SetBool("dashAttack", false);
                isAttacking = false;
                yield break;
            }

            if(HitWall() || NoGroundAhead()) {
                break;
            }

            movingRight = directionToPlayer.x > 0 ? true : false;
            transform.localScale = new Vector3(Mathf.Sign(directionToPlayer.x), 1, 1);
            rb.velocity = new Vector2(Mathf.Sign(directionToPlayer.x) * dashSpeed, rb.velocity.y);
            elapsedTime += Time.deltaTime;

            yield return null;
        }

        rb.velocity = Vector2.zero;
        animator.SetBool("dashAttack", false);
        isAttacking = false;
    }

    private IEnumerator JumpAttack()
    {
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        movingRight = directionToPlayer.x > 0 ? true : false;
        transform.localScale = new Vector3(Mathf.Sign(directionToPlayer.x), 1, 1);
        rb.velocity = new Vector2(Mathf.Sign(directionToPlayer.x) * 6, jumpForce);
        yield return new WaitForSeconds(1.25f);
        animator.SetBool("jumpAttack", false);
        isAttacking = false;
    }

    private void ResetAttack()
    {
        canAttack = true;
    }

    public override void Stomp(bool jumpStomp) {
        if(canStomp || !jumpStomp) {
            audioSource.clip = stomp;
            animator.SetBool("stomped", true);
            audioSource.Play();
            base.Die();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }
}
