using System.Collections; 
using UnityEngine;
using UnityEngine.SceneManagement;

public class Coin : MonoBehaviour {

    public int score;

    private bool oneTime = true;

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player") && oneTime) {
            oneTime = false;
            other.gameObject.GetComponent<PlayerController>().SetCoins(other.gameObject.GetComponent<PlayerController>().GetCoins() + 1);
            other.gameObject.GetComponent<PlayerController>().SetScore(other.gameObject.GetComponent<PlayerController>().GetScore() + score);
            GetComponent<AudioSource>().Play();
            GetComponent<Renderer>().enabled = false;
            Destroy(gameObject, 1f);
        }
    }
}