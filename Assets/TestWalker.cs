using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using UnityEngine.EventSystems;

public class TestWalker : MonoBehaviour,IPointerClickHandler
{
    [SerializeField]
    private GameObject Target;


    // Update is called once per frame

    public Path path;

    public float speed = 2;

    public float nextWaypointDistance = 3;

    private int currentWaypoint = 0;

    public bool reachedEndOfPath;

    public void OnPointerClick(PointerEventData eventData)
    {
        var seeker = GetComponent<Seeker>();

      path = seeker.StartPath(gameObject.transform.position,Target.transform.position);
      path.BlockUntilCalculated();
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
        var speedFactor = reachedEndOfPath ? Mathf.Sqrt(distanceToWaypoint / nextWaypointDistance) : 1f;

        // Direction to the next waypoint
        // Normalize it so that it has a length of 1 world unit
        Vector3 dir = (path.vectorPath[currentWaypoint] - transform.position).normalized;
        // Multiply the direction by our desired speed to get a velocity
        Vector3 velocity = dir * speed * speedFactor;

        // Move the agent using the CharacterController component
        // Note that SimpleMove takes a velocity in meters/second, so we should not multiply by Time.deltaTime
        

        // If you are writing a 2D game you may want to remove the CharacterController and instead use e.g transform.Translate
        transform.position += velocity * Time.deltaTime;
    }
}
