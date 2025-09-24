using UnityEngine;

public class PlutoTilt : MonoBehaviour
{
    void Start()
    {
        // Pluto Axial Tilt
        transform.rotation = Quaternion.Euler(2.0f, 0f, 0f);
    }
}