using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HeartText : MonoBehaviour
{
    public Text heart;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        UpdateHeartText();

    }

    void UpdateHeartText()
    {

        heart.text = "x" + Game.heart_num;

    }
}
