using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform muzzle;          // נקודת יציאה
    [SerializeField] private PlayerInputReader input;   // רפרנס ל-Input
    [SerializeField] private string ownerTag = "Player";

    [Header("Inventory")]
    [SerializeField] private List<WeaponData> loadout = new();
    private int currentIndex;
    private float fireCooldown;

    void Update()
    {
        if (loadout.Count == 0) return;

        var w = loadout[currentIndex];
        fireCooldown -= Time.deltaTime;

        if (input.SwitchNextPressed) Switch(+1);
        if (input.SwitchPrevPressed) Switch(-1);

        bool wantsToFire = w.isAutomatic ? input.FireHeld : (input.FireHeld && fireCooldown <= 0f);
        if (wantsToFire && fireCooldown <= 0f)
        {
            Fire(w);
            fireCooldown = Mathf.Max(1f / Mathf.Max(w.fireRate, WeaponData.MIN_FIRE_RATE), 0.01f);
        }
    }

    private void Fire(WeaponData w)
    {
        if (!w.projectilePrefab || !muzzle) return;

        var go = Instantiate(w.projectilePrefab, muzzle.position, muzzle.rotation);
        if (go.TryGetComponent<Projectile>(out var proj))
        {
            proj.damage = w.damage;
            proj.speed = w.muzzleVelocity;
            proj.ownerTag = ownerTag;
            proj.Launch(muzzle.right); // חשוב: כיוון לפי ציר +X של muzzle
        }

        // SFX, VFX אופציונלי
    }

    private void Switch(int dir)
    {
        if (loadout.Count <= 1) return;
        currentIndex = (currentIndex + dir + loadout.Count) % loadout.Count;
        GameManager.I?.NotifyWeaponSwitched();
    }

    public void AddWeapon(WeaponData data, bool switchToNew = false)
    {
        if (!data) return;
        loadout.Add(data);
        if (switchToNew) currentIndex = loadout.Count - 1;
        GameManager.I?.NotifyWeaponSwitched();
    }

    public WeaponData Current => loadout.Count > 0 ? loadout[currentIndex] : null;
}
