using UnityEngine;
using TMPro;

public class StatusMenuState : UIMonoBehaviour, IGameState
{
    public class Config
    {
        public Actor Actor;
        public InGameMenu Parent;
        public StateMachine StateMachine;
        public StateStack StateStack;
    }

    [SerializeField] ActorSummaryPanel ActorSummary;
    [SerializeField] ItemInfoWidget ItemInfo;
    [SerializeField] StatsWidget StatsWidget;

    private Config config;

    public void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            config.StateMachine.Change(Constants.FRONT_MENU_STATE, config.Parent.FrontConfig);
        }
    }

    public void Enter(object o)
    {
        if (CheckUIConfigAndLogError(o, GetName()) || ConvertConfig<Config>(o, out var config) && config == null)
        {
            LogManager.LogError("Config is null or not of type Config in StatsMenuState");
            return;
        }
        gameObject.SafeSetActive(true);
        this.config = config;
        ActorSummary.Init(new ActorSummaryPanel.Config {
            Actor = config.Actor,
            ShowExp = true
        });
        InitStatsWidget(config.Actor.Stats);
        InitItemInfo(config.Actor);
    }

    public bool Execute(float deltaTime)
    {
        HandleInput();
        return true;
    }

    public void Exit()
    {
        gameObject.SafeSetActive(false);
    }

    public string GetName()
    {
        return "StatusMenuState";
    }

    private void InitStatsWidget(Stats stats)
    {
        if (StatsWidget == null)
        {
            LogManager.LogError("StatsWidget is null in StatusMenuState");
            return;
        }
        StatsWidget.Init(new StatsWidget.Config{ Stats = stats });
    }

    private void InitItemInfo(Actor actor)
    {
        if (ItemInfo == null)
        {
            LogManager.LogError("ItemInfoWidget is null in StatusMenuState");
            return;
        }
        ItemInfo.Init(new ItemInfoWidget.Config{ Actor = actor });
    }
}
