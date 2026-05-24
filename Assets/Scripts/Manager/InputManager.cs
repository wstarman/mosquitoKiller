using Assets.Scripts.Datas;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Windows.Kinect;
namespace Assets.Scripts.Manager
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }
        KinectSensor sensor;
        BodyFrameReader bodyReader;
        Body[] bodies;
        public static bool useKinect = true;

        public Text InputDebugLabel;

        readonly float displayMagnification = 10f;
        readonly float skillThresholdBig = 40f;     // 技能判斷可容許的誤差(角度)
        readonly float skillThresholdSmall = 25f;   // 較小的誤差，目前僅用於爆炸手勢的上手臂角度
        readonly int minSkillRemainingFrame = 5; // 技能需持續被偵測多少幀才會釋放

        int currentSkillRemainingFrame = 0;
        Skill prevSkill = Skill.None;

        public static event Action<int> ReleaseSkill;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

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
                DetectSkillKinect();
            }
            else
            {
                UpdateMouse();
                DetectSkillKey();
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
            //    bodies[i] = null;   // 僅偵測一人
            //}

            foreach (var body in bodies)
            {
                if (body == null) continue;
                if (!body.IsTracked) continue;

                var joints = body.Joints;

                var leftHand = joints[JointType.HandLeft].Position;
                var rightHand = joints[JointType.HandRight].Position;
                var leftShoulder = joints[JointType.ShoulderLeft].Position;
                var rightShoulder = joints[JointType.ShoulderRight].Position;
                var leftElbow = joints[JointType.ElbowLeft].Position;
                var rightElbow = joints[JointType.ElbowRight].Position;
                var shoulder = joints[JointType.SpineShoulder].Position;

                bool use3D = true;

                Vector3 leftHandPos = new Vector3(leftHand.X, leftHand.Y, use3D ? leftHand.Z : 0);
                Vector3 rightHandPos = new Vector3(rightHand.X, rightHand.Y, use3D ? rightHand.Z : 0);
                Vector3 leftShoulderPos = new Vector3(leftShoulder.X, leftShoulder.Y, use3D ? leftShoulder.Z : 0);
                Vector3 rightShoulderPos = new Vector3(rightShoulder.X, rightShoulder.Y, use3D ? rightShoulder.Z : 0);
                Vector3 leftElbowPos = new Vector3(leftElbow.X, leftElbow.Y, use3D ? leftElbow.Z : 0);
                Vector3 rightElbowPos = new Vector3(rightElbow.X, rightElbow.Y, use3D ? rightElbow.Z : 0);
                Vector3 shoulderPos = new Vector3(shoulder.X, shoulder.Y, use3D ? shoulder.Z : 0);

                // 同步手的位置至GM
                var leftHandPos2D = leftHandPos;
                var rightHandPos2D = rightHandPos;
                leftHandPos2D.z = rightHandPos2D.z = 0;
                GameManager.Instance.leftHand = leftHandPos2D * displayMagnification;
                GameManager.Instance.rightHand = rightHandPos2D * displayMagnification;

                // 偵測技能
                Vector3 upVec = new Vector3(0, 1, 0);
                Vector3 leftVec = use3D ? leftShoulderPos - shoulderPos : new Vector3(-1, 0, 0);
                Vector3 rightVec = use3D ? rightShoulderPos - shoulderPos : new Vector3(1, 0, 0);
                if (use3D)
                {
                    leftVec.Normalize();
                    rightVec.Normalize();
                }
                Vector3 leftUpVec = use3D ? leftVec + upVec : new Vector3(-1, 1, 0);
                Vector3 rightUpVec = use3D ? rightVec + upVec : new Vector3(1, 1, 0);

                Vector3 leftUpperArm = leftElbowPos - leftShoulderPos;
                Vector3 rightUpperArm = rightElbowPos - rightShoulderPos;
                Vector3 leftForearm = leftHandPos - leftElbowPos;
                Vector3 rightForearm = rightHandPos - rightElbowPos;

                float leftElbowAngel = Vector3.Angle(leftUpperArm, leftForearm);
                float rightElbowAngel = Vector3.Angle(rightUpperArm, rightForearm);
                float leftShoulderAngelUp = Vector3.Angle(leftUpperArm, upVec);
                float leftShoulderAngelDiagonal = Vector3.Angle(leftUpperArm, leftUpVec);
                float leftShoulderAngelHorizontal = Vector3.Angle(leftUpperArm, leftVec);
                float rightShoulderAngelUp = Vector3.Angle(rightUpperArm, upVec);
                float rightShoulderAngelDiagonal = Vector3.Angle(rightUpperArm, rightUpVec);
                float rightShoulderAngelHorizontal = Vector3.Angle(rightUpperArm, rightVec);

                InputDebugLabel.text = $"Elbow Angel: [{leftElbowAngel}, {rightElbowAngel}]\n" +
                                        $"Shoulder Angel: Up:[{leftShoulderAngelUp}, {rightShoulderAngelUp}]\n" +
                                        $"\tDiagonal:[{leftShoulderAngelDiagonal}, {rightShoulderAngelDiagonal}]\n" +
                                        $"\tHorizontal[{leftShoulderAngelHorizontal}, {rightShoulderAngelHorizontal}]\n";
                Skill currentSkill = Skill.None;
                // 雙手打直
                if (leftElbowAngel < skillThresholdBig && rightElbowAngel < skillThresholdBig)
                {
                    // skill 1 波動拳
                    if (Vector3.Angle(leftUpperArm, rightUpperArm) < skillThresholdBig)
                    {
                        if (Vector3.Angle(leftUpperArm, new Vector3(-1, 0, 0)) < skillThresholdBig ||
                           Vector3.Angle(rightUpperArm, new Vector3(-1, 0, 0)) < skillThresholdBig)
                        {
                            InputDebugLabel.text += "Skill: Hadoken(L)!\n";
                            currentSkill = Skill.HadokenLeft;
                        }
                        if (Vector3.Angle(leftUpperArm, new Vector3(1, 0, 0)) < skillThresholdBig ||
                            Vector3.Angle(rightUpperArm, new Vector3(1, 0, 0)) < skillThresholdBig)
                        {
                            InputDebugLabel.text += "Skill: Hadoken(R)!\n";
                            currentSkill = Skill.HadokenRight;
                        }
                    }
                    // skill 2 爆炸
                    if (leftShoulderAngelDiagonal < skillThresholdSmall && rightShoulderAngelDiagonal < skillThresholdSmall)
                    {
                        InputDebugLabel.text += "Skill: Explotion!\n";
                        currentSkill = Skill.Explotion;
                    }
                }
                // skill 3 電蚊拍
                if (Math.Abs(leftElbowAngel - 90) < skillThresholdBig && rightElbowAngel < skillThresholdBig &&
                    leftShoulderAngelUp < skillThresholdBig && rightShoulderAngelUp < skillThresholdBig)
                {
                    InputDebugLabel.text += "Skill: Electric Mosquito Swatter!\n";
                    currentSkill = Skill.Swatter;
                }

                // 處理技能釋放
                if (currentSkill == prevSkill)
                {
                    currentSkillRemainingFrame++;
                    if (currentSkillRemainingFrame > minSkillRemainingFrame)
                    {
                        ReleaseSkill.Invoke((int)currentSkill);
                    }
                }
                else
                {
                    prevSkill = currentSkill;
                    currentSkillRemainingFrame = 0;
                }
            }

            frame.Dispose();
        }

        void UpdateMouse()
        {
            Vector3 pos = new Vector3(
                (Input.mousePosition.x / Screen.width - 0.5f) * 19.8f,
                (Input.mousePosition.y / Screen.height - 0.5f) * 10,
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

        void DetectSkillKinect()
        {

        }

        void DetectSkillKey()
        {

        }
    }
}