using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpDawnLift : MonoBehaviour
{
    [SerializeField] float downDistance = 2f;
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float rideKeepTime = 0.1f;

    Vector3 startPos;
    Vector3 downPos;
    float rideTimer;

    public bool openDoor = false;

    void Start()
    {
        startPos = transform.position;
        downPos = startPos + Vector3.down * downDistance;
    }

    void FixedUpdate()
    {
        if (rideTimer > 0f)
        {
            rideTimer -= Time.fixedDeltaTime;
        }

        openDoor = rideTimer > 0f;

        Vector3 targetPos = rideTimer > 0f ? downPos : startPos;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPos,
            moveSpeed * Time.fixedDeltaTime
        );
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") ||
            collision.gameObject.CompareTag("PlayerClone") ||
            collision.gameObject.CompareTag("PlayerCloneBat"))
        {
            rideTimer = rideKeepTime;
        }
    }
}
