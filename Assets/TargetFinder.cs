using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetFinder : MonoBehaviour
{
    [Header("Target Finder Settings")]
    [SerializeField]
    private float detectionRadius = 10f;

    [SerializeField]
    private LayerMask targetLayer;

    [SerializeField]
    private GameObject indicatorPrefab;

    private Transform closestTarget;
    // Veøejné API
    public Transform ClosestTarget => closestTarget;

    void Update()
    {
        if(FindClosestTarget())
        {
            indicatorPrefab.SetActive(true);
            indicatorPrefab.transform.position = closestTarget.position; // offset above target
        }
        else
        {
            indicatorPrefab.SetActive(false);
        }
    }

    private bool FindClosestTarget()
    {
        Collider[] targetsInRange = Physics.OverlapSphere(transform.position, detectionRadius, targetLayer);
        float closestDistanceSqr = Mathf.Infinity;
        Transform nearestTarget = null;
        foreach (Collider targetCollider in targetsInRange)
        {
            float distanceSqr = (targetCollider.transform.position - transform.position).sqrMagnitude;
            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                nearestTarget = targetCollider.transform;
            }
        }
        closestTarget = nearestTarget;

        if (closestTarget != null)
            return true;
        return false;
    }
}
