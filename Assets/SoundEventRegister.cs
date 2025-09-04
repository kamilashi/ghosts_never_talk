using GNT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEventRegister : MonoBehaviour
{
    private FMODUnity.StudioEventEmitter[] emitters;
    void Awake()
    {
        /*if(GameManager.isLoaded)
        {
            RegisterSoundEvents();
        }
        else
        {
            GameManager.OnInitialized += RegisterSoundEvents;
        }*/
        GameManager.OnSceneLoadFinishEvent += RegisterSoundEvents;
    }

    private void OnDestroy()
    {
        GameManager.OnSceneLoadFinishEvent -= RegisterSoundEvents;

        if(emitters == null)
        {
            return;
        }

        for (int i = 0; i < emitters.Length; i++)
        {
            emitters[i].Stop();
        }
    }

    // Update is called once per frame
    [ContextMenu("RegisterSoundEvents")]
    void RegisterSoundEvents()
    {
        emitters = gameObject.GetComponents<FMODUnity.StudioEventEmitter>();
        for (int i = 0; i < emitters.Length; i++)
        {
            GameManager.Instance.RegisterSoundbankEvent(emitters[i].EventInstance, emitters[i].EventReference.Path);
        }
    }

}
