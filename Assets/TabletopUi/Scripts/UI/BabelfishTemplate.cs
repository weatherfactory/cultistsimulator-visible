using System.Text.RegularExpressions;
using UnityEngine;

namespace TabletopUi.Scripts.UI
{
    public class BabelfishTemplate : Babelfish
    {
        [SerializeField] private string template;

        private static readonly Regex ParameterPattern = new Regex(@"\{(\w+)\}");

        public void SetTemplate(string newTemplate)
        {
            template = newTemplate;
            UpdateTextFromTemplate();
        }

        public override void OnLanguageChanged()
        {
            base.OnLanguageChanged();
            UpdateTextFromTemplate();
        }

        private void UpdateTextFromTemplate()
        {
            if (template != null && tmpText != null)
            {
                tmpText.text = ParameterPattern.Replace(template, match => LanguageManager.Get(match.Groups[1].Value));
            }
        }
    }
}
