using Assets.Core;
using UnityEngine;

namespace Assets.CS.TabletopUI
{
    public class Registry : MonoBehaviour {

        public static Compendium Compendium {
            get {
                return m_compendium;
            }
        }

        public static IDice Dice
        {
            get { return new Dice();}
        }

        private static Compendium m_compendium;

        public void ImportContentToCompendium() {
            m_compendium = new Compendium();
            ContentImporter ContentImporter = new ContentImporter();
            ContentImporter.PopulateCompendium(m_compendium);
        }

    }
}
