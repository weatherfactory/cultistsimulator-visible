using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Assets.Scripts.Application.Spheres.Angels;
using SecretHistories.Assets.Scripts.Application.Tokens.TravelItineraries;
using SecretHistories.Commands;
using SecretHistories.Enums;
using SecretHistories.Fucine;
using SecretHistories.Spheres;
using SecretHistories.Spheres.Angels;
using SecretHistories.UI;
using TMPro;
using UnityEngine;

namespace SecretHistories.Spheres
{
    [IsEmulousEncaustable(typeof(Sphere))]

    public class RoomSphere: Sphere
    {
        private bool _traversable;
        public override bool Traversable => _traversable;
        public override bool UnderstateContents => true;

        [SerializeField]
        private TextMeshProUGUI _label;
 
        public void Start()
        {
            RoomChoreographer rc = gameObject.GetComponent<RoomChoreographer>();
            
            if (rc!=null && rc.GetWalkableFloors().Any())
                _traversable = true;
            if(_label!=null)
                _label.text = gameObject.name;

        }

        public override SphereCategory SphereCategory => SphereCategory.World;
        public override float TokenHeartbeatIntervalMultiplier => 1;
        public override bool AllowDrag => true;

        public override void Emphasise()
        {
            if(_label!=null)
                _label.alpha = 1f;
        }

        public override void Understate()
        {
            if (_label != null)
                _label.alpha = 0f;
        }
        public override bool TryAcceptToken(Token token, Context context)
        {
            if(IsTokenInRangeOfThisRoom(token))
                return base.TryAcceptToken(token, context);
            return false;
        }
        public override void AcceptToken(Token token, Context context)
        {
            base.AcceptToken(token, context);

   
            token.HideGhost();

        }

        public override bool TryDisplayDropInteractionHere(Token forToken)
        {
            if (IsTokenInRangeOfThisRoom(forToken))
                //if so, display ghost.
                return forToken.DisplayGhostAtChoreographerDrivenPosition(this);
            else
                return false;

        }
        public override bool DisplayGhostAt(Token forToken, Vector3 overridingWorldPosition)
        {
            return forToken.DisplayGhostAtChoreographerDrivenPosition(this, overridingWorldPosition);
        }

        private bool IsTokenInRangeOfThisRoom(Token token)
        {
            if (!token.CurrentState.ShouldObserveRangeLimits())
                return true;
            //Get the home sphere location of this token.
            //Is it the same as this sphere?
            if (token.GetHomeSphere()==this)
                //if so, display ghost.
                return true;
            //Is its path this sphere + lower down?
            var homeSpherePath = token.GetHomeSphere().GetAbsolutePath();
            if (this.GetAbsolutePath().Contains(homeSpherePath))
                return true;

            //If neither of these, return false.
            return false;
        }
    }
}
