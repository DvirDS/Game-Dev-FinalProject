using UnityEngine;
using System;
using System.Collections.Generic;


[System.Serializable]
public class WeaponSaveData
{
    public WeaponData Weapon;
    public int Ammo;
}

public class GameManager : MonoBehaviour
{
    // Singleton
    public static GameManager I { get; private set; }
    public static GameManager Instance => I;

    // Game States
    public enum GameState { MainMenu, Play, Pause, Dialogue, GameOver }
    [SerializeField] private GameState state = GameState.MainMenu;
    public GameState State => state;

    // Events
    public event Action<GameState> OnStateChanged;
    public event Action<int, int> OnPlayerHealthChanged;
    public event Action OnWeaponSwitched;

    public int Score { get; private set; }
    public event Action<int> OnScoreChanged;
    public event Action<int> OnAmmoChanged;

    public List<WeaponSaveData> PlayerOwnedWeapons { get; set; } = new List<WeaponSaveData>();

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    // ----- State transitions -----
    public void StartGame()
    {
        Score = 0;
        OnScoreChanged?.Invoke(Score);
        PlayerOwnedWeapons.Clear();
        SetState(GameState.Play);
    }
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
        Time.timeScale = (state == GameState.Pause || state == GameState.GameOver || state == GameState.Dialogue) ? 0f : 1f;
    }

    // ----- Health bridge for UI -----
    public void NotifyPlayerHealth(int current, int max)
    {
        OnPlayerHealthChanged?.Invoke(current, max);
        if (current <= 0 && state != GameState.GameOver)
            GameOver();
    }

    public void AddScore(int amount)
    {
        if (amount <= 0) return;
        Score += amount;
        OnScoreChanged?.Invoke(Score);
    }

    public void DeductScore(int amount)
    {
        if (amount <= 0) return;
        Score -= amount;
        if (Score < 0) Score = 0;
        OnScoreChanged?.Invoke(Score);
    }

    public void NotifyAmmoChanged(int currentAmmo)
    {
        OnAmmoChanged?.Invoke(currentAmmo);
    }

    public void NotifyWeaponSwitched()
    {
        OnWeaponSwitched?.Invoke();
    }
}
