using System.Collections; 
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;

public class LevelComplete : MonoBehaviour {
    
    public float restartDelay = 5f;
    
    private bool oneTime = true;

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player") && oneTime) {
            GameObject.FindWithTag("Hud").GetComponent<Hud>().StopTimer();
            GameObject.FindWithTag("Player").GetComponent<PlayerController>().SetIsInvincible(true);
            
            Data.score = GameObject.FindWithTag("Hud").GetComponent<Hud>().GetRemainingTimeInMilliseconds() + GameObject.FindWithTag("Player").GetComponent<PlayerController>().GetScore();
            Data.scoreString = Data.score.ToString();
            Data.completed = true;

            GetComponent<Renderer>().enabled = false;
            StartCoroutine(WaitForGrounded(other.GetComponent<Animator>(), other.GetComponent<PlayerController>(), other.GetComponent<Rigidbody2D>()));
            StartCoroutine(RestartSceneAfterDelay());
        }
    }
        
    private IEnumerator RestartSceneAfterDelay() {
        yield return new WaitForSeconds(restartDelay);
        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator WaitForGrounded(Animator animator, PlayerController controller, Rigidbody2D rb) {
        GameObject.FindWithTag("MusicManager").GetComponent<AudioSource>().Stop();
        while (!animator.GetBool("onGround")) {
            yield return null;
        }

        rb.velocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        GameObject.FindWithTag("Player").GetComponent<Animator>().SetBool("complete", true);
        GameObject.FindWithTag("Player").GetComponent<Animator>().SetTrigger("completed");
        controller.enabled = false;
        GetComponent<AudioSource>().Play(); 
    }
}