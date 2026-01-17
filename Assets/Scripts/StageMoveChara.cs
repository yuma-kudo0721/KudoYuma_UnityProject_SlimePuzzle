using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class StageMoveChara : MonoBehaviour
{

    [SerializeField]
    float charaSpeed = 5f;

     Rigidbody2D rb;  　//物理挙動

    Animator anim;　
    
    Vector2 a;　
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        
    }

    // Update is called once per frame
    void Update()
    {
        rb.velocity = new Vector2(charaSpeed, rb.velocity.y);
        anim.speed = 1;

    if (transform.position.x >= 9) {
        SceneManager.LoadScene("Stage1"); // シーンを移動
    }

        

    }

    
}
