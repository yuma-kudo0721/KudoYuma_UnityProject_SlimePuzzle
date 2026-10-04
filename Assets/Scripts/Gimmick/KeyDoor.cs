using UnityEngine;

/// <summary>プレイヤーが鍵を持って近づくと鍵を鍵穴に装着し、ドアを開ける。</summary>
public class KeyDoor : MonoBehaviour
{
    [SerializeField] Transform keySocket;
    [SerializeField] Door door;

    bool unlocked;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (unlocked || !other.CompareTag("Player")) return;

        Player player = other.GetComponentInParent<Player>();
        if (player == null) return;

        FollowKey[] keys = FindObjectsOfType<FollowKey>();
        FollowKey key = null;
        foreach (FollowKey candidate in keys)
        {
            if (candidate.CanUseBy(player.ControllSlimeNumber))
            {
                key = candidate;
                break;
            }
        }

        if (key == null) return;
        if (!key.Dock(keySocket, door)) return;

        unlocked = true;
    }
}
