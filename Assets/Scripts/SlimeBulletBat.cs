using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeBulletBat : MonoBehaviour
{
    [SerializeField] float dis = 10f; // 射程距離
    [SerializeField] float disDelete = 10f; // 削除距離
    [SerializeField] LayerMask dropBulletLayer; // ドロップ後のレイヤー
    [SerializeField] GameObject splashSlime;

    [SerializeField] GameObject slimeClone;//スライムのクローン

    //public float bulletSpeed = 30f;
    Vector2 firstPos;
    Rigidbody2D rb;
    Collider2D col;
    Transform player;
    public bool droping = false;

    void Start()
    {
        firstPos = transform.position;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        player = GameObject.FindWithTag("Player").transform;

        // 弾丸の初速度を設定
        //rb.AddForce(new Vector2(bulletSpeed, 0), ForceMode2D.Impulse);
    }

    void Update()
    {
        // 射程を超えた場合に重力を適用
        if (Vector2.Distance(firstPos, transform.position) >= dis && !droping)
        {
            droping = true;
            rb.gravityScale = 3;

            // レイヤーを設定（LayerMask からレイヤー番号を取得）
            gameObject.layer = Mathf.RoundToInt(Mathf.Log(dropBulletLayer.value, 2));
        }


    }

    void OnCollisionEnter2D(Collision2D other)
    {
        // プレイヤー以外のオブジェクトに衝突した場合、ドロップ処理を開始
        if (!droping && other.gameObject.CompareTag("Player") == false)
        {
            droping = true;
            rb.gravityScale = 3;

            // レイヤーを設定（LayerMask からレイヤー番号を取得）
            gameObject.layer = Mathf.RoundToInt(Mathf.Log(dropBulletLayer.value, 2));
        }
        if (other.gameObject.CompareTag("Player") == false)
        {
            Instantiate(splashSlime, transform.position, Quaternion.identity);
        }







    }
}
