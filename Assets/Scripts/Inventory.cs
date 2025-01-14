using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public GameObject mushroom;
    public GameObject fireflower;
    public GameObject star;
    
    public AudioClip healing;
    public AudioClip powerup;
    public AudioClip invincible;
    public AudioClip starmanRunningOut;

    public float duration = 20.0f;
    public float cycleSpeed = 2.0f;

    private GameObject showingItem;

    private Dictionary<string, int> items;

    private bool isInventoryOpen;
    private bool isInvincible = false;

    private int currentIndex;

    private string[] itemKeys;

    private void Start() {
        AudioSource audio = GetComponent<AudioSource>();
    }

    private void Update()
    {
        items = GetComponent<PlayerController>().GetItems();

        if (Input.GetKeyDown(KeyCode.Q) && Mathf.Abs(GetComponent<Rigidbody2D>().velocity.x) < 0.1f && GetComponent<PlayerController>().CheckGrounded())
        {
            if (isInventoryOpen)
            {
                GetComponent<Animator>().SetTrigger("closeInventory");
                CloseInventory();
            }
            else if (HasItems())
            {
                GetComponent<Animator>().SetBool("isInventoryOpen", true);
                GetComponent<Animator>().SetTrigger("openInventory");
                OpenInventory();
            }
        }

        if (isInventoryOpen)
        {
            HandleInventoryNavigation();
        }
    }

    private void OpenInventory()
    {
        isInventoryOpen = true;
        currentIndex = 0;
        itemKeys = GetNonEmptyItemKeys();
        StartCoroutine(ShowItemAfterOpenInventoryDelay());
        Debug.Log("Inventory Opened");
        Debug.Log($"Current Item: {itemKeys[currentIndex]} (x{items[itemKeys[currentIndex]]})");
    }

    public void CloseInventory()
    {
        GetComponent<Animator>().SetBool("isInventoryOpen", false);
        Destroy(showingItem);
        isInventoryOpen = false;
    }

    private void HandleInventoryNavigation()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Destroy(showingItem);
            currentIndex = (currentIndex + 1) % itemKeys.Length;
            ShowItem(itemKeys[currentIndex]);
            Debug.Log($"Current Item: {itemKeys[currentIndex]} (x{items[itemKeys[currentIndex]]})");
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Destroy(showingItem);
            currentIndex = (currentIndex - 1 + itemKeys.Length) % itemKeys.Length;
            ShowItem(itemKeys[currentIndex]);
            Debug.Log($"Current Item: {itemKeys[currentIndex]} (x{items[itemKeys[currentIndex]]})");
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            string selectedItem = itemKeys[currentIndex];
            if (items[selectedItem] > 0)
            {
                UseItem(selectedItem);
            }
        }
    }

    private void UseItem(string item)
    {
        switch (item) {
            case "mushroom":
                GetComponent<PlayerController>().SetHealth(GetComponent<PlayerController>().GetHealth() + 50);
                GetComponent<AudioSource>().PlayOneShot(healing);
                break;
            case "fireflower":
                GetComponent<AudioSource>().PlayOneShot(powerup);
                break;
            case "star":
                GetComponent<AudioSource>().PlayOneShot(invincible, 0.25f);
                if (!isInvincible)
                {
                    StartCoroutine(InvincibilityEffect());
                }
                break;
            default:
                return;
        }

        GetComponent<PlayerController>().SetItem(item, GetComponent<PlayerController>().GetItem(item) - 1);
        
        Destroy(showingItem);
        GetComponent<Animator>().SetTrigger("closeInventory");
        CloseInventory();

        // if (GetComponent<PlayerController>().GetItem(item) <= 0)
        // {
        //     itemKeys = GetNonEmptyItemKeys();
        //     if (itemKeys.Length == 0)
        //     {
        //         Destroy(showingItem);
        //         GetComponent<Animator>().SetTrigger("closeInventory");
        //         CloseInventory();
        //     }
        //     else
        //     {
        //         Destroy(showingItem);
        //         currentIndex = Mathf.Clamp(currentIndex, 0, itemKeys.Length - 1);
        //         ShowItem(itemKeys[currentIndex]);
        //     }
        // }
    }

    private bool HasItems()
    {
        foreach (var count in items.Values)
        {
            if (count > 0) return true;
        }
        return false;
    }

    private string[] GetNonEmptyItemKeys()
    {
        List<string> nonEmptyKeys = new List<string>();
        foreach (var kvp in items)
        {
            if (kvp.Value > 0)
            {
                nonEmptyKeys.Add(kvp.Key);
            }
        }
        return nonEmptyKeys.ToArray();
    }

    private void ShowItem(string item) {
        GameObject spawnItem;
        switch (item) {
            case "mushroom":
                spawnItem = mushroom;
                break;
            case "fireflower":
                spawnItem = fireflower;
                break;
            case "star":
                spawnItem = star;
                break;
            default:
                return;
        }

        showingItem = Instantiate(spawnItem, transform.position + new Vector3(transform.localScale.x * 0.75f, 0.25f, 0f), Quaternion.identity, transform);
        showingItem.transform.localScale = new Vector3(0.75f, 0.75f, 0);
    }

    public bool GetIsInventoryOpen() {
        return isInventoryOpen;
    }

    private IEnumerator InvincibilityEffect()
    {
        GameObject.FindWithTag("MusicManager").GetComponent<AudioSource>().Stop();
        isInvincible = true;
        
        bool oneTime = true;
        float timer = 0f;

        while (timer < duration)
        {
            if(timer > duration - 2.5f && oneTime) {
                GetComponent<AudioSource>().PlayOneShot(starmanRunningOut, 0.5f);
                oneTime = false;
            }

            timer += Time.deltaTime;

            float hue = Mathf.Repeat(Time.time * cycleSpeed, 1.0f); 
            Color rainbowColor = Color.HSVToRGB(hue, 1.0f, 1.0f); 

            GetComponent<SpriteRenderer>().color = rainbowColor;

            yield return null;
        }

        GameObject.FindWithTag("MusicManager").GetComponent<AudioSource>().Play();
        GetComponent<SpriteRenderer>().color = Color.white;
        isInvincible = false;
    }

    public bool GetIsInvincible() {
        return isInvincible;
    }

    private IEnumerator ShowItemAfterOpenInventoryDelay() {
        yield return new WaitForSeconds(0.1f);
        ShowItem(itemKeys[currentIndex]);
    }
}
