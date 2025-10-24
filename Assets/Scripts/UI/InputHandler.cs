using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * This script acts as a global input manager.
 * Its primary role is to find the 'PlayerInputReader' in any loaded scene.
 * In its Update loop, it specifically listens for the "Pause" input action.
 * When pause is pressed, it checks the GameManager's state and tells it to
 * either pause or resume the game.
 * It is designed to be persistent (or re-find its components) across scene loads.
 */

public class InputHandler : MonoBehaviour
{
    private PlayerInputReader inputReader;

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
    }

    void Update()
    {
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