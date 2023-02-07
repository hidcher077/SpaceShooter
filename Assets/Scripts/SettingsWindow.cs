using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsWindow : MonoBehaviour
{
    [SerializeField] private Slider _slideMusic;
    [SerializeField] private Slider _sliderEffect;

    private void Awake()
    {
        _slideMusic.value = Controller.Instance.MusicSource.volume;
        _slideMusic.value = Controller.Instance.EffectSource.volume;

    }

    public void ChangeValueMusic(float value)
    {
        Controller.Instance.MusicSource.volume = value;
    }
    public void ChangeValueEffect(float value)
    {
        Controller.Instance.EffectSource.volume = value;
    }
}
