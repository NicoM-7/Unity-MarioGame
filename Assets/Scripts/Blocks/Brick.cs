using System.Collections;
using UnityEngine;

public class Brick : Block
{
    protected override void OnHit()
    {
        GetComponent<Renderer>().enabled = false;
        GetComponent<Rigidbody2D>().isKinematic = true;
        gameObject.layer = 0;
        GetComponent<AudioSource>().Play();
    }

    protected override void OnHitsDepleted()
    {
        Destroy(gameObject, 0.4f);
    }
}
