using UnityEngine;

public class DownReleaseTrampoline : MonoBehaviour
{
    [Header("Launch")]
    [SerializeField] float minLaunchPower = 15f;
    [SerializeField] float maxLaunchPower = 45f;
    [SerializeField] float maxChargeTime = 1.5f;

    [Header("Squash")]
    [SerializeField] Transform squashTarget;
    [SerializeField] Vector3 pressedScale = new Vector3(1.25f, 0.55f, 1f);
    [SerializeField] float squashSpeed = 12f;

    Rigidbody2D playerRb;
    Vector3 normalScale;
    Vector3 normalLocalPosition;

    float chargeTime;
    bool wasHoldingDown;
    bool isPressed;

    void Awake()
    {
        if (squashTarget == null)
        {
            squashTarget = transform;
        }

        normalScale = squashTarget.localScale;
        normalLocalPosition = squashTarget.localPosition;
    }

    void Update()
    {
        if (playerRb != null)
        {
            bool holdingDown =
                Input.GetKey(KeyCode.DownArrow) ||
                Input.GetKey(KeyCode.S);

            isPressed = holdingDown;

            if (holdingDown)
            {
                wasHoldingDown = true;
                chargeTime += Time.deltaTime;
                chargeTime = Mathf.Min(chargeTime, maxChargeTime);
            }

            bool releasedDown =
                Input.GetKeyUp(KeyCode.DownArrow) ||
                Input.GetKeyUp(KeyCode.S);

            if (wasHoldingDown && releasedDown)
            {
                LaunchPlayer();
                wasHoldingDown = false;
                isPressed = false;
                chargeTime = 0f;
            }
        }
        else
        {
            isPressed = false;
            wasHoldingDown = false;
            chargeTime = 0f;
        }

        UpdateSquash();
    }

    void UpdateSquash()
    {
        Vector3 targetScale = isPressed ? pressedScale : normalScale;

        float heightLost = normalScale.y - targetScale.y;
        Vector3 targetPosition = normalLocalPosition + new Vector3(0f, -heightLost * 1f, 0f);

        squashTarget.localScale = Vector3.Lerp(
            squashTarget.localScale,
            targetScale,
            Time.deltaTime * squashSpeed
        );

        squashTarget.localPosition = Vector3.Lerp(
            squashTarget.localPosition,
            targetPosition,
            Time.deltaTime * squashSpeed
        );
    }

    void LaunchPlayer()
    {
        float chargeRate = chargeTime / maxChargeTime;
        float power = Mathf.Lerp(minLaunchPower, maxLaunchPower, chargeRate);

        playerRb.velocity = new Vector2(playerRb.velocity.x, power);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerRb = other.GetComponent<Rigidbody2D>();
        wasHoldingDown = false;
        chargeTime = 0f;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerRb = null;
        wasHoldingDown = false;
        isPressed = false;
        chargeTime = 0f;
    }
}