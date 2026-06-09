using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BloodEffect : MonoBehaviour
{
    ParticleSystem particle;
    float _timer;

    void Start()
    {
        particle = GetComponent<ParticleSystem>();
        particle.Play();    // the partical will destroy itself so don't worry about it
    }

    void Update()
    {
    }
}
