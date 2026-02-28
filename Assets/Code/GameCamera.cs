using UnityEngine;

public class GameCamera : MonoBehaviour
{
    public Transform target1;
    public Transform target2;
    public Vector3 offset;

    public float smoothSpeed = 5f;
    public float screenEdgeBuffer = 2f;
    public float minZoom = 5f;
    public float maxZoom = 15f;

    // 🔥 NUEVAS VARIABLES PARA EL FONDO
    public Transform backgroundTransform;
    public SpriteRenderer backgroundRenderer;

    [Header("Fondo cuando están cerca")]
    public Vector3 closeScaleMultiplier = new Vector3(0.9f, 0.9f, 1f);
    public Color closeColor = new Color(0.7f, 0.7f, 0.7f, 1f);

    private Vector3 originalScale;
    private Color originalColor;

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();

        if (backgroundTransform != null)
            originalScale = backgroundTransform.localScale;

        if (backgroundRenderer != null)
            originalColor = backgroundRenderer.color;
    }

    void LateUpdate()
    {
        if (target1 == null || target2 == null) return;

        MoveCamera();
        ZoomCamera();
    }

    void MoveCamera()
    {
        Vector3 middlePoint = (target1.position + target2.position) / 2f;

        Vector3 desiredPosition = new Vector3(middlePoint.x + offset.x, middlePoint.y + offset.y, offset.z);

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }

    void ZoomCamera()
    {
        float width = Mathf.Max(target1.position.x, target2.position.x) - Mathf.Min(target1.position.x, target2.position.x);
        float height = Mathf.Max(target1.position.y, target2.position.y) - Mathf.Min(target1.position.y, target2.position.y);

        float aspectRatio = cam.aspect;

        float requiredSizeX = (width / aspectRatio) / 2f;
        float requiredSizeY = height / 2f;

        float requiredSize = Mathf.Max(requiredSizeX, requiredSizeY);

        float newZoom = requiredSize + screenEdgeBuffer;

        newZoom = Mathf.Clamp(newZoom, minZoom, maxZoom);

        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, newZoom, Time.deltaTime * smoothSpeed);

        // 🔥 EFECTO FONDO BASADO EN ZOOM
        float zoom01 = Mathf.InverseLerp(maxZoom, minZoom, cam.orthographicSize);

        if (backgroundTransform != null)
        {
            Vector3 targetScale = Vector3.Scale(originalScale, closeScaleMultiplier);
            backgroundTransform.localScale = Vector3.Lerp(originalScale, targetScale, zoom01);
        }

        if (backgroundRenderer != null)
        {
            backgroundRenderer.color = Color.Lerp(originalColor, closeColor, zoom01);
        }
    }
}