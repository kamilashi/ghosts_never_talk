using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNT
{
    /*
        public interface I_Interactable
        {
            public void Interact(Collider2D teleporteeCollider = null);
        }*/
    enum AnimationState
    {
        Inactive,
        Enter,
        Loop,
        Exit
    }

    public class Interactable : MonoBehaviour
    {
        [Header("Interactable")]
        public int UIPromptKey;
        public float InteractRadius;
        public GroundLayer ContainingGroundLayer;
        public AnimationClip InteractAnimation;
        public Vector2 LocalOffset;
        public float SnapSpeed = 1.0f;
        public bool SnapToLocalOffset = true;

        //private bool isEnabled = true;
        VfxPlayer vfxPlayer;

        void Awake()
        {
            if (ContainingGroundLayer == null)
            {
                ContainingGroundLayer = this.transform.GetComponentInParent<GroundLayer>();
            }

            vfxPlayer = gameObject.GetComponent<VfxPlayer>();
        }

        public bool IsInRange(Vector3 interactorPos/*, ref float squareDistance*/)
        { 
            bool isInRange = false;

            if (ContainingGroundLayer == GlobalData.Instance.ActiveScene.ActiveGroundLayer)
            {
                Vector3 toInteractor = interactorPos;
                toInteractor -= this.transform.position;
                toInteractor.z = 0.0f;

                isInRange = toInteractor.sqrMagnitude <= InteractRadius * InteractRadius;
            }

            return isInRange;
        }

        private IEnumerator MoveToInteractionX(Transform interactorTransform, GroundMovement interactorGroundMovement = null)
        {
            Debug.Log("Interact coroutine started");

            Vector3 targetWorldPosition = this.transform.position;
            targetWorldPosition.x += LocalOffset.x;
            float currentDistance = -1.0f;
            float initialDistanceX = Mathf.Abs(interactorTransform.position.x - transform.position.x);
            float epsilon = 0.01f;

            do
            {
                Vector3 toInteractable = interactorTransform.position;
                toInteractable -= targetWorldPosition;

                currentDistance = Mathf.Abs(toInteractable.x);

                float travelProgressLinear = currentDistance / initialDistanceX;
                Debug.Log(travelProgressLinear);

                float velocityX = Mathf.Sign(toInteractable.x) * Mathf.Min(SnapSpeed * Time.deltaTime, currentDistance);
                Vector3 translate = Vector3.zero;
                translate.x = velocityX;
                interactorTransform.Translate(translate);
                yield return null;
            }
            while (currentDistance > epsilon); 
            
            if (interactorGroundMovement != null)
            {
                interactorGroundMovement.StopAndPlayAnimation(InteractAnimation);
            }

            Debug.Log("Interact coroutine ended");

            yield return null;
        }

        public void Interact(Transform interactorTransform, GroundMovement groundMovement = null)
        {
            if (groundMovement != null)
            {
                groundMovement.StopAndPlayAnimation(InteractAnimation);
            }

            //StartCoroutine(MoveToInteractionX(interactorTransform, groundMovement));
        }
    }
}