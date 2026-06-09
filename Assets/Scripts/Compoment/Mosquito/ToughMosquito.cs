using UnityEngine;

public class ToughMosquito : MosquitoBase
{
    [Header("Tough Mosquito")]
    public float FallSpeed = 1.5f;
    public float RecoverDuration = 3f;

    public Transform StaggerIndicator;

    enum ToughState { Normal, Falling }
    ToughState _toughState = ToughState.Normal;
    float _recoverTimer;
    int _hits;

    protected override void Update()
    {
        if (_toughState == ToughState.Falling)
        {
            transform.Translate(Vector2.down * FallSpeed * Time.deltaTime);
            _recoverTimer -= Time.deltaTime;
            if (_recoverTimer <= 0f)
                Recover();
        }
        else
        {
            base.Update();
        }
    }

    protected override void OnClapHit()
    {
        _hits++;
        if (_hits >= 2 || _state == MosquitoState.Attacking) //Parried lol
        {
            OnDeath(DamageSource.Explosion);
            return;
        }
        // First hit: start falling
        _toughState = ToughState.Falling;
        _recoverTimer = RecoverDuration;

        StaggerIndicator.gameObject.SetActive(true);
    }

    void Recover()
    {
        _toughState = ToughState.Normal;
        _moveDir = Random.insideUnitCircle.normalized;

        StaggerIndicator.gameObject.SetActive(false);
        EnterState(MosquitoState.Wandering);
    }
}
