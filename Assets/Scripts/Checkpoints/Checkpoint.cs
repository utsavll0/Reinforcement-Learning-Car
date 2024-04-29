using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private bool hit = false;

    public void HitCheckpoint()
    {
        hit = true;
    }

    public bool IsHit()
    {
        return hit;
    }

    public void ResetCheckpoint()
    {
        hit = false;
    }
}
