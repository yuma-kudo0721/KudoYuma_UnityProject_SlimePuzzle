using System.Collections;
using UnityEngine;

public class FollowKey : MonoBehaviour
{
    [Header("追従")]
    [SerializeField] float followDistance = 0.8f;
    [SerializeField] float followHeight = 0.5f;
    [SerializeField] float followSpeed = 8f;
    [SerializeField] SpriteRenderer playerSprite;

    [Header("ふわふわ動く設定")]
    [SerializeField] float floatAmplitude = 0.12f;
    [SerializeField] float floatSpeed = 2f;

    [Header("鍵穴へ移動する設定")]
    [SerializeField] float heightAlignDuration = 0.5f;
    [SerializeField] float rotateDuration = 0.5f;
    [SerializeField] float moveToSocketDuration = 0.5f;
    [SerializeField] float waitBeforeFade = 0.5f;

    int ownerSlimeIndex;

    Transform player;
    Player ownerPlayer;
    Rigidbody2D body;
    Collider2D keyCollider;
    SpriteRenderer keyRenderer;

    Vector3 idlePosition;
    Quaternion initialRotation;
    Transform initialParent;
    bool initialColliderEnabled;
    bool initialBodySimulated;
    bool initialRendererEnabled;
    bool initialRendererFlipX;
    bool pausedBySwitch;
    float timeScaleBeforeDock = 1f;
    bool dockPausedTime;

    public bool IsCollected { get; private set; }
    public bool IsUsed { get; private set; }

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        keyCollider = GetComponent<Collider2D>();
        keyRenderer = GetComponent<SpriteRenderer>();
        idlePosition = transform.position;
        initialRotation = transform.rotation;
        initialParent = transform.parent;
        initialColliderEnabled = keyCollider == null || keyCollider.enabled;
        initialBodySimulated = body == null || body.simulated;
        initialRendererEnabled = keyRenderer == null || keyRenderer.enabled;
        initialRendererFlipX = keyRenderer != null && keyRenderer.flipX;

        ownerPlayer = FindObjectOfType<Player>();
        if (ownerPlayer != null)
            ownerPlayer.Respawned += ResetKey;
    }

    void OnDestroy()
    {
        if (ownerPlayer != null)
            ownerPlayer.Respawned -= ResetKey;
    }

    void ResetKey()
    {
        // 使用済みの鍵はリスポーンしても復活させない。
        if (IsUsed) return;

        StopAllCoroutines();
        if (dockPausedTime)
        {
            Time.timeScale = timeScaleBeforeDock;
            dockPausedTime = false;
        }

        transform.SetParent(initialParent, true);
        transform.position = idlePosition;
        transform.rotation = initialRotation;

        player = null;
        playerSprite = null;
        ownerSlimeIndex = 0;
        pausedBySwitch = false;
        IsCollected = false;
        IsUsed = false;

        if (body != null)
        {
            body.velocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.simulated = initialBodySimulated;
        }

        if (keyCollider != null)
            keyCollider.enabled = initialColliderEnabled;

        if (keyRenderer != null)
        {
            keyRenderer.enabled = initialRendererEnabled;
            keyRenderer.flipX = initialRendererFlipX;
        }
    }

    void LateUpdate()
    {
        if (IsUsed) return;

        float floatOffset = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;

        // 持ち主から切り替えている間、または拾われる前はその場で揺れる。
        if (pausedBySwitch || player == null || playerSprite == null)
        {
            transform.position = idlePosition + Vector3.up * floatOffset;
            return;
        }

        float direction = playerSprite.flipX ? 1f : -1f;
        Vector3 target = player.position + new Vector3(
            direction * followDistance,
            followHeight + floatOffset,
            0f
        );

        transform.position = Vector3.Lerp(
            transform.position,
            target,
            followSpeed * Time.deltaTime
        );
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsCollected || !other.CompareTag("Player")) return;

        Collect(other.transform.root);
    }

    void Collect(Transform targetPlayer)
    {
        Player playerScript = targetPlayer.GetComponentInParent<Player>();

        if (playerScript != null)
        {
            player = playerScript.transform;
            ownerSlimeIndex = playerScript.ControllSlimeNumber;
            playerSprite = playerScript.GetComponentInChildren<SpriteRenderer>();
        }
        else
        {
            player = targetPlayer;
        }

        IsCollected = true;

        if (body != null)
        {
            body.velocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.simulated = false;
        }

        if (keyCollider != null)
            keyCollider.enabled = false;
    }

    public void OnSlimeSwitch(int currentSlimeIndex)
    {
        if (!IsCollected || IsUsed) return;

        bool switchedAwayFromOwner = currentSlimeIndex != ownerSlimeIndex;

        if (switchedAwayFromOwner && !pausedBySwitch)
        {
            idlePosition = transform.position;
        }

        pausedBySwitch = switchedAwayFromOwner;
    }


    public bool Dock(Transform socket, Door door)
    {
        if (!IsCollected || IsUsed || socket == null) return false;

        IsUsed = true;
        StartCoroutine(DockAnimation(socket, door));
        return true;
    }

    IEnumerator DockAnimation(Transform socket, Door door)
    {
        timeScaleBeforeDock = Time.timeScale;
        dockPausedTime = true;
        Time.timeScale = 0f;

        float elapsed = 0f;
        Vector3 startPosition = transform.position;
        Vector3 heightPosition = new Vector3(
            startPosition.x,
            socket.position.y,
            startPosition.z
        );

        // 先に鍵穴の高さへ移動する。
        while (elapsed < heightAlignDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / heightAlignDuration);
            transform.position = Vector3.Lerp(startPosition, heightPosition, t);
            yield return null;
        }

        transform.position = heightPosition;

        // 高さが合ったら90度回転し、絵を左右反転する。
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation =
            socket.rotation * Quaternion.Euler(0f, 0f, 90f);

        if (keyRenderer != null)
            keyRenderer.flipX = !keyRenderer.flipX;

        elapsed = 0f;
        while (elapsed < rotateDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / rotateDuration);
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            yield return null;
        }

        transform.rotation = endRotation;

        // 回転後に鍵穴の位置まで移動する。
        startPosition = transform.position;
        elapsed = 0f;

        while (elapsed < moveToSocketDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / moveToSocketDuration);
            transform.position = Vector3.Lerp(startPosition, socket.position, t);
            yield return null;
        }

        // 鍵穴に固定する。鍵穴オブジェクトの拡大率は引き継がない。
        transform.SetParent(socket, true);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

        Time.timeScale = timeScaleBeforeDock;
        dockPausedTime = false;

        if (door != null)
        {
            if (waitBeforeFade > 0f)
                yield return new WaitForSeconds(waitBeforeFade);

            // ドアが薄くなり始める前に鍵を消す。
            if (keyRenderer != null)
                keyRenderer.enabled = false;

            door.UnlockWithKey();

            while (!door.IsKeyFadeComplete)
                yield return null;
        }

        if (keyRenderer != null)
        {
            keyRenderer.enabled = false;
        }

        Destroy(gameObject);
    }

    public bool CanUseBy(int slimeIndex)
    {
        return IsCollected && !IsUsed && slimeIndex == ownerSlimeIndex;
    }

    public void OnSlimeMerged(int currentSlimeIndex)
    {
        if (!IsCollected || IsUsed) return;

        ownerSlimeIndex = currentSlimeIndex;
        pausedBySwitch = false;
    }
}
