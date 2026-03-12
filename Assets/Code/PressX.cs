using System.Collections;
using UnityEngine;

public class PressX : MonoBehaviour
{
    public PlayerController P1;
    public PlayerController P2;
    //public IA P2IA;

    public GameObject PressX_P1;
    public GameObject PressX_P2;

    public GameObject[] Counter; // 3,2,1,GO
    public AudioClip[] CounterSounds; // sonido para 3,2,1,GO

    public GameObject[] ObjectsToActivate; // objetos que se activan en GO

    public AudioSource audioSource;

    bool P1Ready = false;
    bool P2Ready = false;
    bool counterStarted = false;

    void Start()
    {
        P1.enabled = false;
        P2.enabled = false;
        //P2IA.enabled = false;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            P1Ready = true;
            if (PressX_P1 != null)
                PressX_P1.GetComponent<SpriteRenderer>().enabled = false;
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            P2Ready = true;
            if (PressX_P2 != null)
                PressX_P2.GetComponent<SpriteRenderer>().enabled = false;
        }

        if (P1Ready && P2Ready && !counterStarted)
        {
            counterStarted = true;
            StartCoroutine(PlayCounter());
        }
    }

    IEnumerator PlayCounter()
    {
        for (int i = 0; i < Counter.Length; i++)
        {
            if (Counter[i] != null)
                Counter[i].SetActive(true);

            float waitTime = 1f;

            if (i < CounterSounds.Length && CounterSounds[i] != null && audioSource != null)
            {
                audioSource.PlayOneShot(CounterSounds[i]);
                waitTime = CounterSounds[i].length;
            }

            yield return new WaitForSeconds(waitTime);

            if (i == Counter.Length - 1)
            {
                foreach (GameObject obj in ObjectsToActivate)
                {
                    if (obj != null)
                        obj.SetActive(true);
                }

                P1.enabled = true;
                P2.enabled = true;
                //P2IA.enabled = true;
            }

            if (Counter[i] != null)
                Counter[i].SetActive(false);
        }
    }
}