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
        Life.maxValue = MaxHP;

    }

    // Update is called once per frame
    void Update()
    {
        Life.value = CurrentHP;
        CheckDeath();
    }
    private void OnCollisionEnter(Collision collision)
    {
        ApplyDamage(1f);
    }

    public float LoseHealth(float damage)
    {
        ApplyDamage(damage);
        return CurrentHP;
    }

    private void ApplyDamage(float damage)
    {
        CurrentHP -= damage;
        if (CurrentHP < 0f) CurrentHP = 0f;
        Life.value = CurrentHP;
        CheckDeath();
    }
    private void CheckDeath()
    {
        if (CurrentHP <= 0f)
        {
            Destroy(gameObject);
        }
    }

}
