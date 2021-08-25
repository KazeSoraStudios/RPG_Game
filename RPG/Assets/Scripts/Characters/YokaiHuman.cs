using UnityEngine;

namespace RPG_Character
{
    public class YokaiHuman : MonoBehaviour
    {
        [SerializeField] string AreaId;
        [SerializeField] string EventId;
        [SerializeField] string NPCTextId;
        [SerializeField] Sprite HumanSprite;
        [SerializeField] private RuntimeAnimatorController HumanAnimator;

        private SpriteRenderer spriteRenderer;

        private void Awake() 
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                LogManager.LogError($"Sprite Renderer is not on entity {name}");
            }
        }

        private void Start()
        {
            var gameState = ServiceManager.Get<GameLogic>().GameState;
            if (gameState.IsEventComplete(AreaId, EventId))
                ChangeToHuman();
        }

        public void ChangeToHuman()
        {
            gameObject.SafeSetActive(true);
            spriteRenderer.sprite = HumanSprite;
            if (HumanAnimator != null)
                GetComponent<Animator>().runtimeAnimatorController = HumanAnimator;
            else
                GetComponent<Animator>().enabled = false;
            if (!NPCTextId.IsEmptyOrWhiteSpace())
            {
                var dialogue = gameObject.AddComponent<SimpleDialogueNPC>();
                dialogue.SetText(NPCTextId);
            }
        }
    }
}
