using UnityEngine;
using RPG_Character;

public class TeleportTrigger : MonoBehaviour
{
    [SerializeField] Vector2 TeleportPosition;
    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (!other.tag.Equals("Player"))
            return;
        var character = other.GetComponent<Character>();
        character.ReturnFromCombat();
        character.UpdateMovement(Vector2.zero);
        Actions.Teleport(character.Entity, TeleportPosition);
    }
}