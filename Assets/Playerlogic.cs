using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class TankController : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float rotationSpeed = 120f; // grados por segundo

    Rigidbody rb;
    Vector2 input;
    InputAction moveAction;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        moveAction = new InputAction("Move", InputActionType.Value);
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
    }

    void OnEnable() => moveAction?.Enable();
    void OnDisable() => moveAction?.Disable();
    void OnDestroy() => moveAction?.Dispose();

    void Update()
    {
        if (moveAction == null) return;
        input = moveAction.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        Quaternion turn = Quaternion.Euler(0f, input.x * rotationSpeed * Time.fixedDeltaTime, 0f);
        rb.MoveRotation(rb.rotation * turn);

        Vector3 vel = transform.forward * (input.y * moveSpeed);
        vel.y = rb.linearVelocity.y;
        rb.linearVelocity = vel;
    }
}