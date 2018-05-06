using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour {

    public static ScoreManager instance = null;

    [SerializeField]
    int scoreTotal = 0;

    private Text scoreText;

	// Use this for initialization
	void Awake () {
        if (instance == null)
            instance = this;

        scoreText = GameObject.Find("ScoreText").GetComponent<Text>();
    }

    public void IncreaseScore(int score)
    {
        scoreTotal += score;

        scoreText.text = "Score: " + scoreTotal;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
