using TMPro;
using UnityEngine;

namespace TabletopUi.Scripts.UI
{
    public class VersionNews : Babelfish
    {
        public TextMeshProUGUI text;

        private const string VersionNewsFile = "VersionNews";

        public override void OnLanguageChanged()
        {
            base.OnLanguageChanged();
            var textAsset = Resources.Load<TextAsset>(VersionNewsFile);

            if (textAsset == null)
                Debug.LogWarning("Could not load Resources/" + VersionNewsFile);
            else if (text == null)
                Debug.LogWarning("Loaded text asset but no text mesh to use specified.");
            else
                text.text += "\n\n" + textAsset.text;
        }
    }
}
