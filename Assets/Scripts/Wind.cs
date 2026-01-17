using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wind : MonoBehaviour
{
    // x軸方向に加える風の力
    [SerializeField]
    private float windX = 10f;

    // y軸方向に加える風の力
    [SerializeField]
    private float windY = 0f;

    [SerializeField]
    AudioClip se_wind;

    private AudioSource snd;

    void Start()
    {
        snd = gameObject.AddComponent<AudioSource>();

        // 風の音を設定
        snd.clip = se_wind;
        
        // 音声をループさせる設定
        snd.loop = true;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // 当たった相手のrigidbodyコンポーネントを取得
        Rigidbody2D otherRigidbody = other.gameObject.GetComponent<Rigidbody2D>();

        // otherRigidbodyがnullではない場合（相手のGameObjectにrigidbodyがついている場合）
        if (otherRigidbody != null)
        {
            // 相手のrigidbodyに風の力を加える
            Vector2 windForce = new Vector2(windX, windY);
            otherRigidbody.AddForce(windForce, ForceMode2D.Force);

            // 音声が再生中でない場合のみ再生
            if (!snd.isPlaying && otherRigidbody.velocity.magnitude > 0)
            {
                snd.Play();
                
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 風域を出た場合に音声を停止
        if (snd.isPlaying)
        {
            snd.Stop();
        }
    }
}
