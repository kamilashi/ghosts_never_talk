using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GNT
{
    public class InteractableTrigger : Interactable
    {
        [Header("Interactable Trigger")]
        public UnityEvent TriggerAction;

        [Header("Transform Animation - needs to move into a separate component")]
        public Transform AnimatedTransform;
        public float EnterExitHeightDelta = 1.0f;
        public float EnterExitSpeed = 1.0f;

        public float LoopSpeed = 1.0f;
        public float LoopSwingAmplitude = 1.0f;
        public float LoopRotateRadPerSecond = 1.0f;

        [SerializeField]
        AnimationState currentState;
        [SerializeField]
        Quaternion initialRotation;
        [SerializeField]
        Vector3 initialRotationEuler;
        Coroutine animationCoroutine;
        float timer;
        float initialPosY;

        private void Awake()
        {
            BaseAwakeInteractable();

            Debug.Assert(AnimatedTransform != null, "Please specify the animated transform for this script, even if it belongs to the same gameObject!");
            Debug.Assert(TriggerAction != null);

            currentState = AnimationState.Inactive;
            timer = 0.0f;
            initialPosY = AnimatedTransform.position.y;
            initialRotation = AnimatedTransform.rotation;
            initialRotationEuler = initialRotation.eulerAngles;

            splinePointObjectType = SplinePointObjectType.InteractableTrigger;
        }
        void Start()
        {
            OnBecomeVisible();
        }

        void Update()
        {
            if(currentState == AnimationState.Loop)
            {
                TransformAnimateLoop();
            }
        }

        void OnBecomeVisible()
        {
            GlobalData.Instance.ActiveSceneDynamicRef.AddPlayerVisibleInteractableTrigger(this);
        }

        void OnBecomeInvisible()
        {
            GlobalData.Instance.ActiveSceneDynamicRef.RemovePlayerVisibleInteractableTrigger(this);
        }

        protected override void onInteractCoroutineFinished()
        {
            TriggerAction?.Invoke();
        }
        public override void OnBecomeAvailable()
        {
            base.OnBecomeAvailable();
            TransformAnimateEnter();
        }

        public override void OnBecomeUnavailable()
        {
            base.OnBecomeUnavailable();
            TransformAnimateExit();
        }

        private IEnumerator OnTransformAnimateCoroutine(int direction /* +1 = Up, -1 = down*/)
        {
            float error = 0.0f;
            float targetYPos = direction > 0.0f ? initialPosY + EnterExitHeightDelta : initialPosY;
            Quaternion startRotation = AnimatedTransform.rotation;

            float posDiffY = Mathf.Abs(targetYPos - AnimatedTransform.position.y);
            do
            {
                float progress = posDiffY / EnterExitHeightDelta;
                float velocityY = (float)direction * Mathf.Min(EnterExitHeightDelta * Time.deltaTime * Mathf.SmoothStep(0.2f, 0.8f, progress), posDiffY);
                AnimatedTransform.Translate(0.0f, velocityY, 0.0f);
                posDiffY = Mathf.Abs(targetYPos - AnimatedTransform.position.y);

                float eulerY = Mathf.LerpAngle(startRotation.eulerAngles.y, initialRotation.eulerAngles.y, progress);
                float deltaY = Mathf.DeltaAngle(transform.rotation.eulerAngles.y, eulerY);
                AnimatedTransform.Rotate(Vector3.up , deltaY < 0.0f ? (deltaY * Mathf.Deg2Rad) : (180.0f + deltaY) * Mathf.Deg2Rad);
                //AnimatedTransform.Rotate(Vector3.up , deltaY * Mathf.Deg2Rad);
                //Debug.Log("eulerY " + eulerY);
                //Debug.Log("deltaY " + deltaY);
                //Debug.Log("SPIN COROUTINE");

                yield return null;
            }
            while (posDiffY != 0.0f);

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
            float translateDeltaY = initialPosY + EnterExitHeightDelta + yOffset - AnimatedTransform.position.y;
            AnimatedTransform.Translate(0.0f, translateDeltaY, 0.0f);

            AnimatedTransform.Rotate(Vector3.up, Time.deltaTime * LoopRotateRadPerSecond);

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
