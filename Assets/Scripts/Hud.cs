using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class Hud : MonoBehaviour {
    
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI livesText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI mushroomText;
    public TextMeshProUGUI fireflowerText;
    public TextMeshProUGUI starText;

    public Slider healthSlider;

    private float remainingTime;
    
    private int coins;
    private int lives;
    private int currentHealth;

    private bool isRunning;

    private void Start() {
        remainingTime = FindObjectOfType<LevelManager>().GetTimeRemaining();
        coins = 0;
        lives = Data.lives;
        currentHealth = 100;
        isRunning = true;
        UpdateHud();
    }

    private void Update() {
        if (isRunning) {
            remainingTime -= Time.deltaTime;
            if (remainingTime <= 0) {
                remainingTime = 0;
                isRunning = false;
                OnTimerEnd();
            }
            UpdateHud();
        }
    }

    private void OnTimerEnd() {
        GameObject.FindWithTag("Player").GetComponent<PlayerController>().SetHealth(0);
        StartCoroutine(GameObject.FindWithTag("Player").GetComponent<PlayerController>().TimeOut());
    }

    private void UpdateHud() {
        UpdateCoins();
        UpdateHealthBar();
        UpdateTimer();
        UpdateScore();
        UpdateItems();
        UpdateLives();
    }

    private string AddLeadingZeros(int number, int leadingZeros)
    {
        string format = new string('0', leadingZeros);
        return number.ToString(format);
    }

    public string UpdateTimer() {
        int minutes = Mathf.FloorToInt(remainingTime / 60F);
        int seconds = Mathf.FloorToInt(remainingTime % 60F);
        string time = $"{minutes:00}:{seconds:00}";
        timerText.text = time;
        return timerText.text;
    }

    public string UpdateCoins() {
        string coins = AddLeadingZeros(GameObject.FindWithTag("Player").GetComponent<PlayerController>().GetCoins(), 2);
        coinText.text = $"x {coins}";
        return coinText.text;
    }

    public string UpdateHealthBar() {
        int playerHealth = GameObject.FindWithTag("Player").GetComponent<PlayerController>().GetHealth();
        healthSlider.value = playerHealth;
        healthText.text = $"{playerHealth} HP";
        return healthText.text;
    }

    public string UpdateScore() {
        string playerScore = AddLeadingZeros(GameObject.FindWithTag("Player").GetComponent<PlayerController>().GetScore(), 7);
        scoreText.text = $"Score: {playerScore}";
        return scoreText.text;
    }

    public void UpdateItems() {
        string mushrooms = AddLeadingZeros(GameObject.FindWithTag("Player").GetComponent<PlayerController>().GetItem("mushroom"), 2);
        string fireflowers = AddLeadingZeros(GameObject.FindWithTag("Player").GetComponent<PlayerController>().GetItem("fireflower"), 2);
        string stars = AddLeadingZeros(GameObject.FindWithTag("Player").GetComponent<PlayerController>().GetItem("star"), 2);
        mushroomText.text = $"x {mushrooms}";
        fireflowerText.text = $"x {fireflowers}";
        starText.text = $"x {stars}";
    }

    public string UpdateLives() {
        string lives = AddLeadingZeros(Data.lives, 2);;
        livesText.text = $"x {lives}";
        return coinText.text;
    }

    public void StartTimer() {
        isRunning = true;
    }

    public void StopTimer() {
        isRunning = false;
    }

    public void ResetTimer() {
        remainingTime = FindObjectOfType<LevelManager>().GetTimeRemaining();
        UpdateTimer();
    }

    public int GetRemainingTimeInMilliseconds() {
        return (int)(remainingTime * 1000);
    }
}
