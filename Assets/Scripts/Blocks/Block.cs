using System.Collections;
using UnityEngine;

public abstract class Block : MonoBehaviour
{
    public float bounceHeight = 0.5f;
    public float bounceSpeed = 1f;   
    
    public int maxHits = 1;         
    public int score = 0; 
    
    public bool infiniteHits = false;
    public bool canHammerHit = false; 

    private Vector2 originalPosition;
    private int remainingHits;

    protected virtual void Start()
    {
        originalPosition = transform.position;
        remainingHits = maxHits;
    }

    public void Hit()
    {
        if (infiniteHits)
        {
            OnHit();
            StartCoroutine(BounceCoroutine());
            return;
        }

        if (remainingHits > 0)
        {
            GameObject.FindWithTag("Player").GetComponent<PlayerController>().SetScore(GameObject.FindWithTag("Player").GetComponent<PlayerController>().GetScore() + score);
            remainingHits--;
            OnHit();
            StartCoroutine(BounceCoroutine());

            if (remainingHits == 0)
            {
                OnHitsDepleted();
            }
        }
    }

    private IEnumerator BounceCoroutine()
    {
        Vector2 targetPosition = originalPosition + Vector2.up * bounceHeight;

        while (transform.position.y < targetPosition.y)
        {
            transform.position = new Vector2(transform.position.x, transform.position.y + bounceSpeed * Time.deltaTime);
            yield return null;
        }

        while (transform.position.y > originalPosition.y)
        {
            transform.position = new Vector2(transform.position.x, transform.position.y - bounceSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = originalPosition;
        OnBounceComplete();
    }

    public int GetRemainingHits()
    {
        return remainingHits;
    }

    protected abstract void OnHit();          
    
    protected virtual IEnumerator StopForceAfterTime(float duration, GameObject spawnedItem) { 
        yield return new WaitForSeconds(duration);
        spawnedItem.GetComponent<Rigidbody2D>().velocity = new Vector2(GetComponent<Rigidbody2D>().velocity.x, 0f);
    }

    protected virtual void OnBounceComplete() { }  
        
    protected virtual void OnHitsDepleted() { }
}
