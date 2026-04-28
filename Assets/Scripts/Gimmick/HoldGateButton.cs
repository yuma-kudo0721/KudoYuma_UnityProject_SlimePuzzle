using UnityEngine;

public class HoldGateButton : MonoBehaviour
{
    SpriteRenderer sp;
    int pressCount = 0;
    bool isLocked = false;

    public Sprite normalSprite;
    public Sprite pushedSprite;

    public bool openDoor = false;

    void Start()
    {
        sp = GetComponent<SpriteRenderer>();

        if (sp != null && normalSprite != null)
        {
            sp.sprite = normalSprite;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isLocked) return;

        pressCount++;
        UpdateState();
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (isLocked) return;

        pressCount = Mathf.Max(0, pressCount - 1);
        UpdateState();
    }

    void UpdateState()
    {
        openDoor = pressCount > 0;

        if (sp == null) return;

        if (openDoor && pushedSprite != null)
        {
            sp.sprite = pushedSprite;
        }
        else if (!openDoor && normalSprite != null)
        {
            sp.sprite = normalSprite;
        }
    }

    public void LockPressed()
    {
        isLocked = true;
        openDoor = true;

        if (sp != null && pushedSprite != null)
        {
            sp.sprite = pushedSprite;
        }
    }
}
