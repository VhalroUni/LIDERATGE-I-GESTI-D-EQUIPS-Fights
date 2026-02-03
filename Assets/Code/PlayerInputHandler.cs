using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInput m_PlayerInput;
    private PlayerMovement m_PlayerMovement;
    private PlayerAttack m_PlayerAttack;

    private void Awake()
    {
        m_PlayerInput = GetComponent<PlayerInput>();
        PlayerMovement[] l_Movers = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
        var l_Index = m_PlayerInput.playerIndex;
        m_PlayerMovement = l_Movers.FirstOrDefault(m => m.GetPlayerIndex() == l_Index);

        PlayerAttack[] l_Attackers = FindObjectsByType<PlayerAttack>(FindObjectsSortMode.None);
        m_PlayerAttack = l_Attackers.FirstOrDefault(a => a.GetPlayerIndex() == l_Index);
    }

    public void OnMove(InputAction.CallbackContext _Context)
    {
        if(m_PlayerInput != null)
            m_PlayerMovement.SetInputVector(_Context.ReadValue<Vector2>());
    }

    public void OnAttack(InputAction.CallbackContext _Context)
    {
        if (m_PlayerAttack != null && _Context.started)
            if (_Context.ReadValue<float>() > 0.5f)
                m_PlayerAttack.Attack();
    }
}
