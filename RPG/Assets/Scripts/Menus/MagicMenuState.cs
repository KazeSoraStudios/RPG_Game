using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MagicMenuState : UIMonoBehaviour, IGameState, IScrollHandler
{
    public class Config
    {
        public InGameMenu Parent;
        public Actor Actor;
    }

    [SerializeField] TextMeshProUGUI ActorNameText;
    [SerializeField] TextMeshProUGUI DescriptionText;
    [SerializeField] TextMeshProUGUI MpText;
    [SerializeField] ProgressBar MpBar;
    [SerializeField] ScrollView ScrollView;
    [SerializeField] GridLayoutGroup Grid;

    private InGameMenu parent;
    private StateStack stack;
    private StateMachine stateMachine;
    private List<ItemListCell.Config> configs = new List<ItemListCell.Config>();

    public void Enter(object o = null)
    {
        if (CheckUIConfigAndLogError(o, GetName()) || ConvertConfig<Config>(o, out var config) && config == null)
            return;
        gameObject.SafeSetActive(true);
        parent = config.Parent;
        stack = parent.Stack;
        stateMachine = parent.StateMachine;
        ActorNameText.SetText(config.Actor.Name);
        var stats = config.Actor.Stats;
        var mp = stats.Get(Stat.MP);
        var maxMp = stats.Get(Stat.MaxMP);
        MpText.SetText(string.Format(Constants.STAT_FILL_TEXT, mp, maxMp));
        MpBar.SetTargetFillAmountImmediate(mp / (float)maxMp);
        configs.Clear();
        SetUpMenu(config.Actor);
    }

    public bool Execute(float deltaTime)
    {
        HandleInput();
        ScrollView.Execute();
        return true;
    }

    public void Exit()
    {
        ScrollView.CleanUp();
        gameObject.SafeSetActive(false);
    }

    public string GetName()
    {
        return "MagicMenuState";
    }

    public void HandleInput()
    {
        ScrollView.HandleInput();

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            stateMachine.Change(Constants.FRONT_MENU_STATE, parent.FrontConfig);
        }
    }

    public string GetPrefabPath()
    {
        return Constants.ITEM_LIST_CELL_PREFAB_PATH;
    }

    public void InitCell(int index, ScrollViewCell cell)
    {
        if (index < 0)
        {
            LogManager.LogError($"Index[{index}] is out of range for magic menu configs.");
            return;
        }
        if (!ConvertConfig<ItemListCell>(cell, out var listCell) || listCell == null)
        {
            LogManager.LogError($"Cell {cell.GetType()}, passed to maigc menu but expected ItemListCell");
            return;
        }
        // If we go past the end return the empty item we added
        var config = index < configs.Count - 1 ? configs[index] : configs[configs.Count - 1];
        listCell.Init(config);
    }

    public void OnAfterLoad()
    {
        
    }

    public Vector2 GetCellSize()
    {
        return Grid.cellSize;
    }

    public int GetNumberOfCells()
    {
        return configs.Count;
    }

    private void SetUpMenu(Actor actor)
    {
        //foreach(var spell in actor.Spells)
        //{
        //    configs.Add(new ItemListCell.Config
        //    {
        //        Name = spell.LocalizedName(),
        //        Amount = spell.MpCost.ToString(),
        //        ShowIcon = false
        //    });
        //}

        var spells = ServiceManager.Get<GameData>().Spells;
        foreach (var entry in spells)
        {
            var spell = entry.Value;
            configs.Add(new ItemListCell.Config
            {
                Spell = spell,
                ShowIcon = false
            });
        }
        configs.Add(new ItemListCell.Config
        {
            ShowIcon = false
        });

        ScrollView.Init(this, SetDescription, parent);
    }

    private void SetDescription(int index)
    {
        var cell = configs[index];
        var description = cell.Spell?.Description ?? string.Empty;
        DescriptionText.SetText(ServiceManager.Get<LocalizationManager>().Localize(description));
    }
}
