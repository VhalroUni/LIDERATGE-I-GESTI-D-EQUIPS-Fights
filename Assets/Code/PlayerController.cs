using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputAction m_MoveInput;
    [SerializeField] private InputAction m_AttackKeyInput;

    private PlayerMovement m_Movement;
    private PlayerAttack m_Attack;

    private void Start()
    {
        m_Movement = GetComponent<PlayerMovement>();
        m_Attack = GetComponent<PlayerAttack>();
    }

    private void Awake()
    {
        m_MoveInput.performed += ReadInput;
        m_MoveInput.canceled += ReadInput;

        m_AttackKeyInput.performed += AttackInput;
        m_AttackKeyInput.canceled += AttackInput;
    }

    private void OnEnable()
    {
        m_MoveInput.Enable();

        m_AttackKeyInput.Enable();
    }

    private void OnDisable()
    {
        m_MoveInput.Disable();

        m_AttackKeyInput.Disable();
    }

    private void ReadInput(InputAction.CallbackContext _Context)
    {
        var l_Input = _Context.ReadValue<Vector2>();
        m_Movement.SetInputVector(l_Input);
    }

    private void AttackInput(InputAction.CallbackContext _Context)
    {
        float l_Value = _Context.ReadValue<float>();
        if (l_Value > 0.5f && m_Attack != null)
            m_Attack.Attack();
    }
}