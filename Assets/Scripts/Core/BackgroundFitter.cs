using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundFitter : MonoBehaviour
{
    private SpriteRenderer sr;
    private Camera mainCamera;
    private const float cameraWorldWidthFactor = 2f;
    private const float backgroundScaleFactor = 1f;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (transform.hasChanged || mainCamera.transform.hasChanged)
        {
            FitBackgroundToCamera();
        }
    }

    void FitBackgroundToCamera()
    {
        if (sr == null || sr.sprite == null || mainCamera == null)
        {
            if (sr == null) sr = GetComponent<SpriteRenderer>();
            if (mainCamera == null) mainCamera = Camera.main;
            return;
        }

        float camHeight = mainCamera.orthographicSize * cameraWorldWidthFactor;
        float camWidth = camHeight * mainCamera.aspect;

        float spriteHeight = sr.sprite.bounds.size.y;
        float spriteWidth = sr.sprite.bounds.size.x;

        float scaleX = camWidth / spriteWidth;
        float scaleY = camHeight / spriteHeight;

        float finalScale = Mathf.Max(scaleX, scaleY);

        transform.localScale = new Vector3(finalScale, finalScale, backgroundScaleFactor);
    }
}