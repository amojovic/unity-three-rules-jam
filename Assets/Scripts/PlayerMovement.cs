using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{
    [Header("Mode Z - Roll & Break")]
    public float rollSpeed = 8f;
    public float jumpForce = 10f;
    public float breakForce = 5f;
    private bool isGroundedZ;
    public Transform groundCheckZ;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public PhysicsMaterial2D noFrictionMaterial;
    private PhysicsMaterial2D defaultMaterial;

    [Header("Mode X - Climb")]
    public float climbSpeed = 4f;
    private bool isTouchingClimbWall;
    private bool isClimbing;

    [Header("Mode X - Animation (Visual2)")]
    public Transform visual2Transform;
    private SpriteRenderer visual2Renderer;
    public Sprite sprite1;
    public Sprite sprite2;
    public float animationSpeed = 0.5f;
    private float animationTimer = 0f;
    private bool useSprite1 = true;

    [Header("Mode C - Float")]
    public float floatSpeed = 3f;
    public float floatRiseSpeed = 2f;
    private float floatTimer = 0f;
    private float cooldownTimer = 0f;
    private bool isFloating;

    [Header("Mode C - Animation (Visual3)")]
    public Transform visual3Transform;
    private SpriteRenderer visual3Renderer;
    public Sprite sprite3;
    public Sprite sprite4;
    public float animationSpeedVisual3 = 0.5f;
    private float animationTimerVisual3 = 0f;
    private bool useSprite3 = true;

    [Header("Mode C - Timer Sprite")]
    public GameObject timerSprite;
    public Sprite number0;
    public Sprite number1;
    public Sprite number2;
    public Sprite number3;
    public Sprite number4;
    public Sprite number5;
    public Sprite number6;
    public Sprite number7;
    public Sprite number8;
    public Sprite number9;
    public float timerOffsetY = 0.5f;
    private Dictionary<int, Sprite> numberSpriteMap;
    private SpriteRenderer timerSpriteRenderer;

    [Header("Audio")]
    public AudioSource movementAudioSource;
    public AudioSource switchAudioSource;
    public AudioClip moveSoundZ;
    public float moveSoundZVolume = 0.7f;
    public AudioClip jumpSoundZ;
    public float jumpSoundZVolume = 1.0f;
    public AudioClip moveSoundX;
    public float moveSoundXVolume = 0.7f;
    public AudioClip moveSoundC;
    public float moveSoundCVolume = 0.7f;
    public AudioClip switchToZSound;
    public float switchToZSoundVolume = 1.0f;
    public AudioClip switchToXSound;
    public float switchToXSoundVolume = 1.0f;
    public AudioClip switchToCSound;
    public float switchToCSoundVolume = 1.0f;
    public AudioClip deathSound;
    public float deathSoundVolume = 1.0f;
    public AudioClip breakWallSound;
    public float breakWallSoundVolume = 1.0f;

    [Header("Death Effect")]
    public ParticleSystem deathParticlePrefab;
    public ParticleSystem DestroyParticleEffect;
    private bool isDead;
    private Vector3 startPosition;

    private Rigidbody2D rb;
    private Collider2D playerCollider;
    private char currentMode = 'z';
    private bool isMoving;
    private Collider2D[] ventColliders;

    [Header("Visual (Child Object)")]
    public Transform visual1Transform;
    private SpriteRenderer visual1Renderer;

    [Header("Visual Rotation")]
    public float rotationSpeed = 300f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        startPosition = transform.position;

        if (playerCollider != null) defaultMaterial = playerCollider.sharedMaterial;

        if (visual1Transform != null) visual1Renderer = visual1Transform.GetComponent<SpriteRenderer>();
        if (visual2Transform != null) visual2Renderer = visual2Transform.GetComponent<SpriteRenderer>();
        if (visual3Transform != null) visual3Renderer = visual3Transform.GetComponent<SpriteRenderer>();

        if (timerSprite != null)
        {
            timerSpriteRenderer = timerSprite.GetComponent<SpriteRenderer>();
            if (timerSpriteRenderer != null) timerSprite.SetActive(false);
        }

        numberSpriteMap = new Dictionary<int, Sprite>
        {
            { 0, number0 }, { 1, number1 }, { 2, number2 }, { 3, number3 }, { 4, number4 },
            { 5, number5 }, { 6, number6 }, { 7, number7 }, { 8, number8 }, { 9, number9 }
        };

        if (movementAudioSource != null) movementAudioSource.loop = true;
        if (switchAudioSource != null) switchAudioSource.loop = false;

        GameObject[] vents = GameObject.FindGameObjectsWithTag("Vent");
        ventColliders = new Collider2D[vents.Length];
        for (int i = 0; i < vents.Length; i++)
            ventColliders[i] = vents[i].GetComponent<Collider2D>();

        UpdateVentCollisions(false);
        SwitchToMode('z');
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetScene();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("MainMenu");
            return;
        }

        if (isDead) return;

        HandleModeSwitching();

        switch (currentMode)
        {
            case 'z': ModeZMovement(); break;
            case 'x': ModeXMovement(); break;
            case 'c': ModeCMovement(); break;
        }

        UpdateTimers();
        UpdateTimerSprite();
    }

    void HandleModeSwitching()
    {
        if (Input.GetKeyDown(KeyCode.Z) && currentMode != 'z') SwitchToMode('z');
        else if (Input.GetKeyDown(KeyCode.X) && currentMode != 'x') SwitchToMode('x');
        else if (Input.GetKeyDown(KeyCode.C) && currentMode != 'c' && cooldownTimer <= 0) SwitchToMode('c');
    }

    void SwitchToMode(char newMode)
    {
        currentMode = newMode;
        rb.gravityScale = (newMode == 'c') ? 0f : 1f;
        rb.linearDamping = (newMode == 'c') ? 1f : 0f;

        if (newMode == 'c')
        {
            floatTimer = 4f;
            isFloating = true;
            if (timerSprite != null) timerSprite.SetActive(true);
        }
        else
        {
            isFloating = false;
            if (timerSprite != null) timerSprite.SetActive(false);
        }

        if (newMode == 'z' || newMode == 'x') rb.linearVelocity = Vector2.zero;

        if (playerCollider != null)
            playerCollider.sharedMaterial = (newMode == 'z' && noFrictionMaterial != null) ? noFrictionMaterial : defaultMaterial;

        UpdateVentCollisions(newMode == 'c');

        if (movementAudioSource != null && movementAudioSource.isPlaying)
        {
            movementAudioSource.Stop();
            isMoving = false;
        }

        if (switchAudioSource != null)
        {
            AudioClip switchSound = null;
            float volume = 1.0f;
            switch (newMode)
            {
                case 'z': switchSound = switchToZSound; volume = switchToZSoundVolume; break;
                case 'x': switchSound = switchToXSound; volume = switchToXSoundVolume; break;
                case 'c': switchSound = switchToCSound; volume = switchToCSoundVolume; break;
            }
            if (switchSound != null) switchAudioSource.PlayOneShot(switchSound, volume);
        }

        if (visual1Renderer != null && visual2Renderer != null && visual3Renderer != null)
        {
            if (newMode == 'z')
            {
                visual1Renderer.enabled = true;
                visual2Renderer.enabled = false;
                visual3Renderer.enabled = false;
                visual1Transform.localPosition = Vector3.zero;
                visual2Transform.localPosition = new Vector3(0, 0, -5f);
                visual3Transform.localPosition = new Vector3(0, 0, -5f);
            }
            else if (newMode == 'x')
            {
                visual1Renderer.enabled = false;
                visual2Renderer.enabled = true;
                visual3Renderer.enabled = false;
                visual1Transform.localPosition = new Vector3(0, 0, -5f);
                visual2Transform.localPosition = Vector3.zero;
                visual3Transform.localPosition = new Vector3(0, 0, -5f);
                useSprite1 = true;
                visual2Renderer.sprite = sprite1;
                animationTimer = 0f;
            }
            else
            {
                visual1Renderer.enabled = false;
                visual2Renderer.enabled = false;
                visual3Renderer.enabled = true;
                visual1Transform.localPosition = new Vector3(0, 0, -5f);
                visual2Transform.localPosition = new Vector3(0, 0, -5f);
                visual3Transform.localPosition = Vector3.zero;
                useSprite3 = true;
                visual3Renderer.sprite = sprite3;
                animationTimerVisual3 = 0f;
            }
        }

        if (newMode == 'c' && movementAudioSource != null && moveSoundC != null && !isMoving)
        {
            movementAudioSource.clip = moveSoundC;
            movementAudioSource.volume = moveSoundCVolume;
            movementAudioSource.Play();
            isMoving = true;
        }
    }

    void ModeZMovement()
    {
        isGroundedZ = Physics2D.OverlapCircle(groundCheckZ.position, groundCheckRadius, groundLayer);
        float moveInput = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * rollSpeed, rb.linearVelocity.y);

        if (isGroundedZ && (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            if (switchAudioSource != null && jumpSoundZ != null)
                switchAudioSource.PlayOneShot(jumpSoundZ, jumpSoundZVolume);
        }

        if (visual1Transform != null && Mathf.Abs(moveInput) > 0.01f)
        {
            float direction = -Mathf.Sign(moveInput);
            visual1Transform.Rotate(0, 0, direction * rotationSpeed * Time.deltaTime);
        }

        if (movementAudioSource != null && moveSoundZ != null)
        {
            if (Mathf.Abs(moveInput) > 0.01f)
            {
                if (!isMoving)
                {
                    movementAudioSource.clip = moveSoundZ;
                    movementAudioSource.volume = moveSoundZVolume;
                    movementAudioSource.Play();
                    isMoving = true;
                }
            }
            else if (isMoving)
            {
                movementAudioSource.Stop();
                isMoving = false;
            }
        }
    }

    void ModeXMovement()
    {
        float moveInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        if (Input.GetKey(KeyCode.W)) verticalInput = 1;
        else if (Input.GetKey(KeyCode.S)) verticalInput = -1;

        rb.linearVelocity = new Vector2(moveInput * climbSpeed, rb.linearVelocity.y);

        if (isTouchingClimbWall)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, verticalInput * climbSpeed);
            if (Mathf.Abs(verticalInput) < 0.1f) rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        }

        if (visual2Transform != null && visual2Renderer != null)
        {
            if (Mathf.Abs(moveInput) > 0.01f || Mathf.Abs(verticalInput) > 0.01f)
            {
                animationTimer += Time.deltaTime;
                if (animationTimer >= animationSpeed)
                {
                    useSprite1 = !useSprite1;
                    visual2Renderer.sprite = useSprite1 ? sprite1 : sprite2;
                    animationTimer = 0f;
                }

                float direction = 0f;
                if (Mathf.Abs(moveInput) > 0.01f) direction = -Mathf.Sign(moveInput);
                else if (Mathf.Abs(verticalInput) > 0.01f) direction = Mathf.Sign(verticalInput);
                visual2Transform.Rotate(0, 0, direction * rotationSpeed * Time.deltaTime);
            }
            else
            {
                if (!useSprite1)
                {
                    useSprite1 = true;
                    visual2Renderer.sprite = sprite1;
                    animationTimer = 0f;
                }
            }
        }

        if (movementAudioSource != null && moveSoundX != null)
        {
            if (Mathf.Abs(moveInput) > 0.01f || Mathf.Abs(verticalInput) > 0.01f)
            {
                if (!isMoving)
                {
                    movementAudioSource.clip = moveSoundX;
                    movementAudioSource.volume = moveSoundXVolume;
                    movementAudioSource.Play();
                    isMoving = true;
                }
            }
            else if (isMoving)
            {
                movementAudioSource.Stop();
                isMoving = false;
            }
        }
    }

    void ModeCMovement()
    {
        float moveInput = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * floatSpeed, floatRiseSpeed);

        if (visual3Renderer != null)
        {
            animationTimerVisual3 += Time.deltaTime;
            if (animationTimerVisual3 >= animationSpeedVisual3)
            {
                useSprite3 = !useSprite3;
                visual3Renderer.sprite = useSprite3 ? sprite3 : sprite4;
                animationTimerVisual3 = 0f;
            }
        }
    }

    void UpdateTimers()
    {
        if (currentMode == 'c')
        {
            floatTimer -= Time.deltaTime;
            if (floatTimer <= 0)
            {
                SwitchToMode('z');
                cooldownTimer = 5f;
            }
        }

        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;
    }

    void UpdateTimerSprite()
    {
        if (timerSprite != null && timerSpriteRenderer != null && numberSpriteMap != null && currentMode == 'c' && !isDead)
        {
            int secondsRemaining = Mathf.CeilToInt(floatTimer);
            if (numberSpriteMap.ContainsKey(secondsRemaining))
                timerSpriteRenderer.sprite = numberSpriteMap[secondsRemaining];
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ClimbWall"))
        {
            isTouchingClimbWall = true;
            if (currentMode == 'x') rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        }

        if (currentMode == 'z' && collision.gameObject.CompareTag("BreakableWall"))
        {
            if (collision.relativeVelocity.magnitude > breakForce)
            {
                if (switchAudioSource != null && breakWallSound != null)
                    switchAudioSource.PlayOneShot(breakWallSound, breakWallSoundVolume);

                if (DestroyParticleEffect != null)
                {
                    ParticleSystem effect = Instantiate(DestroyParticleEffect, collision.gameObject.transform.position, Quaternion.identity);
                    effect.Play();
                    Destroy(effect.gameObject, effect.main.duration);
                }
                Destroy(collision.gameObject);
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ClimbWall"))
        {
            isTouchingClimbWall = false;
            if (currentMode == 'x' && isClimbing) isClimbing = false;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Spike"))
        {
            if (currentMode == 'z' || currentMode == 'c') Die();
        }
        if (other.CompareTag("KillBox"))
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        rb.isKinematic = true;

        if (visual1Renderer != null) visual1Renderer.enabled = false;
        if (visual2Renderer != null) visual2Renderer.enabled = false;
        if (visual3Renderer != null) visual3Renderer.enabled = false;

        if (timerSprite != null) timerSprite.SetActive(false);
        if (movementAudioSource != null && movementAudioSource.isPlaying) movementAudioSource.Stop();

        if (switchAudioSource != null && deathSound != null)
            switchAudioSource.PlayOneShot(deathSound, deathSoundVolume);

        if (deathParticlePrefab != null)
        {
            ParticleSystem effect = Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
            effect.Play();
            Destroy(effect.gameObject, effect.main.duration);
        }
    }

    void ResetScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void UpdateVentCollisions(bool ignore)
    {
        if (playerCollider == null || ventColliders == null) return;

        foreach (Collider2D ventCollider in ventColliders)
        {
            if (ventCollider != null)
                Physics2D.IgnoreCollision(playerCollider, ventCollider, ignore);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheckZ != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheckZ.position, groundCheckRadius);
        }
    }
}