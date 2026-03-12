using System.Collections;
using UnityEngine;

public class PressX : MonoBehaviour
{
    public PlayerController P1;
    public PlayerController P2;
    public IA P2IA;

    public GameObject PressX_P1;
    public GameObject PressX_P2;

    public GameObject[] Counter;

    bool P1Ready = false;
    bool P2Ready = false;
    bool counterStarted = false;

    SpriteRenderer SR;

    void Start()
    {
        P1.enabled = false;
        P2.enabled = false;
        P2IA.enabled = false;

        SR = GetComponent<SpriteRenderer>();
    }

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

        if (P1Ready && P2Ready && !counterStarted)
        {
            counterStarted = true;
            StartCoroutine(PlayCounter());
        }
    }

    public IEnumerator PlayCounter()
    {
        Counter[0].SetActive(true);
        yield return new WaitForSeconds(1f);

        Counter[0].SetActive(false);
        Counter[1].SetActive(true);
        yield return new WaitForSeconds(1f);

        Counter[1].SetActive(false);
        Counter[2].SetActive(true);
        yield return new WaitForSeconds(1f);

        Counter[2].SetActive(false);

        P1.enabled = true;
        P2.enabled = true;
        P2IA.enabled = true;
    }
}