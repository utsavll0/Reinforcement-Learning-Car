using System;
using UnityEngine;

public class CalculateTime : MonoBehaviour
{
    private int attempts;
    private int success;
    private DateTime _startTime;
    private int _totalTime;

    private void Awake()
    {
        attempts = 0;
        success = 0;
    }

    public void StartAttempt()
    {
        attempts += 1;
        _startTime = DateTime.Now;
    }

    public void Finish()
    {
        success += 1;
        _totalTime += (DateTime.Now - _startTime).Seconds;
    }

    private void OnDestroy()
    {
        Debug.Log($"Attempts: {attempts} Success: {success}");
        Debug.Log($"Average time to complete: {_totalTime / (success + 0.0001f)}");
    }
}
