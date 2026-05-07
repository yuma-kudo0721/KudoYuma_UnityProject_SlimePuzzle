using System.Collections;
using UnityEngine;

public class SlimeClone : MonoBehaviour
{
    public float size;
    [SerializeField] Vector2 defaultSize;
    [SerializeField] float respawnDelay = 0.2f;

    Rigidbody2D rb;
    Collider2D col;
    SpriteRenderer sp;
    bool isRespawning = false;
    bool isMerging = false;

    void Start()
    {
        defaultSize = transform.localScale;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        sp = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        UpdataSlimeSize(size);
    }

    void UpdataSlimeSize(float size)
    {
        transform.localScale = defaultSize + ((defaultSize * size) * 0.3f);
    }

    public bool TryBeginMerge()
    {
        if (isMerging) return false;

        isMerging = true;
        if (col != null)
        {
            col.enabled = false;
        }

        return true;
    }

    public void RespawnAt(Vector3 position)
    {
        if (!isRespawning)
        {
            StartCoroutine(RespawnCoroutine(position));
        }
    }

    IEnumerator RespawnCoroutine(Vector3 position)
    {
        isRespawning = true;

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        col.enabled = false;

        Color c = sp.color;
        c.a = 0f;
        sp.color = c;

        yield return new WaitForSeconds(respawnDelay);

        transform.position = position;

        c.a = 1f;
        sp.color = c;
        col.enabled = true;

        isRespawning = false;
    }
}
