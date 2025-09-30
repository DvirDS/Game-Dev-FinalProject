using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    // Singleton
    public static GameManager I { get; private set; }
    // Alias לשמירה על תאימות לקוד שקורא Instance
    public static GameManager Instance => I;

    // Game States
    public enum GameState { MainMenu, Play, Pause, Dialogue, GameOver }
    [SerializeField] private GameState state = GameState.MainMenu;
    public GameState State => state;

    // Events
    public event Action<GameState> OnStateChanged;
    public event Action<int, int> OnPlayerHealthChanged; // current, max
    public event Action OnWeaponSwitched;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    // ----- State transitions -----
    public void StartGame() => SetState(GameState.Play);
    public void PauseGame() => SetState(GameState.Pause);
    public void ResumeGame() => SetState(GameState.Play);
    public void OpenDialogue() => SetState(GameState.Dialogue);
    public void EndDialogue() => SetState(GameState.Play);
    public void GameOver() => SetState(GameState.GameOver);

    public void SetState(GameState next)
    {
        if (state == next) return;
        state = next;
        OnStateChanged?.Invoke(state);
        Time.timeScale = (state == GameState.Pause || state == GameState.GameOver) ? 0f : 1f;
    }

    // ----- Health bridge for UI -----
    public void NotifyPlayerHealth(int current, int max)
    {
        OnPlayerHealthChanged?.Invoke(current, max);
        if (current <= 0 && state != GameState.GameOver)
            GameOver();
    }

    // ----- Weapon switch notification -----
    public void NotifyWeaponSwitched()
    {
        OnWeaponSwitched?.Invoke();
    }
}
