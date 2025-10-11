// ShopPanelController.cs
using UnityEngine;
using UnityEngine.UI;

public class ShopPanelController : MonoBehaviour
{
    [SerializeField] private Button[] buyButtons;
    [SerializeField] private Button closeButton;

    NPCShop current;

    public void Bind(NPCShop shop)
    {
        current = shop;

        for (int i = 0; i < buyButtons.Length; i++)
        {
            int idx = i;
            buyButtons[i].onClick.RemoveAllListeners();
            buyButtons[i].onClick.AddListener(() => current.PurchaseWeapon(idx));
        }

        if (closeButton)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => current.CloseShop());
        }
    }
}
