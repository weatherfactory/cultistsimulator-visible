using System.Collections;
using System.Collections.Generic;
using SecretHistories.Assets.Scripts.Application.Spheres;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Entities
{
    public class Way : MonoBehaviour
    {
        public List<RoomSphere> LinkedRoomSpheres = new List<RoomSphere>();
        // This may be a Fucine entity later. For now it's just a marker.
        public void Start()
        {
            //TODO: hide the Way on startup unless debugging.
        }

        /// <summary>
        /// This isn't called. I'm adding all the linked rooms in the editor. This will get old really fast at scale.
        /// After prototyping, consider
        /// - editor script to bake connections, or
        /// - marshalling object checks for Ways and Rooms in the whole scene and then finds overlaps in their rects
        /// </summary>
        /// <param name="roomSphere"></param>
        public void LinkRoomSphere(RoomSphere roomSphere)
        {
            LinkedRoomSpheres.Add(roomSphere);
        }
    }
}