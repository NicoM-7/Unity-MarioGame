using UnityEngine;
using System.Collections;

public abstract class Enemy : MonoBehaviour
{
    [Header("Enemy Properties")]
    public int health = 100;
    public float moveSpeed = 2f;
    public int power = 10;

    [Header("Idle Movement Settings")]
    public float idleMovementDistance = 1f;
    public LayerMask groundLayer;
    public LayerMask enemyLayer;
    public LayerMask platformLayer;

    [Header("Knockback Settings")]
    public float knockbackForce = 5f; // Force to apply to the player on contact

    [Header("Player Detection")]
    public LayerMask playerLayer; // Assign the player layer here
    public Vector2 detectionBoxSize = new Vector2(1f, 1f); // Size of the detection box

    public int score = 1000;

    public bool canStomp;

    protected AudioSource audioSource;
    protected Rigidbody2D rb;
    protected Animator animator;
    protected bool isAttacking;
    protected bool movingRight = true;
    protected bool dead;
    protected bool canMove;

    private Vector2 startPosition;
    private bool canSwitchDirections;

    protected virtual void Start()
    {
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        startPosition = transform.position;
        canMove = true;
        canSwitchDirections = true;
    }

    protected virtual void FixedUpdate()
    {
        if(!canMove) {
            return;
        }

        if (!isAttacking)
        {
            HandleMovement();
        }

        if(!dead) {
            CheckForPlayer();
        }

        animator.SetBool("grounded", OnGround());
    }

    private void HandleMovement()
    {
        if (HitWall() || NoGroundAhead())
        {
            SwitchDirection();
        }

        float moveDirection = movingRight ? 1 : -1;
        rb.velocity = new Vector2(moveDirection * moveSpeed, rb.velocity.y);
    }

    protected bool HitWall()
    {
        Vector2 position = transform.position;

        // Offset the raycast by 0.5 units in the direction of movement
        float offset = 0.6f;
        position.x += movingRight ? offset : -offset;

        Vector2 direction = movingRight ? Vector2.right : Vector2.left;
        float distance = 0.1f;

        // Draw the debug ray
        Debug.DrawRay(position, direction * distance, Color.red);

        RaycastHit2D hit = Physics2D.Raycast(position, direction, distance, groundLayer | enemyLayer);
        return hit.collider != null && hit.collider.gameObject != gameObject;
    }

    protected bool OnGround()
    {
        Vector2 position = transform.position;
        float distance = 0.6f;

        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.down, distance, groundLayer | platformLayer);
        return hit.collider != null;
    }

    private void SwitchDirection()
    {
        movingRight = !movingRight;

        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    public void TakeDamage(int damage, Vector2 force, float applyKnockbackDelay, bool meleeAttack, bool stompAttack, bool hammerStomp)
    {
        health -= damage;
        rb.velocity = Vector2.zero;

        if(stompAttack) {
            Stomp(!hammerStomp);
        } else if(meleeAttack) {
            StartCoroutine(DisableMovementBriefly(applyKnockbackDelay, force));
        }
    }

    protected virtual void Die()
    {
        dead = true;
        gameObject.layer = 2;
        StartCoroutine(WaitForGround());
        Destroy(gameObject, 2f);
    }

    public abstract void Stomp(bool jumpStomp);

    private void CheckForPlayer()
    {
        Vector2 detectionCenter = (Vector2)transform.position;
        Collider2D playerCollider = Physics2D.OverlapBox(detectionCenter, detectionBoxSize, 0f, playerLayer);

        if (playerCollider != null)
        {
            PlayerController player = playerCollider.GetComponent<PlayerController>();
            if (player != null)
            {
                if(GameObject.FindWithTag("Player").GetComponent<Inventory>().GetIsInvincible()) {
                    animator.SetTrigger("dead");
                    Die();
                    return;
                }

                Rigidbody2D playerRb = player.GetComponent<Rigidbody2D>();
                Vector2 playerVelocity = playerRb != null ? playerRb.velocity : Vector2.zero;

                Vector2 knockbackDirection;

                knockbackDirection.x = Mathf.Sign(((Vector2)playerCollider.transform.position - detectionCenter).x) * 1.5f;
                knockbackDirection.y = 1.5f;

                player.TakeDamage(power);
                player.ApplyKnockback(knockbackDirection * knockbackForce);
            }
        }
    }

    private IEnumerator DisableMovementBriefly(float applyKnockbackDelay, Vector2 force)
    {
        animator.SetTrigger("hurt");
        canMove = false;
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(applyKnockbackDelay);
        bool onGround = OnGround();
        rb.AddForce(force, ForceMode2D.Impulse);
        if(onGround) {
            while(OnGround()) {
                yield return null;
            }
        }
        while(!OnGround()) {
            yield return null;
        }
        rb.velocity = Vector2.zero;
        if (health <= 0)
        {
            animator.SetTrigger("dead");
            rb.simulated = false;
            dead = true;
            Die();
            yield break;
        }
        animator.SetTrigger("hurt");
        canMove = true;
    }

    private IEnumerator WaitForGround() {
        while(!OnGround()) {
            yield return null;
        }
        GameObject.FindWithTag("Player").GetComponent<PlayerController>().SetScore(GameObject.FindWithTag("Player").GetComponent<PlayerController>().GetScore() + score);
        rb.simulated = false;
    }

    protected bool NoGroundAhead()
    {
        Vector2 position = transform.position;
        Vector2 direction = movingRight ? Vector2.right : Vector2.left;
        float distance = 0.5f;

        Vector2 raycastOrigin = position + new Vector2(direction.x * distance, -distance);
        Debug.DrawRay(raycastOrigin, Vector2.down * distance, Color.red);
        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, Vector2.down, distance, groundLayer | platformLayer);

        return hit.collider == null; // If no ground is detected, return true
    }
}
