using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class WorldSpaceButton : MonoBehaviour
{
    public UnityEvent OnPressed;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Explosion"))
            OnPressed.Invoke();
    }
}
