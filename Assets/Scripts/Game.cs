using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unity.VisualScripting;
using JetBrains.Annotations;

public class Game : MonoBehaviour
{
    [SerializeField]
    Player player;
    [SerializeField]
    TextMeshProUGUI txt;
    [SerializeField]
    TextMeshProUGUI txtTitle;
    [SerializeField]
    MapScroll map;
    [SerializeField]
    GameObject water;
    [SerializeField]
    AudioClip se_start, se_over, se_clear;
    AudioSource snd;


    public static int life_num = 5; // int型
    public static int heart_num = 3; // int型

    public GameObject gameOverText;
    public GameObject pressZText;

    [SerializeField] GameObject p;

















    enum Mode
    {
        Title, Game, Over, Clear, remain
    };

    Mode mode = Mode.Title;
    // Start is called before the first frame update
    void Start()
    {
        snd = gameObject.AddComponent<AudioSource>();
        player.enabled = false;
        txt.enabled = false;
        mode = Mode.Title;
        // 非表示にする
        gameOverText.SetActive(false);
        pressZText.SetActive(false);

        life_num = PlayerPrefs.GetInt("life", 5);
        heart_num = PlayerPrefs.GetInt("heart", 3);





    }

    // Update is called once per frame
    void Update()
    {
        switch (mode)
        {
            case Mode.Title:
                Title();
                break;

            case Mode.Clear:
            case Mode.remain:
                CheckKeyNextTitle();
                break;

            case Mode.Over:
                Select();
                break;

        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            SceneManager.LoadScene("Title");
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("削除しました");
        }

        life_num = Mathf.Clamp(life_num, 0, 5);
        heart_num = Mathf.Clamp(heart_num, 0, 3);


    }

    void Title()
    {//最初

        // snd.PlayOneShot(se_start);
        // water.transform.position = new Vector3(0,0,0);
        //map.isStop = false;
        txt.enabled = true;
        txtTitle.enabled = false;
        player.enabled = true;
        player.Reset();
        mode = Mode.Game;

    }

    public void Remain(bool touchedBottom)
    {
        heart_num--;
        PlayerPrefs.SetInt("heart", heart_num);



        if (heart_num <= 0 || touchedBottom)
        {
            life_num--;
            PlayerPrefs.SetInt("life", life_num);

            Debug.Log("残機が減りました。残機: " + life_num);
            PlayerPrefs.SetInt("heart", 3);

            gameOverText.SetActive(true);
            pressZText.SetActive(true);
            Destroy(p);

        }



        if (life_num <= 0)
        {
            Destroy(p);
            StartGameover();

        }
        if (life_num <= 0 || heart_num <= 0 || touchedBottom)
        {
            mode = Mode.remain;
        }




    }

    void Select()
    {
        if (!Input.GetKeyDown(KeyCode.Z)) { return; }
        SceneManager.LoadScene("StageSelectTest");


    }

    public void StartGameover()
    {

        snd.PlayOneShot(se_over);
        map.isStop = true;
        txt.enabled = false;
        txtTitle.text = "GAME OVER";
        txtTitle.enabled = true;
        player.enabled = false;
        mode = Mode.Over;
        PlayerPrefs.SetInt("life", 5);
        PlayerPrefs.SetInt("heart", 3);

    }

    public void StartGameclear()
    {
        snd.PlayOneShot(se_clear);
        map.isStop = true;
        txt.enabled = false;
        txtTitle.text = "GAME CLEAR";
        txtTitle.enabled = true;
        player.enabled = false;
        nextStage();
        mode = Mode.Clear;
    }

    public void nextStage()
    {
        // ステージのクリア数を取得
        int StageUnlock = PlayerPrefs.GetInt("StageUnlock");
        int NextScene = SceneManager.GetActiveScene().buildIndex + 1;



        if (NextScene < 9)
        {
            if (StageUnlock < NextScene)
            {



                PlayerPrefs.SetInt("StageUnlock", NextScene);
                PlayerPrefs.Save();

            }
            SceneManager.LoadScene("StageSelectTest");

        }
        else
            //ステージ選択画面に戻る（0:build時の1番上のシーンを読み込む）
            SceneManager.LoadScene("StageSelectTest");
    }

    void CheckKeyNextTitle()
    {
        //Zきーが押されていない場合は戻る
        if (!Input.GetKeyDown(KeyCode.Z)) { return; }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        mode = Mode.Title;
    }


}
