using UnityEngine;
using UnityEngine.UI;

public class LifeController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    int MaxHP = 100;
    float CurrentHP;
    public Slider Life;
    void Start()
    {
        CurrentHP = MaxHP;

    }

    // Update is called once per frame
    void Update()
    {
        CurrentHP -= Time.deltaTime;
        Life.value = CurrentHP / 100;
    }
    private void OnCollisionEnter(Collision collision)
    {
        CurrentHP--;
        Life.value = CurrentHP / 100;
    }

}
