using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject creditsPanel;

    [SerializeField] private GameObject firstMainSelected;
    [SerializeField] private GameObject firstCreditsSelected;

    private void Awake()
    {
        ShowMain();
    }

    public void OpenCredits()
    {
        mainPanel.SetActive(false);
        creditsPanel.SetActive(true);

        if (firstCreditsSelected != null)
            EventSystem.current?.SetSelectedGameObject(firstCreditsSelected);
    }

    public void CloseCredits()
    {
        creditsPanel.SetActive(false);
        mainPanel.SetActive(true);

        if (firstMainSelected != null)
            EventSystem.current?.SetSelectedGameObject(firstMainSelected);
    }
    public void PlayGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void QuitGame()
    {
        UnityEditor.EditorApplication.isPlaying = false;
        Application.Quit();
    }

    private void ShowMain()
    {
        if (mainPanel != null) mainPanel.SetActive(true);
        if (creditsPanel != null) creditsPanel.SetActive(false);
    }
}