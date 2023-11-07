using TMPro;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    [SerializeField] private GameController _gameController;
    [SerializeField] private TextMeshProUGUI _scorDisplay;
    [SerializeField] private GameObject _startButton;

    void Start()
    {
        _gameController.OnGameOver += ShowStartButton;
        _gameController.OnGameStart += HideStartButton;
        _gameController.OnScoreChanged += UpdateScore;
    }

    private void UpdateScore(int value)
    {
        _scorDisplay.text = value.ToString();
    }

    public void ShowStartButton()
    {
        _startButton.SetActive(true);
    }
    public void HideStartButton()
    {
        _startButton.SetActive(false);
    }


}
