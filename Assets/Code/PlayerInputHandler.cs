using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInput m_PlayerInput;
    private PlayerMovement m_PlayerMovement;

    private void Awake()
    {
        m_PlayerInput = GetComponent<PlayerInput>();
        PlayerMovement[] l_Movers = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
        var l_Index = m_PlayerInput.playerIndex;
        m_PlayerMovement = l_Movers.FirstOrDefault(m => m.GetPlayerIndex() == l_Index);
    }

    public void OnMove(InputAction.CallbackContext _Context)
    {
        if(m_PlayerInput != null)
            m_PlayerMovement.SetInputVector(_Context.ReadValue<Vector2>());
    }
}
