using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public enum Direction { Horizontal, Vertical }
    public Direction moveDirection = Direction.Horizontal; // Direction of movement
    public float moveUnits = 5f; // Distance the platform will move
    public float moveSpeed = 2f; // Speed of movement

    private Vector3 startPosition; // Initial position of the platform
    private Vector3 targetPosition; // Target position of the platform
    private bool movingToTarget = true; // Direction of movement (true = towards target)

    private Transform platformCenter; // The center of the platform (the parent object)

    void Start()
    {
        // Cache the starting position
        startPosition = transform.position;

        // Calculate the target position based on direction and distance
        if (moveDirection == Direction.Horizontal)
        {
            targetPosition = startPosition + new Vector3(moveUnits, 0f, 0f);
        }
        else // Vertical
        {
            targetPosition = startPosition + new Vector3(0f, moveUnits, 0f);
        }

        // Find the center object (platform's parent)
        platformCenter = transform;
    }

    void FixedUpdate()
    {
        // Move the platform
        MovePlatform();
    }

    private void MovePlatform()
    {
        // Determine the target position
        Vector3 target = movingToTarget ? targetPosition : startPosition;

        // Move towards the target position
        platformCenter.position = Vector3.MoveTowards(platformCenter.position, target, moveSpeed * Time.deltaTime);

        // Check if the platform has reached the target or start position
        if (Vector3.Distance(platformCenter.position, target) < 0.01f)
        {
            movingToTarget = !movingToTarget; // Reverse the direction
        }
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
                    collision.transform.SetParent(platformCenter);
                    return;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Remove the player from the platform's parent
            collision.transform.SetParent(null);
        }
    }
}
