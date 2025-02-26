using UnityEngine;

public class OrbitalCamera : MonoBehaviour
{
    [SerializeField] private Transform target;        // The sphere to orbit around
    [SerializeField] private float distance = 50.0f;  // Distance from target
    [SerializeField] private float xSpeed = 120.0f;   // Horizontal orbit speed
    [SerializeField] private float ySpeed = 120.0f;   // Vertical orbit speed
    [SerializeField] private float yMinLimit = -80f;  // Minimum vertical angle
    [SerializeField] private float yMaxLimit = 80f;   // Maximum vertical angle
    [SerializeField] private float zoomSpeed = 4.0f;  // Scrollwheel zoom speed

    private float x = 0.0f;
    private float y = 0.0f;

    void Start()
    {
        if (target == null)
        {
            target = GameObject.Find("Sphere")?.transform;
            if (target == null)
            {
                Debug.LogError("OrbitalCamera: Aucun target trouvé ! Assigné manuellement dans l'Inspector.");
                return;
            }
        }

        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        // Placer immédiatement la caméra dans une position correcte
        transform.position = target.position + new Vector3(0, 0, -distance);
        transform.LookAt(target.position);
    }


    void LateUpdate()
    {
        if (target && Input.GetMouseButton(1)) // Right mouse button
        {
            x += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02f;
            y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;

            // Clamp vertical rotation
            y = ClampAngle(y, yMinLimit, yMaxLimit);
        }

        // Handle zooming with scroll wheel
        distance -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        distance = Mathf.Clamp(distance, 5, 20); // Keep camera between 5 and 20 units from target

        // Convert angles to rotation
        Quaternion rotation = Quaternion.Euler(y, x, 0);

        // Calculate camera position
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
        Vector3 position = rotation * negDistance + target.position;

        // Apply rotation and position
        transform.rotation = rotation;
        transform.position = position;
    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}