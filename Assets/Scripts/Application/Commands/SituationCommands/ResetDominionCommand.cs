﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Spheres;
using SecretHistories.UI;

namespace SecretHistories.Assets.Scripts.Application.Commands.SituationCommands
{
    public class ResetDominionCommand: ISituationCommand
    {
        public DominionEnum Identifier { get; set; }

        public bool Execute(Situation situation)
        {
            var dominion = situation.Dominions.SingleOrDefault(d => d.Identifier == Identifier);
            if (dominion != null)
            {
                var spheresToRemove = new List<Sphere>(dominion.Spheres);
                foreach (var s in spheresToRemove)
                    dominion.RemoveSphere(s.Id);

                return true;
            }
            else
            {
                NoonUtility.LogWarning($"Tried to populate dominion {Identifier} in situation {situation.Id}, but can't find that dominion id");
                return false;
            }
        }

        public ResetDominionCommand(DominionEnum identifier)
        {
            Identifier = identifier;
        }

        public bool IsValidForState(StateEnum forState)
        {
            return true;
        }
    }
}
