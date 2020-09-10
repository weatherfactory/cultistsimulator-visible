using System.Text.RegularExpressions;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Services;
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

        public override void OnCultureChanged(CultureChangedArgs args)
        {
            base.OnCultureChanged(args);
            UpdateTextFromTemplate();
        }

        private void UpdateTextFromTemplate()
        {
            if (template != null && tmpText != null)
            {
                tmpText.text = ParameterPattern.Replace(template, match => Registry.Get<ILocStringProvider>().Get(match.Groups[1].Value));
            }
        }
    }
}
