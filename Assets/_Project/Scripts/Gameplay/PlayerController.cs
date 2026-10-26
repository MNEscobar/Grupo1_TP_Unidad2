using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Mueve al jugador sobre el plano XZ con WASD o flechas.
/// El input se lee en Update y el movimiento se aplica en FixedUpdate
/// sobre el Rigidbody, para que las colisiones y los triggers con los
/// recolectables funcionen con la física de Unity.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;

    private Rigidbody body;
    private Vector3 input;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();

        // Evita que el jugador se incline al chocar con objetos.
        body.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void Update()
    {
        input = ReadMoveInput();
    }

    private void FixedUpdate()
    {
        Vector3 velocity = input * moveSpeed;
        velocity.y = body.linearVelocity.y;

        body.linearVelocity = velocity;
    }

    private static Vector3 ReadMoveInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return Vector3.zero;

        float x = 0f;
        float z = 0f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            x -= 1f;

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            x += 1f;

        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            z -= 1f;

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            z += 1f;

        // Evita que el movimiento diagonal sea más rápido.
        return Vector3.ClampMagnitude(new Vector3(x, 0f, z), 1f);
    }
}
