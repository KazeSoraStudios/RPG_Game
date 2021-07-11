using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RPG_UI
{
    public class UIController : ConfigMonoBehaviour
    {
        [SerializeField] public Transform MenuLayer;
        [SerializeField] public Transform PoolLayer;
        [SerializeField] public Transform BlockingLayer;
        [SerializeField] public Transform TextLayer;
        [SerializeField] public Transform CombatLayer;
        [SerializeField] public Image ScreenImage;
        private InGameMenu gameMenu;

        private List<ScrollViewCell> pool = new List<ScrollViewCell>();

        private void Awake()
        {
            ServiceManager.Register(this);
        }

        void OnDestroy()
        {
            ServiceManager.Unregister(this);
        }

        public void InitUI()
        {
            LoadGameMenu();
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

        public void OpenMenu(Map map, StateStack stack)
        {
            Show();
            gameMenu.Init(map, stack);
            stack.Push(gameMenu);
        }

        public Textbox GetTextbox(TextBoxAnchor anchor)
        {
            var asset = ServiceManager.Get<AssetManager>().Load<Textbox>(Constants.TEXTBOX_PREFAB);
            var textbox = Instantiate(asset);
            textbox.gameObject.SafeSetActive(false);
            textbox.transform.SetParent(TextLayer, false);
            textbox.SetTextBoxAnchor(anchor);
            var stack = ServiceManager.Get<GameLogic>().Stack;
            textbox.SetUp(stack);
            return textbox;
        }

        public void AddMenuScreen(Transform transform)
        {
            transform.SetParent(MenuLayer, false);
        }

        public void AddPoolItem(Transform transform)
        {
            transform.SetParent(PoolLayer, false);
        }

        public void AddTextScreen(Transform transform)
        {
            transform.SetParent(TextLayer, false);
        }

        public void AddBlockingScreen(Transform transform)
        {
            transform.SetParent(BlockingLayer, false);
        }

        /// <summary>
        /// 1 - menu, 2 - pool, 4 - text, 8 - blocking
        /// </summary>
        /// <param name="layers"></param>
        public void ClearChildren(short layers)
        {
            if ((layers & 0x1) == 0x1)
                for (int i = MenuLayer.childCount - 1; i > -1; i--)
                    Destroy(MenuLayer.GetChild(i).gameObject);
            if ((layers & 0x2) == 0x2)
                for (int i = PoolLayer.childCount - 1; i > -1; i--)
                    Destroy(PoolLayer.GetChild(i).gameObject);
            if ((layers & 0x4) == 0x4)
                for (int i = TextLayer.childCount - 1; i > -1; i--)
                    Destroy(TextLayer.GetChild(i).gameObject);
            if ((layers & 0x8) == 0x8)
                for (int i = BlockingLayer.childCount - 1; i > -1; i--)
                    Destroy(BlockingLayer.GetChild(i).gameObject);
        }

        public void Show()
        { 
            gameObject.SafeSetActive(true);
        }

        public void Hide()
        {
            gameObject.SafeSetActive(false);
        }

        private void LoadGameMenu()
        {
            var asset = ServiceManager.Get<AssetManager>().Load<InGameMenu>(Constants.IN_GAME_MENU_PREFAB);
            if (asset == null)
            {
                LogManager.LogError("Unable to load InGameMenu.");
                return;
            }
            gameMenu = Instantiate(asset, Vector3.zero, Quaternion.identity);
            gameMenu.transform.SetParent(MenuLayer);
            gameMenu.SetUpUI(this);
            gameObject.SafeSetActive(false);
        }
    }
}