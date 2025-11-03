using LlamAcademy.Spring.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CollisionTrigger : MonoBehaviour
{
    [SerializeField] private SpringToScale scaleSpring;
    [SerializeField] private Vector3 nudgeAmount = new Vector3(-10f, -10f, -10f);

    [Header("Events")]
    [SerializeField] 
    private UnityEvent onPlayerEnter;

    private void Start()
    {
        //scaleSpring = GetComponent<SpringToScale>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (scaleSpring != null)
                scaleSpring.Nudge(nudgeAmount);
            onPlayerEnter?.Invoke();
        }
    }
}
