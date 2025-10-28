using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RageCounter : MonoBehaviour
{
    [SerializeField]
    private Image fillImage;

    [SerializeField]
    private float maxRage;

    private float currentRage = 0;

    private void Awake()
    {
         currentRage = 0;
    }

    private void LateUpdate()
    {
        fillImage.fillAmount = currentRage / maxRage;
    }

    public void IncreaseRage(float value)
    {
        currentRage += value;
    }
}
