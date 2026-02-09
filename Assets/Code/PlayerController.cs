using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputAction m_MoveInput;
    [SerializeField] private InputAction m_MeleeAttackKeyInput;
    [SerializeField] private InputAction m_AreaAttackKeyInput;
    [SerializeField] private InputAction m_UltimateKeyInput;
    [SerializeField] private InputAction m_DistanceAttackKeyInput;
    [SerializeField] private InputAction m_TeleportKeyInput;
    [SerializeField] private InputAction m_BlockKeyInput;

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

        m_MeleeAttackKeyInput.performed += MeleeAttackInput;
        m_MeleeAttackKeyInput.canceled += MeleeAttackInput;
    }

    private void OnEnable()
    {
        m_MoveInput.Enable();

        m_MeleeAttackKeyInput.Enable();
    }

    private void OnDisable()
    {
        m_MoveInput.Disable();

        m_MeleeAttackKeyInput.Disable();
    }

    private void ReadInput(InputAction.CallbackContext _Context)
    {
        var l_Input = _Context.ReadValue<Vector2>();
        m_Movement.SetInputVector(l_Input);
    }

    private void MeleeAttackInput(InputAction.CallbackContext _Context)
    {
        float l_Value = _Context.ReadValue<float>();
        if (l_Value > 0.5f && m_Attack != null)
            m_Attack.MeleeAttack();
    }

    private void AreaAttackInput(InputAction.CallbackContext _Context)
    {
        float l_Value = _Context.ReadValue<float>();
        if (l_Value > 0.5f && m_Attack != null)
            m_Attack.AreaAtack();
    }

    private void UltimateInput(InputAction.CallbackContext _Context)
    {
        float l_Value = _Context.ReadValue<float>();
        if (l_Value > 0.5f && m_Attack != null)
            m_Attack.Ultimate();
    }

    private void DistanceAttackInput(InputAction.CallbackContext _Context)
    {
        float l_Value = _Context.ReadValue<float>();
        if (l_Value > 0.5f && m_Attack != null)
            m_Attack.DistanceAttack();
    }

    private void TeleportAttackInput(InputAction.CallbackContext _Context)
    {
        float l_Value = _Context.ReadValue<float>();
        if (l_Value > 0.5f && m_Attack != null)
            m_Attack.Teleport();
    }

    private void BlockInput(InputAction.CallbackContext _Context)
    {
        float l_Value = _Context.ReadValue<float>();
        if (l_Value > 0.5f && m_Attack != null)
            m_Attack.Block();
    }
}