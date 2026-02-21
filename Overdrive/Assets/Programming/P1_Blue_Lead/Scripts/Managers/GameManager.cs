using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MGR<MonoBehaviour>
{
    [SerializeField] string[] scenes; 
    [SerializeField] Arena[] arenas;
    [SerializeField] GameState currentGameState;
    [SerializeField] private int ArenasCleared;
    public event Action OnRunStarted;
    public event Action OnGameOver;
    
    private void Start()
    {
        if (scenes == null || scenes.Length == 0)
        {
            Debug.LogError("No scenes specified in GameManager");
            return;
        }
    }

    private void OnEnable()
    {
        foreach (Arena arena in arenas)
        {
            arena.OnArenaCleared += HandleArenaCleared;
        }
    }
    
    private void StartRun()
    {
        currentGameState = GameState.IN_RUN;
        OnRunStarted?.Invoke();
    }

    private void EndRun(bool victory)
    {
        currentGameState = victory ? GameState.VICTORY : GameState.GAME_OVER;
        OnGameOver?.Invoke();
    }

    public void PauseGame()
    {
        currentGameState = GameState.PAUSED;
    }

    public void GoToHub()
    {
        ChangeScene("Hub");
        currentGameState = GameState.HUB;
    }
    
    private void ChangeScene(int index)
    {
        if (index < 0 || index >= scenes.Length)
        {
            Debug.LogWarning($"Invalid scene index: {index}. Ignoring change request.");
            return;
        }
        
        SceneManager.LoadScene(scenes[index]);
    }

    private void ChangeScene(string sceneName)
    {
        int index = Array.IndexOf(scenes, sceneName);
        if (index == -1)
        {
            Debug.LogWarning($"Scene '{sceneName}' not found. Ignoring change request.");
            return;
        }
        
        SceneManager.LoadScene(scenes[index]);
    }

    private void HandleArenaCleared(int arenaNum)
    {
        ArenasCleared++;
    }

    private void OnDisable()
    {
        foreach (Arena arena in arenas)
        {
           arena.OnArenaCleared -= HandleArenaCleared;
        }
    }
}

enum GameState
{
    HUB,
    IN_RUN,
    PAUSED,
    GAME_OVER,
    VICTORY
}
