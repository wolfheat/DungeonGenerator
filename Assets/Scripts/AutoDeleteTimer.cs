using System;
using System.Collections;
using UnityEngine;

public class AutoDeleteTimer : MonoBehaviour
{
    [SerializeField] private float deleteTime; 

    
    void Start()
    {
        StartCoroutine(DeleteTimerRoutine());
    }

    private IEnumerator DeleteTimerRoutine()
    {
        yield return new WaitForSeconds(deleteTime);
        Destroy(gameObject);
    }

}
