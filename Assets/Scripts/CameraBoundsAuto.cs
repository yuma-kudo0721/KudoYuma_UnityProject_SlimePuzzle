using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBoundsAuto : MonoBehaviour
{
    [SerializeField] float wallThickness = 1f;

    Camera cam;
    BoxCollider2D leftWall;
    BoxCollider2D rightWall;
    BoxCollider2D topWall;
    BoxCollider2D bottomWall;

    void Start()
    {
        cam = GetComponent<Camera>();

        leftWall = CreateWall("LeftWall");
        rightWall = CreateWall("RightWall");
        topWall = CreateWall("TopWall");
        bottomWall = CreateWall("BottomWall");
    }

    void LateUpdate()
    {
        if (cam == null || !cam.orthographic) return;

        float height = cam.orthographicSize * 2f;
        float width = height * cam.aspect;
        Vector3 pos = cam.transform.position;

        leftWall.transform.position = new Vector3(pos.x - width / 2f - wallThickness / 2f, pos.y, 0f);
        rightWall.transform.position = new Vector3(pos.x + width / 2f + wallThickness / 2f, pos.y, 0f);
        topWall.transform.position = new Vector3(pos.x, pos.y + height / 2f + wallThickness / 2f, 0f);
        bottomWall.transform.position = new Vector3(pos.x, pos.y - height / 2f - wallThickness / 2f, 0f);

        leftWall.size = new Vector2(wallThickness, height + wallThickness * 2f);
        rightWall.size = new Vector2(wallThickness, height + wallThickness * 2f);
        topWall.size = new Vector2(width + wallThickness * 2f, wallThickness);
        bottomWall.size = new Vector2(width + wallThickness * 2f, wallThickness);
    }

    BoxCollider2D CreateWall(string wallName)
    {
        GameObject wall = new GameObject(wallName);
        wall.transform.parent = transform;
        wall.layer = LayerMask.NameToLayer("BulletWall");
        BoxCollider2D col = wall.AddComponent<BoxCollider2D>();
        return col;
    }

}
