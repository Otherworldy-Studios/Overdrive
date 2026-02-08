using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class StanceVignette : MonoBehaviour
{
    [SerializeField] private float min = 0.1f;
    [SerializeField] private float max = 1f;
    [SerializeField] private float response = 10f;
    private VolumeProfile profile;
    private Vignette vignette;
    public void Initialize(VolumeProfile _profile)
    {
        profile = _profile;
        if (!profile.TryGet(out vignette))
        {
            vignette = profile.Add<Vignette>();
        }
        vignette.intensity.Override(min);
    }

    public void UpdateVignette(float deltaTime, Stance stance)
    {
        var targetIntensity = stance is Stance.Stand ? min : max;
        vignette.intensity.value = Mathf.Lerp(vignette.intensity.value, targetIntensity, 1f - Mathf.Exp(-response * deltaTime));
        Debug.Log(vignette.intensity.value);
    }
}
