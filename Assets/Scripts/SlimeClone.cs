using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeClone : MonoBehaviour
{
    public float size;
    [SerializeField] Vector2 defaultSize;


    // 他のスクリプトで変更できるようにする

    void Start()
    {
        defaultSize = transform.localScale;
    }

    void Update()
    {
        UpdataSlimeSize(size);
    }
    void UpdataSlimeSize(float size)
    {
        transform.localScale = defaultSize + ((defaultSize * size) * 0.3f);

    }
}


