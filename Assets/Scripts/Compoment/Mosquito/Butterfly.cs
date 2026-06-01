using UnityEngine;

public class Butterfly : MosquitoBase
{
    protected override void UpdateWandering()
    {
        transform.Translate(_moveDir * MoveSpeed * Time.deltaTime);

        _wanderTimer -= Time.deltaTime;
        if (_wanderTimer <= 0f)
        {
            _wanderTimer = WanderChangeInterval + Random.Range(-0.3f, 0.3f);
            _moveDir = Random.insideUnitCircle.normalized;
        }
        // No attack state transition — butterflies only wander
    }

    // Killing the butterfly deducts score instead of adding
    protected override void OnDeath(DamageSource source)
    {
        ScoreManager.Instance?.Add(-ScoreValue);
        ReturnToPool();
    }

}
