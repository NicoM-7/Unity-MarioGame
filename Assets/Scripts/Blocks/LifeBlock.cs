using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LifeBlock : Block
{
    public Sprite empty;
    public GameObject lifePrefab;
    
    public float lifeJumpForce = 5f; 

    protected override void OnHit()
    {
        Data.lives++;
        GameObject spawnedLife = Instantiate(lifePrefab, (Vector2)transform.position + Vector2.up, Quaternion.identity);

        Rigidbody2D lifeRigidbody = spawnedLife.GetComponent<Rigidbody2D>();
        if (lifeRigidbody != null)
        {
            lifeRigidbody.AddForce(Vector2.up * lifeJumpForce, ForceMode2D.Impulse);
        }
        
        StartCoroutine(base.StopForceAfterTime(0.1f, spawnedLife));
        Destroy(spawnedLife, 1f);
        GetComponent<AudioSource>().Play();
    }

    protected override void OnHitsDepleted()
    {
        GetComponent<SpriteRenderer>().sprite = empty;
        GetComponent<Animator>().enabled = false;
    }
}
