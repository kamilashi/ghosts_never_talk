using GNT;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SoundEventRegister : MonoBehaviour
{
    private FMODUnity.StudioEventEmitter[] emitters;
    public static int emitterObjectsCount = 0;

    void Awake()
    {
        if (!GameManager.isLoaded)
        {
            return;
        }

        emitterObjectsCount++;

        emitters = gameObject.GetComponents<FMODUnity.StudioEventEmitter>();

        for (int i = 0; i < emitters.Length; i++)
        {
            RegisterSoundEvent(emitters[i]);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        for (int i = 0; i < emitters.Length; i++)
        {
            emitters[i].Play();
            RegisterSoundEvent(emitters[i]);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        for (int i = 0; i < emitters.Length; i++)
        {
            emitters[i].Stop();
        }
    }

    private void OnDestroy()
    {
        if(emitters == null)
        {
            return;
        }

        for (int i = 0; i < emitters.Length; i++)
        {
            emitters[i].Stop();
        }
    }

    void RegisterSoundEvent(FMODUnity.StudioEventEmitter emitter)
    {
        GameManager.Instance.RegisterSoundbankEvent(emitter.EventInstance, emitter.EventReference.Path);
    }

}
