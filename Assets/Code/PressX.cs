using UnityEngine;

public class PressX : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public PlayerController P1;
    public PlayerController P2;
    public GameObject PressX_P1;
    public GameObject PressX_P2;
    bool P1Ready = false;
    bool P2Ready = false;
    void Start()
    {
        P1.enabled = false;
        P2.enabled = false;

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W)) 
        { 
            
            P1Ready = true;
            PressX_P1.GetComponent<SpriteRenderer>().enabled = false;
        }
        if (Input.GetKeyDown(KeyCode.P))
        {

            P2Ready = true;
            PressX_P2.GetComponent<SpriteRenderer>().enabled = false;
        }
        if (P1Ready && P2Ready)
        {

            P1.enabled = true;
            P2.enabled = true;
        }

    }
}
