using UnityEngine;

namespace Cernobil.Player
{
    [RequireComponent(typeof(Rigidbody2D), typeof(KeyboardMovementInput))]
    public sealed class PlayerMovement : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float moveSpeed = 4f;
        [SerializeField] private bool sprintEnabled;
        [SerializeField, Min(1)] private float sprintMultiplier = 1.6f;
        public bool IsSprinting { get; private set; }
        private Rigidbody2D body;
        private IMovementInput movementInput;
        private Vector2 direction;

        public void EnableSprint() => sprintEnabled = true;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            movementInput = GetComponent<IMovementInput>();
        }

        private void OnEnable()
        {
            body = GetComponent<Rigidbody2D>();
            movementInput = GetComponent<IMovementInput>();
        }

        private void Update()
        {
            direction = movementInput.ReadMovement();
            IsSprinting = sprintEnabled && direction.sqrMagnitude > 0.01f &&
                (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
        }

        private void FixedUpdate()
        {
            body.linearVelocity = direction * moveSpeed * (IsSprinting ? sprintMultiplier : 1f);
        }

        private void OnDisable()
        {
            direction = Vector2.zero;
            IsSprinting = false;
            if (body != null) body.linearVelocity = Vector2.zero;
        }
    }
}
