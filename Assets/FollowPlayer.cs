using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform player;
    private CharacterController controller;

    [SerializeField]
    private float speed = 5f;

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
    }
}
