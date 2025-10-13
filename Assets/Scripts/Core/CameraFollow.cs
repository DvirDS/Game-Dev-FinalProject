using UnityEngine;

[DefaultExecutionOrder(0)] 
public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float offsetX = -37.21f;

    [Header("Camera Bounds")]
    [SerializeField] float minX;
    [SerializeField] float maxX;

    [Header("Smoothing")]
    [SerializeField, Min(0.01f)] float smoothTime = 0.12f;

    [Header("Pixel Snap (optional)")]
    [SerializeField] bool pixelSnap = false;       
    [SerializeField, Min(1f)] float pixelsPerUnit = 16f;

    float sampledTargetX, velX;

    void Awake()
    {
        sampledTargetX = transform.position.x;
    }

    void FixedUpdate()
    {
        if (!target) return;
        sampledTargetX = target.position.x + offsetX;
    }

    void LateUpdate()
    {
        if (!target) return;

        float desiredX = Mathf.Clamp(sampledTargetX, minX, maxX);
        float newX = Mathf.SmoothDamp(transform.position.x, desiredX, ref velX, smoothTime);

        if (pixelSnap && pixelsPerUnit > 0f)
            newX = Mathf.Round(newX * pixelsPerUnit) / pixelsPerUnit;

        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }
}
