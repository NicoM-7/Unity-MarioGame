using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    public Sprite activatedFlagSprite;
    public Transform respawnPoint;

    [Header("Player Layer")]
    public LayerMask playerLayer;

    public int score = 10000;

    private SpriteRenderer spriteRenderer;
    private bool isActivated = false;

    void Start()
    {
        if(Data.checkpointPosition.HasValue) {
            transform.Find("flag").GetComponent<SpriteRenderer>().sprite = activatedFlagSprite;
            isActivated = true;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (respawnPoint == null)
        {
            respawnPoint = this.transform;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & playerLayer) != 0 && !isActivated)
        {
            ActivateCheckpoint();
        }
    }

    private void ActivateCheckpoint()
    {
        GameObject.FindWithTag("Player").GetComponent<PlayerController>().SetScore(GameObject.FindWithTag("Player").GetComponent<PlayerController>().GetScore() + score);
        GetComponent<AudioSource>().Play();
        isActivated = true;
        transform.Find("flag").GetComponent<SpriteRenderer>().sprite = activatedFlagSprite;
        Data.checkpointPosition = respawnPoint.position;
    }
}
