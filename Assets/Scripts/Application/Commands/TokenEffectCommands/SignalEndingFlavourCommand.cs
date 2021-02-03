﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Entities;
using SecretHistories.UI;

namespace SecretHistories.Commands
{
    public class SignalEndingFlavourCommand: IAffectsTokenCommand
    {
        private EndingFlavour _endingFlavour;
        public SignalEndingFlavourCommand(EndingFlavour endingFlavour)
        {
            _endingFlavour = endingFlavour;
        }

        public bool ExecuteOn(Token token)
        {
            Watchman.Get<BackgroundMusic>().SignalEndingFlavour(_endingFlavour);
            return true;
        }

        public bool ExecuteOn(ITokenPayload payload)
        {
            return false;
        }
    }
}