using UnityEngine;

public class ButtonDoor : MonoBehaviour
{
    [SerializeField] HoldGateButton[] buttons;
    [SerializeField] int requiredPressCount = 2;
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float openHeight = 3f;

    Vector3 closedPos;
    Vector3 openPos;
    bool isUnlocked = false;

    void Start()
    {
        closedPos = transform.position;
        openPos = closedPos + Vector3.up * openHeight;
    }

    void Update()
    {
        if (!isUnlocked && buttons != null && buttons.Length >= requiredPressCount)
        {
            int pressedCount = 0;
            foreach (var button in buttons)
            {
                if (button != null && button.openDoor)
                {
                    pressedCount++;
                }
            }

            if (pressedCount >= requiredPressCount)
            {
                isUnlocked = true;
                foreach (var button in buttons)
                {
                    if (button != null)
                    {
                        button.LockPressed();
                    }
                }
            }
        }

        Vector3 targetPos = isUnlocked ? openPos : closedPos;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
    }
}