using System.Text.RegularExpressions;
using SecretHistories.UI;
using SecretHistories.Services;
using UnityEngine;

namespace SecretHistories.Enums.UI
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
            if (template != null && GetTextComponent() != null)
            {
                GetTextComponent().text = ParameterPattern.Replace(template, match => Registry.Get<ILocStringProvider>().Get(match.Groups[1].Value));
            }
        }
    }
}
