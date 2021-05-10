using UnityEngine;

namespace RPG_Character
{
    public class Entity : MonoBehaviour
    {
        [SerializeField] int height;
        [SerializeField] int width;
        [SerializeField] int selectPadding;
        [SerializeField] float speed = 1.0f;
        [SerializeField] Vector2 movement;
        [SerializeField] Rigidbody2D rigidbody;

        private Vector2 movementBeforeTextbox;

        private void Start()
        {
            rigidbody.velocity = Vector2.zero;
        }

        public void UpdateMovement(Vector2 movement, Map map = null)
        {
            this.movement.x = Mathf.Ceil(movement.x);
            this.movement.y = Mathf.Ceil(movement.y);

            if (movement != Vector2.zero)
                return;

            if (map == null || map.GetEntity(transform.position) is var entity && entity == null)
                return;
            if (entity.GetInstanceID() == GetInstanceID())
                map.RemoveEntity(this);
            var x = transform.position.x + movement.x;
            var y = transform.position.y + movement.y;
            var newPosition = new Vector2Int((int)x, (int)y);
            map.AddEntity(this, newPosition);
        }

        public void CombatMovement(Vector3 movement)
        {
            transform.position = movement;
        }

        private void FixedUpdate()
        {
            var move = rigidbody.position + movement * Time.fixedDeltaTime * speed;
            rigidbody.MovePosition(move);
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
        //function Entity:SetTilePos(x, y, layer, map)

        //    if map:GetEntity(self.mTileX, self.mTileY, self.mLayer) == self then
        //        map:RemoveEntity(self)
        //    end

        //    if map:GetEntity(x, y, layer, map) ~= nil then
        //        for k, v in pairs(map:GetEntity(x, y, layer, map)) do
        //            print(k, v)
        //        end
        //        assert(false)
        //    end

        //    self.mTileX = x or self.mTileX
        //    self.mTileY = y or self.mTileY
        //    self.mLayer = layer or self.mLayer

        //    map:AddEntity(self)
        //    local x, y = map:GetTileFoot(self.mTileX, self.mTileY)
        //    self.mSprite:SetPosition(x, y + self.mHeight * 0.5)
        //    self.mX = x
        //    self.mY = y


        //end

        public Vector2 GetSelectPosition()
        {
            var position = transform.position;
            var y = position.y + height * 0.5f + selectPadding;
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