using UnityEngine;

namespace RPG_Character
{
    public class Entity : MonoBehaviour
    {
        [SerializeField] float speed = 1.0f;
        [SerializeField] Vector2 movement;
        [SerializeField] Rigidbody2D rigidbody;

        private Vector2 movementBeforeTextbox;

        private void Start()
        {
            rigidbody.velocity = Vector2.zero;
        }

        public void UpdateMovement(Vector2 movement)
        {
            this.movement.x = Mathf.Ceil(movement.x);
            this.movement.y = Mathf.Ceil(movement.y);
        }

        public void CombatMovement(Vector3 movement)
        {
            transform.position = movement;
        }

        private void FixedUpdate()
        {
            if (rigidbody.bodyType == RigidbodyType2D.Static)
                return;
            var move = rigidbody.position + movement * Time.fixedDeltaTime * speed;
            rigidbody.MovePosition(move);
        }

        public void SetTilePosition(Vector2 position)
        {
            transform.position = position;
        }

        public void SetTilePosition(int x, int y, int layer, Map map)
        {
            if (map.GetEntity(transform.position))
            {
                map.RemoveEntity(this);
            }

            var position = new Vector2(x, y);
            transform.position = position;
            map.AddEntity(this);

        }

        public Vector2 GetSelectPosition()
        {
            var position = transform.position;
            var y = position.y + 1;
            return new Vector2(position.x, y);
        }

        public Vector2 GetTargetPosition()
        {
            var position = transform.position;
            var x = position.x - 1.5f;
            return new Vector2(x, position.y);
        }

        public void Hide()
        {
            gameObject.SafeSetActive(false);
        }

        public void Show()
        {
            gameObject.SafeSetActive(true);
        }

        public void PrepareForTextbox()
        {
            movementBeforeTextbox = movement;
            movement = Vector2.zero;
        }

        public void ReturnFromTextbox()
        {
            movement = movementBeforeTextbox;
        }
    }
}