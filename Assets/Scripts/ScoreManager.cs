using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;   // accessible partout : ScoreManager.Instance
    public int Score { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void AddPoints(int points)
    {
        Score += points;
        Debug.Log("Score : " + Score);
    }
}