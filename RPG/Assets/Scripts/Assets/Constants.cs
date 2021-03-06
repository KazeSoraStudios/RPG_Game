public class Constants
{
    // PREFABS
    public static string HERO_PREFAB_PATH = "Prefabs/Character/Hero";
    public static string TEST_MAP_PREFAB_PATH = "Prefabs/Maps/Grid";
    public static string ITEM_LIST_CELL_PREFAB_PATH = "Prefabs/UI/ItemCell";

    // Portraits
    public static string PORTRAIT_PATH = "Textures/Portraits/";

    // Animations
    public static string HURT_ANIMATION = "hurt";
    public static string DEATH_ANIMATION = "death";

    // Stats
    public static int MAX_SPEED = 255;
    public static float CHANCE_TO_HIT = 0.7f;
    public static float CHANCE_TO_CRIT = 0.1f;
    public static float CHANCE_TO_DODGE = 0.03f;
    public static float COUNTER_MULTIPLIER = 0.99999f;
    public static float ESCAPE_CHANCE = 0.35f;
    public static float ESCAPE_BONUS = 0.15f;
    public static float CHANCE_TO_STEAL = 0.05f;
    public static int STEAL_BONUS = 50;
    public static float STEAL_MIN_CHANCE = 0.05f;
    public static float STEAL_MAX_CHANCE = 0.95f;
    public static string STAT_FILL_TEXT = "{0}/{1}";
    public static string EMPTY_ITEM_TEXT = "--";
    
    // Defaults
    public static int EMPTY_EVENT_COUNTDOWN = -999;
    public static float SELECT_MARKER_BOUNCE_SPEED = 5.0f;

    // Character States
    public static string EMPTY_STATE = "empty";
    public static string WAIT_STATE = "wait";
    public static string MOVE_STATE = "move";
    public static string STROLL_STATE = "stroll";
    public static string STAND_STATE = "stand";
    public static string FOLLOW_PATH_STATE = "follow";
    public static string HURT_STATE = "hurt";
    public static string DIE_STATE = "death";


    // Game states
    public static string HANDIN_STATE = "handin";
    public static string HANDOFF_STATE = "handoff";
    public static string EXPLORE_STATE = "explore";
    public static string BATTLE_STATE = "battle";
    public static string MENU_STATE = "menu";
    public static string IN_GAME_MENU_STATE = "ingame";
    public static string FRONT_MENU_STATE = "front";
    public static string ITEM_MENU_STATE = "item";
    public static string STATUS_MENU_STATE = "status";
    public static string EQUIP_MENU_STATE = "equip";
    public static string MAGIC_MENU_STATE = "magic";
    public static string OPTION_MENU_STATE = "option";
    public static string TITLE_MENU_STATE = "title";
    public static string GAME_OVER_STATE = "gameover";

    // Storyboard states
    public static string SCREEN_STATE = "screenstate";
}