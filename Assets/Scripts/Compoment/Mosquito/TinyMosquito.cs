using UnityEngine;

public class TinyMosquito : MosquitoBase
{
    [Header("Tiny Mosquito")]
    public float IrregularFactor = 4f;  // Frequency of sinusoidal direction wobble

    protected override void Start()
    {
        MoveSpeed = 4f;
        WanderChangeInterval = 0.3f;
        AttackWindupDuration = 0.8f;
        base.Start();
    }

    protected override void UpdateWandering()
    {
        // Sinusoidal wobble to create irregular flight path
        _moveDir += new Vector2(
            Mathf.Sin(Time.time * Random.Range(0, IrregularFactor)),
            Mathf.Cos(Time.time * Random.Range(0, IrregularFactor) * 1.3f)
        ) * 0.15f;
        _moveDir = _moveDir.normalized;

        base.UpdateWandering();
    }

    protected override void OnHandTouch() => OnDeath(DamageSource.Explosion);
}
