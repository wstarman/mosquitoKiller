using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Manager
{
    public class UIManager : MonoBehaviour
    {
        public Text DebugLabel;
        int currentSkill = 0;

        // Start is called before the first frame update
        void Start()
        {
            GameManager.SkillReleased += this.OnSkillReleased;
        }

        // Update is called once per frame
        void Update()
        {
            float dis = (GameManager.Instance.leftHand - GameManager.Instance.rightHand).magnitude;
            DebugLabel.text = $"Using Device: {(InputManager.useKinect ? "Kinect" : "Mouse")}\n"+
                              $"Left Hand: {GameManager.Instance.leftHand}\n" +
                              $"Right Hand: {GameManager.Instance.rightHand}\n" +
                              $"Distance: {dis}\n" + 
                              $"CurrentSkill: {currentSkill}\n";
        }
        void OnSkillReleased(int sId)
        {
            currentSkill = sId;
        }
    }
}
