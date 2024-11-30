using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GNT
{
    public class GroundLayerPositionMapper : MonoBehaviour
    {
        private SceneInterface activeScene;
        private SpriteRenderer spriteRenderer;
        private Collider2D collider;

        // Start is called before the first frame update
        void Start()
        {
            activeScene = GlobalData.Instance.ActiveScene;
            spriteRenderer = this.GetComponent<SpriteRenderer>();
            collider = this.GetComponent<Collider2D>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void TranslateToGroundHookPosition(float heightOffset)
        {
            spriteRenderer.enabled = false;
            GameObject positionHook = activeScene.ActiveGroundLayer.ScreenBottomHook;
            Vector3 deltaPositionTranslate = activeScene.ActiveGroundLayer.EdgeCollider.transform.position;
            deltaPositionTranslate.x = positionHook.transform.position.x;
            deltaPositionTranslate.y += collider.bounds.extents.y - collider.offset.y;
            deltaPositionTranslate -= this.transform.position;

            spriteRenderer.sortingOrder = activeScene.ActiveGroundLayer.SpriteLayerOrder;

            transform.Translate(deltaPositionTranslate);
            spriteRenderer.enabled = true;
        }
    }
}