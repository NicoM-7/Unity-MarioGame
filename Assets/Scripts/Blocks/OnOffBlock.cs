using System.Collections;
using UnityEngine;

public class OnOffBlock : Block
{
    private static bool isOn;
    private SpriteRenderer spriteRenderer;
    
    public Sprite onSprite;
    public Sprite offSprite;

    public Sprite greenBlockSolid;
    public Sprite greenBlockHollow;
    public Sprite redBlockSolid;
    public Sprite redBlockHollow;

    private void Awake() {
        isOn = false;
    }

    protected override void Start()
    {
        base.Start();
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateSprite();
    }

    protected override void OnHit()
    {
        GetComponent<AudioSource>().Play();
        isOn = !isOn;

        foreach (var block in FindObjectsOfType<OnOffBlock>())
        {
            block.UpdateSprite();
        }

        UpdateRedBlueBlockSolidity();
    }

    private void UpdateSprite()
    {
        spriteRenderer.sprite = isOn ? onSprite : offSprite;
    }

    private void UpdateRedBlueBlockSolidity()
    {
        var redBlocks = GameObject.FindGameObjectsWithTag("RedBlock");
        var greenBlocks = GameObject.FindGameObjectsWithTag("GreenBlock");

        foreach (var redBlock in redBlocks)
        {
            var collider = redBlock.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = !isOn;
                if(!isOn) {
                    redBlock.GetComponent<SpriteRenderer>().sprite = redBlockSolid;
                } else {
                    redBlock.GetComponent<SpriteRenderer>().sprite = redBlockHollow;
                }
            }
        }

        foreach (var greenBlock in greenBlocks)
        {
            var collider = greenBlock.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = isOn;
                if(isOn) {
                    greenBlock.GetComponent<SpriteRenderer>().sprite = greenBlockSolid;
                } else {
                    greenBlock.GetComponent<SpriteRenderer>().sprite = greenBlockHollow;
                }
            }
        }
    }
}
