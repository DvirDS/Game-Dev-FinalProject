using System.Collections.Generic;
using UnityEngine;

public class NPCShop : MonoBehaviour
{
    [Header("Shop Setup")]
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private List<WeaponData> weaponsForSale;
    [SerializeField] private GameObject interactionPrompt;

    private bool playerInRange = false;
    private PlayerInputReader playerInput; 

    void Start()
    {
        if (shopPanel) shopPanel.SetActive(false);
        if (interactionPrompt) interactionPrompt.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && playerInput != null && playerInput.InteractPressed)
        {
            OpenShop();
        }
    }

    public void OpenShop()
    {
        if (shopPanel)
        {
            shopPanel.SetActive(true);
            shopPanel.GetComponent<ShopPanelController>()?.Bind(this);
        }
        GameManager.I?.SetState(GameManager.GameState.Dialogue);
    }


    public void CloseShop()
    {
        if (shopPanel) shopPanel.SetActive(false);
        GameManager.I?.ResumeGame();
    }

    public void PurchaseWeapon(int weaponIndex)
    {
        if (weaponIndex < 0 || weaponIndex >= weaponsForSale.Count) return;
        WeaponData weaponToBuy = weaponsForSale[weaponIndex];

        var playerWeaponController = playerInput.GetComponent<WeaponController>();
        if (playerWeaponController == null) return;

        if (GameManager.I.Score >= weaponToBuy.price)
        {
            GameManager.I.DeductScore(weaponToBuy.price);
            playerWeaponController.AddWeapon(weaponToBuy, switchToNew: true);
        }
        else
        {
            Debug.Log("Not enough score!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerInput = other.GetComponent<PlayerInputReader>(); 
            if (interactionPrompt) interactionPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerInput = null; 
            if (interactionPrompt) interactionPrompt.SetActive(false);
            CloseShop();
        }
    }
}