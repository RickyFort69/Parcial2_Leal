using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    Transform cam;

    void Start()
    {
        if (Camera.main != null) cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (cam != null) transform.rotation = cam.rotation;
    }
}