using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform muzzle;            // נקודת יציאה
    [SerializeField] private PlayerInputReader input;     // רפרנס ל-Input
    [SerializeField] private string ownerTag = "Player";
    [SerializeField] private SpriteRenderer ownerSprite;  // כדי לדעת אם flipX פעיל

    [Header("Inventory")]
    [SerializeField] private List<WeaponData> loadout = new();
    private int currentIndex;
    private float fireCooldown;

    [Header("Facing/Muzzle")]
    [SerializeField] private bool mirrorMuzzleWithFacing = true;
    private float muzzleDefaultLocalX;

    // חדש: זוכרים את מצב הלחיצה בפריים הקודם כדי לזהות "לחיצה טרייה"
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

        // זיהוי קצה לחיצה (Pressed edge): רק בפריים שבו עברנו מ-לא לחוץ -> לחוץ
        bool firePressedEdge = input.FireHeld && !prevFireHeld;

        // מצב ירי רצוי:
        // אוטומטי => כל עוד לחוץ; חצי-אוטומטי => רק בקצה לחיצה
        bool wantsToFire = w.isAutomatic ? input.FireHeld : firePressedEdge;

        if (wantsToFire && fireCooldown <= 0f)
        {
            Fire(w);
            // קובעים קולדאון לפי fireRate (כדורים לשנייה)
            fireCooldown = Mathf.Max(1f / Mathf.Max(w.fireRate, WeaponData.MIN_FIRE_RATE), 0.01f);
        }

        // עדכון מצב לחיצה לפריים הבא
        prevFireHeld = input.FireHeld;
    }

    private void ApplyMuzzleFacing()
    {
        if (!muzzle || !ownerSprite) return;

        // אם flipX=true – פונים שמאלה
        float sign = ownerSprite.flipX ? -1f : 1f;

        var lp = muzzle.localPosition;
        lp.x = muzzleDefaultLocalX * sign;
        muzzle.localPosition = lp;

        // מסובב את המזלג, כך שציר right יצביע קדימה (ימין=0°, שמאל=180°)
        muzzle.localRotation = Quaternion.Euler(0f, ownerSprite.flipX ? 180f : 0f, 0f);
    }

    private void Fire(WeaponData w)
    {
        if (!w.projectilePrefab || !muzzle) return;

        // פרמטרים לפיזור
        int count = Mathf.Max(1, w.bulletsPerShot);     // כמה קליעים בכל ירייה
        float totalSpread = Mathf.Max(0f, w.spreadAngle);
        float halfSpread = totalSpread * 0.5f;

        // קובע זווית לקליע יחיד או מניפה:
        //  - אם count == 1: סטייה אקראית קלה בתוך ה-spread (ל-SMG מרגיש "ריסוס")
        //  - אם count > 1: פריסה סימטרית ואחידה על פני המניפה כולה
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
            // n קליעים פרוסים על מניפה: מה-(-halfSpread) עד (+halfSpread)
            float step = (count > 1 && totalSpread > 0f) ? (totalSpread / (count - 1)) : 0f;

            for (int i = 0; i < count; i++)
            {
                float angle = -halfSpread + step * i;

                // אופציונלי: "רעידות" קלות בין הקליעים (הסר/השאר לפי טעם)
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

        // SFX/VFX אופציונלי כאן...
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
