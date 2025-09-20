using UnityEngine;

public class MarsTilt : MonoBehaviour
{
    void Start()
    {
        // Mars axial tilt
        transform.rotation = Quaternion.Euler(25.19f, 0f, 0f);
    }
}

