using UnityEngine;
using System.Collections;
public class Unit_Idle : MonoBehaviour
{
    [SerializeField] private float breathingSpeed = 1.0f;
    [SerializeField] private float breathingAmount = 0.01f;
    
    private Vector3 originalScale;
    
    private void Start()
    {
        originalScale = transform.localScale;
    }
    
    private void Update()
    {
        float breathingFactor = Mathf.Sin(Time.time * breathingSpeed) * breathingAmount + 1.0f;
        transform.localScale = originalScale * breathingFactor;
    }
}
