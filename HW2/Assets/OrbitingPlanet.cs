using UnityEngine;

public class OrbitingPlanet : MonoBehaviour
{
    public float rotationSpeed = 30f; // degrees per second

    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
    }
}
