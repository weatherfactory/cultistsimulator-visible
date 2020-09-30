using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Entities;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.UI
{
    public class CreditsWindow: MonoBehaviour
    {
        [SerializeField] public ElementStackToken ElementStackPrefab;
        [SerializeField] public ExhibitCards CardsExhibit;


        public bool Initialised { get; }

        public void OnEnable()
        {
            Debug.Log("starting");
            if (!Initialised)
                Initialise();
        }

        private void Initialise()
        {
            CardsExhibit.ProvisionElementStack("reason", 1, Source.Fresh());



        }
    }
}
