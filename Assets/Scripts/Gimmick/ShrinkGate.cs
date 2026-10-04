using UnityEngine;

public class ShrinkGate : MonoBehaviour
{
    [SerializeField] HoldGateButton holdButton;
    [SerializeField] float slideDistance = 5f;
    [SerializeField] float changeSpeed = 5f;

    Vector3 closedPos;
    Vector3 openPos;

    void Start()
    {
        closedPos = transform.position;
        openPos = closedPos + new Vector3(slideDistance, 0f, 0f);
    }

    void Update()
    {
        if (holdButton == null) return;

        // world座標の横方向（X軸）だけを移動
        float targetX = holdButton.openDoor ? openPos.x : closedPos.x;
        Vector3 currentPos = transform.position;
        currentPos.x = Mathf.MoveTowards(currentPos.x, targetX, changeSpeed * Time.deltaTime);
        transform.position = currentPos;
    }
}