using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [SerializeField]
    private Transform followTransform;
    [SerializeField]
    private Vector3 offset;
    [SerializeField]
    private float smoothTime;

    [SerializeField]
    private Transform boundry;
    private Camera cam;

    private Vector3 currentVelocity;

    void Awake()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        transform.position = Vector3.SmoothDamp(transform.position, followTransform.position + offset, ref currentVelocity, smoothTime);

        float halfCamHeight = cam.orthographicSize;
        float halfCamWidth = halfCamHeight * cam.aspect;
        float halfBoundryHeight = boundry.localScale.y / 2;
        float halfBoundryWidth = boundry.localScale.x / 2;
        float clampedY = Mathf.Clamp(transform.position.y, boundry.position.y - halfBoundryHeight + halfCamHeight, boundry.position.y + halfBoundryHeight - halfCamHeight);
        float clampedX = Mathf.Clamp(transform.position.x, boundry.position.x - halfBoundryWidth  + halfCamWidth,  boundry.position.x + halfBoundryWidth  - halfCamWidth);
        transform.position = new Vector3(clampedX, clampedY, transform.position.z);
    }
}
