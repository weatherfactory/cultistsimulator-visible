using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Interfaces;
using SecretHistories.States;
using SecretHistories.UI;

namespace SecretHistories.Commands
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
            var burnImageUnderTokenCommand=new BurnImageUnderTokenCommand(_image);
            situation.SendCommandToSubscribers(burnImageUnderTokenCommand);
            return true;
        }
    }


    public class BurnImageUnderTokenCommand : IAffectsTokenCommand
    {
        private string _image;

        public CommandCategory CommandCategory => CommandCategory.Anchor;


        public BurnImageUnderTokenCommand(string image)
        {
            _image = image;
        }

        
        public bool ExecuteOn(Token token)
        {

            Watchman.Get<TabletopImageBurner>().ShowImageBurn(_image, token.Location.Anchored3DPosition, 20f, 2f,
                    TabletopImageBurner.ImageLayoutConfig.CenterOnToken);
            return true;
        }

        public bool ExecuteOn(ITokenPayload payload)
        {
            return false;
        }
    }
}
