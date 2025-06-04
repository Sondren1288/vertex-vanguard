using UnityEngine;

public class RandomRotation : MonoBehaviour
{
    [SerializeField] private float chewSpeed = 2.0f;
    [SerializeField] private float maxRotationX = 5.0f;
    [SerializeField] private float maxRotationZ = 8.0f;
    
    private Vector3 targetRotation;
    private Vector3 startRotation;
    private float timeToNewRotation;
    private float timeSinceLastRotation;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startRotation = transform.localEulerAngles;
        SetNewTargetRotation();
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceLastRotation += Time.deltaTime;
        
        if (timeSinceLastRotation >= timeToNewRotation)
        {
            SetNewTargetRotation();
            timeSinceLastRotation = 0f;
        }
        
        // Smoothly interpolate toward the target rotation
        transform.localRotation = Quaternion.Slerp(
            transform.localRotation, 
            Quaternion.Euler(targetRotation), 
            chewSpeed * Time.deltaTime
        );
    }
    
    private void SetNewTargetRotation()
    {
        // Create random rotation angles around X and Z axes for chewing effect
        float randomX = Random.Range(-maxRotationX, maxRotationX);
        float randomZ = Random.Range(-maxRotationZ, maxRotationZ);
        
        targetRotation = new Vector3(
            startRotation.x + randomX,
            startRotation.y,
            startRotation.z + randomZ
        );
        
        // Set random time before next rotation change (0.5 to 1.5 seconds)
        timeToNewRotation = Random.Range(0.5f, 1.5f);
    }
}
