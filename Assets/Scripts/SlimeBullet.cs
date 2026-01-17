using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeBullet : MonoBehaviour
{
    [SerializeField] float dis = 10f; // 射程距離
    [SerializeField] float disDelete = 10f; // 削除距離
    [SerializeField] LayerMask dropBulletLayer; // ドロップ後のレイヤー
    [SerializeField] GameObject splashSlime;

    [SerializeField] GameObject slimeClone;//スライムのクローン
    [SerializeField] GameObject slimeBulletBat;
    //public float bulletSpeed = 30f;
    Vector2 firstPos;
    Rigidbody2D rb;
    Collider2D col;
    Transform player;
    public bool droping = false;

    ShindoBat shin;

    void Start()
    {
        firstPos = transform.position;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        player = GameObject.FindWithTag("Player").transform;
        GameObject bat = GameObject.FindGameObjectWithTag("Bat");
        shin = bat.GetComponent<ShindoBat>();


        // 弾丸の初速度を設定
        //rb.AddForce(new Vector2(bulletSpeed, 0), ForceMode2D.Impulse);
    }

    void Update()
    {
        // 射程を超えた場合に重力を適用
        if (Vector2.Distance(firstPos, transform.position) >= dis && !droping)
        {
            droping = true;
            rb.gravityScale = 20;

            // レイヤーを設定（LayerMask からレイヤー番号を取得）
            gameObject.layer = Mathf.RoundToInt(Mathf.Log(dropBulletLayer.value, 2));
        }

        // プレイヤーから一定距離離れたらオブジェクトを削除
        if (Vector2.Distance(transform.position, player.position) >= disDelete)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        // プレイヤー以外のオブジェクトに衝突した場合、ドロップ処理を開始
        if (!droping && other.gameObject.CompareTag("Player") == false)
        {
            droping = true;

            // レイヤーを設定（LayerMask からレイヤー番号を取得）
            gameObject.layer = Mathf.RoundToInt(Mathf.Log(dropBulletLayer.value, 2));
        }
        if (other.gameObject.CompareTag("Player") == false)
        {
            Instantiate(splashSlime, transform.position, Quaternion.identity);
        }

        if (other.gameObject.CompareTag("Bat"))
        {
            shin.hp--;
            StartCoroutine(shin.FlashWhite());
            StartCoroutine(shin.InvincibilityTime());

            if (shin.hp <= 0)
            {

                Destroy(gameObject);
                Destroy(other.gameObject);
                Instantiate(slimeBulletBat, transform.position, Quaternion.identity);
            }
        }





    }


}
