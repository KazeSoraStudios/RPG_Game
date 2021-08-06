using UnityEngine;
using RPG_Character;

public class TeleportTrigger : MonoBehaviour
{
    [SerializeField] Vector2 TeleportPosition;
    [SerializeField] bool EnterBuilding;
    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (!other.tag.Equals("Player"))
            return;
        var character = other.GetComponent<Character>();
        character.PrepareForSceneChange();
        character.UpdateMovement(Vector2.zero);
        ServiceManager.Get<RPG_Audio.AudioManager>().SetOverallVolume(1f);
        if (EnterBuilding)
		{
            ServiceManager.Get<RPG_Audio.AudioManager>().SetOverallVolume(0.5f);
		}
		Actions.Teleport(character.Entity, TeleportPosition);
    }

}