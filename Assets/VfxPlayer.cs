using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GNT
{
    public class VfxPlayer : MonoBehaviour
    {
        public GameObject VfxPrefab;
        //public string MaterialAnimationInputName;
        public float StartSpeed = 1.0f;
        public float EndSpeed = 1.0f;
        public float StartEndDuration = 1.0f;
        public float RunDuration = -1.0f;


        private SpriteRenderer cashedVfxSpriteRenderer;
        private GameObject cashedVfxInstance;
        [SerializeField]
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
            if(RunDuration > 0.0f)
            {
                cashedVfxSpriteRenderer.material.SetFloat("_ProgressFraction", 1.0f);
            }
        }

        private IEnumerator OnVfxPlayCoroutine(float duration, float speed, AnimationInputMode inputMode = AnimationInputMode.NonInverse, bool destroyOnFinish = false)
        {
            float elapsedTime = duration * Mathf.Abs((float)inputMode - vfxProgressFraction);
            vfxProgressFraction = Mathf.Abs(((float)inputMode) - (elapsedTime / duration));
            do
            {
                cashedVfxSpriteRenderer.material.SetFloat("_ProgressFraction", vfxProgressFraction);
                elapsedTime += speed*Time.deltaTime;
                vfxProgressFraction = Mathf.Abs(((float)inputMode) - (elapsedTime / duration));
                vfxProgressFraction = Mathf.Clamp(vfxProgressFraction, 0.0f, 1.0f);
                yield return null;
            }
            while (elapsedTime < duration);

            cashedVfxSpriteRenderer.material.SetFloat("_ProgressFraction", vfxProgressFraction);

            if (destroyOnFinish)
            {
                Destroy(cashedVfxInstance);
                cashedVfxSpriteRenderer = null;
            }
        }

        public void PlayVfxStart(int spriteLayerOrder)
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            Destroy(cashedVfxInstance);
            cashedVfxSpriteRenderer = null;

            cashedVfxInstance = Instantiate<GameObject>(VfxPrefab, this.transform);
            cashedVfxSpriteRenderer = cashedVfxInstance.GetComponent<SpriteRenderer>();
            cashedVfxSpriteRenderer.sortingOrder = spriteLayerOrder;

            animationCoroutine = StartCoroutine(OnVfxPlayCoroutine(StartEndDuration, StartSpeed));
        }

        public void PlayVfxFinish()
        {
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            animationCoroutine = StartCoroutine(OnVfxPlayCoroutine(StartEndDuration, EndSpeed, AnimationInputMode.Inverse, true));
        }
    }
}