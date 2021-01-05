﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Interfaces;
using SecretHistories.UI;
using SecretHistories.Infrastructure;


namespace SecretHistories.States
{
    public abstract class TokenState
    {
        public abstract bool Docked(Token token);
        public abstract bool InPlayerDrivenMotion(Token token);
        public abstract bool InSystemDrivenMotion(Token token);
        public abstract bool CanDecay(Token token);



    }
}