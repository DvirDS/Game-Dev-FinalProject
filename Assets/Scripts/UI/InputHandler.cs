using UnityEngine;

public class InputHandler : MonoBehaviour
{
    private PlayerInputReader inputReader;

    // We subscribe to the GameManager's event when this object is enabled
    void OnEnable()
    {
        if (GameManager.I != null)
        {
            GameManager.I.OnStateChanged += HandleGameStateChange;
        }
        // If GameManager doesn't exist yet, we'll try again in Start
        else
        {
            // This is a fallback, but OnEnable should be sufficient
            Invoke(nameof(SubscribeToGameManager), 0.1f);
        }
    }

    private void SubscribeToGameManager()
    {
        if (GameManager.I != null)
        {
            GameManager.I.OnStateChanged += HandleGameStateChange;
        }
    }

    // Always unsubscribe when the object is disabled or destroyed
    void OnDisable()
    {
        if (GameManager.I != null)
        {
            GameManager.I.OnStateChanged -= HandleGameStateChange;
        }
    }

    // This function will be called automatically by the GameManager
    private void HandleGameStateChange(GameManager.GameState newState)
    {
        if (newState == GameManager.GameState.Play)
        {
            // The game is now in the 'Play' state! THIS is the time to find the player's input.
            inputReader = FindAnyObjectByType<PlayerInputReader>();
        }
        else
        {
            // If we are not in the 'Play' state (e.g., back in MainMenu), clear the reference.
            inputReader = null;
        }
    }

    void Update()
    {
        // The 'inputReader' variable will only have a value when we are in the Play state.
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