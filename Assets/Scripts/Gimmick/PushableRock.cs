using System.Collections;
using UnityEngine;

public class PushableRock : MonoBehaviour
{
    [SerializeField] float requiredSlimeSize = 1f;

    [Header("Unlocked Rock")]
    [SerializeField] float unlockedMass = 8f;
    [SerializeField] float unlockedDrag = 0.8f;
    [SerializeField] float unlockedAngularDrag = 0.8f;
    [SerializeField] float maxRollSpeed = 3.5f;
    [SerializeField] float maxFallSpeed = 12f;

    [Header("Locked Rock")]
    [SerializeField] float lockedMass = 20f;
    [SerializeField] float lockedDrag = 2.5f;
    [SerializeField] float lockedAngularDrag = 2.5f;

    [Header("Landing")]
    [SerializeField] float landingImpactSpeed = 5f;
    [SerializeField] float landingBounceVelocity = 0.2f;

    [Header("Respawn")]
    [SerializeField] Transform respawnPoint;
    [SerializeField] float respawnDelay = 2f;

    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    Collider2D[] colliders;

    bool isUnlocked = false;
    bool isRespawning = false;
    bool isPulledByTether = false;

    Vector3 startPosition;
    float previousYVelocity;

    [Header("Camera Area Respawn")]
    [SerializeField] Camera targetCamera;
    [SerializeField] Transform playerTarget;
    [SerializeField] float outsideMargin = 0.1f;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        colliders = GetComponents<Collider2D>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        startPosition = transform.position;

        ApplyLockedState(true);
    }

    void Update()
    {
        if (isRespawning) return;

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (playerTarget == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerTarget = player.transform;
            }
        }

        if (targetCamera == null || playerTarget == null) return;

        Vector3 viewPos = targetCamera.WorldToViewportPoint(playerTarget.position);

        bool isOutside =
            viewPos.x < -outsideMargin ||
            viewPos.x > 1f + outsideMargin ||
            viewPos.y < -outsideMargin ||
            viewPos.y > 1f + outsideMargin;

        if (isOutside)
        {
            TryRespawn();
        }
    }


    void FixedUpdate()
    {
        previousYVelocity = rb.velocity.y;

        if (rb.velocity.y < -maxFallSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, -maxFallSpeed);
        }

        if (!isUnlocked) return;

        float clampedX = Mathf.Clamp(rb.velocity.x, -maxRollSpeed, maxRollSpeed);
        rb.velocity = new Vector2(clampedX, rb.velocity.y);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (isRespawning) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        Player player = collision.gameObject.GetComponent<Player>();
        if (player == null) return;

        bool hasEnoughSize = player.slimeSize >= requiredSlimeSize;

        if (hasEnoughSize)
        {
            ApplyUnlockedState();
        }
        else
        {
            ApplyLockedState();
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (isRespawning) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        ApplyLockedState();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Needle"))
        {
            TryRespawn();
            return;
        }

        if (IsLandingCollision(collision) && previousYVelocity <= -landingImpactSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Min(rb.velocity.y, landingBounceVelocity));
            rb.angularVelocity *= 0.5f;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Needle"))
        {
            TryRespawn();
        }
    }

    public void RequestRespawn()
    {
        TryRespawn();
    }

    void TryRespawn()
    {
        if (isRespawning) return;

        StartCoroutine(RespawnAfterDelay());
    }

    IEnumerator RespawnAfterDelay()
    {
        isRespawning = true;

        HideRock();

        yield return new WaitForSeconds(respawnDelay);

        Respawn();

        ShowRock();

        isRespawning = false;
    }

    void HideRock()
    {
        spriteRenderer.enabled = false;

        foreach (Collider2D col in colliders)
        {
            col.enabled = false;
        }

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.simulated = false;
    }

    void ShowRock()
    {
        transform.position = respawnPoint != null ? respawnPoint.position : startPosition;

        rb.simulated = true;
        spriteRenderer.enabled = true;

        foreach (Collider2D col in colliders)
        {
            col.enabled = true;
        }
    }

    void Respawn()
    {
        Vector3 target = respawnPoint != null ? respawnPoint.position : startPosition;

        transform.position = target;
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        isPulledByTether = false;

        ApplyLockedState(true);
    }

    public void SetPulledByTether(bool value)
    {
        isPulledByTether = value;

        if (isPulledByTether)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            return;
        }

        if (isUnlocked)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        else
        {
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        }
    }

    void ApplyLockedState(bool force = false)
    {
        if (!force && !isUnlocked) return;

        isUnlocked = false;
        rb.mass = lockedMass;
        rb.drag = lockedDrag;
        rb.angularDrag = lockedAngularDrag;
        rb.constraints = isPulledByTether ? RigidbodyConstraints2D.None : RigidbodyConstraints2D.FreezePositionX;
    }

    void ApplyUnlockedState()
    {
        if (isUnlocked) return;

        isUnlocked = true;
        rb.mass = unlockedMass;
        rb.drag = unlockedDrag;
        rb.angularDrag = unlockedAngularDrag;
        rb.constraints = RigidbodyConstraints2D.None;
    }

    bool IsLandingCollision(Collision2D collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y > 0.5f)
            {
                return true;
            }
        }

        return false;
    }
}
