using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        // luôn nhìn về camera
        transform.LookAt(transform.position + cam.forward);
    }
}
