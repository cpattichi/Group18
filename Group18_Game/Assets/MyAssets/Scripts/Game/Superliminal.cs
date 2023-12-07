using UnityEngine;
using UnityEngine.UI;

public class Superliminal : MonoBehaviour
{
    [Header("Components")]
    public Transform target;            // The target object we picked up for scaling
    public Image crosshairImage;        // The crosshair UI image

    [Header("Parameters")]
    public LayerMask targetMask;        // The layer mask used to hit only potential targets with a raycast
    public LayerMask ignoreTargetMask;  // The layer mask used to ignore the player and target objects while raycasting
    public float offsetFactor;          // The offset amount for positioning the object so it doesn't clip into walls
    public float collisionCheckDistance = 0.05f;  // The distance to check for collisions to prevent clipping

    float originalDistance;             // The original distance between the player camera and the target
    float originalScale;                // The original scale of the target objects prior to being resized
    Vector3 targetScale;                // The scale we want our object to be set to each frame

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Initialize the crosshair
        InitializeCrosshair();
    }

    void InitializeCrosshair()
    {
        // Check if a crosshair image is assigned
        if (crosshairImage != null)
        {
            // Set the crosshair to the center of the screen
            crosshairImage.rectTransform.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        }
        else
        {
            Debug.LogWarning("Crosshair image is not assigned!");
        }
    }

    void Update()
    {
        HandleInput();
        ResizeTarget();

        // Update crosshair position
        UpdateCrosshairPosition();
    }

    void HandleInput()
    {
        // Check for left mouse click
        if (Input.GetMouseButtonDown(0))
        {
            // If we do not currently have a target
            if (target == null)
            {
                // Fire a raycast with the layer mask that only hits potential targets
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, targetMask))
                {
                    // Set our target variable to be the Transform object we hit with our raycast
                    target = hit.transform;

                    // Disable physics for the object
                    target.GetComponent<Rigidbody>().isKinematic = true;

                    // Calculate the distance between the camera and the object
                    originalDistance = Vector3.Distance(transform.position, target.position);

                    // Save the original scale of the object into our originalScale Vector3 variable
                    originalScale = target.localScale.x;

                    // Set our target scale to be the same as the original for the time being
                    targetScale = target.localScale;
                }
            }
            // If we DO have a target
            else
            {
                // Reactivate physics for the target object
                target.GetComponent<Rigidbody>().isKinematic = false;

                // Set our target variable to null
                target = null;
            }
        }
    }

    void UpdateCrosshairPosition()
    {
        // Check if a crosshair image is assigned
        if (crosshairImage != null)
        {
            // Set the crosshair to the center of the screen
            crosshairImage.rectTransform.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        }
    }

    void ResizeTarget()
    {
        // If our target is null
        if (target == null)
        {
            // Return from this method, nothing to do here
            return;
        }

        // Cast a ray forward from the camera position, ignore the layer that is used to acquire targets
        // so we don't hit the attached target with our ray
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity, ignoreTargetMask))
        {
            // Check if the ray hit a wall or object
            if (hit.collider != null && hit.collider.gameObject != target.gameObject)
            {
                // Adjust the position to prevent the object from going through other objects
                target.position = hit.point - transform.forward * offsetFactor * targetScale.x;

                // Check for collisions in the adjusted position
                if (CheckForCollisions())
                {
                    // If there's a collision, revert to the previous position
                    target.position = hit.point - transform.forward * offsetFactor * targetScale.x;
                }
            }
            else
            {
                // Set the new position of the target by getting the hit point and moving it back a bit
                // depending on the scale and offset factor
                target.position = hit.point - transform.forward * offsetFactor * targetScale.x;

                // Check for collisions in the adjusted position
                if (CheckForCollisions())
                {
                    // If there's a collision, revert to the previous position
                    target.position = hit.point - transform.forward * offsetFactor * targetScale.x;
                }
            }

            // Calculate the current distance between the camera and the target object
            float currentDistance = Vector3.Distance(transform.position, target.position);

            // Calculate the ratio between the current distance and the original distance
            float s = currentDistance / originalDistance;

            // Set the scale Vector3 variable to be the ratio of the distances
            targetScale.x = targetScale.y = targetScale.z = s;

            // Set the scale for the target object, multiplied by the original scale
            target.localScale = targetScale * originalScale;
        }
    }

    bool CheckForCollisions()
    {
        // Perform a raycast to check for collisions in front of the target
        RaycastHit hit;
        if (Physics.Raycast(target.position, transform.forward, out hit, collisionCheckDistance, ignoreTargetMask))
        {
            // If a collision is detected, return true
            return true;
        }

        // No collision detected
        return false;
    }
}
