using System.Collections;
using _Scripts.Scriptable_Objects.Configs;
using DG.Tweening;
using UnityEngine;

public class AudioFilterController
{
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
    private readonly AudioFilterConfig _config;
    public AudioFilterController(AudioFilterConfig config)
    {
        _config = config;
    }
    
    private readonly string _parameterName = "Filter";
    private readonly float _maxValueMult = 22000f;
    private Tween _fadeTween;
    // ===== ===== ===== ===== ===== ===== ===== ===== ===== ===== 
    
    /// <param name="enable">on/off?</param>
    /// <param name="instant">switch instantly?</param>
    public void ToggleFilter(bool enable, bool instant = false)
    {
        float filterOn = _config.OnOffFilterAmount.x * _maxValueMult;
        float filterOff = _config.OnOffFilterAmount.y * _maxValueMult;
        float targetCutoff = enable ? filterOn : filterOff;

        if (instant)
            _config.AudioMixer.SetFloat(_parameterName, targetCutoff);

        else
            FadeFilterTween(targetCutoff);
    }

    private void FadeFilterTween(float targetCutoff)
    {
        _fadeTween?.Kill();
        _config.AudioMixer.GetFloat(_parameterName, out float startCutoff);

        _fadeTween = DOVirtual.Float(startCutoff, targetCutoff, _config.TransitionDuration, value =>
            {
                _config.AudioMixer.SetFloat(_parameterName, value);
            }).SetUpdate(true);
    }
}
