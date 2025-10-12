using UnityEngine;

public class LevelExit : MonoBehaviour
{
    [SerializeField] private string sceneToLoad; // כאן נכניס את שם השלב הבא

    private bool isExiting = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // ודא שהשחקן הוא זה שנכנס ושלא התחלנו כבר את המעבר
        if (other.CompareTag("Player") && !isExiting)
        {
            isExiting = true;

            // --- זה החלק הכי חשוב ---
            // 1. מצא את בקר הנשקים של השחקן
            var weaponController = other.GetComponent<WeaponController>();
            if (weaponController != null)
            {
                // 2. שמור את רשימת הנשקים הנוכחית ב-GameManager
                weaponController.SaveWeaponsToGameManager();
            }

            // 3. מצא את ה-SceneLoader וטען את השלב הבא
            var sceneLoader = FindFirstObjectByType<SceneLoader>();
            if (sceneLoader != null)
            {
                sceneLoader.LoadScene(sceneToLoad);
            }
            else
            {
                Debug.LogError("SceneLoader not found in the scene!");
            }
        }
    }
}