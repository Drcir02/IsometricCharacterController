using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ThrowObject : MonoBehaviour
{
    private TargetFinder targetFinder;

    [SerializeField]
    private KeyCode throwKey = KeyCode.Mouse0;

    [Header("Throw Actions")]
    [SerializeField]
    private UnityEvent throwAction;

    void Start()
    {
        // Find the TargetFinder component in the scene
        targetFinder = FindObjectOfType<TargetFinder>();
    }

    void Update()
    {
        if (Input.GetKeyDown(throwKey) && targetFinder.ClosestTarget != null)
        {
            Debug.Log("Call ThrowAction");
            throwAction?.Invoke();
        }
    }
}
