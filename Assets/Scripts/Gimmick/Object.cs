using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object : MonoBehaviour
{
    Rigidbody2D rb;
    private bool isNeedleActive = false;


    [SerializeField] GameObject slimeDeath;

    [SerializeField] Transform playerRespawnPoint;



    // Start is called before the first frame update
    void Start()
    {
        //自分のゲームオブジェクトからRigidBodyを得る
        //rb = GetComponent<Rigidbody2D>();
        //kinematicで動かなくなる
        //rb.bodyType = RigidbodyType2D.Kinematic;



    }

    // Update is called once per frame
    void Update()
    {
        // Groundオブジェクトに接触した場合に削除
        /*if (Physics2D.OverlapCircle(new Vector2(transform.position.x, transform.position.y - 0.5f), 0.1f, LayerMask.GetMask("Ground")))
        {
            Destroy(gameObject); // 自分自身を削除
        }*/


    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            Player player = col.GetComponent<Player>();
            if (player != null && playerRespawnPoint != null)
            {
                player.RespawnAt(playerRespawnPoint.position);
            }
        }

        if (col.CompareTag("PlayerClone"))
        {
            SlimeClone clone = col.GetComponent<SlimeClone>();
            Player player = GameObject.FindWithTag("Player").GetComponent<Player>();

            if (clone != null && player != null)
            {
                if (!clone.TryBeginMerge()) return;

                player.RemoveSlimeFromCount(col.gameObject);
                player.AddSlimeWithCap(clone.size + 1);

                Destroy(col.gameObject);

            }
        }

        if (col.CompareTag("PlayerBullet"))
        {
            Player player = GameObject.FindWithTag("Player").GetComponent<Player>();

            if (player != null)
            {
                player.AddSlimeWithCap(1f); // ← サイズ戻す
                Destroy(col.gameObject);   // ← 弾消す
            }
        }







    }



    /*IEnumerator Needle()
    {
        bool b = true;
        Transform pos = transform;
        float a = 0.1f;
        for (int i = 0; i < 6; i++)
        {
            if (b)
            {
                pos.position = new Vector2(pos.position.x + a, pos.position.y);
                b = false;
            }
            else
            {
                pos.position = new Vector2(pos.position.x - a, pos.position.y);
                b = true;
            }

            yield return new WaitForSeconds(0.08f);
        }

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.velocity = new Vector2(0, 0);
    }*/

}
