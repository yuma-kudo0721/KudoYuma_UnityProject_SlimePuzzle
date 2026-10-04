using UnityEngine;

public class WallCrawler2D : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] float speed = 2f;
    [SerializeField] bool loop = true;
    [SerializeField] float arriveDistance = 0.05f;

    [Header("Saw Rotation")]
    [SerializeField] Transform rotateTarget;
    [SerializeField] float rotateSpeed = 360f;

    [Header("Route")]
    [SerializeField] Transform[] normalRoute;
    [SerializeField] Transform[] switchedRoute;

    [Header("Saw Route Switch")]
    [SerializeField] SawRouteButton sawRouteButton;

    [Header("Player")]
    [SerializeField] Player player;

    [Header("Damage")]
    [SerializeField] bool damagePlayer = true;
    [SerializeField] Transform playerRespawnPoint;

    Transform[] currentRoute;
    bool usingSwitchedRoute;
    bool hasRouteChangeRequest;
    bool requestedSwitchState;
    int targetIndex;

    Vector3 initialPosition;
    Quaternion initialRotation;

    void Start()
    {
        if (rotateTarget == null)
        {
            rotateTarget = transform;
        }

        initialPosition = transform.position;
        initialRotation = rotateTarget.rotation;

        if (player != null)
        {
            player.Respawned += ResetSaw;
        }

        ResetSaw();
    }

    void Update()
    {
        RotateSaw();
        CheckSwitchRequest();
        MoveRoute();
    }

    void RotateSaw()
    {
        if (rotateTarget == null) return;

        rotateTarget.Rotate(0f, 0f, rotateSpeed * Time.deltaTime * 2f);
    }

    void ResetSaw()
    {
        // のこぎり専用ボタンだけをOFFにする。
        // GateButton（ゲート）は変更しない。
        if (sawRouteButton != null)
        {
            sawRouteButton.ResetButton();
        }

        currentRoute = normalRoute;
        usingSwitchedRoute = false;
        hasRouteChangeRequest = false;
        requestedSwitchState = false;

        if (normalRoute != null &&
            normalRoute.Length > 0 &&
            normalRoute[0] != null)
        {
            transform.position = normalRoute[0].position;
            targetIndex = normalRoute.Length > 1 ? 1 : 0;
        }
        else
        {
            transform.position = initialPosition;
            targetIndex = 0;
        }

        if (rotateTarget != null)
        {
            rotateTarget.rotation = initialRotation;
        }
    }

    void CheckSwitchRequest()
    {
        bool switchOn = IsSwitchOn();

        if (switchOn == usingSwitchedRoute) return;

        hasRouteChangeRequest = true;
        requestedSwitchState = switchOn;
    }

    bool IsSwitchOn()
    {
        return sawRouteButton != null && sawRouteButton.IsOn;
    }

    void MoveRoute()
    {
        if (currentRoute == null || currentRoute.Length == 0) return;

        Transform target = currentRoute[targetIndex];
        if (target == null) return;

        transform.position = Vector2.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, target.position) <= arriveDistance)
        {
            transform.position = target.position;

            bool arrivedRouteStart = targetIndex == 0;

            if (arrivedRouteStart && hasRouteChangeRequest)
            {
                ApplyRequestedRoute();
                return;
            }

            targetIndex++;

            if (targetIndex >= currentRoute.Length)
            {
                targetIndex = loop ? 0 : currentRoute.Length - 1;
            }
        }
    }

    void ApplyRequestedRoute()
    {
        usingSwitchedRoute = requestedSwitchState;
        currentRoute = usingSwitchedRoute ? switchedRoute : normalRoute;
        hasRouteChangeRequest = false;

        if (currentRoute == null || currentRoute.Length == 0)
        {
            targetIndex = 0;
            return;
        }

        if (currentRoute[0] != null)
        {
            transform.position = currentRoute[0].position;
        }

        targetIndex = currentRoute.Length > 1 ? 1 : 0;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        HitPlayer(other);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HitPlayer(collision.collider);
    }

    void HitPlayer(Collider2D other)
    {
        if (!damagePlayer) return;
        if (!other.CompareTag("Player")) return;

        Player hitPlayer = other.GetComponent<Player>();
        if (hitPlayer == null) return;

        if (playerRespawnPoint != null)
        {
            hitPlayer.RespawnAt(playerRespawnPoint.position);
        }
        else
        {
            hitPlayer.RespawnAt(hitPlayer.initialPosition);
        }
    }

    void OnDestroy()
    {
        if (player != null)
        {
            player.Respawned -= ResetSaw;
        }
    }
}