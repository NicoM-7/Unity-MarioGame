using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupBlock : Block
{
    public Sprite empty;
    
    public Powerup powerup;

    public GameObject mushroomPrefab;
    public GameObject fireflowerPrefab;
    public GameObject starPrefab;

    public float powerupJumpForce = 5f; 

    private GameObject powerupPrefab;

    protected override void OnHit()
    {
        switch (powerup)
        {
            case Powerup.mushroom:
                powerupPrefab = mushroomPrefab;
                GameObject.FindWithTag("Player").GetComponent<PlayerController>().SetItem("mushroom", GameObject.FindWithTag("Player").GetComponent<PlayerController>().GetItem("mushroom") + 1);
                break;
            case Powerup.fireflower:
                powerupPrefab = fireflowerPrefab;
                GameObject.FindWithTag("Player").GetComponent<PlayerController>().SetItem("fireflower", GameObject.FindWithTag("Player").GetComponent<PlayerController>().GetItem("fireflower") + 1);
                break;
            case Powerup.star:
                powerupPrefab = starPrefab;
                GameObject.FindWithTag("Player").GetComponent<PlayerController>().SetItem("star", GameObject.FindWithTag("Player").GetComponent<PlayerController>().GetItem("star") + 1);
                break;
            default:
                return;
        }

        GameObject spawnedPowerup = Instantiate(powerupPrefab, (Vector2)transform.position + Vector2.up, Quaternion.identity);

        Rigidbody2D powerupRigidbody = spawnedPowerup.GetComponent<Rigidbody2D>();
        if (powerupRigidbody != null)
        {
            powerupRigidbody.AddForce(Vector2.up * powerupJumpForce, ForceMode2D.Impulse);
        }

        StartCoroutine(base.StopForceAfterTime(0.1f, spawnedPowerup));
        Destroy(spawnedPowerup, 1f);
        GetComponent<AudioSource>().Play();
    }

    protected override void OnHitsDepleted()
    {
        GetComponent<SpriteRenderer>().sprite = empty;
        GetComponent<Animator>().enabled = false;
    }
}
