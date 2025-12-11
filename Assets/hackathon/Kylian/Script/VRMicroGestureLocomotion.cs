using UnityEngine;

public class VRMicroGestureLocomotion : MonoBehaviour
{
    public OVRHand hand;
    public float moveSpeed = 2.0f;
    public Rigidbody rb;
    public Transform cameraTransform; // assign CenterEyeAnchor in Inspector

    private Vector3 moveDirection = Vector3.zero;

    void Update()
    {
        // Read current microgesture
        OVRHand.MicrogestureType mgType = hand.GetMicrogestureType();

        // --- Keep your switch-case exactly as is ---
        switch (mgType)
        {
            case OVRHand.MicrogestureType.SwipeLeft:
                moveDirection = -transform.right;
                break;

            case OVRHand.MicrogestureType.SwipeRight:
                moveDirection = transform.right;
                break;

            case OVRHand.MicrogestureType.SwipeForward:
                moveDirection = transform.forward;
                break;

            case OVRHand.MicrogestureType.SwipeBackward:
                moveDirection = -transform.forward;
                break;

            case OVRHand.MicrogestureType.NoGesture:
                break;
            case OVRHand.MicrogestureType.ThumbTap:
                moveDirection = Vector3.zero;
                break;
            default:
                break;
        }

        // --- Convert moveDirection to be camera-relative ---
        Vector3 camForward = cameraTransform.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = cameraTransform.right;
        camRight.y = 0;
        camRight.Normalize();

        Vector3 cameraRelativeMove = moveDirection.x * camRight + moveDirection.z * camForward;

        // Continuous movement with Rigidbody
        Vector3 newPosition = rb.position + cameraRelativeMove * moveSpeed * Time.deltaTime;
        rb.MovePosition(newPosition);
    }
}
