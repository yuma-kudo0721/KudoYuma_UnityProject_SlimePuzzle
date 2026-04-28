using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField] GateButton button;
    [SerializeField] HoldGateButton holdButton;

    [SerializeField] UpDawnLift upDawnLift;
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float openHeight = 3f;

    Vector3 closedPos;
    Vector3 openPos;

    void Start()
    {
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
    }
}
