using UnityEngine;
using UnityEngine.SceneManagement;

public class InputHandler : MonoBehaviour
{
    private PlayerInputReader inputReader;

    // The Start method will run on the first scene
    void Start()
    {
        FindAndAssignInputReader();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // The OnSceneLoaded method will run on subsequent scenes
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindAndAssignInputReader();
    }

    // This function now ONLY searches for the reader, without checking the game state.
    // This removes the race condition.
    private void FindAndAssignInputReader()
    {
        inputReader = FindAnyObjectByType<PlayerInputReader>();
        if (inputReader == null)
        {
            // It's okay if we don't find it right away, Update will keep trying.
        }
        else
        {
            Debug.Log("InputHandler successfully found the PlayerInputReader.");
        }
    }

    void Update()
    {
        // --- NEW: Robustness Check ---
        // If we don't have a reader for any reason, try to find it again.
        if (inputReader == null)
        {
            FindAndAssignInputReader();
            // If we still couldn't find it, exit for this frame.
            if (inputReader == null) return;
        }

        // This logic now checks the game state only AFTER a button press is detected.
        if (inputReader.PausePressed)
        {
            // We can safely check the GameManager state here because it will be ready.
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