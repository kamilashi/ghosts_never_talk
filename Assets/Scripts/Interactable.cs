using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNT
{
    enum AnimationState
    {
        Inactive,
        Enter,
        Loop,
        Exit
    }

    public class Interactable : SplinePointObject
    {
        [Header("Interactable")]
        public int UIPromptKey;
        public float InteractRadius;
        public AnimationClip InteractAnimation;
        public float LocalOffsetX;
        public float SnapSpeed = 1.0f;
        public bool SnapToLocalOffset = true;

        [SerializeField]
        protected VfxPlayer vfxPlayerStaticRef;

        protected void BaseAwakeInteractable()
        {
            base.BaseAwakeSplinePointObject();
            vfxPlayerStaticRef = gameObject.GetComponent<VfxPlayer>();
        }

        public bool IsInRangeX(Vector3 interactorPos/*, ref float squareDistance*/)
        { 
            bool isInRange = false;

            if (ContainingGroundLayer == GlobalData.Instance.ActiveSceneDynamicRef.ActiveGroundLayer)
            {
                Vector3 toInteractor = interactorPos;
                toInteractor -= this.transform.position;

                isInRange = Mathf.Abs(toInteractor.x) <= InteractRadius;
            }

            return isInRange;
        }

        private IEnumerator MoveToInteractionX(Transform interactorTransform, System.Action onCoroutineFinishedInteractAction, GroundMovement interactorGroundMovement = null)
        {
            if(SnapToLocalOffset)
            {
                Vector3 targetWorldPosition = this.transform.position;
                targetWorldPosition.x += LocalOffsetX;
                float currentDistance = -1.0f;
                float initialDistanceX = Mathf.Abs(interactorTransform.position.x - transform.position.x);
                float epsilon = 0.01f;

                do
                {
                    Vector3 toInteractable = targetWorldPosition;
                    toInteractable -= interactorTransform.position;

                    currentDistance = Mathf.Abs(toInteractable.x);

                    float travelProgressLinear = currentDistance / initialDistanceX;

                    float velocityX = Mathf.Sign(toInteractable.x) * Mathf.Min(SnapSpeed * Time.deltaTime * Mathf.SmoothStep(0.2f, 0.8f, travelProgressLinear), currentDistance);
                    Vector3 translate = Vector3.zero;
                    translate.x = velocityX;
                    interactorTransform.Translate(translate);

                    if (interactorGroundMovement != null)
                    {
                        interactorGroundMovement.AddSplineLocalOffset(velocityX);
                    }

                    yield return null;
                }
                while (currentDistance > epsilon);
            }
            
            if (interactorGroundMovement != null)
            {
                interactorGroundMovement.StopAndPlayAnimation(InteractAnimation);
            }

            onCoroutineFinishedInteractAction?.Invoke();
        }

        protected virtual void onInteractCoroutineFinished()
        { }
        public virtual void OnBecomeAvailable()
        {
            vfxPlayerStaticRef.PlayVfxEnter(ContainingGroundLayer.SpriteLayerOrder, InteractRadius * 2.0f);
        }
        public virtual void OnBecomeUnavailable()
        {
            vfxPlayerStaticRef.PlayVfxExit();
        }

        public void Interact(Transform interactorTransform, GroundMovement groundMovement = null)
        {
            StartCoroutine(MoveToInteractionX(interactorTransform, onInteractCoroutineFinished, groundMovement));
        }

    }
}