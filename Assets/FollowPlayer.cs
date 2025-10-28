using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FollowPlayer : MonoBehaviour
{
    public Transform player;
    private CharacterController controller;

    [SerializeField]
    private float speed = 5f;

    [SerializeField]
    private float distanceToJump = 3f;

    [SerializeField]
    private float distanceToDestroy = .35f;

    [SerializeField]
    private float animationHeight = 4f;

    [Header("Evenets")]
    [SerializeField]
    private UnityEvent onCloseToPlayer;

    [SerializeField]
    private UnityEvent onDistanceToDestroy;

    private bool onCloseCalled = false;
    private bool onDestroyCalled = false;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }
    
    void Update()
    {
        // Literally just move towards the player
        Vector3 dir = player.position - this.transform.position;
        dir.y = 0f;
        dir.Normalize();

        Vector3 move = dir * speed * Time.deltaTime;
        controller.Move(move);

        float dist = Vector3.Distance(this.transform.position, player.position);

        if (dist < distanceToJump && !onCloseCalled)
        {
            onCloseCalled = true;
            onCloseToPlayer?.Invoke();
            speed = 16f;
        }

        if (dist < distanceToDestroy && !onDestroyCalled)
        {
            onDestroyCalled = true;
            onDistanceToDestroy?.Invoke();
            Destroy(this.gameObject);
        }
    }
}
