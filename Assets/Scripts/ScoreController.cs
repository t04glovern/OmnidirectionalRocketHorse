using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreController : MonoBehaviour {

    public TextMeshPro scoreText;

    int score = 0;

	void Start() 
    {
        UpdateText();
	}

    public void AddPoint()
    {
        score++;
        UpdateText();
    }

    void UpdateText()
    {
        scoreText.text = "Score: " + score.ToString();
    }
}