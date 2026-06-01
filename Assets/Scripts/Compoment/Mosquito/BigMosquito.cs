using UnityEngine;

public class BigMosquito : MosquitoBase
{
    [Header("Big Mosquito")]
    public GameObject BloodSplatterPrefab;

    protected override void Start()
    {
        MoveSpeed = 1f;
        AttackWindupDuration = 5f;
        base.Start();
    }

    protected override void OnClapHit()
    {
        if (BloodSplatterPrefab != null)
            Instantiate(BloodSplatterPrefab, transform.position, Quaternion.identity);
        base.OnClapHit();
    }
}
