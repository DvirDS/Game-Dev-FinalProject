// MainMenu.cs
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private SceneLoader loader;
    [SerializeField] private string gameSceneName = "Game";

    public void OnStartClicked()
    {
        GameManager.I?.ResetGameData();
        loader.LoadScene(gameSceneName);
    }
    public void OnQuitClicked() => loader.Quit();


}