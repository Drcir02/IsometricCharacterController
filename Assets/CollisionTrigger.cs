using LlamAcademy.Spring.Runtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionTrigger : MonoBehaviour
{
    [SerializeField] private SpringToScale scaleSpring;

    private void Start()
    {
        //scaleSpring = GetComponent<SpringToScale>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            scaleSpring.Nudge(new Vector3(-10f, -10f, -10f));
        }
    }
}
