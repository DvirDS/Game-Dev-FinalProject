using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

/// מציג ויזואלית את הנשק על השחקן ומאתר עוגן "Muzzle" מתוך פריפאב הנשק.
public class WeaponVisualController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private WeaponController weaponController;
    [SerializeField] private Transform weaponMount;
    [SerializeField] private SpriteRenderer ownerSprite;

    [Header("Visual Prefabs (Optional)")]
    [Tooltip("מיפוי אופציונלי: נשק -> פריפאב ויזואלי. אם לא קיים, נשתמש ב-icon של ה-WeaponData.")]
    [SerializeField] private List<WeaponSkin> skins = new();

    [Header("Sorting")]
    [Tooltip("אם ריק, נשתמש בשכבת המיון של השחקן")]
    [SerializeField] private string sortingLayerOverride = "";
    [SerializeField] private int orderInLayerOffset = +1;

    [Header("Mount Offset (Right hand)")]
    [Tooltip("האופסט של הנשק ביד ימין; בצד שמאל נפעיל סימן מינוס על X")]
    [SerializeField] private Vector2 rightHandOffset = new Vector2(0.12f, 0.02f);

    private GameObject currentView;
    private WeaponData lastShown;
    private Vector3 baseViewScale = Vector3.one;

    // ===== חדש: עוגן הקנה שנאתר מתוך ה-view =====
    public Transform CurrentMuzzle { get; private set; }
    private Transform runtimeMuzzleAnchor;

    [Serializable]
    public struct WeaponSkin
    {
        public WeaponData weapon;
        public GameObject prefab; // Prefab עם SpriteRenderer/Animator וכו'
    }

    void Reset()
    {
        weaponController = GetComponentInParent<WeaponController>();
        ownerSprite = GetComponentInParent<SpriteRenderer>();
        if (!weaponMount) weaponMount = transform;
    }

    void Awake()
    {
        if (!weaponController) weaponController = GetComponentInParent<WeaponController>();
        if (!ownerSprite) ownerSprite = GetComponentInParent<SpriteRenderer>();
        if (!weaponMount) weaponMount = transform;
    }

    void Update()
    {
        var current = weaponController ? weaponController.Current : null;
        if (current != lastShown)
        {
            SwapVisual(current);
            lastShown = current;
        }
    }

    void LateUpdate()
    {
        if (!weaponMount || !ownerSprite) return;

        bool flip = ownerSprite.flipX;

        // 1) מיקום המתקן (Mount) – Spiegel לצד שמאל
        weaponMount.localPosition = new Vector3(
            Mathf.Abs(rightHandOffset.x) * (flip ? -1f : 1f),
            rightHandOffset.y,
            weaponMount.localPosition.z
        );

        // 2) היפוך סקייל של ה-view (לא גם flipX וגם סקייל!)
        if (currentView)
        {
            var s = currentView.transform.localScale;
            s.x = Mathf.Abs(baseViewScale.x) * (flip ? -1f : 1f);
            s.y = baseViewScale.y;
            s.z = baseViewScale.z;
            currentView.transform.localScale = s;
        }
    }

    private void SwapVisual(WeaponData data)
    {
        if (currentView) Destroy(currentView);
        currentView = null;
        CurrentMuzzle = null;

        if (!data || !weaponMount) return;

        // האם יש פריפאב ויזואלי ייעודי?
        var skin = skins.FirstOrDefault(x => x.weapon == data);
        GameObject prefab = skin.prefab;

        if (prefab)
        {
            currentView = Instantiate(prefab, weaponMount);
        }
        else if (data.icon)
        {
            currentView = new GameObject($"WeaponView_{data.displayName ?? "Weapon"}");
            currentView.transform.SetParent(weaponMount, false);

            var sr = currentView.AddComponent<SpriteRenderer>();
            sr.sprite = data.icon;

            if (!string.IsNullOrEmpty(sortingLayerOverride))
                sr.sortingLayerName = sortingLayerOverride;
            else if (ownerSprite)
                sr.sortingLayerID = ownerSprite.sortingLayerID;

            sr.sortingOrder = (ownerSprite ? ownerSprite.sortingOrder : 0) + orderInLayerOffset;
        }

        if (currentView)
        {
            currentView.transform.localPosition = Vector3.zero;
            currentView.transform.localRotation = Quaternion.identity;

            baseViewScale = currentView.transform.localScale == Vector3.zero
                ? Vector3.one
                : currentView.transform.localScale;

            // ===== חדש: למצוא עוגן "Muzzle" מתוך פריפאב הנשק =====
            CurrentMuzzle =
                FindChildRecursive(currentView.transform, "Muzzle") ??
                FindChildRecursive(currentView.transform, "MuzzleAnchor");

            if (!CurrentMuzzle)
            {
                // Fallback: ניצור עוגן זמני בקצה ימין של הספרייט (0,0) – תוכל להזיז בפריפאב אחר כך
                if (!runtimeMuzzleAnchor)
                    runtimeMuzzleAnchor = new GameObject("_RuntimeMuzzle").transform;

                runtimeMuzzleAnchor.SetParent(currentView.transform, false);
                runtimeMuzzleAnchor.localPosition = Vector3.zero;
                runtimeMuzzleAnchor.localRotation = Quaternion.identity;
                CurrentMuzzle = runtimeMuzzleAnchor;
            }
        }
    }

    private Transform FindChildRecursive(Transform root, string name)
    {
        if (root.name.Equals(name, StringComparison.OrdinalIgnoreCase)) return root;
        for (int i = 0; i < root.childCount; i++)
        {
            var t = FindChildRecursive(root.GetChild(i), name);
            if (t) return t;
        }
        return null;
    }
}
