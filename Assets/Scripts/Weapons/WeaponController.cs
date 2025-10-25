using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class WeaponController : MonoBehaviour
{
    private class OwnedWeapon
    {
        public WeaponData Data;
        public int CurrentAmmo;

        public OwnedWeapon(WeaponData data)
        {
            Data = data;
            CurrentAmmo = data.ammoPerPurchase;
        }
    }

    [Header("Firing Logic")]
    [SerializeField] private LayerMask obstacleMask;

    [Header("Input / Owner")]
    [SerializeField] private PlayerInputReader input;
    [SerializeField] private string ownerTag = "Player";

    [Header("Visuals (per-weapon muzzle)")]
    [SerializeField] private WeaponVisualController visuals;
    [Tooltip("Fallback only: used if visuals or its CurrentMuzzle is missing")]
    [SerializeField] private Transform muzzle;

    [Header("Muzzle Flash (optional)")]
    [SerializeField] private string muzzleFlashStateName = "MuzzleFlash";
    [SerializeField] private bool muzzleFlashNormallyHidden = true;
    [SerializeField] private float muzzleFlashAutoHideDelay = 0.08f;

    [Header("Inventory")]
    [SerializeField] private WeaponData startingWeapon;
    private List<OwnedWeapon> loadout = new();
    private int currentIndex;
    private float fireCooldown;
    private bool prevFireHeld;

    public WeaponData Current => (loadout.Count > 0 && currentIndex < loadout.Count) ? loadout[currentIndex].Data : null;

    private Transform GetMuzzle()
    {
        if (visuals && visuals.CurrentMuzzle) return visuals.CurrentMuzzle;
        return muzzle;
    }

    private void Awake()
    {
        if (!input) input = GetComponentInParent<PlayerInputReader>();
    }

    void Start()
    {
        loadout.Clear();
        if (GameManager.I != null && GameManager.I.PlayerOwnedWeapons.Count > 0)
        {
            foreach (var savedWeapon in GameManager.I.PlayerOwnedWeapons)
            {
                var weaponInstance = new OwnedWeapon(savedWeapon.Weapon);
                weaponInstance.CurrentAmmo = savedWeapon.Ammo;
                loadout.Add(weaponInstance);
            }
            EquipWeapon(0);
        }
        else
        {
            if (startingWeapon != null)
            {
                AddWeapon(startingWeapon, true);
            }
        }
        UpdateAmmoUI();
    }

    private void Update()
    {
        if (GameManager.I != null && GameManager.I.State != GameManager.GameState.Play)
        {
            return;
        }
        if (loadout.Count == 0) return;

        var currentOwnedWeapon = loadout[currentIndex];
        var w = currentOwnedWeapon.Data;
        fireCooldown -= Time.deltaTime;

        if (input.SwitchNextPressed) Switch(+1);
        if (input.SwitchPrevPressed) Switch(-1);

        bool firePressedEdge = input.FireHeld && !prevFireHeld;
        bool wantsToFire = w.isAutomatic ? input.FireHeld : firePressedEdge;

        if (wantsToFire && fireCooldown <= 0f)
        {
            if (!w.hasInfiniteAmmo && currentOwnedWeapon.CurrentAmmo <= 0)
            {
                Debug.Log("Out of ammo for " + w.displayName);
            }
            else
            {
                Fire(w);

                if (!w.hasInfiniteAmmo)
                {
                    currentOwnedWeapon.CurrentAmmo--;
                }

                float safeRate = Mathf.Max(w.fireRate, 0.01f);
                fireCooldown = 1f / safeRate;
                UpdateAmmoUI();
            }
        }

        prevFireHeld = input.FireHeld;
    }

    public void AddWeapon(WeaponData data, bool switchToNew = false)
    {
        if (!data) return;

        OwnedWeapon existingWeapon = loadout.FirstOrDefault(w => w.Data == data);

        if (existingWeapon != null)
        {
            existingWeapon.CurrentAmmo += data.ammoPerPurchase;
        }
        else
        {
            loadout.Add(new OwnedWeapon(data));
            if (switchToNew)
            {
                EquipWeapon(loadout.Count - 1);
            }
        }
        UpdateAmmoUI();
    }

    private void EquipWeapon(int index)
    {
        if (index < 0 || index >= loadout.Count) return;

        currentIndex = index;
        UpdateAmmoUI();
        GameManager.I?.NotifyWeaponSwitched();
    }

    private void Switch(int dir)
    {
        if (loadout.Count <= 1) return;
        int newIndex = (currentIndex + dir + loadout.Count) % loadout.Count;
        EquipWeapon(newIndex);
    }

    private void UpdateAmmoUI()
    {
        int ammoToShow = 0;
        if (Current != null)
        {
            ammoToShow = Current.hasInfiniteAmmo ? -1 : loadout[currentIndex].CurrentAmmo;
        }
        GameManager.I?.NotifyAmmoChanged(ammoToShow);
    }

    public void SaveWeaponsToGameManager()
    {
        if (GameManager.I != null)
        {
            GameManager.I.PlayerOwnedWeapons.Clear();
            foreach (var ownedWeapon in loadout)
            {
                GameManager.I.PlayerOwnedWeapons.Add(new WeaponSaveData
                {
                    Weapon = ownedWeapon.Data,
                    Ammo = ownedWeapon.CurrentAmmo
                });
            }
        }
    }

    private void Fire(WeaponData w)
    {
        var m = GetMuzzle();
        if (!m || !w || !w.projectilePrefab) return;

        Vector2 raycastOrigin = transform.position;
        Vector2 targetMuzzlePos = m.position;
        Vector2 directionToMuzzle = targetMuzzlePos - raycastOrigin;
        float distanceToMuzzle = directionToMuzzle.magnitude;
        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, directionToMuzzle.normalized, distanceToMuzzle, obstacleMask);
        Vector3 spawnPosition = hit.collider ? (Vector3)(hit.point + hit.normal * 0.01f) : m.position;
        int count = Mathf.Max(1, w.bulletsPerShot);
        float totalSpread = Mathf.Max(0f, w.spreadAngle);
        float halfSpread = totalSpread * 0.5f;
        float sign = Mathf.Sign(m.lossyScale.x);
        Vector3 baseDir = (sign >= 0f) ? m.right : -m.right;

        if (count == 1)
        {
            float ang = (totalSpread > 0f) ? Random.Range(-halfSpread, halfSpread) : 0f;
            Quaternion spreadRot = Quaternion.AngleAxis(ang, Vector3.forward);
            ShootOne(w, spawnPosition, m.rotation, spreadRot * baseDir);
        }
        else
        {
            float step = (count > 1 && totalSpread > 0f) ? (totalSpread / (count - 1)) : 0f;
            for (int i = 0; i < count; i++)
            {
                float ang = -halfSpread + step * i;
                Quaternion spreadRot = Quaternion.AngleAxis(ang, Vector3.forward);
                ShootOne(w, spawnPosition, m.rotation, spreadRot * baseDir);
            }
        }
        PlayMuzzleFlash();
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

    private void PlayMuzzleFlash()
    {
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
}