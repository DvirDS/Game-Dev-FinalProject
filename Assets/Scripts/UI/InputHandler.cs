using UnityEngine;
using UnityEngine.SceneManagement; 

public class InputHandler : MonoBehaviour
{
    private PlayerInputReader inputReader;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (GameManager.I != null && GameManager.I.State == GameManager.GameState.Play)
        {
            inputReader = FindAnyObjectByType<PlayerInputReader>();
            if (inputReader == null)
            {
                Debug.LogError("FATAL: InputHandler could not find a PlayerInputReader in the new scene!");
            }
            else
            {
                Debug.Log("InputHandler successfully found the new PlayerInputReader.");
            }
        }
    }

    void Update()
    {
        if (inputReader != null && inputReader.PausePressed)
        {
            if (GameManager.I.State == GameManager.GameState.Play)
            {
                GameManager.I.PauseGame();
            }
            else if (GameManager.I.State == GameManager.GameState.Pause)
            {
                GameManager.I.ResumeGame();
            }
        }
    }

}