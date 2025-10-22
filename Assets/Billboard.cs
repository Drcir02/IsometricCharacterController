using UnityEngine;

public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        Vector3 targetPos = Camera.main.transform.position;
        targetPos.y = transform.position.y; // ignore vertical rotation
        transform.LookAt(targetPos);
        transform.Rotate(-90f, 0f, 0f);
    }
}
