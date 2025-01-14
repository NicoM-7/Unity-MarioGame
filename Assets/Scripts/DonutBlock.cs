using UnityEngine;

public class DonutBlock : MonoBehaviour
{
    public float fallDelay = 3f; // Time before the block falls
    public float fallSpeed = -3f; // Consistent falling speed
    public Vector2 respawnPosition; // Position to respawn the block

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    
    private bool playerOnBlock = false;
    private bool isFalling = false;

    private float timer = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb.bodyType = RigidbodyType2D.Kinematic; // Ensure the block is kinematic
        respawnPosition = transform.position;
    }

    private void Update()
    {
        if (playerOnBlock)
        {
            timer += Time.deltaTime;
            if (timer >= fallDelay)
            {
                MakeBlockFall();
            }
        }

        if (transform.position.y < -13f)
        {
            RespawnBlock();
        }

        UpdateColor();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Check if the collision is coming from above
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (Mathf.Sign(contact.normal.y) == -1f)
                {
                    playerOnBlock = true;
                    collision.transform.SetParent(transform); // Make the player a child of the block
                    return; // Exit loop as we already know the player is on top
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerOnBlock = false;
            timer = 0f;
            collision.transform.SetParent(null); // Remove the player as a child of the block
        }
    }

    private void MakeBlockFall()
    {
        rb.velocity = new Vector2(0, fallSpeed); // Set a consistent falling speed
        isFalling = true;
    }

    private void RespawnBlock()
    {
        rb.velocity = Vector2.zero; // Stop movement
        transform.position = respawnPosition; // Reset position
        playerOnBlock = false; // Reset player state
        isFalling = false;
        timer = 0f; // Reset the timer
    }

    private void UpdateColor()
    {
        if (playerOnBlock || isFalling)
        {
            spriteRenderer.color = Color.red;
        }
        else
        {
            spriteRenderer.color = Color.white;
        }
    }
}
