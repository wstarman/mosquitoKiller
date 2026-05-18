using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hands : MonoBehaviour
{
    public bool IsRightHand;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (IsRightHand)
        {
            Vector3 pos = GameManager.Instance.rightHand;
            pos.z = 0;
            transform.position = pos;
        }
        else
        {
            Vector3 pos = GameManager.Instance.leftHand;
            pos.z = 0;
            transform.position = pos;
        }
    }
}
