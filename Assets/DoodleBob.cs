using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DoodleBob : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent onHit;

    public GameObject mesh;

    public void HitByBoomerang()
    {
        onHit?.Invoke();
        layDownDoodleBob();
        changeLayer("Default");
        mesh.transform.localPosition = new Vector3(0f, 0f, 1f);
    }

    private void layDownDoodleBob()
    {
        // Rotate DoodleBob to lay down
        transform.rotation = Quaternion.Euler(90f, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
    }

    private void changeLayer(string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer == -1)
        {
            Debug.LogWarning($"Layer '{layerName}' does not exist.");
            return;
        }
        gameObject.layer = layer;
    }
}
