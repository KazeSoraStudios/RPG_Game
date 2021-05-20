using System;
using UnityEngine;
using UnityEngine.UI;
using RPG_Character;

namespace RPG_UI
{
    public class ActorSummaryList : ConfigMonoBehaviour
    {
        public class Config
        {
            public Party Party;
            public Action<int, int> OnClick;
            public Action OnBack;
        }

        [SerializeField] float SelectionPadding;
        [SerializeField] Image SelectionArrow;
        [SerializeField] ActorSummaryPanel[] ActorSummaries;

        private int menu = 0;
        private int currentSelection;
        private int memberCount = 1;
        private Config config;

        public void Init(Config config)
        {
            if (CheckUIConfigAndLogError(config, name))
            {
                return;
            }
            this.config = config;
            InitPanels();
        }

        public void HandleInput()
        {
            //local item = self.mSelections:SelectedItem()

            //if item.id == "magic" then
            //    if Keyboard.JustPressed(KEY_SPACE) then
            //        self.mPartyMenu:OnClick()
            //    end
            //    return
            //end
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                IncreaseSelection();
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                DecreaseSelection();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                config.OnClick?.Invoke(currentSelection, menu);

            }
            else if (Input.GetKeyDown(KeyCode.Backspace))
            {
                config.OnBack?.Invoke();
            }
        }

        public void SetMenu(int menu)
        {
            this.menu = menu;
        }

        public void HideCursor()
        {
            SelectionArrow.gameObject.SetActive(false);
        }

        public void ShowCursor()
        {
            SelectionArrow.gameObject.SetActive(true);
        }

        public int GetSelection()
        {
            return currentSelection;
        }

        public void IncreaseSelection()
        {
            currentSelection++;
            if (currentSelection >= memberCount)
                currentSelection = 0;
            var y = ActorSummaries[currentSelection].transform.position.y;
            var position = new Vector2(SelectionArrow.transform.position.x, y);
            SelectionArrow.transform.position = position;
        }

        public void DecreaseSelection()
        {
            currentSelection--;
            if (currentSelection < 0)
                currentSelection = memberCount - 1;
            var y = ActorSummaries[currentSelection].transform.position.y;
            var position = new Vector2(SelectionArrow.transform.position.x, y);
            SelectionArrow.transform.position = position;
        }

        private void InitPanels()
        {
            var partyMembers = config.Party.Members;
            memberCount = partyMembers.Count;
            var numberOfPanels = ActorSummaries.Length;
            int i = 0;
            for (; i < memberCount && i < numberOfPanels; i++)
            {
                var config = new ActorSummaryPanel.Config
                {
                    Actor = partyMembers[i],
                    ShowExp = true
                };
                ActorSummaries[i].Init(config);
            }
            for (; i < numberOfPanels; i++)
                ActorSummaries[i].gameObject.SafeSetActive(false);
        }
    }
}