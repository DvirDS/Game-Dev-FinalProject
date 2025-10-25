using UnityEngine;
using UnityEngine.SceneManagement;


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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindAndAssignInputReader();
    }

    private void FindAndAssignInputReader()
    {
        inputReader = FindAnyObjectByType<PlayerInputReader>();
    }

    void Update()
    {
        if (inputReader == null)
        {
            FindAndAssignInputReader();
            if (inputReader == null) return;
        }

        if (inputReader.PausePressed)
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