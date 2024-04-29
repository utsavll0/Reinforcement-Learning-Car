using System;
using UnityEngine;

public enum TrackType
{
    Known,
    Unknown
}

public class Manager : MonoBehaviour
{
    private int knownTrackAttempts;
    private int unknownTrackAttempts;

    private int knownFinishes;
    private int unknownFinishes;
    private void Awake()
    {
        knownFinishes = 0;
        knownTrackAttempts = 0;
        unknownFinishes = 0;
        unknownTrackAttempts = 0;
    }

    public void IncrementAttempt(TrackType type, bool didFinish)
    {
        if (type == TrackType.Known)
        {
            knownTrackAttempts += 1;
            knownFinishes += didFinish ? 1 : 0;
        }
        else if (type == TrackType.Unknown)
        {
            unknownTrackAttempts += 1;
            unknownFinishes += didFinish ? 1 : 0;
        }
    }

    private void OnDestroy()
    {
        Debug.Log($"Total Known Attempts: {knownTrackAttempts}");
        Debug.Log($"Total Known Finishes: {knownFinishes}");
        Debug.Log($"Total Unknown Attempts: {unknownTrackAttempts}");
        Debug.Log($"Total Unknown Finishes: {unknownFinishes}");
    }
}
