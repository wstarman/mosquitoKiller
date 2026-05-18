using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Vector3 leftHand = new(-200, 0, 0);
    public Vector3 rightHand = new(200, 0, 0);
    public float distance;
    public bool isHandContact = false;  // 記錄手是否碰在一起

    public static event Action OnHandClap;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Init();
    }

    void Init()
    {
        leftHand = new(-2, 0, 0);
        rightHand = new(2, 0, 0);
    }

    void Start()
    {
        
    }

    void Update()
    {

    }
    public void UpdateHand()
    {
        distance = (leftHand - rightHand).magnitude;
        if(distance < 1.5)
        {
            if (!isHandContact)
            {
                OnHandClap?.Invoke();
            }
            isHandContact = true;
        }
        else if(distance > 2.0)
        {
            isHandContact = false;
        }
    }
}
