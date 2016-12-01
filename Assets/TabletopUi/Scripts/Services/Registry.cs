using System.Collections.Generic;
using Assets.Core;
using Assets.TabletopUi.Scripts.Interfaces;
using UnityEngine;
using UnityEngine.Assertions;

namespace Assets.CS.TabletopUI
{
    public class Registry : MonoBehaviour
    {
        private static ICompendium m_compendium;
        private static TabletopManager m_tabletopmanager;
        private static IDice m_dice;
        private static IDraggableHolder m_draggableHolder;

        public static TabletopManager TabletopManager
        {
            get
            {
                Assert.IsNotNull(m_tabletopmanager, "tabletop manager never registered");
                return m_tabletopmanager;
            }
        }

        public static IDraggableHolder DraggableHolder
        {
            get
        {
                Assert.IsNotNull(m_draggableHolder, "tabletop manager never registered");
                return m_draggableHolder;
            }
        }

        public static ICompendium Compendium
        {
            get
            {
                Assert.IsNotNull(m_tabletopmanager, "compendium never registered");
                return m_compendium;
            }
        }

        public static IDice Dice
        {
            get { return m_dice; }
        }



        public void RegisterCompendium(Compendium c)
        {
            m_compendium = c;

        }

        public void RegisterTabletopManager(TabletopManager tm)
        {
            m_tabletopmanager = tm;
        }

        public void RegisterDice(IDice dice)
        {
            m_dice = dice;
        }

        public void RegisterDraggableHolder(IDraggableHolder dh)
        {
            m_draggableHolder = dh;
        }


}
}
