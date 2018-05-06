using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour {

    public static ScoreManager instance = null;

    [SerializeField]
    int scoreTotal = 0;

	// Use this for initialization
	void Awake () {
        if (instance == null)
            instance = this;
	}

    public void IncreaseScore(int score)
    {
        scoreTotal += score;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
