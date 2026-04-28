using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DownOnlyGate : MonoBehaviour
{
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float downDistance = 3f;

    Vector3 targetPos;
    bool isDropping = false;

    void Start()
    {
        targetPos = transform.position + Vector3.down * downDistance;
    }

    void Update()
    {
        if (!isDropping) return;

        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
    }

    public void DropGate()
    {
        isDropping = true;
    }
}
