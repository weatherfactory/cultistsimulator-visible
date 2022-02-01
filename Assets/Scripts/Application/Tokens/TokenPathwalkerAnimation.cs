using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pathfinding;
using SecretHistories.UI;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Tokens
{
    public class TokenPathwalkerAnimation: MonoBehaviour
    {
        [SerializeField]
        private Vector3 _startPosition;
        [SerializeField]
        public Vector3 EndPosition;
        

        public Path path;

        private float _speed = 2;

        public float nextWaypointDistance = 3;

        private int currentWaypoint = 0;

        public bool reachedEndOfPath;
        private Token _token;
        private Context _context;
        public bool Defunct { get; set; }


        public event Action<Token, Context> OnTokenArrival;

        public virtual void Begin(Token token, Context context,float speed)
        {
            _token = token;
            _context = context;
            _speed = speed;

            transform.SetAsLastSibling();

            var seeker = token.gameObject.GetComponent<Seeker>();
            _startPosition = token.transform.position;


            path = seeker.StartPath(_startPosition, EndPosition,OnPathComplete);
         //   path.BlockUntilCalculated();
        }
        public void OnPathComplete(Path p)
        {
        //do nothing right now. THis takes the place of BlockUntilCalculated, right?
        }


        public void Retire()
        {

            Defunct = true;
            Destroy(this);
        }

        
        public void OnDestroy()
        {

        }
    }
}
