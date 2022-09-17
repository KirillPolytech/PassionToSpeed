using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class Car : MonoBehaviour
{
    Rigidbody Rb;

    float AxisX, AxisY;

    [Header("Velocities")]
    public int MotorToque = 150;
    public int BrakeTorque = 250;
    public int SteerWheels = 2;
    public int WheelsReturnsBackSpeed = 2;
    public WheelCollider[] WheelsColliders;
    public Transform[] WheelsModels;
    [Header("Canvas")]
    public TextMeshProUGUI SpeedCanvas;

    [Header("AntiStuckSystem")]
    public int RaysNumber = 45;
    public float RaysMaxAngle = 180f;
    public float WheelWidth = 0.15f;
    float OrgRadius;
    Car CarController;
    [Header("Materials")]
    MeshRenderer MeshRender;
    public Material BackLightsEmission;
    public Material BackLights;
    int BackLightsIndex = -99;
    private void Start()
    {
        Rb = GetComponent<Rigidbody>();

        MeshRender = GetComponent<MeshRenderer>();
    }
    private void Awake()
    {
        OrgRadius = WheelsColliders[0].radius;
        CarController = GetComponentInParent<Car>();
    }
    private void FixedUpdate()
    {
        AxisX = Input.GetAxis("Horizontal");
        AxisY = Input.GetAxis("Vertical");
        FrontWheels();
        BackWheels();
        Brake();

        BackLightsCondition();

        for (int i = 0; i < 2; i++)
        {
            WheelsModels[i].localEulerAngles = new Vector3(WheelsModels[i].localEulerAngles.x, WheelsColliders[i].steerAngle - WheelsModels[i].localEulerAngles.z, WheelsModels[i].localEulerAngles.z);
        }
        for (int i = 0; i < 4; i++)
        {
            WheelsModels[i].Rotate(WheelsColliders[i].rpm / 60 * 360 * Time.fixedDeltaTime, 0, 0);

            AntiStuckSystem(WheelsColliders[i], WheelsModels[i]);
        }

        // KM/H. 
        SpeedCanvas.text = "Speed " + (int)(Rb.velocity.magnitude * 3.6);
    }
    void AntiStuckSystem(WheelCollider WheelsColliders, Transform WheelModel)
    {
        float _radiusOffset = 0f;
        for (int i = 0; i < RaysNumber; i++)
        {
            Vector3 RayDirection = Quaternion.AngleAxis(WheelsColliders.steerAngle, transform.up) * Quaternion.AngleAxis(i * (RaysMaxAngle / RaysNumber) + ((180f - RaysMaxAngle) / 2f), transform.right ) * transform.forward;
            if (Physics.Raycast(WheelsColliders.transform.position, RayDirection, out RaycastHit hit, OrgRadius))
            {
                if (!hit.transform.IsChildOf(CarController.transform))
                {
                    Debug.DrawLine(WheelModel.position, hit.point, Color.red);
                    _radiusOffset = Mathf.Max(_radiusOffset, WheelsColliders.radius - hit.distance);
                }
            }
            Debug.DrawRay(WheelModel.position, RayDirection * OrgRadius, Color.green);

            if (Physics.Raycast(WheelsColliders.transform.position + WheelModel.right * WheelWidth * .5f, RayDirection, out RaycastHit righthit, OrgRadius))
            {
                if (!hit.transform.IsChildOf(CarController.transform))
                {
                    Debug.DrawLine(WheelModel.position, righthit.point, Color.red);
                    _radiusOffset = Mathf.Max(_radiusOffset, WheelsColliders.radius - righthit.distance);
                }
            }
            Debug.DrawRay(WheelModel.position + WheelModel.right * WheelWidth * 0.5f, RayDirection * OrgRadius, Color.green);

            if (Physics.Raycast(WheelsColliders.transform.position - WheelModel.right * WheelWidth * 0.5f, RayDirection, out RaycastHit leftthit, OrgRadius))
            {
                if (!hit.transform.IsChildOf(CarController.transform))
                {
                    Debug.DrawLine(WheelModel.position, leftthit.point, Color.red);
                    _radiusOffset = Mathf.Max(_radiusOffset, WheelsColliders.radius - leftthit.distance);
                }
            }
            Debug.DrawRay(WheelModel.position - WheelModel.right * WheelWidth * 0.5f, RayDirection * OrgRadius, Color.green);
        }
        WheelsColliders.radius = Mathf.LerpUnclamped(WheelsColliders.radius, OrgRadius + _radiusOffset, Time.deltaTime * 10f);
    }
    void FrontWheels()
    {
        for (int i = 0; i < 2; i++)
        {
            WheelsColliders[i].steerAngle = Mathf.Clamp(WheelsColliders[i].steerAngle + SteerWheels * AxisX, -45f, 45f);
            WheelsColliders[i].motorTorque = Mathf.Clamp(WheelsColliders[i].motorTorque + MotorToque, 0, 1000) * AxisY;

            if (AxisX == 0)
            {
                if (WheelsColliders[i].steerAngle < -1) { WheelsColliders[i].steerAngle += WheelsReturnsBackSpeed; } else if (WheelsColliders[i].steerAngle > 1) { WheelsColliders[i].steerAngle -= WheelsReturnsBackSpeed; }
            }
        }
    }
    void BackWheels()
    {
        for (int i = 2; i < 4; i++)
        {
            WheelsColliders[i].motorTorque = Mathf.Clamp(WheelsColliders[i].motorTorque + MotorToque, 0, 1000) * AxisY;
        }
    }
    void Brake()
    {
        for (int i = 0; i < 4; i++)
        {
            WheelsColliders[i].brakeTorque = BrakeTorque * (Input.GetButton("Jump") ? 1 : 0);
        }
    }
    void BackLightsCondition()
    {
        Material[] Materials;
        Materials = MeshRender.materials;
        if (AxisY < 0) 
        {
            for (int i = 0; i < Materials.Length; i++)
            {
                if (Materials[i].name == "BackLights (Instance)")
                {
                    Materials[i] = BackLightsEmission;
                    BackLightsIndex = i;
                }
            }
        }
        else if (BackLightsIndex > -1)
        {
            Materials[BackLightsIndex] = BackLights;
        }
        MeshRender.materials = Materials;
    }
}

/*
for (int i = 0; i < 2; i++)
{
    if (AxisX == 0)
    {
        if (Wheels[i].steerAngle < -1) { Wheels[i].steerAngle += WheelsReturnsBackSpeed; } else if (Wheels[i].steerAngle > 1) { Wheels[i].steerAngle -= WheelsReturnsBackSpeed; }
    }
}
*/
//  Input.GetButton("Jump") ? 1 : 0 

// WheelsModels[i].transform.position = WheelsColliders[i].transform.position;
// WheelsModels[i].transform.rotation = WheelsColliders[i].transform.rotation;