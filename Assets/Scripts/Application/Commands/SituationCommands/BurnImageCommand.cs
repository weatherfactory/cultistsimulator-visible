using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Assets.Scripts.Application.Commands.TokenEffectCommands;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.States;
using SecretHistories.UI;

namespace SecretHistories.Commands
{
    public class BurnImageCommand:ISituationCommand
    {
        private string _image;

        public bool IsValidForState(StateEnum forState)
        {
            return true;
        }

        public bool IsObsoleteInState(StateEnum forState)
        {
            return false;
        }

        public BurnImageCommand(string image)
        {
            _image = image;
        }


        public bool Execute(Situation situation)
        {
            var burnImageUnderTokenCommand=new BurnImageUnderTokenCommand(_image);
            situation.Token.ExecuteTokenEffectCommand(burnImageUnderTokenCommand);
            return true;
        }


    }



    
}
