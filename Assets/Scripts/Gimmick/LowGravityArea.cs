using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LowGravityArea : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] private float gravityMultiplier = 0.3f;

    private Rigidbody2D playerRb;
    private float originalGravityScale;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerRb = other.attachedRigidbody;

        if (playerRb == null)
        {
            return;
        }

        originalGravityScale = playerRb.gravityScale;
        playerRb.gravityScale = originalGravityScale * gravityMultiplier;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || playerRb == null)
        {
            return;
        }

        playerRb.gravityScale = originalGravityScale;
        playerRb = null;
    }
}