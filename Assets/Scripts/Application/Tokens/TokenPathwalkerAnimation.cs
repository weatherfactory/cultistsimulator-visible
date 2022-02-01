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
            path.BlockUntilCalculated();
        }
        public void OnPathComplete(Path p)
        {
        //do nothing right now. THis takes the place of BlockUntilCalculated, right?
        }

        public void Update()
        {
            if (path == null)
            {
                // We have no path to follow yet, so don't do anything
                return;
            }
            reachedEndOfPath = false;
            // The distance to the next waypoint in the path
            float distanceToWaypoint;
            while (true)
            {
                // If you want maximum performance you can check the squared distance instead to get rid of a
                // square root calculation. But that is outside the scope of this tutorial.
                distanceToWaypoint = Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]);
                if (distanceToWaypoint < nextWaypointDistance)
                {
                    // Check if there is another waypoint or if we have reached the end of the path
                    if (currentWaypoint + 1 < path.vectorPath.Count)
                    {
                        currentWaypoint++;
                    }
                    else
                    {
                        // Set a status variable to indicate that the agent has reached the end of the path.
                        // You can use this to trigger some special code if your game requires that.
                        reachedEndOfPath = true;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            // Slow down smoothly upon approaching the end of the path
            // This value will smoothly go from 1 to 0 as the agent approaches the last waypoint in the path.
            var easingFactor = reachedEndOfPath ? Mathf.Sqrt(distanceToWaypoint / nextWaypointDistance) : 1f;

            // Direction to the next waypoint
            // Normalize it so that it has a length of 1 world unit
            Vector3 dir = (path.vectorPath[currentWaypoint] - transform.position).normalized;
            // Multiply the direction by our desired speed to get a velocity
            Vector3 velocity = dir * _speed * easingFactor;

            // If you are writing a 2D game you may want to remove the CharacterController and instead use e.g transform.Translate
            transform.position += velocity * Time.deltaTime;
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
