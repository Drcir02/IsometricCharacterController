using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boomerang : MonoBehaviour
{
    [Header("References")]
    public Transform player;                 // pokud není nastaveno, najde objekt s tagem "Player"
    public Transform handTransform;          // transform, kam se boomerang vrací (napø. ruka)
    public LayerMask targetLayer;            // nastav na vrstvu "Target"

    [Header("Movement")]
    [SerializeField] private float throwSpeed = 12f;
    [SerializeField] private float returnSpeed = 14f;
    [SerializeField] private float arriveDistance = 0.35f;
    [SerializeField] private float rotateSpeed = 720f; // stupnì/s pøi rotaci boomerangu

    private bool inHand = true;
    private bool isThrown = false;
    private bool isReturning = false;

    private Vector3 targetPoint;
    private Transform targetTransform;
    private Coroutine currentRoutine;

    private void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (handTransform == null && player != null)
        {
            // pokud není ruka nastavená, použije se samotný player transform jako fallback
            handTransform = player;
        }

        AttachToHandImmediate();
    }

    private void Update()
    {
        // jednoduché ovládání myší (levé tlaèítko)
        if (inHand && Input.GetMouseButtonDown(0))
        {
            TryThrowAtMouseTarget();
        }

        // pøi stisku pravého tlaèítka vynutit návrat
        if (!inHand && Input.GetMouseButtonDown(1))
        {
            StartReturn();
        }
    }

    private void TryThrowAtMouseTarget()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, targetLayer))
        {
            // musíme mít hit objekt se správným tagem (DoodleBob)
            if (hit.transform != null && hit.transform.CompareTag("DoodleBob"))
            {
                targetTransform = hit.transform;
                targetPoint = hit.point;
                ThrowToTarget();
            }
        }
    }

    private void ThrowToTarget()
    {
        if (!inHand) return;
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        inHand = false;
        isThrown = true;
        isReturning = false;

        // odpojit od ruky, aby se pohyboval samostatnì
        transform.SetParent(null);
        currentRoutine = StartCoroutine(ThrowRoutine());
    }

    private IEnumerator ThrowRoutine()
    {
        // let k cíli
        while (isThrown && !isReturning)
        {
            Vector3 targetPos = (targetTransform != null) ? targetTransform.position : targetPoint;
            //targetPos.y = transform.position.y; // udržet výšku (nebo odkomentovat pro 3D prùlet)

            Vector3 dir = (targetPos - transform.position);
            float dist = dir.magnitude;

            if (dist <= arriveDistance)
            {
                // narazil na cíl -> zaèni návrat
                isReturning = true;
                isThrown = false;
                break;
            }

            Vector3 move = dir.normalized * throwSpeed * Time.deltaTime;
            transform.position += move;

            RotateAlongVelocity(move);

            yield return null;
        }

        // krátká pauza pøi zásahu (mùžeš pøidat animaci nebo efekt)
        yield return new WaitForSeconds(0.05f);

        // zaèni návrat
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(ReturnRoutine());
    }

    private void StartReturn()
    {
        if (inHand) return;
        if (currentRoutine != null) StopCoroutine(currentRoutine);

        isThrown = false;
        isReturning = true;
        currentRoutine = StartCoroutine(ReturnRoutine());
    }

    private IEnumerator ReturnRoutine()
    {
        // vrací se k rukojeti (handTransform mùže být pohyblivý)
        while (isReturning)
        {
            Vector3 handPos = handTransform.position;
            Vector3 toHand = handPos - transform.position;
            float dist = toHand.magnitude;

            if (dist <= arriveDistance)
            {
                // dorazil do ruky
                AttachToHandImmediate();
                isReturning = false;
                inHand = true;
                currentRoutine = null;
                yield break;
            }

            Vector3 move = toHand.normalized * returnSpeed * Time.deltaTime;
            transform.position += move;

            RotateAlongVelocity(move);

            yield return null;
        }
    }

    private void RotateAlongVelocity(Vector3 velocity)
    {
        if (velocity.sqrMagnitude > 0.0001f)
        {
            Quaternion target = Quaternion.LookRotation(velocity.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, rotateSpeed * Time.deltaTime);
            // pøidat otáèení kolem lokální osy pro "víøení" efektního boomerangu
            transform.Rotate(Vector3.forward, 720f * Time.deltaTime, Space.Self);
        }
    }

    private void AttachToHandImmediate()
    {
        // pøipojit k ruce a resetovat transform
        transform.SetParent(handTransform);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        inHand = true;
        isThrown = false;
        isReturning = false;
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }
    }

    // veøejné API: zjistit, jestli je boomerang v ruce
    public bool IsInHand()
    {
        return inHand;
    }

    // pokud boomerang narazí fyzicky, mùžeme také detekovat kolizi a spustit návrat
    private void OnTriggerEnter(Collider other)
    {
        if (!isThrown) return;

        if (other.CompareTag("DoodleBob"))
        {
            // pøi zásahu cílového DoodleBoba se okamžitì vrací
            isThrown = false;
            isReturning = true;
            if (currentRoutine != null) StopCoroutine(currentRoutine);
            currentRoutine = StartCoroutine(ReturnRoutine());
        }
    }
}