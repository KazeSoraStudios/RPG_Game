using System;
using System.Collections.Generic;
using UnityEngine;
using RPG_Character;
using RPG_UI;
using RPG_GameData;

namespace RPG_Combat
{
    public class CombatChoiceState : ConfigMonoBehaviour, IGameState
    {
        public class Config
        {
            public Actor Actor;
            public ICombatState State;
        }

        [SerializeField] MenuOptionsList OptionsList;

        private StateStack stack;
        private ICombatState combatState;
        private Actor actor;

        public void Init(Config config)
        {
            if (CheckUIConfigAndLogError(config, GetName()))
                return;
            combatState = config.State;
            actor = config.Actor;
            stack = combatState.Stack();
            InitMenuOptionsList();
        }

        public void Hide()
        {
            gameObject.SafeSetActive(false);
        }

        public void Show()
        {
            gameObject.SafeSetActive(true);
        }

        public bool Execute(float deltaTime)
        {
            //OptionsList.ApplySelectionBounce(deltaTime);
            return true;
        }

        public void HandleInput()
        {
            OptionsList.HandleInput();
        }

        public void Enter(object o)
        {
            Show();
        }

        public void Exit()
        {
            Hide();
        }

        public string GetName()
        {
            return "CombatChoiceState";
        }

        private void InitMenuOptionsList()
        {
            if (OptionsList == null)
            {
                LogManager.LogError("MenuOptionsList is null in CombatChoiceState.");
                return;
            }
            var config = new MenuOptionsList.Config
            {
                ShowSelection = true,
                OnClick = OnSelect
            };
            OptionsList.Init(config);
        }

        private void OnSelect(int selection)
        {
            LogManager.LogDebug($"{selection} selected.");
            switch (selection)
            {
                case 0: // Fight
                    OptionsList.HideCursor();
                    var targetConfig = new CombatTargetState.Config
                    {
                        GameState = combatState,
                        CombatTargetType = CombatTargetType.One,
                        CanSwitchSides = false,
                        OnSelect = TakeAction,
                        OnExit = () => OptionsList.ShowCursor()
                    };
                    var targetState = combatState.GetUI().TargetState;
                    targetState.Init(targetConfig);
                    stack.Push(targetState);
                    break;
                case 1: // Item
                    OnSelectAction(BrowseList.Item, "ID_ITEM_ACTION_TITLE");
                    break;
                case 2: // Magic
                    OnSelectAction(BrowseList.Magic, "ID_MAGIC_ACTION_TITLE");
                    break;
                case 3:// Special
                    OnSelectAction(BrowseList.Special, "ID_SPECIAL_ACTION_TITLE");
                    break;
                case 4: // Flee
                    // Remove the choice state (this)
                    stack.Pop();
                    var queue = combatState.EventQueue();
                    var fleeConfig = new CEFlee.Config
                    {
                        Actor = actor,
                        State = combatState
                    };
                    var fleeEvent = new CEFlee(fleeConfig);
                    var speed = -1;//fleeEvent.CalculatePriority(queue);
                    queue.Add(fleeEvent, speed);
                    break;
                default:
                    LogManager.LogError($"Selection: {selection} is not a valid combat choice.");
                    break;
            }
        }

        private void OnSelectAction(BrowseList browseList, string title)
        {
            OptionsList.HideCursor();
            Action<CombatBrowseListState, string> onSelect = null;
            if (browseList == BrowseList.Item)
                onSelect = OnSelectItem;
            else if (browseList == BrowseList.Magic)
                onSelect = OnSelectMagic;
            else if (browseList == BrowseList.Special)
                onSelect = OnSelectSpecial;
            var config = new CombatBrowseListState.Config
            {
                Browse = browseList,
                Title = title,
                Actor = actor,
                Stack = stack,
                OnExit = () => { 
                    combatState.HideTip();
                    OptionsList.ShowCursor();
                },
                OnRender = (c) => { return c == null ? string.Empty : c.Name; }, 
                OnFocus = (c) => {
                    var text = string.Empty;
                    if (c == null || c.Description.IsEmptyOrWhiteSpace())
                        text = c.Description;
                    combatState.ShowTip(text);
                },
                OnSelect = onSelect
            };
            combatState.GetUI().ActionBrowseList.Init(config);
            stack.Push(combatState.GetUI().ActionBrowseList);
        }

        private void OnSelectItem(CombatBrowseListState action, string id)
        {
            if (id.IsEmptyOrWhiteSpace())
            {
                LogManager.LogError("Empty Item id passed to OnSelectItem.");
                return;
            }
            var items = ServiceManager.Get<GameData>().Items;
            if (!items.Contains(id))
            {
                LogManager.LogError($"Id [{id}] not found in GameData Items.");
                return;
            }
            var item = items[id];
            var targeter = CreateActionTargeter(action, item);
            if (targeter == null)
            {
                LogManager.LogError($"CombatTargetState was returned null to OnSelectItem.");
                return;
            }
            stack.Push(targeter);
        }

