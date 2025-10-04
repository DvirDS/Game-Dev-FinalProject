using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// WeaponController:
/// - Fires projectiles using the per-weapon muzzle (from WeaponVisualController).
/// - Rebinds muzzle-flash Animator per shot so it always matches the active weapon.
/// - Handles weapon switching.
public class WeaponController : MonoBehaviour
{
    [Header("Input / Owner")]
    [SerializeField] private PlayerInputReader input;
    [SerializeField] private string ownerTag = "Player";

    [Header("Visuals (per-weapon muzzle)")]
    [SerializeField] private WeaponVisualController visuals;  // drag the component from Player
    [Tooltip("Fallback only: used if visuals or its CurrentMuzzle is missing")]
    [SerializeField] private Transform muzzle;

    [Header("Muzzle Flash (optional)")]
    [SerializeField] private string muzzleFlashStateName = "MuzzleFlash";
    [SerializeField] private bool muzzleFlashNormallyHidden = true;
    [SerializeField] private float muzzleFlashAutoHideDelay = 0.08f;

    [Header("Inventory")]
    [SerializeField] private List<WeaponData> loadout = new();
    private int currentIndex;
    private float fireCooldown;
    private bool prevFireHeld;

    // -------- Utils --------
    private Transform GetMuzzle()
    {
        if (visuals && visuals.CurrentMuzzle) return visuals.CurrentMuzzle;
        return muzzle;
    }

    // -------- Unity --------
    private void Awake()
    {
        if (!input) input = GetComponentInParent<PlayerInputReader>();
    }

    private void Update()
    {
        if (loadout.Count == 0) return;

        var w = loadout[currentIndex];
        fireCooldown -= Time.deltaTime;

        if (input.SwitchNextPressed) Switch(+1);
        if (input.SwitchPrevPressed) Switch(-1);

        bool firePressedEdge = input.FireHeld && !prevFireHeld;
        bool wantsToFire = w.isAutomatic ? input.FireHeld : firePressedEdge;

        if (wantsToFire && fireCooldown <= 0f)
        {
            Fire(w);
            float safeRate = Mathf.Max(w.fireRate, 0.01f);
            fireCooldown = 1f / safeRate;
        }

        prevFireHeld = input.FireHeld;
    }

    // -------- Firing --------
    private void Fire(WeaponData w)
    {
        var m = GetMuzzle();
        if (!m || !w || !w.projectilePrefab) return;

        int count = Mathf.Max(1, w.bulletsPerShot);
        float totalSpread = Mathf.Max(0f, w.spreadAngle);
        float halfSpread = totalSpread * 0.5f;

        // Base direction:
        // use sign of global X-scale so shots flip when the weapon is mirrored.
        float sign = Mathf.Sign(m.lossyScale.x);
        Vector3 baseDir = (sign >= 0f) ? m.right : -m.right;

        if (count == 1)
        {
            float ang = (totalSpread > 0f) ? Random.Range(-halfSpread, halfSpread) : 0f;
            Quaternion spreadRot = Quaternion.AngleAxis(ang, Vector3.forward);
            ShootOne(w, m.position, m.rotation, spreadRot * baseDir);
        }
        else
        {
            float step = (count > 1 && totalSpread > 0f) ? (totalSpread / (count - 1)) : 0f;
            for (int i = 0; i < count; i++)
            {
                float ang = -halfSpread + step * i;
                Quaternion spreadRot = Quaternion.AngleAxis(ang, Vector3.forward);
                ShootOne(w, m.position, m.rotation, spreadRot * baseDir);
            }
        }

        PlayMuzzleFlash(); // FX
    }

    private void ShootOne(WeaponData w, Vector3 pos, Quaternion rot, Vector3 dir)
    {
        var go = Instantiate(w.projectilePrefab, pos, rot);

        if (go.TryGetComponent<Projectile>(out var proj))
        {
            proj.damage = w.damage;
            proj.speed = w.muzzleVelocity;
            proj.ownerTag = ownerTag;
            proj.Launch(dir);
        }
        else
        {
            var rb = go.GetComponent<Rigidbody2D>();
            if (rb)
            {
                rb.gravityScale = 0f;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                rb.linearVelocity = dir.normalized * w.muzzleVelocity;
            }
        }
    }

    // -------- Muzzle Flash --------
    private void PlayMuzzleFlash()
    {
        // Always fetch the animator from the *current* muzzle, in case weapon changed.
        Animator anim = null;
        var m = GetMuzzle();
        if (m) anim = m.GetComponentInChildren<Animator>(true);
        if (!anim) return;

        var sr = anim.GetComponent<SpriteRenderer>();
        if (muzzleFlashNormallyHidden && sr) sr.enabled = true;

        if (!string.IsNullOrEmpty(muzzleFlashStateName))
            anim.Play(muzzleFlashStateName, -1, 0f);
        else
            anim.Play(0, -1, 0f);

        if (muzzleFlashNormallyHidden && muzzleFlashAutoHideDelay > 0f)
            StartCoroutine(HideMuzzleFlashAfterDelay(anim, muzzleFlashAutoHideDelay));
    }

    private IEnumerator HideMuzzleFlashAfterDelay(Animator anim, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (anim)
        {
            var sr = anim.GetComponent<SpriteRenderer>();
            if (sr) sr.enabled = false;
        }
    }

    // -------- Inventory --------
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
