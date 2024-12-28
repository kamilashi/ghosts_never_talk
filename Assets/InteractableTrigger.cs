using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNT
{
    public class InteractableTrigger : Interactable
    {
        [Header("Transform Animation - Interactable Trigger")]
        public float EnterExitHeightDelta = 1.0f;
        public float EnterExitSpeed = 1.0f;

        public float LoopSpeed = 1.0f;
        public float LoopSwingAmplitude = 1.0f;

        AnimationState currentState;
        Coroutine animationCoroutine;
        float timer;
        float initialPosY;

        private void Awake()
        {
            currentState = AnimationState.Inactive;
            initialPosY = transform.position.y;
            timer = 0.0f;
        }
        void Start()
        {
            OnBecameVisible();
        }

        void Update()
        {
            if(currentState == AnimationState.Loop)
            {
                TransformAnimateLoop();
            }
        }

        void OnBecameVisible()
        {
            GlobalData.Instance.ActiveScene.AddPlayerVisibleInteractableTrigger(this);
        }

        void OnBecameInvisible()
        {
            GlobalData.Instance.ActiveScene.RemovePlayerVisibleInteractableTrigger(this);
        }
        private IEnumerator OnTransformAnimateCoroutine(int direction /* +1 = Up, -1 = down*/)
        {
            float error = 0.0f;
            float targetYPos = direction > 0.0f ? initialPosY + EnterExitHeightDelta : initialPosY;

            float posDiffY = Mathf.Abs(targetYPos - transform.position.y);
            do
            {
                float velocityY = (float)direction * Mathf.Min(EnterExitHeightDelta * Time.deltaTime * Mathf.SmoothStep(0.2f, 0.8f, posDiffY/ EnterExitHeightDelta),  posDiffY);
                transform.Translate(0.0f, velocityY, 0.0f);
                posDiffY = Mathf.Abs(targetYPos - transform.position.y);
                yield return null;
            }
            while (posDiffY > error);

            currentState = (currentState == AnimationState.Enter) ? AnimationState.Loop : AnimationState.Inactive;
        }

        public void TransformAnimateEnter()
        {
            if(currentState != AnimationState.Enter)
            {
                if (animationCoroutine != null)
                {
                    StopCoroutine(animationCoroutine);
                }

                currentState = AnimationState.Enter;
                timer = 0.0f;
                animationCoroutine = StartCoroutine(OnTransformAnimateCoroutine(1));
            }
        }

        public void TransformAnimateLoop()
        {
            float yOffset = Mathf.Sin(timer) * LoopSwingAmplitude;
            float translateDeltaY = initialPosY + EnterExitHeightDelta + yOffset - transform.position.y;
            transform.Translate(0.0f, translateDeltaY, 0.0f);

            //transform.Rotate(Vector3.up, Time.deltaTime * LoopSpeed);

            timer += Time.deltaTime * LoopSpeed;
        }

        public void TransformAnimateExit()
        {
            if (currentState != AnimationState.Exit)
            {
                if (animationCoroutine != null)
                {
                    StopCoroutine(animationCoroutine);
                }
                currentState = AnimationState.Exit;
                animationCoroutine = StartCoroutine(OnTransformAnimateCoroutine(-1));
            }
        }
    }
}