        private void OnSelectMagic(CombatBrowseListState action, string id)
        {
            if (id.IsEmptyOrWhiteSpace())
            {
                LogManager.LogError("Empty Spell id passed to OnSelectItem.");
                return;
            }
            var spells = ServiceManager.Get<GameData>().Spells;
            if (!spells.ContainsKey(id))
            {
                LogManager.LogError($"Id [{id}] not found in GameData Spells.");
                return;
            }
            var spell = spells[id];
            if (!actor.CanCast(spell)) // TODO play sound effect
                return;
            var targeter = CreateActionTargeter(action, spell);
            if (targeter == null)
            {
                LogManager.LogError($"CombatTargetState was returned null to OnSelectMagic.");
                return;
            }
            stack.Push(targeter);
        }

        private void OnSelectSpecial(CombatBrowseListState action, string id)
        {
            if (id.IsEmptyOrWhiteSpace())
            {
                LogManager.LogError("Empty Special id passed to OnSelectItem.");
                return;
            }
            var specials = ServiceManager.Get<GameData>().Specials;
            if (!specials.ContainsKey(id))
            {
                LogManager.LogError($"Id [{id}] not found in GameData Specials.");
                return;
            }
            var special = specials[id];
            if (!actor.CanCast(special)) // TODO play sound effect
                return;
            var targeter = CreateActionTargeter(action, special);
            if (targeter == null)
            {
                LogManager.LogError($"CombatTargetState was returned null to OnSelectSpecial.");
                return;
            }
            stack.Push(targeter);
        }

        private CombatTargetState CreateActionTargeter(CombatBrowseListState action, object def)
        {
            
            if (def == null)
            {
                LogManager.LogError("Def is null, cannot create Targeter.");
                return null;
            }
            var isItem = true;
            Tuple<Func<ICombatState, bool, List<Actor>>, bool> selectorInfo;
            string description = string.Empty;
            if (def is ItemInfo info)
            {
                selectorInfo = GetItemSelectorInfo(info);
                description = info.Description;
            }
            else if (def is Spell spell)
            {
                selectorInfo = GetSpellSelectorInfo((Spell)def);
                isItem = false;
                description = spell.Description;
            }
            else
            {
                LogManager.LogError($"Def is unsupported Type: {def.GetType()}.");
                return null;
            }
            combatState.ShowTip(description);
            action.Hide();
            Hide();

            Action<List<Actor>> onSelect = (targets) => {
                // Pop all the UI states
                stack.Pop();
                stack.Pop();
                stack.Pop();
                var queue = combatState.EventQueue();
                IEvent combatEvent;
                if (isItem)
                {
                    var item = (ItemInfo)def;
                    var itemUse = ServiceManager.Get<GameData>().ItemUses[item.Use];
                    var itemConfig = new CEUseItemEvent.Config
                    { 
                        Actor = actor,
                        CombatState = combatState,
                        Item = item,
                        ItemUse = itemUse,
                        Targets = targets,
                        IsPlayer = true
                    };
                    combatEvent = new CEUseItemEvent(itemConfig);
                }
                else
                {
                    var spell = (Spell)def;
                    var spellConfig = new CECastSpellEvent.Config
                    { 
                        Actor = actor,
                        CombatState = combatState,
                        spell = spell,
                        IsPlayer = true,
                        Targets = targets
                    };
                    combatEvent = new CECastSpellEvent(spellConfig);
                }
                var priority = -1;//combatEvent.CalculatePriority(queue);
                queue.Add(combatEvent, priority);
            };

            Action onExit = () => {
                action.Show();
                Show();
            };

            var config = new CombatTargetState.Config
            {
                GameState = combatState,
                CanSwitchSides = selectorInfo.Item2,
                DefaultSelector = selectorInfo.Item1,
                OnSelect = onSelect,
                OnExit = onExit
            };
            var targetState = combatState.GetUI().TargetState;
            targetState.Init(config);
            return targetState;
        }

        private Tuple<Func<ICombatState, bool, List<Actor>>, bool> GetItemSelectorInfo(ItemInfo item)
        {
            var itemUses = ServiceManager.Get<GameData>().ItemUses;
            if (!itemUses.ContainsKey(item.Use))
            {
                LogManager.LogError($"GameData ItemUses does not contain ItemUse [{item.Use}] for Item [{item.Id}].");
                return null;
            }
            var target = itemUses[item.Use].Target;
            return Tuple.Create(target.Selector, target.SwitchSides);
        }

        private Tuple<Func<ICombatState, bool, List<Actor>>, bool> GetSpellSelectorInfo(Spell spell)
        {            
            if (spell == null)
            {
                LogManager.LogError("Null Spell passed to GetSpellSelectorInfo.");
                return null;
            }
            return Tuple.Create(spell.ItemTarget.Selector, spell.ItemTarget.SwitchSides);

        }

        private void TakeAction(List<Actor> targets)
        {
            // Pop browse and action state
            stack.Pop();
            stack.Pop();
            var config = new CEAttack.Config
            { 
                Actor = actor,
                CombatState = combatState,
                IsPlayer = true,
                Targets = targets,
                IsCounter = false
            };
            var attackEvent = new CEAttack(config);
            var queue = combatState.EventQueue();
            var priority = -1;// TODO possibly queue attacks? attackEvent.CalculatePriority(queue);
            queue.Add(attackEvent, priority);
        }
    }
}