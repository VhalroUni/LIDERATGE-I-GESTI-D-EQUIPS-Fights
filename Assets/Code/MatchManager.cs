using UnityEngine;

public class MatchManager : MonoBehaviour
{
    [Header("Players")]
    [SerializeField] private LifeController player1Life;
    [SerializeField] private LifeController player2Life;
    [SerializeField] private string player1Name = "1";
    [SerializeField] private string player2Name = "2";

    [Header("UI")]
    [SerializeField] private WinnerUI winnerUI;

    private bool matchEnded;

    private void OnEnable()
    {
        if (player1Life != null) player1Life.OnDeath += OnPlayerDeath;
        if (player2Life != null) player2Life.OnDeath += OnPlayerDeath;
    }

    private void OnDisable()
    {
        if (player1Life != null) player1Life.OnDeath -= OnPlayerDeath;
        if (player2Life != null) player2Life.OnDeath -= OnPlayerDeath;
    }

    private void OnPlayerDeath(LifeController dead)
    {
        if (matchEnded) return;
        matchEnded = true;

        string winnerName = (dead == player1Life) ? player2Name : player1Name;

        Debug.Log($"[MatchManager] Muerte detectada de {dead.name}. Ganador: {winnerName}");

        if (winnerUI != null)
        {
            winnerUI.ShowWinner(winnerName);
        }
        else
        {
            Debug.LogWarning("[MatchManager] winnerUI no asignado en el inspector.");
        }
    }
}