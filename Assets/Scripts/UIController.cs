using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TetrisManager tetrisManager;

    public GameObject gameOverPanel;

    public void UpdateScore()
    {
        scoreText.text = $"SCORE: {tetrisManager.score:n0}";
    }

    public void UpdateGameOver()
    {
        gameOverPanel.SetActive(tetrisManager.gameOver);
    }

    public void PlayAgain()
    {
        tetrisManager.SetGameOver(false);
    }

    public void ResetScene()
    {
        SceneManager.LoadScene(0);
    }

}
