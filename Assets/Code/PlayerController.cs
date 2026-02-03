using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputAction m_Move;

    private PlayerMovement m_Movement;

    private void Start()
    {
        m_Movement = GetComponent<PlayerMovement>();
    }

    private void Awake()
    {
        m_Move.performed += ReadInput;
        m_Move.canceled += ReadInput;
    }

    private void OnEnable()
    {
        m_Move.Enable();
    }

    private void OnDisable()
    {
        m_Move.Disable();
    }

    private void ReadInput(InputAction.CallbackContext _Context)
    {
        var l_Input = _Context.ReadValue<Vector2>();
        m_Movement.SetInputVector(l_Input);
    }
}
