using UnityEngine;

// התכונה הזו מאפשרת לסקריפט לרוץ גם בעורך, כדי שתראה את השינוי מיד
[ExecuteInEditMode]
[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundFitter : MonoBehaviour
{
    private SpriteRenderer sr;
    private Camera mainCamera;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        // בצע את ההתאמה רק אם משהו השתנה, כדי לחסוך בביצועים
        if (transform.hasChanged || mainCamera.transform.hasChanged)
        {
            FitBackgroundToCamera();
        }
    }

    void FitBackgroundToCamera()
    {
        if (sr == null || sr.sprite == null || mainCamera == null)
        {
            // אם אין ספרייט או מצלמה, אין מה לעשות
            if (sr == null) sr = GetComponent<SpriteRenderer>();
            if (mainCamera == null) mainCamera = Camera.main;
            return;
        }

        // 1. חשב את גובה ורוחב המצלמה בעולם המשחק
        float camHeight = mainCamera.orthographicSize * 2f;
        float camWidth = camHeight * mainCamera.aspect;

        // 2. קבל את גובה ורוחב התמונה המקוריים
        float spriteHeight = sr.sprite.bounds.size.y;
        float spriteWidth = sr.sprite.bounds.size.x;

        // 3. חשב את יחס הגודל הדרוש כדי למלא את המסך
        float scaleX = camWidth / spriteWidth;
        float scaleY = camHeight / spriteHeight;

        // 4. בחר את יחס הגודל הגדול יותר כדי להבטיח שהרקע מכסה הכל,
        // גם אם חלק ממנו יוצא מהמסך (עדיף על פני פסים ריקים)
        float finalScale = Mathf.Max(scaleX, scaleY);

        // 5. החל את הגודל החדש על הרקע
        transform.localScale = new Vector3(finalScale, finalScale, 1f);
    }
}