using Assets.Scripts.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Compoment
{
    public class Explotion : MonoBehaviour
    {
        int counter = 0;
        bool show = false;

        // Start is called before the first frame update
        void Start()
        {
            GetComponent<Renderer>().enabled = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (counter > 0) counter--;
            else if (show)
            {
                GetComponent<Renderer>().enabled = show = false;
            }
        }

        void OnEnable()
        {
            GameManager.OnHandClap += OnHandClap;
        }

        void OnDisable()
        {
            GameManager.OnHandClap -= OnHandClap;
        }

        void OnHandClap()
        {
            Vector3 pos = (GameManager.Instance.leftHand + GameManager.Instance.rightHand) / 2;
            pos.z = 0;
            transform.position = pos;
            GetComponent<Renderer>().enabled = show = true;
            counter = 60;   // show 1 sec
        }
    }
}