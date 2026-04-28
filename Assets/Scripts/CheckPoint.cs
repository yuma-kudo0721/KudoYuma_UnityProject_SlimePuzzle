using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    public int checkpointID; // ← Inspectorで番号を設定

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerPrefs.SetInt("LastCheckpoint", checkpointID);
            PlayerPrefs.Save();
            Debug.Log("設定完了");
        }

    }
}