using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.States;

namespace SecretHistories.Commands.SituationCommands
{
    public class BurnImageCommand:ISituationCommand
    {
        private string _image;

        public CommandCategory CommandCategory => CommandCategory.Anchor;


        public BurnImageCommand(string image)
        {
            _image = image;
        }



        public bool Execute(Situation situation)
        {
            situation.GetAnchor().BurnImageUnderToken(_image);
            return true;
  
        }
    }
}
