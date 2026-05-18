using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class InputManager : MonoBehaviour
{
    KinectSensor sensor;
    BodyFrameReader bodyReader;
    Body[] bodies;
    public static bool useKinect = true;

    void Start()
    {
        try
        {
            sensor = KinectSensor.GetDefault();
            sensor.Open();
            bodyReader = sensor.BodyFrameSource.OpenReader();
            bodies = new Body[sensor.BodyFrameSource.BodyCount];
            useKinect = true;
        }
        catch
        {
            useKinect = false;
        }
    }

    void Update()
    {
        if (useKinect)
        {
            UpdateKinect();
        }
        else
        {
            UpdateMouse();
        }
        GameManager.Instance.UpdateHand();
    }

    void UpdateKinect()
    {
        if (bodyReader == null) return;

        var frame = bodyReader.AcquireLatestFrame();
        if (frame == null) return;

        frame.GetAndRefreshBodyData(bodies);

        //for(int i=1;i< sensor.BodyFrameSource.BodyCount; i++)
        //{
        //    bodies[i] = null;   // ¶È°»´ú¤@¤H
        //}

        foreach (var body in bodies)
        {
            if (body == null) continue;
            if (!body.IsTracked) continue;

            var joints = body.Joints;

            var leftHand = joints[JointType.HandLeft].Position;
            var rightHand = joints[JointType.HandRight].Position;

            Vector3 leftHandPos = new Vector3(leftHand.X, leftHand.Y, leftHand.Z);
            GameManager.Instance.leftHand = leftHandPos * 10f;

            Vector3 rightHandPos = new Vector3(rightHand.X, rightHand.Y, rightHand.Z);
            GameManager.Instance.rightHand = rightHandPos * 10f;
        }

        frame.Dispose();
    }

    void UpdateMouse()
    {
        Vector3 pos = new Vector3(
            (Input.mousePosition.x / Screen.width-0.5f) * 19.8f,
            (Input.mousePosition.y / Screen.height-0.5f) * 10,
            0
        );

        Vector3 offset = new Vector3(2.0f, 0, 0);
        if (Input.GetMouseButton(0))
        {
            GameManager.Instance.leftHand = pos;
            GameManager.Instance.rightHand = pos;
        }
        else
        {
            GameManager.Instance.leftHand = pos - offset;
            GameManager.Instance.rightHand = pos + offset;
        }
    }
}
