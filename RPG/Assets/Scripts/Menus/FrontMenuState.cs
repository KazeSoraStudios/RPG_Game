using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace RPG_UI
{
    public class FrontMenuState : UIMonoBehaviour, IGameState
    {
        public class Config
        {
            public InGameMenu Parent;
            public string MapName;
        }

        [SerializeField] TextMeshProUGUI MenuNameText;
        [SerializeField] TextMeshProUGUI GoldText;
        [SerializeField] TextMeshProUGUI TimePlayedText;
        [SerializeField] MenuOptionsList MenuList;
        [SerializeField] ActorSummaryList SummariesController;

        private bool inActorSummary;
        private int framesPassed = 0;
        private int updateTimeFrames = 30;
        private Config config;
        private StateStack stack;
        private StateMachine stateMachine;
        private MenuOptionsList.Config menuConfig = new MenuOptionsList.Config();

        public bool Execute(float deltaTime)
        {
            framesPassed++;
            if (framesPassed >= updateTimeFrames)
            {
                TimePlayedText.SetText(ServiceManager.Get<World>().TimeAsString());
                framesPassed = 0;
            }
            HandleInput();
            return true;
        }

        public void Enter(object o)
        {
            if (CheckUIConfigAndLogError(o, GetName()) || ConvertConfig<Config>(o, out var config) && config == null)
            {
                return;
            }
            gameObject.SafeSetActive(true);
            this.config = config;
            stack = config.Parent.Stack;
            stateMachine = config.Parent.StateMachine;
            inActorSummary = false;
            SummariesController.HideCursor();
            MenuNameText.SetText(ServiceManager.Get<LocalizationManager>().Localize(config.MapName));
            GoldText.SetText(ServiceManager.Get<World>().Gold.ToString());
            TimePlayedText.SetText(ServiceManager.Get<World>().TimeAsString());
            MenuList.Init(menuConfig);
            SummariesController.Init(new ActorSummaryList.Config
            {
                Party = ServiceManager.Get<World>().Party,
                OnClick = HandleSummarySelection,
                OnBack = BackFromPartyMenu
            });
        }

        public void Exit()
        {
            gameObject.SafeSetActive(false);
            inActorSummary = false;
            SummariesController.HideCursor();
            MenuList.HideCursor();
        }

        public void HandleInput()
        {
            int direction = Input.GetKeyDown(KeyCode.UpArrow) ? -1 :
                Input.GetKeyDown(KeyCode.DownArrow) ? 1 : 0;
            if (inActorSummary)
                SummariesController.HandleInput(direction);
            else
                HandleMenuOptionInput(direction);
        }

        public string GetName()
        {
            return "Front Menu State";
        }

        private void HandleMenuOptionInput(float direction)
        {
            if (direction > 0)
            {
                MenuList.IncreaseSelection();
            }
            else if (direction < 0)
            {
                MenuList.DecreaseSelection();
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                var selection = MenuList.GetSelection();
                HandleOptionSelection(selection);

            }
            else if (Input.GetKeyDown(KeyCode.Backspace))
            {
                stack.Pop();
            }
        }

        private void HandleOptionSelection(int selection)
        {
            switch (selection)
            {
                case 0:
                    stateMachine.Change(Constants.ITEM_MENU_STATE, config.Parent.ItemMenuConfig);
                    break;
                case 1:
                case 2:
                case 3:
                    inActorSummary = true;
                    MenuList.HideCursor();
                    SummariesController.SetMenu(selection);
                    MenuNameText.SetText(ServiceManager.Get<LocalizationManager>().Localize("ID_PICK_A_CHARACTER_TEXT"));
                    SummariesController.ShowCursor();
                    break;
                case 4:
                    // Open options window
                    break;
                default:
                    LogManager.LogError($"Unknown selection in front menu state: {selection}");
                    break;
            }
        }

        private void HandleSummarySelection(int selection, int menu)
        {
            var party = ServiceManager.Get<World>().Party;
            switch (menu)
            {
                case 1:
                    config.Parent.StatusConfig.Actor = party.GetActor(selection);
                    stateMachine.Change(Constants.STATUS_MENU_STATE, config.Parent.StatusConfig);
                    break;
                case 2:
                    config.Parent.EquipConfig.Actor = party.GetActor(selection);
                    stateMachine.Change(Constants.EQUIP_MENU_STATE, config.Parent.EquipConfig);
                    break;
                case 3:
                    config.Parent.MagicConfig.Actor = party.GetActor(selection);
                    stateMachine.Change(Constants.MAGIC_MENU_STATE, config.Parent.MagicConfig);
                    break;
                default:
                    LogManager.LogError($"Unknown selection in front menu state: {selection}");
                    break;
            }
        }


        private void BackFromPartyMenu()
        {
            inActorSummary = false;
            MenuNameText.SetText(ServiceManager.Get<LocalizationManager>().Localize(config.MapName));
            SummariesController.HideCursor();
            MenuList.ShowCursor();
        }

        /*

    function FrontMenuState:RenderMenuItem(menu, renderer, x, y, item)

        local color = Vector.Create(1, 1, 1, 1)
        local canSave = self.mParent.mMapDef.can_save
        local text = item.text

        if item.id == "save" and not canSave then
            color = Vector.Create(0.6, 0.6, 0.6, 1)
        end

        if item.id == "load" and not Save:DoesExist() then
            color = Vector.Create(0.6, 0.6, 0.6, 1)
        end

        if item then
            local font = gGame.Font.default
            font:AlignText("left", "center")
            font:DrawText2d(renderer, x, y, text, color)
        end
    end


    function Selection:JumpToFirstItem()
        self.mFocusY = 1
        self.mFocusX = 1
        self.mDisplayStart = 1
    end

    function FrontMenuState:OnMenuClick(index, item)

        if item.id == "items" then
            return self.mStateMachine:Change("items")
        end

        if item.id == "save" then
            if self.mParent.mMapDef.can_save then
                self.mStack:Pop()
                Save:Save()
                self.mStack:PushFit(gRenderer, 0, 0, "Saved!")
            end
            return
        end

        if item.id == "load" then
            if Save:DoesExist() then
                Save:Load()
                gGame.Stack:PushFit(gRenderer, 0, 0, "Loaded!")
            end
            return
        end

        self.mPartyMenu:JumpToFirstItem()
        self.mInPartyMenu = true
        self.mSelections:HideCursor()
        self.mPartyMenu:ShowCursor()
        self.mPrevTopBarText = self.mTopBarText
        self.mTopBarText = "Choose a party member"

        if item.id == "magic" then
            local member = self.mPartyMenu:SelectedItem()
            local memberCount = #self.mPartyMenu.mDataSource

            while member.mActor.mId ~= "mage" and
                self.mPartyMenu.mFocusY < memberCount do

                self.mPartyMenu:MoveDown()
                member = self.mPartyMenu:SelectedItem()
            end

            if(member.mActor.mId ~= "mage") then
                print("Couldn't find mage!");
                self.mPartyMenu:JumpToFirstItem()
            end

        end
    end

    function FrontMenuState:OnPartyMemberChosen(actorIndex, actorSummary)

        local item = self.mSelections:SelectedItem()
        local actor = actorSummary.mActor

        self.mStateMachine:Change(item.stateId, actor)
    end


    function FrontMenuState:Render(renderer)

        local font = gGame.Font.default

        for k, v in ipairs(self.mPanels) do
            v:Render(renderer)
        end

        renderer:ScaleText(self.mParent.mTitleSize, self. mParent.mTitleSize)
        renderer:AlignText("left", "center")
        local menuX = self.mLayout:Left("menu") - 16
        local menuY = self.mLayout:Top("menu") - 24
        self.mSelections:SetPosition(menuX, menuY)
        self.mSelections:Render(renderer)

        local nameX = self.mLayout:MidX("top")
        local nameY = self.mLayout:MidY("top")
        font:AlignText("center", "center")
        font:DrawText2d(renderer, nameX, nameY, self.mTopBarText)

        local goldX = self.mLayout:MidX("gold") - 22
        local goldY = self.mLayout:MidY("gold") + 22

        font:AlignText("right", "top")
        font:DrawText2d(renderer, goldX, goldY, "GP:")
        font:DrawText2d(renderer, goldX, goldY - 25, "TIME:")
        font:AlignText("left", "top")

        font:AlignText("left", "top")
        font:DrawText2d(renderer, math.floor(goldX + 10), math.floor(goldY), gGame.World:GoldAsString())
        font:DrawText2d(renderer, goldX + 10, goldY - 25, gGame.World:TimeAsString())

        self.mPartyMenu:Render(renderer)
    end

    function FrontMenuState:HideOptionsCursor()
        self.mSelections:HideCursor()
    end

    function FrontMenuState:GetPartyAsSelectionTargets()
        local targets = {}

        local x = self.mPartyMenu.mX
        local y = self.mPartyMenu.mY
        local cursorWidth = self.mPartyMenu:CursorWidth()

        for k, v in ipairs(self.mPartyMenu.mDataSource) do

            local indexFrom0 = k - 1

            table.insert(targets,
                         {
                            x = x + cursorWidth * 0.5,
                            y = y - (indexFrom0 * self.mPartyMenu.mSpacingY),
                            summary = v
                        })
        end
        return targets
    end


         */
    }
}