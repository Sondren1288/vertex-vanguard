using UnityEngine;

public class Random_jump : MonoBehaviour
{
    [SerializeField] private float minJumpForce = 3.0f;
    [SerializeField] private float maxJumpForce = 5.0f;
    [SerializeField] private float minTimeBetweenJumps = 1.0f;
    [SerializeField] private float maxTimeBetweenJumps = 3.0f;
    
    private Rigidbody rb;
    private float nextJumpTime;
    private bool isGrounded = true;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("RandomJump script requires a Rigidbody component!");
            enabled = false;
            return;
        }
        
        SetNextJumpTime();
    }
    
    void Update()
    {
        if (Time.time >= nextJumpTime && isGrounded)
        {
            Jump();
            SetNextJumpTime();
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // Consider the object grounded when it collides with something
        isGrounded = true;
    }
    
    void OnCollisionExit(Collision collision)
    {
        // Object is no longer grounded when it leaves a collision
        isGrounded = false;
    }
    
    private void Jump()
    {
        float jumpForce = Random.Range(minJumpForce, maxJumpForce);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        isGrounded = false;
    }
    
    private void SetNextJumpTime()
    {
        float randomDelay = Random.Range(minTimeBetweenJumps, maxTimeBetweenJumps);
        nextJumpTime = Time.time + randomDelay;
    }
}