using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    float counter = 0;
    int seconds;
    public int MaxTime;
    TextMeshProUGUI text;
    
    void Start()
    {
        text = this.GetComponent<TextMeshProUGUI>();
        seconds = MaxTime;
    }

    // Update is called once per frame
    void Update()
    {
        counter += Time.deltaTime;
        if (counter >= 1.0f)
        {
            seconds--;
            counter = 0;
        }
        if(seconds == 0)
        {
            Debug.Log("Cumbull");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        text.text = seconds.ToString();
    }
}
