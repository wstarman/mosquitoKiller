using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyCamera : MonoBehaviour
{
    Vector3 initPos;
    Quaternion initRotation;

    int shakeTimer = 0;
    System.Random r = new System.Random();
    // Start is called before the first frame update
    void Start()
    {
        initPos = transform.position;
        initRotation = transform.rotation;
        GameManager.OnHandClap += shake;
    }

    private void OnDestroy()
    {
        GameManager.OnHandClap -= shake;
    }

    // Update is called once per frame
    void Update()
    {
        if (shakeTimer > 1)
        {
            shakeTimer--;
            if (shakeTimer % 30 == 0)
            {
                transform.position = new(0.5f-(float)r.NextDouble(), 0.5f-(float)r.NextDouble(), initPos.z+0.5f- (float)r.NextDouble());
            }
        }else if (shakeTimer == 1)
        {
            shakeTimer--;
            transform.position = initPos;
            transform.rotation = initRotation;
        }
    }

    public void shake()
    {
        // shakeTimer = 60;
    }
}
