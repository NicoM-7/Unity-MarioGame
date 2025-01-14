using UnityEngine;
using System.Collections;

public class CoinBlock : Block
{
    public Sprite empty;
    public GameObject coinPrefab;
    
    public float coinJumpForce = 5f; 

    protected override void OnHit()
    {
        GameObject.FindWithTag("Player").GetComponent<PlayerController>().SetCoins(GameObject.FindWithTag("Player").GetComponent<PlayerController>().GetCoins() + 1);
        GameObject spawnedCoin = Instantiate(coinPrefab, (Vector2)transform.position + Vector2.up, Quaternion.identity);

        Rigidbody2D coinRigidbody = spawnedCoin.GetComponent<Rigidbody2D>();
        if (coinRigidbody != null)
        {
            coinRigidbody.AddForce(Vector2.up * coinJumpForce, ForceMode2D.Impulse);
        }
        StartCoroutine(base.StopForceAfterTime(0.1f, spawnedCoin));
        Destroy(spawnedCoin, 0.25f);
        GetComponent<AudioSource>().Play();
    }

    protected override void OnHitsDepleted()
    {
        GetComponent<SpriteRenderer>().sprite = empty;
        GetComponent<Animator>().enabled = false;
    }
}
