using UnityEngine;
using TMPro; // This is required to talk to TextMeshPro!

public class TimerUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text timerText;

    private float elapsedTime = 0f;
    private bool isTimerRunning = true;

    void Update()
    {
        if (isTimerRunning)
        {
            // Time.deltaTime is the time passed since the last frame. 
            // Adding this up gives us our real-time stopwatch.
            elapsedTime += Time.deltaTime;

            UpdateTimerDisplay();
        }
    }

    private void UpdateTimerDisplay()
    {
        // Calculate minutes, seconds, and milliseconds
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        int milliseconds = Mathf.FloorToInt((elapsedTime * 100f) % 100f);

        // Format the string to look like 00:00:00
        timerText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
    }

    // You can call these methods from other scripts to control the timer!
    public void StopTimer()
    {
        isTimerRunning = false;
    }

    public void StartTimer()
    {
        isTimerRunning = true;
    }

    public void ResetTimer()
    {
        elapsedTime = 0f;
        UpdateTimerDisplay();
    }
}