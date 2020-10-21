using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.Enums;
using Assets.Core.Interfaces;
using Assets.CS.TabletopUI;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Interfaces
{
    public interface ISituationAnchor: ISituationSubscriber, IAnimatableToken
    {
        SituationController SituationController { get; }

        string SaveLocationInfo { get; set; }
        AnchorDurability Durability { get; }


        void DisplayAsOpen();
        void DisplayAsClosed();

        void Populate(Situation situation);

        bool Retire();

        void DisplayOverrideIcon(string icon);

        void SetParticleSimulationSpace(Transform transform);

    }
}
