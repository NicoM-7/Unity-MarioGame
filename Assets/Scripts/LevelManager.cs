using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using TMPro;

public class LevelManager : MonoBehaviour {
    
    public GameObject pauseMenuUI;
    public GameObject infoMenuUI;
   
    public Sprite coinBlock;

    public AudioClip oneUpSound;
    
    public int time;

    private bool isPaused = false;

    private void Update() {
        int coins = GameObject.FindWithTag("Player").GetComponent<PlayerController>().GetCoins();
        if(coins == 100) {
            Data.lives++;
            GameObject.FindWithTag("Player").GetComponent<PlayerController>().SetCoins(0);
            GetComponent<AudioSource>().PlayOneShot(oneUpSound);
        }
    }

    public void Resume() {
        pauseMenuUI.SetActive(false);
        infoMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        ToggleAudio(true);
        ToggleAnimators(true);  
    }

    public void DisplayInfo(string info) {
        infoMenuUI.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = info;
        infoMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        ToggleAnimators(false);
    }

    public void Pause() {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        ToggleAudio(false);  
        ToggleAnimators(false);
    }

    private void ToggleAudio(bool play) {
        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audio in audioSources) {
            if (play) {
                audio.UnPause();
            } else {
                audio.Pause();
            }
        }
    }

    private void ToggleAnimators(bool enable) {
        Animator[] animators = FindObjectsOfType<Animator>();
        foreach (Animator animator in animators) {
            if(animator.gameObject.TryGetComponent<Block>(out Block block)) {
                if(animator.gameObject.GetComponent<Block>().GetRemainingHits() != 0) {
                    animator.enabled = enable;
                }
            } else {
                animator.enabled = enable;
            }
        }
    }

    public void Quit() {
        Resume();
        SceneManager.LoadScene("MainMenu");
    }

    public void Restart() {
        SceneManager.LoadScene("1-1");
        Resume();  
    }

    public bool GetPaused() {
        return isPaused;
    }

    public int GetTimeRemaining() {
        return time;
    }
}

