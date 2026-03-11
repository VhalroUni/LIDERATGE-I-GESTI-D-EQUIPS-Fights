using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInput m_PlayerInput;
    private PlayerMovement m_PlayerMovement;
    private PlayerAttack m_PlayerAttack;
    private IA m_Ia;

    private void Awake()
    {
        m_PlayerInput = GetComponent<PlayerInput>();

        PlayerMovement[] l_Movers = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
        var l_Index = m_PlayerInput.playerIndex;
        m_PlayerMovement = l_Movers.FirstOrDefault(m => m.GetPlayerIndex() == l_Index);

        PlayerAttack[] l_Attackers = FindObjectsByType<PlayerAttack>(FindObjectsSortMode.None);
        m_PlayerAttack = l_Attackers.FirstOrDefault(a => a.GetPlayerIndex() == l_Index);

        m_Ia = FindAnyObjectByType<IA>();
    }

    public void OnMove(InputAction.CallbackContext _Context)
    {
        if(m_PlayerInput != null)
            m_PlayerMovement.SetInputVector(_Context.ReadValue<Vector2>());
    }

    public void OnMeleeAttack(InputAction.CallbackContext _Context)
    {
        if (m_PlayerAttack != null && _Context.started)
            if (_Context.ReadValue<float>() > 0.1f)
                m_PlayerAttack.MeleeAttack();
    }

    public void OnAreaAttack(InputAction.CallbackContext _Context)
    {
        if (m_PlayerAttack != null && _Context.started)
            if (_Context.ReadValue<float>() > 0.1f)
                m_PlayerAttack.AreaAtack();
    }

    public void OnUltimate(InputAction.CallbackContext _Context)
    {
        if (m_PlayerAttack != null && _Context.started)
            if (_Context.ReadValue<float>() > 0.1f)
                m_PlayerAttack.Ultimate();
    }

    public void OnDistanceAttack(InputAction.CallbackContext _Context)
    {
        if (m_PlayerAttack != null && _Context.started)
            if (_Context.ReadValue<float>() > 0.1f)
                m_PlayerAttack.DistanceAttack();
    }

    public void OnTeleport(InputAction.CallbackContext _Context)
    {
        if (m_PlayerAttack != null && _Context.started)
            if (_Context.ReadValue<float>() > 0.1f)
                m_PlayerAttack.Teleport();
    }

    public void OnBlock(InputAction.CallbackContext _Context)
    {
        if (m_PlayerAttack != null && _Context.started)
            if (_Context.ReadValue<float>() > 0.1f)
                m_PlayerAttack.StartBlock();
            else
                m_PlayerAttack.animator.SetBool("IsBlocking", false);
    }

    public void OnDash(InputAction.CallbackContext _Context)
    {
        if (m_PlayerAttack != null && _Context.started)
            if (_Context.ReadValue<float>() > 0.1f)
                m_PlayerMovement.HandleDash();
    }

    public void IAActive(InputAction.CallbackContext _Context)
    {
        if (m_Ia != null && _Context.started)
            m_Ia.ChangeBehaviour();
    }
}
