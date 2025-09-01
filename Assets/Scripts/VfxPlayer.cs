using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GNT
{
    public class VfxPlayer : MonoBehaviour
    {
        public GameObject VfxPrefab;
        //public string MaterialAnimationInputName;
        public float EnterSpeed = 3.0f;
        public float ExitSpeed = 3.0f;
        public float EnterExitDuration = 1.0f;
        public float LoopDuration = -1.0f;

        private SpriteRenderer cashedVfxSpriteRenderer;
        private GameObject cashedVfxInstance;
        private float vfxProgressFraction = 0.0f;
        private Coroutine animationCoroutine;

        enum AnimationInputMode
        {
            NonInverse = 0,
            Inverse = 1
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if(LoopDuration > 0.0f)
            {
                cashedVfxSpriteRenderer.material.SetFloat("_ProgressFraction", 1.0f);
            }
        }

        private IEnumerator OnVfxPlayCoroutine(float duration, float speed, AnimationInputMode inputMode = AnimationInputMode.NonInverse, bool destroyOnFinish = false)
        {
            float elapsedTime = duration * Mathf.Abs((float)inputMode - vfxProgressFraction);
            vfxProgressFraction = Mathf.Abs(((float)inputMode) - (elapsedTime / duration));
            
            while (elapsedTime < duration) 
            {
               cashedVfxSpriteRenderer.material.SetFloat("_ProgressFraction", vfxProgressFraction);
               elapsedTime += speed * Time.deltaTime;
               vfxProgressFraction = Mathf.Abs(((float)inputMode) - (elapsedTime / duration));
               vfxProgressFraction = Mathf.Clamp(vfxProgressFraction, 0.0f, 1.0f);
               yield return null;
            }

            cashedVfxSpriteRenderer.material.SetFloat("_ProgressFraction", vfxProgressFraction);

            if (destroyOnFinish)
            {
                Destroy(cashedVfxInstance);
                cashedVfxSpriteRenderer = null;
            }
        }

        public void PlayVfxEnter(int spriteLayerOrder, float scale)
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            Destroy(cashedVfxInstance);
            cashedVfxSpriteRenderer = null;

            Transform vfxTransform = this.transform;
            cashedVfxInstance = Instantiate<GameObject>(VfxPrefab, this.transform);
            cashedVfxInstance.transform.localScale = new Vector3(scale, scale, scale);
            cashedVfxSpriteRenderer = cashedVfxInstance.GetComponent<SpriteRenderer>();
            cashedVfxSpriteRenderer.sortingOrder = spriteLayerOrder;

            animationCoroutine = StartCoroutine(OnVfxPlayCoroutine(EnterExitDuration, EnterSpeed));
        }

        public void PlayVfxExit()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            animationCoroutine = StartCoroutine(OnVfxPlayCoroutine(EnterExitDuration, ExitSpeed, AnimationInputMode.Inverse, true));
        }
    }
}