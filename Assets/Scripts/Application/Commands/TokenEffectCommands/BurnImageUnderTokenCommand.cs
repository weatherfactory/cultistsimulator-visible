using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Commands;
using SecretHistories.UI;

namespace SecretHistories.Assets.Scripts.Application.Commands.TokenEffectCommands
{
    public class BurnImageUnderTokenCommand : IAffectsTokenCommand
    {
        private string _image;


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
