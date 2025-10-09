using UnityEngine;

public class LevelStartManager : MonoBehaviour
{
    void Start()
    {
        GameManager.I?.StartGame();
    }
}