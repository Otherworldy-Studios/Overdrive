using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MGR<MonoBehaviour>
{
    [SerializeField] string[] scenes; 
    private void Start()
    {
        if (scenes == null || scenes.Length == 0)
        {
            Debug.LogError("No scenes specified in GameManager");
            return;
        }
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
}
