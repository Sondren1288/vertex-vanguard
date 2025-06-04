using UnityEngine;

public class FaceCameraAlways : MonoBehaviour
{
    public Camera targetCamera;
    void Start()
    {
        if (targetCamera == null) return;
        Vector3 targetPos = targetCamera.transform.position;
        this.transform.LookAt(targetPos);
    }

    void FixedUpdate()
    {
        Vector3 targetPos = targetCamera.transform.position;
        this.transform.LookAt(targetPos);
    }
}