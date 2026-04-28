using UnityEngine;

public class DoubleButtonDoor : MonoBehaviour
{
    [SerializeField] HoldGateButton buttonA;
    [SerializeField] HoldGateButton buttonB;
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
        if (!isUnlocked && buttonA != null && buttonB != null)
        {
            if (buttonA.openDoor && buttonB.openDoor)
            {
                isUnlocked = true;
                buttonA.LockPressed();
                buttonB.LockPressed();
            }
        }

        Vector3 targetPos = isUnlocked ? openPos : closedPos;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            moveSpeed * Time.deltaTime
        );
    }
}
