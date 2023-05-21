using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndPanel : MonoBehaviour
{
    private GameManager gameManager;
    void Start()
    {
        gameManager = GameObject.Find("XR Origin").GetComponent<GameManager>();
        Debug.Log($"GameManager: {gameManager}");
        Debug.Log($"GameManager: {GameObject.Find("XR Origin")}");

        Button restartButton = gameObject.transform.Find("RestartGameButton").GetComponent<Button>();
        restartButton.onClick.AddListener(ResetGame);
    }

    void ResetGame()
    {
        gameManager.ResetGame();
    }

    public void EditTime(TimeSpan time)
    {
        TMP_Text timeText = gameObject.transform.Find("TimeText").GetComponent<TMP_Text>();
        string formattedTime = String.Format("{0:00}:{1:00}:{2:00}", time.Hours, time.Minutes, time.Seconds);
        timeText.text = formattedTime;
    }
}
