using System.Collections.Generic;

public class Constants
{
    // PREFABS
    public const string HERO_PREFAB_PATH = "Prefabs/Character/Hero";
    public const string TEST_NPC_PREFAB_PATH = "Prefabs/Character/TestNPC";
    public const string TEST_MAP_PREFAB_PATH = "Prefabs/Maps/Grid";
    public const string ITEM_LIST_CELL_PREFAB_PATH = "Prefabs/UI/ItemCell";
    public const string FRONT_MENU_PREFAB = "Prefabs/UI/FrontMenu";
    public const string ITEM_MENU_PREFAB = "Prefabs/UI/ItemMenu";
    public const string STATUS_MENU_PREFAB = "Prefabs/UI/StatusMenu";
    public const string EQUIP_MENU_PREFAB = "Prefabs/UI/EquipmentMenu";
    public const string MAGIC_MENU_PREFAB = "Prefabs/UI/MagicMenu";
    public const string OPTION_MENU_PREFAB = "Prefabs/UI/OptionsMenu";
    public const string TITLE_MENU_PREFAB = "Prefabs/UI/TitleScreen";
    public const string GAME_OVER_PREFAB = "Prefabs/UI/GameOverScreen";
    public const string TEXTBOX_PREFAB = "Prefabs/UI/Textbox";

    // Portraits
    public const string PORTRAIT_PATH = "Textures/Portraits/";

    // Animations
    public const string HURT_ANIMATION = "hurt";
    public const string DEATH_ANIMATION = "death";

    // Stats
    public const int MAX_SPEED = 255;
    public const float CHANCE_TO_HIT = 0.7f;
    public const float CHANCE_TO_CRIT = 0.1f;
    public const float CHANCE_TO_DODGE = 0.03f;
    public const float COUNTER_MULTIPLIER = 0.99999f;
    public const float ESCAPE_CHANCE = 0.35f;
    public const float ESCAPE_BONUS = 0.15f;
    public const float CHANCE_TO_STEAL = 0.05f;
    public const int STEAL_BONUS = 50;
    public const float STEAL_MIN_CHANCE = 0.05f;
    public const float STEAL_MAX_CHANCE = 0.95f;
    public const string STAT_FILL_TEXT = "{0}/{1}";
    public const string EMPTY_ITEM_TEXT = "--";
    
    // Defaults
    public const int EMPTY_EVENT_COUNTDOWN = -999;
    public const int MAX_TEXTBOX_CHARACTERS = 225;
    public const float TEXTBOX_CHARACTER_SPEED = 0.02f;
    public const float SELECT_MARKER_BOUNCE_SPEED = 5.0f;

    // Character States
    public const string EMPTY_STATE = "empty";
    public const string WAIT_STATE = "wait";
    public const string MOVE_STATE = "move";
    public const string COMBAT_MOVE_STATE = "combat_move";
    public const string STROLL_STATE = "stroll";
    public const string STAND_STATE = "stand";
    public const string FOLLOW_PATH_STATE = "follow";
    public const string HURT_STATE = "hurt";
    public const string DIE_STATE = "death";
    public const string HURT_ENEMY_STATE = "enemy_hurt";
    public const string DIE_ENEMY_STATE = "enemy_death";
    public static List<string> PARTY_STATES = new List<string> { WAIT_STATE, MOVE_STATE, COMBAT_MOVE_STATE, STAND_STATE, FOLLOW_PATH_STATE, HURT_STATE, DIE_STATE };
    public static List<string> ENEMY_STATES = new List<string> { COMBAT_MOVE_STATE, STAND_STATE, FOLLOW_PATH_STATE, HURT_ENEMY_STATE, DIE_ENEMY_STATE };
    public static List<string> NPC_STATES = new List<string> { STAND_STATE, MOVE_STATE, FOLLOW_PATH_STATE, STROLL_STATE};

    // Game states
    public const string HANDIN_STATE = "handin";
    public const string HANDOFF_STATE = "handoff";
    public const string EXPLORE_STATE = "explore";
    public const string BATTLE_STATE = "battle";
    public const string MENU_STATE = "menu";
    public const string IN_GAME_MENU_STATE = "ingame";
    public const string FRONT_MENU_STATE = "front";
    public const string ITEM_MENU_STATE = "item";
    public const string STATUS_MENU_STATE = "status";
    public const string EQUIP_MENU_STATE = "equip";
    public const string MAGIC_MENU_STATE = "magic";
    public const string OPTION_MENU_STATE = "option";
    public const string TITLE_MENU_STATE = "title";
    public const string GAME_OVER_STATE = "gameover";

    // Storyboard states
    public const string SCREEN_STATE = "screenstate";

}