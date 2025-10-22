using UnityEngine;

public class Billboard : MonoBehaviour
{
    [Header("Billboard Settings")]
    [SerializeField]
    private bool ignoreYPosition = true;

    [SerializeField]
    private Vector3 rotationOffset = new Vector3(-90f, 0f, 0f);

    [SerializeField]
    private bool rotateAnimation = false;

    [SerializeField]
    private float rotationSpeed = 10f;

    private float animatedX = 0f;


    void LateUpdate()
    {
        Vector3 targetPos = Camera.main.transform.position;
        if (ignoreYPosition)
            targetPos.y = transform.position.y; // ignore vertical rotation
        transform.LookAt(targetPos);

        // Animate only X axis, keep Y/Z from offset
        if (rotateAnimation)
        {
            animatedX += rotationSpeed * Time.deltaTime;
            transform.Rotate(new Vector3(animatedX, rotationOffset.y, rotationOffset.z));
        }
        else
        {
            transform.Rotate(rotationOffset);
        }
    }
}
