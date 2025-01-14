using UnityEngine;
using System.Collections;

public class HammerThrow : MonoBehaviour
{
    [Header("Throw Properties")]
    public float throwForce = 10f; // Force of the throw
    public float gravityDelay = 1f; // Time before gravity takes effect
    public float bounceForce = 5f; // Force applied on bounce

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 throwDirection;
    private bool oneTime;
    private bool grounded;
    private bool canPickup;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        throwDirection = GameObject.FindWithTag("Player").GetComponent<Transform>().localScale.x > 0 ? Vector2.right : Vector2.left;
        rb.AddForce(throwDirection * throwForce, ForceMode2D.Impulse);
        StartCoroutine(ActivateGravityAfterDelay());
        oneTime = true;
    }

    private void Update() {

        if(grounded) {
            rb.gravityScale = 0f;
            rb.velocity = Vector2.zero;
            return;
        }

        float rayLength = 0.5f;

        Vector2 origin = transform.position;
        Vector2 boxSize = new Vector2(rayLength, GetComponent<BoxCollider2D>().bounds.size.y);

        RaycastHit2D[] hits = Physics2D.BoxCastAll(origin, boxSize, 0f, throwDirection, rayLength, LayerMask.GetMask("Enemy", "Ground"));

        if (hits.Length != 0 && oneTime)
        {
            oneTime = false;
            if(hits[0].collider.gameObject.layer == LayerMask.NameToLayer("Enemy")) {
                Rigidbody2D enemyRb = hits[0].collider.GetComponent<Rigidbody2D>();
                Vector2 enemyVelocity = enemyRb != null ? enemyRb.velocity : Vector2.zero;
                Vector2 knockbackDirection = new Vector2(Mathf.Sign(((Vector2)hits[0].collider.transform.position - (Vector2)transform.position).x) * 5f, 5f);
                hits[0].collider.GetComponent<Enemy>().TakeDamage(25, knockbackDirection, 0f, true, false, false);
            } else if(hits[0].collider.gameObject.layer == LayerMask.NameToLayer("Ground") && hits[0].collider.TryGetComponent<Block>(out Block block)) {
                foreach(RaycastHit2D hit in hits) {
                    if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground") && hit.collider.TryGetComponent<Block>(out Block block2)) {
                        if( hit.collider.gameObject.GetComponent<Block>().canHammerHit) {
                            hit.collider.gameObject.GetComponent<Block>().Hit();
                        }
                    }
                }
            }
            Bounce(hits[0].normal);
        }
        else if(IsGrounded()) {
            canPickup = true;
            animator.enabled = false;
            grounded = true;
        }
    }

    private void Bounce(Vector2 collisionNormal)
    {
        Vector2 bounceDirection = new Vector2(Mathf.Sign(-rb.velocity.normalized.x) * bounceForce, bounceForce);
        rb.velocity = bounceDirection * bounceForce;
        rb.gravityScale = 4;
    }

    private IEnumerator ActivateGravityAfterDelay()
    {
        yield return new WaitForSeconds(gravityDelay);
        rb.gravityScale = 4;
    }

    private bool IsGrounded()
    {
        float rayLength = 1f; 
        Vector2 origin = transform.position;
        Vector2 direction = Vector2.down;
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, rayLength, LayerMask.GetMask("Ground") | LayerMask.GetMask("Platform"));
        return hit.collider != null;
    }

    private bool IsTouchingWall()
    {
        float rayLength = 1f;
        Vector2 origin = transform.position;

        Vector2 boxSize = new Vector2(rayLength, GetComponent<BoxCollider2D>().bounds.size.y);

        RaycastHit2D hit = Physics2D.BoxCast(origin, boxSize, 0f, throwDirection, rayLength, LayerMask.GetMask("Ground"));
        return hit.collider != null;
    }

    private bool GetCanPickup() {
        return canPickup;
    }
}