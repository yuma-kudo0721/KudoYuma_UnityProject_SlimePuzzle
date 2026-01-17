using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class PlayerInMapSelect : MonoBehaviour
{

    [System.Serializable]
    public class Stage
    {
        public int rightto;
        public int leftto;
        public int downto;
        public int upto;
        public string sceneName;


    }

    [SerializeField]
    Stage[] stagesmove;

    [SerializeField]
    GameObject[] stages;

    [SerializeField]
    GameObject[] circles;

    [SerializeField]
    int onStageNum = 0;

    [SerializeField]
    bool canControll = true;

    int stageUnlock = 0;



    int toStageNum = -1;

    [SerializeField] Color targetColor = Color.red; // 変更したい色











    // Start is called before the first frame update
    void Start()
    {

        stageUnlock = PlayerPrefs.GetInt("StageUnlock", 1);

        // 前回保存したステージ番号を取得
        if (PlayerPrefs.HasKey("SavedStageNum"))
        {
            onStageNum = PlayerPrefs.GetInt("SavedStageNum");

            // 範囲外を防ぐチェック
            if (onStageNum >= 0 && onStageNum < stages.Length)
            {
                // 前回保存されたステージの位置にキャラクターを移動
                transform.position = stages[onStageNum].transform.position;
            }
            else
            {
                // 範囲外なら初期位置にリセット
                onStageNum = 0;
                transform.position = stages[0].transform.position;
            }
        }
        else
        {
            // 保存されたデータがない場合は初期位置にする
            onStageNum = 0;
            transform.position = stages[0].transform.position;
        }

        for (int i = 0; i < circles.Length; i++)
        {
            if (i < stageUnlock - 1)
            {
                // アンロックされているステージは緑色に変更
                circles[i].GetComponent<SpriteRenderer>().color = targetColor;
            }






        }
    }

    // Update is called once per frame
    void Update()
    {
        Move();

    }



    void Move()
    {


        if (canControll)
        {
            if (onStageNum < stageUnlock - 1) //現在のステージをクリアしたら
            {




                if (Input.GetKeyDown(KeyCode.RightArrow) && stagesmove[onStageNum].rightto != -1)
                {
                    toStageNum = stagesmove[onStageNum].rightto;
                }


            }

            if (Input.GetKeyDown(KeyCode.LeftArrow) && stagesmove[onStageNum].leftto != -1)
            {
                toStageNum = stagesmove[onStageNum].leftto;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow) && stagesmove[onStageNum].upto != -1)
            {

                toStageNum = stagesmove[onStageNum].upto;
            }

            if (Input.GetKeyDown(KeyCode.DownArrow) && stagesmove[onStageNum].downto != -1)
            {
                toStageNum = stagesmove[onStageNum].downto;
            }

            if (toStageNum != -1)
            {
                StartCoroutine(Moving(stages[toStageNum].transform.position, toStageNum));
                toStageNum = -1;
            }




            if (Input.GetKeyDown(KeyCode.Z))
            {
                if (onStageNum < stageUnlock)  // 現在のステージがアンロックされていれば
                {
                    StageSelects(stagesmove[onStageNum].sceneName);  // ステージ選択
                }





            }
        }

    }



    public void StageSelects(string stage)
    {
        // 現在のステージ番号を保存する
        PlayerPrefs.SetInt("SavedStageNum", onStageNum);
        PlayerPrefs.Save();

        // 受け取った引数(stage)のステージをロードする
        SceneManager.LoadScene(stage);
    }


    [SerializeField] float seconds = 0.1f;
    [SerializeField] float smooth = 1f;
    IEnumerator Moving(Vector3 pos, int a)
    {
        canControll = false;

        for (int i = 0; i < seconds * smooth; i++)
        {
            transform.position = Vector3.Lerp(stages[onStageNum].transform.position, pos, i / (seconds * smooth));
            yield return new WaitForSeconds(seconds / smooth);
        }


        onStageNum = a;

        canControll = true;


    }
}
