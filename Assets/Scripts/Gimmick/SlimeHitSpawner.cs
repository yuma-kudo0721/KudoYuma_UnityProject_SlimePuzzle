using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SlimeHitSpawner : MonoBehaviour
{
    [Header("Blocks to toggle")]
    [SerializeField] GameObject[] blueBlocks;
    [SerializeField] GameObject[] redBlocks;

    [Header("Activation block color (shows the next color)")]
    [SerializeField] SpriteRenderer activationRenderer;
    [SerializeField] Color blueTint = Color.blue;
    [SerializeField] Color redTint = Color.red;

    Collider2D blockCollider;
    static readonly List<SlimeHitSpawner> activeSpawners = new List<SlimeHitSpawner>();
    static bool blueBlocksActive = true;
    static int lastProcessedBulletId = -1;
    static int lastProcessedHitFrame = -1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetSharedState()
    {
        activeSpawners.Clear();
        blueBlocksActive = true;
        lastProcessedBulletId = -1;
        lastProcessedHitFrame = -1;
    }

    void Awake()
    {
        blockCollider = GetComponent<Collider2D>();
        if (activationRenderer == null)
            activationRenderer = GetComponent<SpriteRenderer>();

        if (!activeSpawners.Contains(this))
            activeSpawners.Add(this);

        ApplyAllBlockStates();
    }

    void OnDestroy()
    {
        activeSpawners.Remove(this);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        bool isSlimeBullet = collision.collider.CompareTag("PlayerBullet");
        bool isSlimeBody =
            collision.collider.GetComponentInParent<Player>() != null ||
            collision.collider.GetComponentInParent<SlimeClone>() != null;

        if (!isSlimeBullet && !isSlimeBody) return;
        if (blockCollider == null || collision.contactCount == 0) return;


        int hitObjectId = collision.rigidbody != null
            ? collision.rigidbody.gameObject.GetInstanceID()
            : collision.collider.transform.root.gameObject.GetInstanceID();
        if (lastProcessedHitFrame == Time.frameCount && lastProcessedBulletId == hitObjectId) return;

        lastProcessedHitFrame = Time.frameCount;
        lastProcessedBulletId = hitObjectId;
        blueBlocksActive = !blueBlocksActive;
        ApplyAllBlockStates();
    }

    static void ApplyAllBlockStates()
    {
        for (int i = activeSpawners.Count - 1; i >= 0; i--)
        {
            SlimeHitSpawner spawner = activeSpawners[i];
            if (spawner == null)
            {
                activeSpawners.RemoveAt(i);
                continue;
            }

            spawner.SetBlocksActive(spawner.blueBlocks, blueBlocksActive);
            spawner.SetBlocksActive(spawner.redBlocks, !blueBlocksActive);

            // 起動ブロックは、次に切り替わって出るブロックの色を示す。
            if (spawner.activationRenderer != null)
                spawner.activationRenderer.color = blueBlocksActive ? spawner.redTint : spawner.blueTint;
        }
    }

    void SetBlocksActive(GameObject[] blocks, bool active)
    {
        if (blocks == null) return;

        foreach (GameObject block in blocks)
        {
            if (block != null)
                block.SetActive(active);
        }
    }
}
