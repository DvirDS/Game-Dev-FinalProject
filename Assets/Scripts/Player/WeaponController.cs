using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform muzzle;            // ����� �����
    [SerializeField] private PlayerInputReader input;     // ����� �-Input
    [SerializeField] private string ownerTag = "Player";
    [SerializeField] private SpriteRenderer ownerSprite;  // ��� ���� �� flipX ����

    [Header("Inventory")]
    [SerializeField] private List<WeaponData> loadout = new();
    private int currentIndex;
    private float fireCooldown;

    [Header("Facing/Muzzle")]
    [SerializeField] private bool mirrorMuzzleWithFacing = true;
    private float muzzleDefaultLocalX;

    // ���: ������ �� ��� ������ ������ ����� ��� ����� "����� �����"
    private bool prevFireHeld;

    void Awake()
    {
        if (!input) input = GetComponentInParent<PlayerInputReader>();
        if (!ownerSprite) ownerSprite = GetComponentInParent<SpriteRenderer>();
        if (muzzle) muzzleDefaultLocalX = Mathf.Abs(muzzle.localPosition.x);
    }

    void Update()
    {
        if (loadout.Count == 0) return;

        if (mirrorMuzzleWithFacing) ApplyMuzzleFacing();

        var w = loadout[currentIndex];
        fireCooldown -= Time.deltaTime;

        if (input.SwitchNextPressed) Switch(+1);
        if (input.SwitchPrevPressed) Switch(-1);

        // ����� ��� ����� (Pressed edge): �� ������ ��� ����� �-�� ���� -> ����
        bool firePressedEdge = input.FireHeld && !prevFireHeld;

        // ��� ��� ����:
        // ������� => �� ��� ����; ���-������� => �� ���� �����
        bool wantsToFire = w.isAutomatic ? input.FireHeld : firePressedEdge;

        if (wantsToFire && fireCooldown <= 0f)
        {
            Fire(w);
            // ������ ������� ��� fireRate (������ ������)
            fireCooldown = Mathf.Max(1f / Mathf.Max(w.fireRate, WeaponData.MIN_FIRE_RATE), 0.01f);
        }

        // ����� ��� ����� ������ ���
        prevFireHeld = input.FireHeld;
    }

    private void ApplyMuzzleFacing()
    {
        if (!muzzle || !ownerSprite) return;

        // �� flipX=true � ����� �����
        float sign = ownerSprite.flipX ? -1f : 1f;

        var lp = muzzle.localPosition;
        lp.x = muzzleDefaultLocalX * sign;
        muzzle.localPosition = lp;

        // ����� �� �����, �� ���� right ����� ����� (����=0�, ����=180�)
        muzzle.localRotation = Quaternion.Euler(0f, ownerSprite.flipX ? 180f : 0f, 0f);
    }

    private void Fire(WeaponData w)
    {
        if (!w.projectilePrefab || !muzzle) return;

        // ������� ������
        int count = Mathf.Max(1, w.bulletsPerShot);     // ��� ������ ��� �����
        float totalSpread = Mathf.Max(0f, w.spreadAngle);
        float halfSpread = totalSpread * 0.5f;

        // ���� ����� ����� ���� �� �����:
        //  - �� count == 1: ����� ������ ��� ���� �-spread (�-SMG ����� "�����")
        //  - �� count > 1: ����� ������� ������ �� ��� ������ ����
        if (count == 1)
        {
            float randomAngle = totalSpread > 0f ? Random.Range(-halfSpread, halfSpread) : 0f;
            Quaternion spreadRot = Quaternion.Euler(0f, 0f, randomAngle);
            Vector3 dir = spreadRot * muzzle.right;

            var go = Instantiate(w.projectilePrefab, muzzle.position, muzzle.rotation);
            if (go.TryGetComponent<Projectile>(out var proj))
            {
                proj.damage = w.damage;
                proj.speed = w.muzzleVelocity;
                proj.ownerTag = ownerTag;
                proj.Launch(dir);
            }
        }
        else
        {
            // n ������ ������ �� �����: ��-(-halfSpread) �� (+halfSpread)
            float step = (count > 1 && totalSpread > 0f) ? (totalSpread / (count - 1)) : 0f;

            for (int i = 0; i < count; i++)
            {
                float angle = -halfSpread + step * i;

                // ���������: "������" ���� ��� ������� (���/���� ��� ���)
                // angle += (step > 0f ? Random.Range(-step * 0.15f, step * 0.15f) : 0f);

                Quaternion spreadRot = Quaternion.Euler(0f, 0f, angle);
                Vector3 dir = spreadRot * muzzle.right;

                var go = Instantiate(w.projectilePrefab, muzzle.position, muzzle.rotation);
                if (go.TryGetComponent<Projectile>(out var proj))
                {
                    proj.damage = w.damage;
                    proj.speed = w.muzzleVelocity;
                    proj.ownerTag = ownerTag;
                    proj.Launch(dir);
                }
            }
        }

        // SFX/VFX ��������� ���...
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
