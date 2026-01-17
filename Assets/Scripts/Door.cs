using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField]
    GateButton button; // Buttonスクリプトを参照するための変数

    [SerializeField]
    float moveSpeed = 0.1f; // ドアが開く速度

    void Start()
    {

    }

    // Updateメソッドも不要であれば削除
    void Update()
    {
        OpenDoor();
    }

    // ドアを開けるメソッド
    void OpenDoor()
    {
        if (button != null && button.openDoor) // ボタンが押されたら
        {
            StartCoroutine(DoorUp()); // コルーチンでドアを開ける
        }
    }

    // ドアが上がる処理（コルーチン）
    IEnumerator DoorUp()
    {
        float maxHeight = 7f;


        while (transform.position.y < maxHeight)
        {
            // ドアを少しずつ上に移動
            transform.position = new Vector2(transform.position.x, transform.position.y + moveSpeed * Time.deltaTime);
            yield return null;  // 次のフレームまで待機
        }


    }
}
