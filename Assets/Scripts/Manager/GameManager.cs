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
    public bool isHandContact = false;  // 是否兩手碰在一起
    public int ep = 100;
    public int[] epCosts;

    [Header("Playing Mode")]
    public float PlayingClapDistance = 1.5f;
    public float PlayingResetDistance = 2.0f;

    [Header("Cursor Mode")]
    public float CursorClapDistance = 0.3f;
    public float CursorResetDistance = 0.6f;

    public static event Action OnHandClap;
    public static event Action<int> SkillReleased;

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
        epCosts = new int[5] { 0, 30, 30, 50, 100};
        InputManager.DetectedSkill += OnSkillDetected;
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

        bool isCursor = GameStateManager.Instance == null ||
                        GameStateManager.Instance.CurrentState != GameState.Playing;
        float clapDist = isCursor ? CursorClapDistance : PlayingClapDistance;
        float resetDist = isCursor ? CursorResetDistance : PlayingResetDistance;

        if (distance < clapDist)
        {
            if (!isHandContact)
            {
                OnHandClap?.Invoke();
            }
            isHandContact = true;
        }
        else if (distance > resetDist)
        {
            isHandContact = false;
        }
    }

    void OnSkillDetected(int sId)
    {
        if (ep >= epCosts[sId])
        {
            SkillReleased?.Invoke(sId);
            // todo: 
            // ep -= epCost[sId - 1];
            // cool down
        }
    }
}
