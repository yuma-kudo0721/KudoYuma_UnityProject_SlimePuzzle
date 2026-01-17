using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeText : MonoBehaviour
{
    public Text lifeText;






    // Start is called before the first frame update
    void Start()
    {









    }

    // Update is called once per frame
    void Update()
    {
        UpdateLifeText();


    }


    void UpdateLifeText()
    {
        lifeText.text = "×" + Game.life_num; // ライフのテキストを更新


    }
}
