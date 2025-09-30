// MainMenu.cs
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private SceneLoader loader;
    [SerializeField] private string gameSceneName = "Game";

    public void OnStartClicked()
    {
        loader.LoadScene(gameSceneName);
        GameManager.I?.StartGame();
    }
    public void OnQuitClicked() => loader.Quit();

    
}
