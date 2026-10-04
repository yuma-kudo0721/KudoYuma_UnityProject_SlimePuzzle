using UnityEngine;

public class SwitchCamera : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private GameObject virtualCamera;

    [Header("Room")]
    [Tooltip("例: forest / cave / castle。部屋ごとに重複しない名前を設定")]
    [SerializeField] private string roomId;

    [Header("Rock Respawn")]
    [SerializeField] private bool respawnRocksOnPlayerExit = true;
    [SerializeField] private PushableRock[] rocksToRespawnOnExit;

    public string RoomId => roomId;

    // 土管選択スクリプトなどから、部屋カメラを直接切り替える用
    public void SetCameraActive(bool active)
    {
        if (virtualCamera != null)
        {
            virtualCamera.SetActive(active);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // プレイヤーかスライムボールが入ったら、この部屋のカメラをON
        if (!other.CompareTag("Player") && !other.CompareTag("PlayerBullet"))
        {
            return;
        }

        if (other.isTrigger)
        {
            return;
        }

        SetCameraActive(true);

        // 部屋の訪問記録はプレイヤーが入ったときだけ保存
        if (other.CompareTag("Player") && !string.IsNullOrEmpty(roomId))
        {
            PlayerPrefs.SetInt($"VisitedRoom_{roomId}", 1);
            PlayerPrefs.Save();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || other.isTrigger)
        {
            return;
        }

        SetCameraActive(false);

        if (respawnRocksOnPlayerExit)
        {
            RespawnRocks();
        }
    }

    private void RespawnRocks()
    {
        PushableRock[] rocks = rocksToRespawnOnExit;

        if (rocks == null || rocks.Length == 0)
        {
            rocks = FindObjectsOfType<PushableRock>();
        }

        foreach (PushableRock rock in rocks)
        {
            if (rock != null)
            {
                rock.RequestRespawn();
            }
        }
    }
}