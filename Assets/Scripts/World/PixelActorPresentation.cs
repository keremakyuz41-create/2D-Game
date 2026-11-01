using UnityEngine;

namespace Cernobil.World
{
    public sealed class PixelActorPresentation : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer visual;
        [SerializeField] private bool faceMouse;
        [SerializeField] private Sprite[] movementFrames;
        private Sprite idleSprite;
        private float animationTime;
        private Rigidbody2D body;
        private Vector3 restingPosition;
        private Camera gameCamera;
        private float walkPhase;

        public void Configure(SpriteRenderer renderer, bool isPlayer)
        {
            visual = renderer;
            faceMouse = isPlayer;
        }

        public void ConfigureAnimation(Sprite[] frames) => movementFrames = frames;

        public void RepairAnimation(Sprite[] frames)
        {
            movementFrames = frames;
            if (frames != null && frames.Length > 0 && frames[0] != null)
                visual.sprite = frames[0];
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            gameCamera = Camera.main;
            restingPosition = visual.transform.localPosition;
            // Missing serialized sprite references must never blank a visible character.
            if (movementFrames != null)
            {
                movementFrames = System.Array.FindAll(movementFrames, frame => frame != null);
                if (visual.sprite == null && movementFrames.Length > 0) visual.sprite = movementFrames[0];
            }
            idleSprite = visual.sprite;
        }

        private void LateUpdate()
        {
            Vector2 velocity = body.linearVelocity;
            float horizontal = velocity.x;
            if (faceMouse && gameCamera != null)
                horizontal = gameCamera.ScreenToWorldPoint(Input.mousePosition).x - transform.position.x;
            if (Mathf.Abs(horizontal) > 0.05f) visual.flipX = horizontal < 0;
            bool walking = velocity.sqrMagnitude > 0.02f;
            bool hasFrames = movementFrames != null && movementFrames.Length > 0;
            if (hasFrames)
            {
                if (walking)
                {
                    animationTime += Time.deltaTime * Mathf.Clamp(velocity.magnitude * 2, 4, 14);
                    visual.sprite = movementFrames[Mathf.FloorToInt(animationTime) % movementFrames.Length];
                }
                else
                {
                    animationTime = 0;
                    visual.sprite = idleSprite;
                }
            }
            if (walking) walkPhase += Time.deltaTime * 12f;
            visual.transform.localPosition = restingPosition + Vector3.up *
                (walking ? Mathf.Abs(Mathf.Sin(walkPhase)) * 0.045f : 0);
            visual.transform.localRotation = Quaternion.Euler(0, 0,
                walking && !hasFrames ? Mathf.Sin(walkPhase) * 2.5f : 0);
            visual.sortingOrder = 100 - Mathf.RoundToInt(transform.position.y * 10);
        }
    }
}
