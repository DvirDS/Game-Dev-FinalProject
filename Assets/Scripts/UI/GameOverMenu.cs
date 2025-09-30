// GameOverMenu.cs
using UnityEngine;

public class GameOverMenu : MonoBehaviour
{
    [SerializeField] private SceneLoader loader;
    [SerializeField] private string mainMenuScene = "MainMenu";

    public void OnRetry()
    {
        loader.LoadScene("Game");
        GameManager.I?.StartGame();
    }
    public void OnMainMenu()
    {
        loader.LoadScene(mainMenuScene);
        GameManager.I?.SetState(GameManager.GameState.MainMenu);
    }
}
