using UnityEngine;

public class SeasonTreeFollower : MonoBehaviour
{
    public Transform earth;          // Drag Earth here
    public Vector3 offset = new Vector3(3, 0, 0); // Where tree should float relative to Earth

    void Update()
    {
        if (earth == null) return;

        // Tree always follows Earth + offset
        transform.position = earth.position + offset;
    }
}
