using UnityEngine;

public class PushableRock : MonoBehaviour
{
    [SerializeField] float requiredSlimeSize = 2f;
    [SerializeField] bool requireFullyMerged = true;

    [Header("Unlocked Rock")]
    [SerializeField] float unlockedMass = 8f;
    [SerializeField] float unlockedDrag = 0.8f;
    [SerializeField] float unlockedAngularDrag = 0.8f;
    [SerializeField] float maxRollSpeed = 3.5f;

    [Header("Locked Rock")]
    [SerializeField] float lockedMass = 20f;
    [SerializeField] float lockedDrag = 2.5f;
    [SerializeField] float lockedAngularDrag = 2.5f;

    Rigidbody2D rb;
    bool isUnlocked = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        ApplyLockedState();
    }

    void FixedUpdate()
    {
        if (!isUnlocked) return;

        float clampedX = Mathf.Clamp(rb.velocity.x, -maxRollSpeed, maxRollSpeed);
        rb.velocity = new Vector2(clampedX, rb.velocity.y);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Player")) return;

        Player player = collision.gameObject.GetComponent<Player>();
        if (player == null) return;

        bool isMerged = player.slimeCount.Count == 1;
        bool hasEnoughSize = player.slimeSize >= requiredSlimeSize;

        if ((!requireFullyMerged || isMerged) && hasEnoughSize)
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
        if (!collision.gameObject.CompareTag("Player")) return;
        ApplyLockedState();
    }

    void ApplyLockedState()
    {
        if (!isUnlocked) return;

        isUnlocked = false;
        rb.mass = lockedMass;
        rb.drag = lockedDrag;
        rb.angularDrag = lockedAngularDrag;
        rb.constraints = RigidbodyConstraints2D.FreezePositionX;
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
}
