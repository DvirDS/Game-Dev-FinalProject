using UnityEngine;

public class LevelExit : MonoBehaviour
{
    [SerializeField] private string sceneToLoad;

    private bool isExiting = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isExiting)
        {
            isExiting = true;

            var weaponController = other.GetComponent<WeaponController>();
            if (weaponController != null)
            {
                weaponController.SaveWeaponsToGameManager();
            }

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