using UnityEngine;

public class SaturnTilt : MonoBehaviour
{
    void Start()
    {
        // Saturn Axial Tilt
        transform.rotation = Quaternion.Euler(26.73f, 0f, 0f);
    }
}