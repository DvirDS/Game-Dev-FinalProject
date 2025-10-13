using UnityEngine;

[ExecuteAlways]
[DefaultExecutionOrder(100)]
public class ParallaxLayer2D : MonoBehaviour
{
    public Camera cam;
    public Vector2 multiplier = new Vector2(0.25f, 0f);
    public bool lockY = true;

    [Header("Pixel Snap (optional)")]
    public bool pixelSnap = false;     
    [Min(1f)] public float pixelsPerUnit = 16f;

    Vector3 startPos;
    Vector3 camStartPos;

    void Start()
    {
        if (!cam) cam = Camera.main;
        startPos = transform.position;
        if (cam) camStartPos = cam.transform.position;
    }

    void LateUpdate()
    {
        if (!cam) { cam = Camera.main; if (!cam) return; }

        Vector3 camDelta = cam.transform.position - camStartPos;

        float x = startPos.x + camDelta.x * multiplier.x;
        float y = startPos.y + (lockY ? 0f : camDelta.y * multiplier.y);

        if (pixelSnap && pixelsPerUnit > 0f)
        {
            x = Mathf.Round(x * pixelsPerUnit) / pixelsPerUnit;
            y = Mathf.Round(y * pixelsPerUnit) / pixelsPerUnit;
        }

        transform.position = new Vector3(x, y, startPos.z);
    }
}
