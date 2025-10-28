using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    private float spawnRate = 1f;

    [SerializeField]
    private GameObject spawnedObject;

    [SerializeField]
    private Transform objectParent;

    private float timer = 0f;

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (timer > spawnRate)
        {
            timer = 0f;
            SpawnPrefab(spawnedObject);
        }
    }

    private void SpawnPrefab(GameObject prefab)
    {
        Instantiate(prefab, transform.position, prefab.transform.rotation, objectParent);
    }
}
