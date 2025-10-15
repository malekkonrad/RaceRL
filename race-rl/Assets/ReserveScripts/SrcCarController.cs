
using UnityEngine;

public class SrcCarController : MonoBehaviour
{

    public SrcWheel[] wheels;

    [Header("CarSettings")]
    public float wheelBase;
    public float rearTrack;
    public float turnRadius;


    [Header("Inputs")]
    public float steeringInput;

    private float ackermannAngleLeft;
    private float ackermannAngleRight;


    void Update()
    {
        steeringInput = Input.GetAxis("Horizontal");

        if (steeringInput > 0)
        {
            ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * steeringInput;
            ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * steeringInput;
        }
        else if (steeringInput < 0)
        {
            ackermannAngleLeft = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius - (rearTrack / 2))) * steeringInput;
            ackermannAngleRight = Mathf.Rad2Deg * Mathf.Atan(wheelBase / (turnRadius + (rearTrack / 2))) * steeringInput;
        }
        else
        {
            ackermannAngleLeft = 0;
            ackermannAngleRight = 0;
        }

        Debug.Log("Left Wheel Angle: " + ackermannAngleLeft);
        Debug.Log("Right Wheel Angle: " + ackermannAngleRight);


        foreach (SrcWheel w in wheels)
        {
            if (w.wheelFrontLeft)
            {
                w.steerAngle = ackermannAngleLeft;
            }
            else if (w.wheelFrontRight)
            {
                w.steerAngle = ackermannAngleRight;
            }
        }


    }

}