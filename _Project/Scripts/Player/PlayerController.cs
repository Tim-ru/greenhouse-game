using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BeltMover))]
public class PlayerController : MonoBehaviour
{
    private BeltMover beltMover;
    private PlayerInputActions input;
    private Vector2 move;

    void Awake()
    {
        beltMover = GetComponent<BeltMover>();
        
        // генерится из .inputactions
        input = new PlayerInputActions();
    }

    void OnEnable()
    {
        input.Enable();
        input.Gameplay.Move.performed += OnMove;
        input.Gameplay.Move.canceled  += OnMove;
    }

    void OnDisable()
    {
        if (input != null)
        {
            input.Gameplay.Move.performed -= OnMove;
            input.Gameplay.Move.canceled  -= OnMove;
            input.Disable();
        }
    }

    void OnMove(InputAction.CallbackContext ctx)
    {
        move = ctx.ReadValue<Vector2>();
    }

    void FixedUpdate()
    {
        // Передаем входные данные в BeltMover для обработки движения
        beltMover.SetInput(move);
    }
}
