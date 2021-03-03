using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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

    [SerializeField] TextMeshProUGUI StrengthText;
    [SerializeField] TextMeshProUGUI SpeedText;
    [SerializeField] TextMeshProUGUI IntelligenceText;
    [SerializeField] TextMeshProUGUI AttackText;
    [SerializeField] TextMeshProUGUI DefenseText;
    [SerializeField] TextMeshProUGUI MagicText;
    [SerializeField] TextMeshProUGUI ResistText;
    [SerializeField] TextMeshProUGUI WeaponText;
    [SerializeField] TextMeshProUGUI ArmorText;
    [SerializeField] TextMeshProUGUI AccesoryText;
    [SerializeField] ActorSummaryPanel ActorSummary;

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
            return;
        }
        gameObject.SafeSetActive(true);
        this.config = config;
        ActorSummary.Init(new ActorSummaryPanel.Config {
            Actor = config.Actor,
            ShowExp = true
        });
        SetStatsText();
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

    private void SetStatsText()
    {
        var actor = config.Actor;
        var stats = actor.Stats;
        StrengthText.SetText(stats.Get(Stat.Strength).ToString());
        SpeedText.SetText(stats.Get(Stat.Speed).ToString());
        IntelligenceText.SetText(stats.Get(Stat.Intelligence).ToString());
        AttackText.SetText(stats.Get(Stat.Attack).ToString());
        DefenseText.SetText(stats.Get(Stat.Defense).ToString());
        MagicText.SetText(stats.Get(Stat.Magic).ToString());
        ResistText.SetText(stats.Get(Stat.Resist).ToString());
        WeaponText.SetText(actor.GetEquipmentName(Actor.EquipSlot.Weapon).ToString());
        ArmorText.SetText(actor.GetEquipmentName(Actor.EquipSlot.Armor).ToString());
        AccesoryText.SetText(actor.GetEquipmentName(Actor.EquipSlot.Accessory1).ToString());
    }
}
