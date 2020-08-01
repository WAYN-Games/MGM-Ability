
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
public class AbilityVisualEffect : MonoBehaviour
{
    private VisualEffect _vfx;
    private bool IsPlaying;

    void Awake()
    {
        _vfx = GetComponent<VisualEffect>();
    }

    private void Start()
    {
        Play();
    }

    public void Play()
    {
        _vfx.Play();
        IsPlaying = true;
    }

    public void Stop()
    {
        _vfx.Stop();
        IsPlaying = false;
    }

    void Update()
    {
        if (!IsPlaying && _vfx.aliveParticleCount <= 0)
        {
            Destroy(gameObject);
        }
    }


}
