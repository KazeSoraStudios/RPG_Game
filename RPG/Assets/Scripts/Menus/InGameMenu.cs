using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameMenu : UIMonoBehaviour, IGameState
{
    [SerializeField] FrontMenuState FrontMenu;
    [SerializeField] ItemMenuState ItemMenu;
    [SerializeField] StatusMenuState StatusMenu;
    [SerializeField] MagicMenuState MagicMenu;
    [SerializeField] EquipMenuState EquipMenu;
    [SerializeField] FrontMenuState OptionMenu;

    public Map Map;
    public StateStack Stack;
    public StateMachine StateMachine;
    public FrontMenuState.Config FrontConfig;
    public StatusMenuState.Config StatusConfig;
    public MagicMenuState.Config MagicConfig;

    private List<ScrollViewCell> pool = new List<ScrollViewCell>();

    void Awake()
    {
        ServiceManager.Register(this);
        var states = new Dictionary<string, IState>();
        states.Add(Constants.FRONT_MENU_STATE, FrontMenu);
        states.Add(Constants.ITEM_MENU_STATE, ItemMenu);
        states.Add(Constants.STATUS_MENU_STATE, StatusMenu);
        states.Add(Constants.MAGIC_MENU_STATE, MagicMenu);
        states.Add(Constants.EQUIP_MENU_STATE, EquipMenu);
        states.Add(Constants.OPTION_MENU_STATE, OptionMenu);
        StateMachine = new StateMachine(states);
        FrontConfig = new FrontMenuState.Config
        {
            Parent = this
        };
        StatusConfig = new StatusMenuState.Config
        {
            Parent = this,
            StateMachine = StateMachine
        };
        MagicConfig = new MagicMenuState.Config
        {
            Parent = this
        };
    }

    void OnDestroy()
    {
        ServiceManager.Unregister(this);
    }

    public void Init(Map map, StateStack stack)
    {
        gameObject.SafeSetActive(true);
        Map = map;
        Stack = stack;
        StateMachine.Change(Constants.FRONT_MENU_STATE, FrontConfig);

    //    this.mStateMachine = StateMachine:Create
    //{
    //    ["frontmenu"] =
    //    function()
    //        return FrontMenuState:Create(this)
    //    end,
    //    ["items"] =
    //    function()
    //        return ItemMenuState:Create(this)
    //    end,
    //    ["magic"] =
    //    function()
    //        return MagicMenuState:Create(this)
    //    end,
    //    ["equip"] =
    //    function()
    //        return EquipMenuState:Create(this)
    //    end,
    //    ['status'] =
    //    function()
    //        return StatusMenuState:Create(this)
    //    end
    //}
    //this.mStateMachine:Change("frontmenu")
    }

    public bool Execute(float deltaTime)
    {
        if (Stack.Top().GetHashCode() == GetHashCode())
            StateMachine.Update(deltaTime);
        return true;
    }

    public void Enter(object o) { }

    public void Exit()
    {
        StateMachine.Stop();
        gameObject.SafeSetActive(false);
    }

    public void HandleInput() { }

    public string GetName()
    {
        return "InGameMenuState";
    }

    public bool HasCell()
    {
        return pool.Count > 0;
    }

    public ScrollViewCell GetCellFromPool()
    {
        if (pool.Count < 1)
        {
            LogManager.LogError("Tried to get cell but pool is empty.");
            return null;
        }
        int index = pool.Count - 1;
        var cell = pool[index];
        pool.RemoveAt(index);
        return cell;
    }

    public void ReturnCellToPool(ScrollViewCell cell)
    {
        pool.Add(cell);
        cell.gameObject.SafeSetActive(false);
        cell.transform.SetParent(transform, false);
    }

    /*
      local this =
    {
        mMapDef = mapDef,
        mStack = stack,
        mTitleSize = 1.2,
        mLabelSize = 0.88,
        mTextSize = 1,
    }     
     */
}
