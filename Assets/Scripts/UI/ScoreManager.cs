using System;
using System.Collections;
using System.Collections.Generic;
using Events;
using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public TMP_Text scoreText;
    private int score = 0;

    public void Score(int amount)
    {
        score += amount;
        scoreText.SetText(score.ToString());
    }

    private void Start()
    {
        this.RegisterListener(EventID.OnPlayerScore, (param) => Score((int)param));
    }
}
