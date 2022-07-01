using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class VFXControler : MonoBehaviour
{

    public List<AbilityVFX> controllers = new List<AbilityVFX>();
    public List<float> lifeTimes = new List<float>();
    public List<bool> isLoopingVfx = new List<bool>();


    // Start is called before the first frame update
    void Start()
    {
        foreach(var controller in controllers)
        {
            float duration = float.MinValue;
            bool isLooping = false;
            ParticleSystem[] psList = controller.VfxPrefab.GetComponentsInChildren<ParticleSystem>();
            foreach (var ps in psList)
            {
                duration = Mathf.Max(duration, ps.main.duration + ps.main.startLifetime.constantMax);
                isLooping |= ps.main.loop;
            }
            lifeTimes.Add(duration);
            isLoopingVfx.Add(isLooping);
        }
    }

}
