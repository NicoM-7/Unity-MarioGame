using UnityEngine;

public class InfoBlock : Block
{
    public string message = "Hello, this is an info block!";

    protected override void OnHit()
    {
        GameObject.FindWithTag("LevelManager").GetComponent<LevelManager>().DisplayInfo(message);
    }
}
