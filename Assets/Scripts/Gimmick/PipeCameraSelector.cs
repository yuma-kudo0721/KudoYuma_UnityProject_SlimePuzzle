using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[Serializable]
public class PipeDestination
{
    [Header("表示")]
    public string displayName = "森の部屋";

    [Header("訪問済み判定")]
    [Tooltip("SwitchCamera に設定した roomId と同じ名前にする")]
    public string roomId = "forest";

    [Header("プレビューする部屋カメラ")]
    public SwitchCamera previewCamera;

    [Header("テレポート先")]
    [Tooltip("行き先土管の出口位置に置く空オブジェクト")]
    public Transform arrivalPoint;
}

public class PipeCameraSelector : MonoBehaviour
{
    [Header("この土管が置かれている部屋のカメラ")]
    [SerializeField] private SwitchCamera currentRoomCamera;

    [Header("行き先一覧")]
    [SerializeField] private List<PipeDestination> allDestinations = new();

    [Header("UI（任意）")]
    [SerializeField] private GameObject selectionPanel;
    [SerializeField] private TextMeshProUGUI destinationNameText;
    [SerializeField] private TextMeshProUGUI guideText;

    private Player player;
    private bool playerInside;
    private bool isSelecting;
    private int selectedIndex;

    // 実際に選択可能な、訪問済み行き先だけが入る
    private readonly List<PipeDestination> unlockedDestinations = new();

    private void Awake()
    {
        if (selectionPanel != null)
        {
            selectionPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isSelecting)
        {
            if (playerInside && Input.GetKeyDown(KeyCode.E))
            {
                StartSelection();
            }

            return;
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ChangeSelection(-1);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangeSelection(1);
        }

        if (Input.GetKeyDown(KeyCode.Z))
        {
            TeleportToSelectedDestination();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            CancelSelection();
        }
    }

    private void StartSelection()
    {
        BuildUnlockedDestinationList();

        if (unlockedDestinations.Count == 0)
        {
            Debug.Log("行ける部屋がまだありません");
            return;
        }

        if (player == null)
        {
            return;
        }

        isSelecting = true;
        selectedIndex = 0;

        // 土管を選んでいる間はプレイヤーを動かさない
        player.SetControl(false);

        if (currentRoomCamera != null)
        {
            currentRoomCamera.SetCameraActive(false);
        }

        if (selectionPanel != null)
        {
            selectionPanel.SetActive(true);
        }

        ShowSelectedPreview();
    }

    private void BuildUnlockedDestinationList()
    {
        unlockedDestinations.Clear();

        foreach (PipeDestination destination in allDestinations)
        {
            if (destination == null)
            {
                continue;
            }

            bool hasVisited =
                PlayerPrefs.GetInt($"VisitedRoom_{destination.roomId}", 0) == 1;

            if (hasVisited)
            {
                unlockedDestinations.Add(destination);
            }
        }
    }

    private void ChangeSelection(int direction)
    {
        selectedIndex += direction;

        if (selectedIndex < 0)
        {
            selectedIndex = unlockedDestinations.Count - 1;
        }

        if (selectedIndex >= unlockedDestinations.Count)
        {
            selectedIndex = 0;
        }

        ShowSelectedPreview();
    }

    private void ShowSelectedPreview()
    {
        // ほかの候補カメラをすべて消す
        foreach (PipeDestination destination in unlockedDestinations)
        {
            if (destination.previewCamera != null)
            {
                destination.previewCamera.SetCameraActive(false);
            }
        }

        PipeDestination selected = unlockedDestinations[selectedIndex];

        // 選んでいる部屋を映す
        if (selected.previewCamera != null)
        {
            selected.previewCamera.SetCameraActive(true);
        }

        if (destinationNameText != null)
        {
            destinationNameText.text = selected.displayName;
        }

        if (guideText != null)
        {
            guideText.text = "← →：行き先を選ぶ\nZ：移動\nX：戻る";
        }
    }

    private void TeleportToSelectedDestination()
    {
        PipeDestination selected = unlockedDestinations[selectedIndex];

        if (selected.arrivalPoint == null)
        {
            Debug.LogWarning($"{selected.displayName} の Arrival Point が未設定です");
            return;
        }

        // カメラは選択中のものをそのまま残す
        if (selected.previewCamera != null)
        {
            selected.previewCamera.SetCameraActive(true);
        }

        player.transform.position = selected.arrivalPoint.position;

        EndSelection();
    }

    private void CancelSelection()
    {
        // プレビュー中の行き先カメラを消す
        foreach (PipeDestination destination in unlockedDestinations)
        {
            if (destination.previewCamera != null)
            {
                destination.previewCamera.SetCameraActive(false);
            }
        }

        // 元の部屋のカメラへ戻す
        if (currentRoomCamera != null)
        {
            currentRoomCamera.SetCameraActive(true);
        }

        EndSelection();
    }

    private void EndSelection()
    {
        isSelecting = false;

        if (selectionPanel != null)
        {
            selectionPanel.SetActive(false);
        }

        if (player != null)
        {
            player.SetControl(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Player foundPlayer = other.GetComponent<Player>();

        if (foundPlayer == null)
        {
            return;
        }

        player = foundPlayer;
        playerInside = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<Player>() != null)
        {
            playerInside = false;
        }
    }
}