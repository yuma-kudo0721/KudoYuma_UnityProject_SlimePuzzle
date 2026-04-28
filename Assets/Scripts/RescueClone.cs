using System.Collections;
using UnityEngine;

public class RescueClone : MonoBehaviour
{
    [SerializeField] Animator anim;
    [SerializeField] Rigidbody2D rb;
    [SerializeField] Collider2D bodyCollider;
    [SerializeField] Player player;
    [SerializeField] LayerMask groundLayer;

    [SerializeField] float jumpPower = 5f;
    [SerializeField] float flyPowerX = 5f;
    [SerializeField] float flyPowerY = 3f;

    [SerializeField] float shakeWidth = 0.05f;
    [SerializeField] float shakeSpeed = 20f;

    [SerializeField] GameObject happyMark;
    [SerializeField] float happyMarkDuration = 0.8f;
    [SerializeField] float happyMarkMoveY = 0.5f;


    bool started = false;
    bool released = false;
    bool flyingToPlayer = false;

    Vector3 startLocalPos;

    void Start()
    {
        if (anim == null) anim = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (bodyCollider == null) bodyCollider = GetComponent<Collider2D>();

        if (happyMark != null)
        {
            happyMark.SetActive(false);
        }


        startLocalPos = transform.localPosition;
    }

    void Update()
    {
        if (!released)
        {
            float x = Mathf.Sin(Time.time * shakeSpeed) * shakeWidth;
            transform.localPosition = new Vector3(
                startLocalPos.x + x,
                startLocalPos.y,
                startLocalPos.z
            );
            return;
        }

        if (started) return;
        if (!IsGrounded()) return;

        started = true;
        StartCoroutine(Sequence());
    }

    public void ReleaseClone()
    {
        released = true;
    }

    IEnumerator Sequence()
    {
        if (player != null)
        {
            player.SetControl(false);
        }

        yield return new WaitForSeconds(0.5f);

        if (happyMark != null)
        {
            StartCoroutine(PlayHappyMarkEffect());
        }

        anim.SetBool("Idle", true);
        rb.velocity = new Vector2(0f, jumpPower);
        yield return new WaitUntil(() => !IsGrounded());
        yield return new WaitUntil(() => IsGrounded());

        yield return new WaitForSeconds(0.15f);

        anim.SetBool("Idle", true);
        rb.velocity = new Vector2(0f, jumpPower);
        yield return new WaitUntil(() => !IsGrounded());
        yield return new WaitUntil(() => IsGrounded());

        yield return new WaitForSeconds(0.15f);


        anim.SetBool("Jump", true);

        Vector2 dir = (player.transform.position - transform.position).normalized;
        rb.velocity = new Vector2(dir.x * flyPowerX, flyPowerY);
        flyingToPlayer = true;
    }

    bool IsGrounded()
    {
        if (bodyCollider == null) return false;
        return bodyCollider.IsTouchingLayers(groundLayer);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!flyingToPlayer) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        flyingToPlayer = false;

        if (player != null)
        {
            player.PlayMergeEffect();
            player.SetControl(true);
        }

        Destroy(gameObject);
    }

    IEnumerator PlayHappyMarkEffect()
    {
        happyMark.SetActive(true);

        SpriteRenderer markRenderer = happyMark.GetComponent<SpriteRenderer>();
        Vector3 startPos = happyMark.transform.localPosition;
        float timer = 0f;

        Color color = markRenderer.color;
        color.a = 1f;
        markRenderer.color = color;

        while (timer < happyMarkDuration)
        {
            timer += Time.deltaTime;
            float t = timer / happyMarkDuration;

            happyMark.transform.localPosition = startPos + Vector3.up * (happyMarkMoveY * t);

            color.a = 1f - t;
            markRenderer.color = color;

            yield return null;
        }

        happyMark.transform.localPosition = startPos;
        happyMark.SetActive(false);
    }



}
