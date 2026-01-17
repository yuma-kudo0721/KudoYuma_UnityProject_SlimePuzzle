using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateButton : MonoBehaviour
{

    Animator anim;　　//アニメーション
    Collider2D col;

    SpriteRenderer sp;

    public Sprite normalSprite;             // 通常時のスプライト
    public Sprite pushedSprite;             // 押された時のスプライト

    public bool openDoor = false;





    // Start is called before the first frame update
    void Start()
    {

        sp = GetComponent<SpriteRenderer>();
        // ボタンのスプライトを初期化
        sp.sprite = normalSprite;


    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // プレイヤーがボタンに触れたとき
        if (other.CompareTag("Player"))
        {
            // ボタンを押された状態に変更
            sp.sprite = pushedSprite;

            openDoor = true;
        }
    }
}
