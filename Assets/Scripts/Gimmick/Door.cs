using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] GateButton button;
    [SerializeField] HoldGateButton holdButton;

    [SerializeField] UpDawnLift upDawnLift;
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float openHeight = 3f;
    [SerializeField] SpriteRenderer[] wallRenderers;
    [SerializeField] Collider2D wallCollider;
    [SerializeField, Range(0f, 1f)] float unlockedAlpha = 0.35f;
    [SerializeField] float fadeSpeed = 2f;

    Vector3 closedPos;
    Vector3 openPos;
    bool keyUnlocked;

    public bool IsKeyFadeComplete
    {
        get
        {
            if (!keyUnlocked || wallRenderers == null) return true;
            foreach (SpriteRenderer renderer in wallRenderers)
            {
                if (renderer != null && renderer.color.a > unlockedAlpha + 0.001f)
                    return false;
            }
            return true;
        }
    }

    public void UnlockWithKey()
    {
        keyUnlocked = true;
        if (wallCollider != null) wallCollider.enabled = false;
    }

    void Start()
    {
        if (wallRenderers == null || wallRenderers.Length == 0)
            wallRenderers = GetComponentsInChildren<SpriteRenderer>();
        if (wallCollider == null) wallCollider = GetComponent<Collider2D>();
        closedPos = transform.position;
        openPos = closedPos + Vector3.up * openHeight;
    }

    void Update()
    {
        bool shouldOpen =
            (button != null && button.openDoor) ||
            (holdButton != null && holdButton.openDoor) ||
            (upDawnLift != null && upDawnLift.openDoor);

        Vector3 targetPos = shouldOpen ? openPos : closedPos;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );

        // 鍵で解錠したときはドアを動かさず、壁の透明度を下げる。
        if (keyUnlocked && wallRenderers != null)
        {
            foreach (SpriteRenderer renderer in wallRenderers)
            {
                if (renderer == null) continue;
                Color color = renderer.color;
                color.a = Mathf.MoveTowards(color.a, unlockedAlpha, fadeSpeed * Time.deltaTime);
                renderer.color = color;
            }
        }
    }
}
