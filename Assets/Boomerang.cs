using System.Collections;
using UnityEngine;

public class Boomerang : MonoBehaviour
{
    [Header("References")]
    public Transform player; // Pokud není nastaveno, najde objekt s tagem "Player"
    public Transform handTransform; // Transform, kam se boomerang vrací (napø. ruka)
    public Transform spatulaObject;

    [Header("Movement")]
    [SerializeField] private float throwSpeed = 10f;
    [SerializeField] private float returnSpeed = 12f;
    [SerializeField] private float arriveDistance = 0.35f;
    [SerializeField] private float rotateSpeed = 4f;
    [SerializeField] private Vector3 rotationOffset = new Vector3(90f, 0f, 0f);
    private Vector3 positionOffset = new Vector3(0f, -1.6f, 0f);

    private bool inHand = true;
    private bool isReturning = false;
    private Transform targetTransform;
    private TargetFinder targetFinder;

    private void Start()
    {
        targetFinder = FindObjectOfType<TargetFinder>();
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        AttachToHandImmediate();
    }

    public void ThrowToTarget()
    {
        if (!inHand) return;
        targetTransform = targetFinder?.ClosestTarget;
        if (targetTransform == null) return;

        inHand = false;
        isReturning = false;
        spatulaObject.localPosition = positionOffset;
        transform.SetParent(null);
    }

    private void FixedUpdate()
    {
        if (inHand) return;

        // rotate around y axis
        transform.RotateAround(transform.position, Vector3.up, rotateSpeed * 360f * Time.fixedDeltaTime);
        // set x axis angle to 90 and z axis to 0
        Vector3 currentRotation = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(0f, currentRotation.y, 0f) * Quaternion.Euler(rotationOffset);

        if (isReturning)
        {
            MoveTowards(handTransform.position, returnSpeed, () =>
            {
                inHand = true;
                isReturning = false;
                AttachToHandImmediate();
            });
        }
        else if (targetTransform != null)
        {
            MoveTowards(targetTransform.position, throwSpeed, () =>
            {
                // Pokud dorazí na cíl, mùeš zde pøidat další logiku
                targetTransform = this.transform; // Nastav cíl na sebe pro návrat
            });
        }
        else
        {
            // Pokud není cíl, vra se do ruky
            isReturning = true;
        }
    }

    private void MoveTowards(Vector3 destination, float speed, System.Action onArrive)
    {
        Vector3 direction = (destination - transform.position).normalized;
        transform.position += direction * speed * Time.fixedDeltaTime;
        if (Vector3.Distance(transform.position, destination) < arriveDistance)
        {
            onArrive?.Invoke();
        }
    }

    private void AttachToHandImmediate()
    {
        transform.SetParent(handTransform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        spatulaObject.localPosition = Vector3.zero;
    }

    public bool IsInHand() => inHand;

    private void OnTriggerEnter(Collider other)
    {
        if (inHand || isReturning) return;

        if (other.CompareTag("DoodleBob"))
        {
            // Zmìna cíle na sebe a návrat po zpodìní
            StartCoroutine(ReturnToHandAfterDelay(1f));
            other.GetComponent<DoodleBob>()?.HitByBoomerang();
        }
    }

    private IEnumerator ReturnToHandAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isReturning = true;
    }
}