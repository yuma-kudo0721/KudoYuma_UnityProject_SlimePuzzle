using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StageMove : MonoBehaviour
{
   public Text stageText; // ステージ番号を表示するためのUIテキスト
    private string stageName = "1-1"; // ステージ名

    void Start()
    {
        ShowStageName();
    }

    void ShowStageName()
    {
        stageText.text = stageName; // ステージ名をテキストに設定
        stageText.gameObject.SetActive(true); // テキストを表示
        Invoke("HideStageName", 2f); // 2秒後に非表示にする
    }

    void HideStageName()
    {
        stageText.gameObject.SetActive(false); // テキストを非表示
    }
}
