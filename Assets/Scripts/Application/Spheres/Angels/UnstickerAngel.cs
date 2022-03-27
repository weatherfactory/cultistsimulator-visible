using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Spheres;
using SecretHistories.Spheres.Angels;
using SecretHistories.UI;

namespace SecretHistories.Assets.Scripts.Application.Spheres.Angels
{

    //Checks sphere for cards that are not being dragged and don't have a current itinerary.
    //Makes them interactable, too.
    //This is a belt-and-braces attempt to fix an intermittent bug. I'm also losing MakeNonInteractable() in the StartDrag event
    public class UnstickerAngel: IAngel
    {
        private const int BEATS_BETWEEN_ANGELRY = 10;
        private int _beatsTowardsAngelry = 0;
        private Sphere _sphereToUnstickTokensFrom;

        public int Authority => 0;
        public void Act(float seconds, float metaseconds)
        {
            _beatsTowardsAngelry++;

            if (_beatsTowardsAngelry >= BEATS_BETWEEN_ANGELRY)
            {
                _beatsTowardsAngelry = 0;
                foreach (var t in _sphereToUnstickTokensFrom.Tokens)
                {
                    if (t.CurrentlyBeingDragged())
                        return;
                    if (t.CurrentItinerary != null && t.CurrentItinerary.IsActive())
                        return;
                    if(t.CurrentState.Docked())
                        unstickToken(t);
                }

            }


        }

        private void unstickToken(Token tokenToUnstick)
        {
            string unstickingInfo =
                new string(
                    $"Unsticking token with payload ID {tokenToUnstick.PayloadEntityId} and TokenState {tokenToUnstick.CurrentState.GetType().Name}");
            NoonUtility.Log(unstickingInfo,0,VerbosityLevel.Essential);
            tokenToUnstick.MakeInteractable();
            var context = new Context(Context.ActionSource.Unknown);
            context.Description = "UnstickAngel acting";
           tokenToUnstick.GoAway(context);
        }

        public void SetWatch(Sphere sphere)
        {
            _sphereToUnstickTokensFrom = sphere;
        }

        public bool MinisterToDepartingToken(Token token, Context context)
        {
            return false;
        }

        public bool MinisterToEvictedToken(Token token, Context context)
        {
            return false;
        }

        public void Retire()
        {
            Defunct = true;

        }

        public bool Defunct { get; protected set; }

        public void ShowRelevantVisibleCharacteristic(List<VisibleCharacteristic> visibleCharacteristics)
        {
          //
        }

        public void HideRelevantVisibleCharacteristic(List<VisibleCharacteristic> visibleCharacteristics)
        {
            //
        }
    }
}
