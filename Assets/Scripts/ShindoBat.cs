using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShindoBat : MonoBehaviour
{
    [SerializeField]
    GameObject startPosi;

    [SerializeField]
    GameObject finishPosi;

    // バットの移動速度
    [SerializeField]
    float batSpeed = 1f;

    private float t = 0f;
    private SpriteRenderer batsp;

    public int hp = 2;

    Color originalColor;

    SpriteRenderer sp;

    CircleCollider2D cc2d;

    [SerializeField]
    Game game;



    // Start is called before the first frame update
    void Start()
    {
        sp = GetComponent<SpriteRenderer>();
        // 初期値としてtを0に設定
        t = 0f;

        // SpriteRenderer コンポーネントの取得
        batsp = GetComponent<SpriteRenderer>();
        hp = 2;
        cc2d = GetComponent<CircleCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // PingPongでtを0と1の間で行き来させる
        t = Mathf.PingPong(Time.time * batSpeed, 1f);

        // startPosiとfinishPosiの間で移動
        transform.position = Vector3.Lerp(startPosi.transform.position, finishPosi.transform.position, t);

        // 向きを反転させる（移動方向によって反転）
        if (t > 0.9f && batsp.flipX)
        {
            batsp.flipX = false;  // 進行方向が逆になるタイミングで反転
        }
        else if (t <= 0.1f && !batsp.flipX)
        {
            batsp.flipX = true;  // 進行方向が元に戻るタイミングで反転解除
        }


    }



    void OnCollisionEnter2D(Collision2D other)
    {

        if (other.gameObject.CompareTag("Player"))
        {
            game.Remain(false);
            // Particle System コンポーネントを取得

        }

    }

    public IEnumerator FlashWhite()
    {

        for (int i = 0; i < 4; i++)
        {
            sp.enabled = false;

            yield return new WaitForSeconds(0.1f);
            sp.enabled = true;

            yield return new WaitForSeconds(0.1f);

        }


    }

    public IEnumerator InvincibilityTime()
    {
        cc2d.enabled = false;
        yield return new WaitForSeconds(0.5f);
        cc2d.enabled = true;
    }
}
