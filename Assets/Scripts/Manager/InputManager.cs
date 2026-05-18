using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class InputManager : MonoBehaviour
{
    KinectSensor sensor;
    BodyFrameReader bodyReader;
    Body[] bodies;

    void Start()
    {
        sensor = KinectSensor.GetDefault();
        sensor.Open();

        bodyReader = sensor.BodyFrameSource.OpenReader();
        bodies = new Body[sensor.BodyFrameSource.BodyCount];
    }

    void Update()
    {
        if (bodyReader == null) return;

        var frame = bodyReader.AcquireLatestFrame();
        if (frame == null) return;

        frame.GetAndRefreshBodyData(bodies);

        for(int i=1;i< sensor.BodyFrameSource.BodyCount; i++)
        {
            bodies[i] = null;   // ¶È°»´ú¤@¤H
        }
        
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

            GameManager.Instance.UpdateHand();
        }
        
        frame.Dispose();
    }
}
