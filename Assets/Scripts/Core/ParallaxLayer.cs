// ParallaxLayer2D.cs
using UnityEngine;

[ExecuteAlways]
public class ParallaxLayer2D : MonoBehaviour
{
    public Camera cam;                       // אם ריק => ניקח Camera.main
    [Tooltip("כמה השכבה זזה יחסית למצלמה. Far ~0.1-0.25, Mid ~0.4-0.6, Near ~0.75-0.9")]
    public Vector2 multiplier = new Vector2(0.25f, 0f);
    [Tooltip("לנעול פרלקסה בציר Y (בדו־מימד צד)")]
    public bool lockY = true;

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
        // הפרש תזוזת המצלמה מאז ההתחלה
        Vector3 camDelta = cam.transform.position - camStartPos;

        float x = startPos.x + camDelta.x * multiplier.x;
        float y = startPos.y + (lockY ? 0f : camDelta.y * multiplier.y);
        transform.position = new Vector3(x, y, startPos.z);
    }
}
