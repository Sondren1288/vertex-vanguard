using UnityEngine;

public class Anim_Rotate : MonoBehaviour
{
    public float rotationSpeed = 10f;

    private void Update(){
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
    }
    
    
    
}
