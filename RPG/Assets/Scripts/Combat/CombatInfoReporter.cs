using RPG_UI;

namespace RPG_Combat
{
    public interface ICombatReporter
    {
        void ReportResult(FormulaResult result, string name);
        void DisplayInfo(string info);
    }

    public class CombatInfoReporter : ICombatReporter
    {
        private CombatUI UI;

        public CombatInfoReporter(CombatUI ui)
        {
            this.UI = ui;
        }

        public void ReportResult(FormulaResult result, string name)
        {
            var message = CreateResultMessage(result, name);
            UI.ShowNotice(message);
        }

        public void DisplayInfo(string info)
        {
            UI.ShowNotice(info);
        }

        private string CreateResultMessage(FormulaResult result, string name)
        {
            var localization = ServiceManager.Get<LocalizationManager>();
            if (result.Result == CombatFormula.HitResult.Miss)
            {
                var message = localization.Localize("ID_MISS_TEXT");
                return string.Format(message, name);
            }
            else if (result.Result == CombatFormula.HitResult.Dodge)
                return localization.Localize("ID_DODGE_TEXT");
            else if (result.Result == CombatFormula.HitResult.Hit)
            { 
                var message = localization.Localize("ID_ATTACK_HIT_TEXT");
                return string.Format(message, result.Damage);
            }
            else
            {
                var message = localization.Localize("ID_CRITICAL_HIT_TEXT");
                return string.Format(message, result.Damage);
            }
        }
    }
}
