using UnityEngine;

public class SawRouteButton : MonoBehaviour
{
    [Header("Sprite")]
    [SerializeField] Sprite normalSprite;
    [SerializeField] Sprite pushedSprite;

    SpriteRenderer spriteRenderer;

    public bool IsOn { get; private set; }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ResetButton();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        IsOn = true;

        if (spriteRenderer != null && pushedSprite != null)
        {
            spriteRenderer.sprite = pushedSprite;
        }
    }

    public void ResetButton()
    {
        IsOn = false;

        if (spriteRenderer != null && normalSprite != null)
        {
            spriteRenderer.sprite = normalSprite;
        }
    }
}