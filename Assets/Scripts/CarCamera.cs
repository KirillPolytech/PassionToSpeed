using UnityEngine;

public class CarCamera : MonoBehaviour
{
    public GameObject Car;
    public int Distance = 5;

    //
    public int RotationSpeed = 3;
    float MouseAxisX;
    private void FixedUpdate()
    {
        //
        MouseAxisX = Input.GetAxis("Mouse X");
        transform.RotateAround(Car.transform.position, Vector3.up, RotationSpeed * MouseAxisX);
        //

        // transform.position = Car.transform.position - Car.transform.forward * Distance + Car.transform.up;
        transform.LookAt(Car.transform.position);
        Debug.DrawRay(transform.position, Car.transform.forward, Color.red);
    }
}
//transform.position = Car.transform.InverseTransformPoint(transform.position) - Car.transform.forward;
