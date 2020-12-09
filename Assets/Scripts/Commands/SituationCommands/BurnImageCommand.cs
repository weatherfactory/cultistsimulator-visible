using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Core.Commands;
using Assets.Core.Entities;
using Assets.Core.States;

namespace Assets.Scripts.Commands.SituationCommands
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
