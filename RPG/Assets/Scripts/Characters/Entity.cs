using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    [SerializeField] int height;
    [SerializeField] int width;
    [SerializeField] int selectPadding;
    [SerializeField] float speed = 1.0f;
    [SerializeField] Vector2 movement;
    [SerializeField] Texture2D texture;
    [SerializeField] Sprite sprite;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] Rigidbody2D rigidbody;
    public float m = 1.0f;
    
    public void UpdateMovement(Vector2 movement, Map map)
    {
        this.movement.x = Mathf.Ceil(movement.x);
        this.movement.y = Mathf.Ceil(movement.y);

        if (movement != Vector2.zero)
            return;

        if (map.GetEntity(transform.position) is var entity && entity == null)
            return;
        if (entity.GetInstanceID() == GetInstanceID())
            map.RemoveEntity(this);
        var x = transform.position.x + movement.x;
        var y = transform.position.y + movement.y;
        var newPosition = new Vector2Int((int)x, (int)y);
        map.AddEntity(this, newPosition);
    }

    public void AddKnockback(Vector2 force)
    {
        var target = new Vector3(transform.position.x + force.x * m, transform.position.y + force.y * m);
        StartCoroutine(mov(target));
    }

    public float time = 1.0f;
    private IEnumerator mov(Vector3 target)
    {
        float t = 0.0f;
        while (t < time)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, target, t / time);
            yield return null;
        }
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
        var x = position.x + width * 0.5f;
        return new Vector2(x, position.y);
    }

    public void Hide()
    {
        spriteRenderer.sortingOrder = -1;
    }

    public void Show()
    {
        spriteRenderer.sortingOrder = 2;
    }

    //function Entity:AddChild(id, entity)
    //    assert(self.mChildren [id] == nil)
    //    self.mChildren [id] = entity
    //end

    //function Entity:RemoveChild(id)
    //    self.mChildren [id] = nil
    //end

    //function Entity:Render(renderer)
    //    renderer:DrawSprite(self.mSprite)

    //    for k, v in pairs(self.mChildren) do
    //        local sprite = v.mSprite
    //        sprite:SetPosition(self.mX + v.mX, self.mY + v.mY)
    //        renderer:DrawSprite(sprite)
    //    end

    //end

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
