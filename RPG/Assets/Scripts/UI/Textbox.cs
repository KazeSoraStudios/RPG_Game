using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RPG_Character;

namespace RPG_UI
{
    public class Textbox : ConfigMonoBehaviour, IGameState
    {
        public class Config
        {
            public string ImagePath;
            public string Text;
            public float AdvanceTime = 2.0f;
            public Action OnFinish;
            public Action OnSelect;
        }

        [SerializeField] Image Image;
        [SerializeField] Image ContinueCaret;
        [SerializeField] TextMeshProUGUI Text;
        [SerializeField] MenuOptionsList OptionsList;

        private bool isDead = true;
        private bool turnOff;
        private int currentPage = 0;
        private int characterIndex = 0;
        private int pageStart = 0;
        private int pageEnd = 0;
        private float nextCharTime;
        private float totalTime;
        private float advanceTime = 0.0f;
        private string text;
        private TMP_TextInfo textInfo;
        private Action onFinish;
        private Action onSelect;
        private StateStack stack;

        public void SetUp(StateStack stack)
        {
            this.stack = stack;
        }

        public void Init(Config config)
        {
            if (config == null)
                return;
            gameObject.SafeSetActive(true);
            advanceTime = config.AdvanceTime;
            text = config.Text;
            Text.maxVisibleCharacters = 0;
            Text.SetText(config.Text);
            Text.ForceMeshUpdate();
            textInfo = Text.textInfo;
            onFinish = config.OnFinish;
            onSelect = config.OnSelect;
            Image.gameObject.SafeSetActive(false);
            isDead = false;
            if (config.ImagePath != null)
                Image.sprite = ServiceManager.Get<AssetManager>().Load<Sprite>(config.ImagePath, () => Image.gameObject.SafeSetActive(true));
        }

        public void Enter(object o)
        {
            ServiceManager.Get<Party>().PrepareForTextboxState();
            ServiceManager.Get<NPCManager>().PrepareForTextboxState();
            nextCharTime = 0.0f;
            totalTime = 0.0f;
            turnOff = false;
            currentPage = 0;
            Text.pageToDisplay = 0;
            currentPage = 0;
            characterIndex = 0;
            pageEnd = textInfo.pageInfo[currentPage].lastCharacterIndex + 1;
        }

        public bool Execute(float deltaTime)
        {
            totalTime += deltaTime;
            nextCharTime += deltaTime;
            if (nextCharTime >= Constants.TEXTBOX_CHARACTER_SPEED)
            {
                nextCharTime = 0.0f;
                characterIndex++;
                if (characterIndex >= pageEnd)
                    characterIndex = pageEnd;
                Text.maxVisibleCharacters = characterIndex;
            }
            if (totalTime >= advanceTime)
                AdvanceOrTurnOff();
            if (turnOff)
                stack.Pop();
            return false;
        }

        public void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Space))
                OnClick();
            else
            { }// SelectionMenu.HandleInput();
        }

        public void Exit()
        {
            onFinish?.Invoke();
            onSelect = null;
            onFinish = null;
            Text.SetText(string.Empty);
            gameObject.SafeSetActive(false);
            ServiceManager.Get<Party>().ReturnFromTextboxState();
            ServiceManager.Get<NPCManager>().ReturnFromTextboxState();
            isDead = true;
        }

        public void OnClick()
        {
            if (characterIndex < pageEnd)
            {
                characterIndex = pageEnd;
                Text.maxVisibleCharacters = characterIndex;
                return;
            }
            AdvanceOrTurnOff();
        }

        public bool IsDead()
        {
            return isDead;
        }

        public bool IsTurningOff()
        {
            return turnOff;
        }

        private void AdvanceOrTurnOff()
        {
            if (currentPage < textInfo.pageInfo.Length && textInfo.pageInfo[currentPage + 1].lastCharacterIndex != 0)
            {
                totalTime = 0.0f;
                characterIndex = 0;
                currentPage++;
                pageStart = textInfo.pageInfo[currentPage].firstCharacterIndex;
                pageEnd = textInfo.pageInfo[currentPage].lastCharacterIndex;
                Text.pageToDisplay = currentPage + 1;
            }
            else
            {
                turnOff = true;
            }
        }

        public string GetName()
        {
            return "Textbox";
        }
    }
}

    /**




function Textbox:Render(renderer)

    local font = gGame.Font.default

    local scale = self.mAppearTween:Value()

    font:AlignText("left", "top")
    self.mPanel:CenterPosition(
        self.mX,
        self.mY,
        self.mWidth * scale,
        self.mHeight * scale)

    self.mPanel:Render(renderer)

    local left = self.mX - (self.mWidth/2 * scale)
    local textLeft = left + (self.mBounds.left * scale)
    local top = self.mY + (self.mHeight/2 * scale)
    local textTop = top + (self.mBounds.top * scale)
    local bottom = self.mY - (self.mHeight/2 * scale)

    if self.mAppearTween:Value() ~= 1 then
        return
    end

    font:DrawText2d(
        renderer,
        textLeft,
        textTop,
        self.mChunks[self.mChunkIndex],
        Vector.Create(1,1,1,1),
        self.mWrap * scale)

    if self.mSelectionMenu then
        font:AlignText("left", "center")
        local menuX = textLeft
        local menuY = bottom + self.mSelectionMenu:GetHeight()
        menuY = menuY + self.mBounds.bottom
        self.mSelectionMenu.mX = menuX
        self.mSelectionMenu.mY = menuY
        self.mSelectionMenu.mScale = scale
        self.mSelectionMenu:Render(renderer)
    end

    if self.mChunkIndex < #self.mChunks then
        -- There are more chunks to come.
        local offset = 12 + math.floor(math.sin(self.mTime*10)) * scale
        self.mContinueMark:SetScale(scale, scale)
        self.mContinueMark:SetPosition(self.mX, bottom + offset)
        renderer:DrawSprite(self.mContinueMark)
    end

    for k, v in ipairs(self.mChildren) do
        if v.type == "text" then
            font:DrawText2d(
                renderer,
                textLeft + (v.x * scale),
                textTop + (v.y * scale),
                v.text,
                Vector.Create(1,1,1,1)
            )
        elseif v.type == "sprite" then
            v.sprite:SetPosition(
                left + (v.x * scale),
                top + (v.y * scale))
            v.sprite:SetScale(scale, scale)
            renderer:DrawSprite(v.sprite)
        end
    end
end
    */