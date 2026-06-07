using UnityEngine;
using UnityEngine.UI;

public class SliderBar : MonoBehaviour
{
    [SerializeField] private Slider slider;

    public void SetHealth(float health)
    {
        if (slider != null)
        {
            slider.value = health;
        }
    }
}