using UnityEngine;
using UnityEngine.UI;

public class LifeController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    float MaxHP = 100;
    float CurrentHP;
    public Slider Life;
    void Start()
    {
        CurrentHP = MaxHP;
        Life.minValue = 0;
        Life.maxValue = MaxHP;

    }

    // Update is called once per frame
    void Update()
    {
        Life.value = CurrentHP;
    }
    private void OnCollisionEnter(Collision collision)
    {
        CurrentHP--;
        Life.value = CurrentHP;
    }

    public float LoseHealth (float damage)
    {
        CurrentHP -= damage;
        return CurrentHP;
    }

}
