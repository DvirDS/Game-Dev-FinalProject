using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float followSpeed = 5f;
    [SerializeField] float offsetX = -37.21f;

    [Header("Camera Bounds")]
    [SerializeField] float minX;
    [SerializeField] float maxX;

    void LateUpdate()
    {
        if (!target) return;

        float targetX = target.position.x + offsetX;
        targetX = Mathf.Clamp(targetX, minX, maxX);
        Vector3 newPos = new Vector3(
            targetX,
            transform.position.y,
            transform.position.z
        );
        transform.position = Vector3.Lerp(transform.position, newPos, followSpeed * Time.deltaTime);
    }
}
