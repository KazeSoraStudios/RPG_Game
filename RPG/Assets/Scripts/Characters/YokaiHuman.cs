using UnityEngine;

namespace RPG_Character
{
    public class YokaiHuman : MonoBehaviour
    {
        [SerializeField] string AreaId;
        [SerializeField] string EventId;
        [SerializeField] Sprite HumanSprite;

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
            {
                spriteRenderer.sprite = HumanSprite;
                // TODO change animator
                GetComponent<Animator>().enabled = false;
            }
        }

        public void ChangeToHuman()
        {
            spriteRenderer.sprite = HumanSprite;
            // TODO change animator
            GetComponent<Animator>().enabled = false;
        }
    }
}
