using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateButton : MonoBehaviour
{
    Animator anim;
    Collider2D col;
    SpriteRenderer sp;

    public Sprite normalSprite;
    public Sprite pushedSprite;

    public bool openDoor = false;

    [SerializeField] OnGravity onGravity;
    [SerializeField] RescueClone rescueClone;

    void Start()
    {
        sp = GetComponent<SpriteRenderer>();
        sp.sprite = normalSprite;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        //if (!collision.CompareTag("Player")) return;

        sp.sprite = pushedSprite;
        openDoor = true;

        if (onGravity != null)
        {
            onGravity.ApplyGravity();
        }

        if (rescueClone != null)
        {
            rescueClone.ReleaseClone();
        }
    }

    public void ResetSwitch()
    {
        sp.sprite = normalSprite;
        openDoor = false;
    }

    /*void OnTriggerStay2D(Collider2D collision)
    {
        // ボタンを押された状態に変更
        sp.sprite = pushedSprite;

        openDoor = true;
    }*/



    /*void OnTriggerExit2D(Collider2D collision)
    {

        // ボタンを押された状態に変更
        sp.sprite = normalSprite;

        openDoor = false;


    }*/
}
