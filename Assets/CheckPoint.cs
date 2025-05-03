using GNT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CheckPoint : SplinePointObject
{
    [Header("CheckPoint")]

    [SerializeField] protected VfxPlayer vfxPlayerStaticRef; // maybe move to the SplinePointObject too
    //////////////////////// 

    // Start is called before the first frame update
    void Awake()
    {
        base.BaseAwakeSplinePointObject();
        vfxPlayerStaticRef = gameObject.GetComponent<VfxPlayer>();
        splinePointObjectType = SplinePointObjectType.CheckPoint;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // needs to be unified with interactable
    public void OnBecomeAvailable()
    {
        isHidden = true;
        vfxPlayerStaticRef.PlayVfxEnter(ContainingGroundLayer.SpriteLayerOrder, DetectionRadius * 2.0f);
    }
    public void OnBecomeUnavailable()
    {
        isHidden = false;
        vfxPlayerStaticRef.PlayVfxExit();
    }

}
