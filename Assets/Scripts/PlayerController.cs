using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{

    public float walkSpeed = 5f;
    public float maxSpeed = 15f;
    public float acceleration = 10f;
    public float deceleration = 15f;
    public float skidThreshold = 3f;
    public float skidFactor = 0.5f;

    public float jumpForce = 10;
    public float holdJumpForce = 50f;
    public float maxHoldJumpTime = 0.25f;
    public float groundCheckDistance = 0.1f;
    public float wallCheckDistance = 0.1f;
    public float wallJumpForce = 10f;
    public float wallJumpVerticalForce = 10f;
    public float invincibilityDuration = 2f;
    public float stompRaycastLength = 0.5f;
    public float bounceForce = 10f;
    public float attackBoxWidth = 1f;
    
    public int power = 25;

    public LayerMask groundLayer;
    public LayerMask platformLayer;
    public LayerMask enemyLayer;
    public LayerMask hammerLayer;

    public AudioClip death;
    public AudioClip jump;
    public AudioClip hurt;
    public AudioClip deadSound;
    public AudioClip fall;

    public GameObject hammer;
    public GameObject fireball;

    private Rigidbody2D rb;

    private Animator animator;

    private AudioSource audio;

    private bool isMoving;
    private bool isJumping;
    private bool isHoldingJump;
    private bool isTouchingWall;
    private bool isGrabbing;
    private bool isWallJumping;
    private bool isGrounded;
    private bool isAccelerating;
    private bool isSkidding;
    private bool canHitBlock;
    private bool dead;
    private bool jumpStarted;
    private bool jumpInputHeld;
    private bool oneTime;
    private bool oneTimeSprint;
    private bool idleAttack;
    private bool idleHammer;
    private bool walkAttack;
    private bool walkHammer;
    private bool runAttack;
    private bool runHammer;
    private bool isInvincible;
    private bool canMove;
    private bool stomped;
    private bool resetJumpForce;
    private bool hasHammer;
    private bool isInventoryOpen; 

    private float moveInput;
    private float jumpTimeCounter;
    private float wallJumpDirection;

    private int coins;
    private int health;
    private int score;
    private int lives;

    private Dictionary<string, int> items = new Dictionary<string, int>{{"mushroom", 0}, {"fireflower", 0}, {"star", 0}};

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audio = GetComponent<AudioSource>();
        coins = 0;
        health = 100;
        score = 0;
        lives = Data.lives;
        oneTimeSprint = true;
        oneTime = true;
        canHitBlock = true;
        canMove = true;
        hasHammer = true;
    }

    private void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
        isAccelerating = Input.GetKey(KeyCode.A);
        isInventoryOpen = GetComponent<Inventory>().GetIsInventoryOpen();

        if (Input.GetKeyDown(KeyCode.Escape)) {
            bool paused = GameObject.FindWithTag("LevelManager").GetComponent<LevelManager>().GetPaused();
            if (paused) {
                GameObject.FindWithTag("LevelManager").GetComponent<LevelManager>().Resume();
            } else {
                GameObject.FindWithTag("LevelManager").GetComponent<LevelManager>().Pause();
            }
        }

        if (!canMove || dead || idleAttack || idleHammer || walkAttack || walkHammer || runAttack || runHammer || isInventoryOpen)
        {
            return;
        }

        if (moveInput != 0 && !isWallJumping)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveInput), 1, 1);
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            audio.clip = jump;
            audio.Play();
            jumpStarted = true;
            isJumping = true;
            jumpTimeCounter = maxHoldJumpTime;
            stomped = false;
        }

        if (Input.GetKey(KeyCode.Space) && isJumping && jumpTimeCounter > 0)
        {
            jumpInputHeld = true;
        }

        if (Input.GetKeyUp(KeyCode.Space) || jumpTimeCounter <= 0)
        {
            jumpInputHeld = false;
            isJumping = false;
        }

        if (isTouchingWall && !isGrounded && !isJumping && rb.velocity.y < 0)
        {
            rb.velocity = new Vector2(0, -4.0f);
            isGrabbing = true;
            animator.SetBool("grab", true);
            transform.localScale = new Vector3(transform.localScale.x * -1, 1, 1);
            if ((isTouchingWall && moveInput < 0 && rb.velocity.x >= 0) || (isTouchingWall && moveInput > 0 && rb.velocity.x <= 0))
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    animator.SetBool("wallJump", true);
                    audio.clip = jump;
                    audio.Play();
                    isWallJumping = true;
                    wallJumpDirection = moveInput > 0 ? -1 : 1;
                    transform.localScale = new Vector3(wallJumpDirection, 1, 1);
                    rb.velocity = new Vector2(wallJumpDirection * wallJumpForce, wallJumpVerticalForce);
                    StartCoroutine(ResetWallJump());
                }
            }
        }
        else
        {
            isGrabbing = false;
            animator.SetBool("grab", false);
            animator.SetBool("wallJump", false);
        }

        HandleAttackInput();

        if (!isGrounded && rb.velocity.y < 0)
        {
            Vector2 boxSize = new Vector2(GetComponent<BoxCollider2D>().size.x, stompRaycastLength);

            RaycastHit2D hit = Physics2D.BoxCast(transform.position, boxSize, 0f, Vector2.down, stompRaycastLength, enemyLayer);
            if (hit.collider != null)
            {
                StompEnemy(hit.collider.gameObject, true);
            }
        }

        if (transform.position.y < -12)
        {
            dead = true;
            rb.velocity = Vector2.zero;
            GameObject.FindWithTag("MusicManager").GetComponent<AudioSource>().Stop();
            StartCoroutine(Fall());
        }

        TryPickUpHammer();

        animator.SetBool("onGround", isGrounded);
        animator.SetFloat("horizontal", Mathf.Abs(rb.velocity.x));
        animator.SetFloat("vertical", rb.velocity.y);
        animator.SetBool("skidding", isSkidding);
        animator.SetBool("idleAttack", idleAttack);
        animator.SetBool("idleHammer", idleHammer);
        animator.SetBool("walkAttack", walkAttack);
        animator.SetBool("walkHammer", walkHammer);
        animator.SetBool("runAttack", runAttack);
        animator.SetBool("runHammer", runHammer);
    }

    private void FixedUpdate()
    {
        isGrounded = CheckGrounded();
        isTouchingWall = CheckWall(moveInput, isGrounded);

        if (canHitBlock && rb.velocity.y >= 0 && (!isGrounded || Input.GetKey(KeyCode.Space)))
        {
            RaycastHit2D[] hits = Physics2D.BoxCastAll(transform.position, new Vector2(GetComponent<BoxCollider2D>().size.x, 0.25f), 0f, Vector2.up, 1.1f, groundLayer);
            if (hits.Length != 0)
            {
                isJumping = false;
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider.TryGetComponent<Block>(out Block block))
                    {
                        canHitBlock = false;
                        StartCoroutine(ResetBlockHit());
                        hit.collider.gameObject.GetComponent<Block>().Hit();
                    }
                }
            }
        }

        if (!canMove || isWallJumping || dead || isWallJumping) {
            return;
        }

        if (idleAttack || idleHammer || walkAttack || walkHammer || runAttack || runHammer || isInventoryOpen)
        {
            // rb.velocity = Vector2.zero;
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }

        if (isTouchingWall)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            oneTimeSprint = true;
            isSkidding = false;
        }
        else if (moveInput != 0)
        {
            float targetSpeed = moveInput * (isAccelerating ? maxSpeed : walkSpeed);
            float speedDifference = targetSpeed - rb.velocity.x;
            if (Mathf.Abs(speedDifference) > skidThreshold && Mathf.Sign(speedDifference) != Mathf.Sign(rb.velocity.x))
            {
                rb.velocity = new Vector2(rb.velocity.x * skidFactor, rb.velocity.y);
                isSkidding = Mathf.Sign(rb.velocity.x) != Mathf.Sign(targetSpeed);
                oneTimeSprint = true;
            }
            else
            {
                isSkidding = false;
                oneTimeSprint = true;
            }

            if (Mathf.Abs(rb.velocity.x) < walkSpeed && !isSkidding && oneTimeSprint)
            {
                rb.velocity = new Vector2(moveInput * walkSpeed, rb.velocity.y);
                oneTimeSprint = false;
            }

            float accelRate = (Mathf.Abs(speedDifference) > skidThreshold) ? deceleration : acceleration;
            rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, targetSpeed, accelRate * Time.fixedDeltaTime), rb.velocity.y);
        }
        else
        {
            isSkidding = false;
            oneTimeSprint = true;
            if (Mathf.Abs(rb.velocity.x) > walkSpeed)
            {
                rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, 0, deceleration * Time.fixedDeltaTime), rb.velocity.y);
            }
            else
            {
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
        }

        if (jumpStarted)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpStarted = false;
        }

        if (isJumping && jumpInputHeld)
        {
            float deltaTime = Time.fixedDeltaTime;
            jumpTimeCounter -= deltaTime;

            if (jumpTimeCounter > 0)
            {
                float speedFactor = Mathf.Clamp01(Mathf.Abs(rb.velocity.x) / maxSpeed);
                float adjustedHoldJumpForce = holdJumpForce + (holdJumpForce * speedFactor * 0.3f);
                float appliedForce = adjustedHoldJumpForce * (deltaTime / maxHoldJumpTime);
                rb.AddForce(new Vector2(0f, appliedForce), ForceMode2D.Force);
            }
            else
            {
                isJumping = false;
            }
        }
    }

    public bool CheckGrounded()
    {
        // Check for ground or platform using BoxCast
        RaycastHit2D hit = Physics2D.BoxCast(transform.position, GetComponent<BoxCollider2D>().size, 0, Vector2.down, groundCheckDistance, groundLayer | platformLayer);

        // If the player is a child of a platform (like the donut block), consider it grounded
        if (transform.parent != null)
        {
            return true;
        }

        // Additional logic to ignore the ground check if jumping or moving vertically while hitting a platform
        if (hit.collider != null)
        {
            if (hit.collider.gameObject.layer == 6 && (isJumping || rb.velocity.y != 0))
            {
                return false;
            }
        }

        // Return true if the player is grounded
        return hit.collider != null;
    }


    private bool CheckWall(float moveInput, bool grounded)
    {
        if (moveInput == 0 && grounded)
        {
            return false;
        }
        else
        {
            Vector2 direction = (moveInput > 0 ? Vector2.right : Vector2.left);
            // Vector2 position = (Vector2)transform.position + (grounded ? Vector2.up * 0.25f : Vector2.down * 1.0f);
            Vector2 position = (Vector2)transform.position;
            Vector2 boxSize = new Vector2(GetComponent<BoxCollider2D>().size.x, 0.1f);

            RaycastHit2D hit = Physics2D.BoxCast(position, boxSize, 0, (isSkidding ? -1f : 1f) * direction, wallCheckDistance, groundLayer);
            return hit.collider != null && Input.GetAxis("Horizontal") != 0;
        }
    }

    public int GetHealth() {
        return health;
    }

    public int GetCoins()
    {
        return coins;
    }

    public int GetScore() {
        return score;
    }

    public int GetItem(string item) {
        return items[item];
    }

    public Dictionary<string, int> GetItems() {
        return items;
    }

    public void SetCoins(int newCoins)
    {
        coins = newCoins;
    }

    public void SetHealth(int newHealth) {
        if(newHealth > 100) {
            health = 100;
        } else if(newHealth <= 0) {
            health = 0;
        } else {
            health = newHealth;
        }
    }

    public void SetScore(int newScore) {
        score = newScore;
    }

    public void SetItem(string item, int value) {
        items[item] = value;
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible || GetComponent<Inventory>().GetIsInvincible()) return;
        GetComponent<Inventory>().CloseInventory();
        SetHealth(health - damage);
    }

    public void ApplyKnockback(Vector2 force)
    {
        if (isInvincible) return;
        rb.velocity = Vector2.zero;
        rb.AddForce(force, ForceMode2D.Impulse);
        StartCoroutine(InvincibilityCoroutine());
        StartCoroutine(DisableMovementBriefly());
    }

    private void StompEnemy(GameObject enemy, bool jumpStomp)
    {
        if(enemy.GetComponent<Enemy>().canStomp) {
            enemy.GetComponent<Enemy>().Stomp(jumpStomp);
            rb.velocity = new Vector2(rb.velocity.x, bounceForce);
            if (Input.GetKey(KeyCode.Space))
            {
                isJumping = true;
                jumpTimeCounter = maxHoldJumpTime;
            }
        }
    }

    private void HandleAttackInput()
    {
        if (!isGrounded) return;

        string attackType = null;

        float resetTime = 0f;
        float registerTime = 0f;
        float combatTime = 0f;
        float rangeAttackSpawnTime = 0f;
        float applyKnockbackDelay = 0f;
        float boxcastWidth = 0.2f;

        bool stompAttack = false;
        bool rangeAttack = false;
        bool meleeAttack = false;
        bool hammerStomp = false;

        int damage = power;

        Vector2 knockback = new Vector2(0f, 0f);

        if (Mathf.Abs(rb.velocity.x) < 0.1f)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                attackType = "idleAttack";
                resetTime = 1.2f;
                combatTime = resetTime;
                knockback = new Vector2(5f, 10f);
                applyKnockbackDelay = 1f;
                damage = power;
                meleeAttack = true;
                boxcastWidth = -0.2f;
            }
            else if (Input.GetKeyDown(KeyCode.D) && hasHammer)
            {
                attackType = "idleHammer";
                resetTime = 1.25f;
                registerTime = 26f / 60f;
                combatTime = 0.15f;
                stompAttack = true;
                hammerStomp = true;
            }
        }
        else if (Mathf.Abs(rb.velocity.x) >= maxSpeed - 0.5f)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                attackType = "runAttack";
                resetTime = 0.5f;
                registerTime = 0.1f;
                combatTime = 0.1f;
                knockback = new Vector2(25f, 10f);
                damage = power * 2;
                meleeAttack = true;
            }
            else if (Input.GetKeyDown(KeyCode.D) && hasHammer)
            {
                attackType = "runHammer";
                resetTime = 0.5f;
                registerTime = 0.2f;
                combatTime = 0.1f;
                knockback = new Vector2(25f, 25f);
                damage = power * 2;
                meleeAttack = true;
            }
        }
        else if (Mathf.Abs(rb.velocity.x) > 0.1f)
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                attackType = "walkAttack";
                resetTime = 0.6f;
                registerTime = 0.15f;
                combatTime = 0.15f;
                knockback = new Vector2(5f, 10f);
                meleeAttack = true;
            }
            else if (Input.GetKeyDown(KeyCode.D) && hasHammer)
            {
                attackType = "walkHammer";
                resetTime = 0.2f;
                combatTime = resetTime;
                rangeAttack = true;
                rangeAttackSpawnTime = 0.4f;
            }
        }

        if (attackType != null)
        {
            StartCoroutine(PerformAttack(attackType, resetTime, registerTime, combatTime, applyKnockbackDelay, rangeAttackSpawnTime, boxcastWidth, meleeAttack, stompAttack, rangeAttack, hammerStomp, damage, knockback));
        }
    }

    private IEnumerator PerformAttack(string attackType, float resetTime, float registerTime, float combatTime, float applyKnockbackDelay, float rangeAttackSpawnTime, float boxcastWidth, bool meleeAttack, bool stompAttack, bool rangeAttack, bool hammerStomp, int damage, Vector2 knockback)
    {
        switch (attackType)
        {
            case "idleAttack":
                idleAttack = true;
                break;
            case "idleHammer":
                idleHammer = true;
                break;
            case "walkAttack":
                walkAttack = true;
                break;
            case "walkHammer":
                walkHammer = true;
                break;
            case "runAttack":
                runAttack = true;
                break;
            case "runHammer":
                runHammer = true;
                break;
        }

        float elapsedTime = 0f;
        bool oneTime = true;

        yield return new WaitForSeconds(registerTime);

        while (elapsedTime < combatTime)
        {
            if(!canMove) {
                break;
            }

            if(rangeAttack) {
                if(attackType == "walkHammer") {
                    if(oneTime) {
                        oneTime = false;
                        yield return new WaitForSeconds(rangeAttackSpawnTime);
                        if(!canMove) {
                            break;
                        }
                        hasHammer = false;
                        Vector3 spawnPosition = transform.position + new Vector3(1f * transform.localScale.x, 0.75f, 0f);
                        Instantiate(hammer, spawnPosition, Quaternion.identity);
                    }
                }
            } else {
                float direction = Mathf.Sign(transform.localScale.x);
                Vector2 boxSize = new Vector2(attackBoxWidth + boxcastWidth, GetComponent<Collider2D>().bounds.size.y);
                Vector2 boxOrigin = (Vector2)transform.position + new Vector2(direction * (boxSize.x / 2f + 0.1f), 0);

                RaycastHit2D hit = Physics2D.BoxCast(boxOrigin, boxSize, 0f, Vector2.right * direction, 0f, enemyLayer);

                if(attackType == "idleHammer") {
                    RaycastHit2D[] brickHits = Physics2D.BoxCastAll((Vector2)transform.position + new Vector2(direction * (boxSize.x / 2f + 0.1f), 0.25f), boxSize, 0f, Vector2.right * direction, 0f, groundLayer);
                    if(brickHits.Length > 0) {
                        foreach(RaycastHit2D brick in brickHits) {
                            if(brick.collider.gameObject.layer == LayerMask.NameToLayer("Ground") && brick.collider.TryGetComponent<Block>(out Block block)) {
                                if(brick.collider.gameObject.GetComponent<Block>().canHammerHit) {
                                    brick.collider.gameObject.GetComponent<Block>().Hit();
                                }
                            }
                        }
                    }
                }

                if (hit.collider != null && oneTime)
                {
                    Rigidbody2D enemyRb = hit.collider.GetComponent<Rigidbody2D>();
                    Vector2 enemyVelocity = enemyRb != null ? enemyRb.velocity : Vector2.zero;

                    Vector2 knockbackDirection;

                    knockbackDirection.x = Mathf.Sign(((Vector2)hit.collider.transform.position - boxOrigin).x) * knockback.x;
                    knockbackDirection.y = knockback.y;
                    
                    hit.collider.GetComponent<Enemy>().TakeDamage(damage, knockbackDirection, applyKnockbackDelay - elapsedTime, meleeAttack, stompAttack, hammerStomp);
                    oneTime = false;
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if(canMove) {
            yield return new WaitForSeconds(resetTime - registerTime - combatTime - rangeAttackSpawnTime);
        }

        switch (attackType)
        {
            case "idleAttack":
                idleAttack = false;
                break;
            case "idleHammer":
                idleHammer = false;
                break;
            case "walkAttack":
                walkAttack = false;
                break;
            case "walkHammer":
                walkHammer = false;
                break;
            case "runAttack":
                runAttack = false;
                break;
            case "runHammer":
                runHammer = false;
                break;
        }
    }

    public IEnumerator DisableMovementBriefly()
    {
        audio.clip = hurt;
        audio.Play();
        animator.SetBool("hurt", true);
        canMove = false;
        yield return new WaitForSeconds(0.5f);
        canMove = true;
        animator.SetBool("hurt", false);
        rb.velocity = new Vector2(0, rb.velocity.y);
        if (health == 0)
        {
            while (!CheckGrounded()) {
                yield return null;
            }
            yield return new WaitForSeconds(0.2f);
            rb.velocity = Vector2.zero;
            rb.simulated = false;
            animator.SetBool("onGround", true);
            animator.SetFloat("vertical", 0);
            animator.SetFloat("horizontal", 0);
            dead = true;
            yield return new WaitForSeconds(0.8f);
            animator.SetBool("dead", true);
            audio.clip = deadSound;
            audio.Play();
            yield return new WaitForSeconds(5f);
            Data.lives--;
            if(Data.lives == 0) {
                SceneManager.LoadScene("MainMenu");
            } else {
                SceneManager.LoadScene("1-1");
            }
        }
    }

    public IEnumerator TimeOut() {
        canMove = false;
        rb.velocity = new Vector2(0, rb.velocity.y);
        while (!CheckGrounded()) {
            yield return null;
        }
        yield return new WaitForSeconds(0.2f);
        rb.velocity = Vector2.zero;
        rb.simulated = false;
        animator.SetBool("onGround", true);
        animator.SetFloat("vertical", 0);
        animator.SetFloat("horizontal", 0);
        dead = true;
        yield return new WaitForSeconds(0.8f);
        animator.SetBool("dead", true);
        audio.clip = deadSound;
        audio.Play();
        yield return new WaitForSeconds(5f);
        Data.lives--;
        if(Data.lives == 0) {
            SceneManager.LoadScene("MainMenu");
        } else {
            SceneManager.LoadScene("1-1");
        }
    }

    private IEnumerator InvincibilityCoroutine()
    {

        isInvincible = true;

        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            for (float i = 0; i < invincibilityDuration; i += 0.2f)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
                yield return new WaitForSeconds(0.2f);
            }
            spriteRenderer.enabled = true;
        }
        else
        {
            yield return new WaitForSeconds(invincibilityDuration);
        }
        
        isInvincible = false;
    }

    private IEnumerator ResetWallJump()
    {
        yield return new WaitForSeconds(0.4f);
        isWallJumping = false;
        oneTime = true;
    }

    private IEnumerator Fall()
    {
        audio.clip = fall;
        audio.Play();
        yield return new WaitForSeconds(3f);
        Data.lives--;
        if(Data.lives == 0) {
            SceneManager.LoadScene("MainMenu");
        } else {
            SceneManager.LoadScene("1-1");
        }
    }

    private IEnumerator ResetBlockHit()
    {
        yield return new WaitForSeconds(0.25f);
        canHitBlock = true;
    }

    private void TryPickUpHammer()
    {
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, 1f, Vector2.zero, 0f, hammerLayer);

        if (hit.collider != null)
        {
            PickUpHammer(hit.collider.gameObject);
        }
    }

    private void PickUpHammer(GameObject hammer)
    {
        hasHammer = true;
        Destroy(hammer);
    }

    public void SetIsInvincible(bool status) {
        isInvincible = status;
    }
}