using UnityEngine;

public class NPCWeaponGiver : MonoBehaviour
{
    [SerializeField] private WeaponData specialWeapon;

    // קריאה מבחוץ כשיש אינטראקציה (למשל באמצעות Input)
    public void GiveWeapon(WeaponController target)
    {
        if (!specialWeapon || !target) return;
        target.AddWeapon(specialWeapon, switchToNew: true);
    }
}
