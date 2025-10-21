using UnityEngine;

public class BadSpongebobController : MonoBehaviour
{
    public Transform player;
    public float speed = 3f;
    CharacterController cc;

    void Start()
    {
        cc = GetComponent<CharacterController>() ?? gameObject.AddComponent<CharacterController>();
    }

    void Update()
    {
        Vector3 d = player.position - transform.position;
        d.y = 0;
        if (d.sqrMagnitude < 0.01f) return;
        cc.Move(d.normalized * speed * Time.deltaTime);
    }
}
