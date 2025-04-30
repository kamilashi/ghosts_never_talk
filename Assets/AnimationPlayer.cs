using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNT
{
    public class AnimationPlayer : MonoBehaviour
    {
        AnimatorStateInfo CurrentAnimationState;
        int CurrentAnimationStateHash = 0;
        int PreviousAnimationStateHash = 0;

        Animator animatorStaticRef;

        void Awake()
        {
            animatorStaticRef = gameObject.GetComponent<Animator>();
        }

        void Update()
        {
            AnimatorStateInfo stateInfo = animatorStaticRef.GetCurrentAnimatorStateInfo(0);
            if(stateInfo.shortNameHash != CurrentAnimationStateHash)
            {
                setCurrentStateHash(stateInfo);
            }
        }

        void setCurrentStateHash(AnimatorStateInfo animationState)
        {
            PreviousAnimationStateHash = CurrentAnimationStateHash;
            CurrentAnimationStateHash = animationState.shortNameHash;
            CurrentAnimationState = animationState;
        }
        
        public bool HasAnimationFinished(int stateHash)
        {
            if (CurrentAnimationStateHash == stateHash) 
            {
                return CurrentAnimationState.normalizedTime >= 1.0f;
            }

            return PreviousAnimationStateHash == stateHash; // if the tested animation IS scheduled, we act as if it hasn't finished yet. If not - it's finished.
        }
    }
}