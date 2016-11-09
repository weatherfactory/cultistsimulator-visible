using UnityEngine;

namespace Assets.CS.TabletopUI
{
    /// <summary>
    /// Populates Compendium, makes it available via a 
    /// </summary>
    public class CompendiumHolder : MonoBehaviour {

        public static Compendium compendium {
            get {
                return m_compendium;
            }
        }

        private static Compendium m_compendium;

        public void Init() {
            m_compendium = new Compendium(new Dice());
            ContentImporter ContentImporter = new ContentImporter();
            ContentImporter.PopulateCompendium(m_compendium);
        }

    }
}
