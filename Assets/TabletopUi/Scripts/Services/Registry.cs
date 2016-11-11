using UnityEngine;

namespace Assets.CS.TabletopUI
{
    public class Registry : MonoBehaviour {

        public static Compendium Compendium {
            get {
                return m_compendium;
            }
        }

        private static Compendium m_compendium;

        public void ImportContentToCompendium() {
            m_compendium = new Compendium(new Dice());
            ContentImporter ContentImporter = new ContentImporter();
            ContentImporter.PopulateCompendium(m_compendium);
        }

    }
}
