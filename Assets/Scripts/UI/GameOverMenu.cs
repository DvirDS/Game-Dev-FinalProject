using UnityEngine;

public class GameOverMenu : MonoBehaviour
{
    [SerializeField] private SceneLoader loader;
    [SerializeField] private string mainMenuScene = "StartMenu";
    [SerializeField] private string firstLevelScene = "Game";

    public void OnRetry()
    {
        GameManager.I?.StartGame();
        loader.LoadScene(firstLevelScene);
    }

    public void OnMainMenu()
    {
        loader.LoadScene(mainMenuScene);
        GameManager.I?.SetState(GameManager.GameState.MainMenu);
    }
}
