using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    GameObject cam;
    [SerializeField]
    GameObject player;
    [SerializeField]
    float speed = 10;
    [SerializeField]
    float leftLimit = 0f;  // カメラがこれ以上左に行けない位置

    void Start()
    {

    }

    void Update()
    {
        Vector3 a = cam.transform.position;

        Vector2 dire = (player.transform.position - cam.transform.position).normalized;

        // 新しいカメラ位置を計算する
        float newX = cam.transform.position.x + (dire.x * speed * Time.deltaTime);

        // 左端を超えないように制限
        if (newX < leftLimit)
        {
            newX = leftLimit;
        }

        // カメラを移動させる
        cam.transform.position = new Vector3(newX, cam.transform.position.y, cam.transform.position.z);
    }
}
