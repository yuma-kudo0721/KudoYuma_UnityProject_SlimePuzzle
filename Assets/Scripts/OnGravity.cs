using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnGravity : MonoBehaviour
{
    [SerializeField] Rigidbody2D targetRb;
    [SerializeField] float gravityScale = 4f;

    void Start()
    {
        if (targetRb == null)
        {
            targetRb = GetComponent<Rigidbody2D>();
        }

        targetRb.bodyType = RigidbodyType2D.Kinematic;
        targetRb.velocity = Vector2.zero;
    }

    public void ApplyGravity()
    {
        targetRb.bodyType = RigidbodyType2D.Dynamic;
        targetRb.gravityScale = gravityScale;
    }
}
