using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Assets.Scripts.Application.Entities;

namespace SecretHistories.Assets.Scripts.Application
{

    public enum RouteDirection
    {
    Vertical,
    Horizontal
    }



    public class Route
    {
        public RouteDirection Direction;
        //Vertical: room 1 top, room 2 bottom
        //Horizontal: room 1 left, room 2 right
        //More exotic routes might have more than 2 rooms
        public List<Room> Rooms;

        public int DoorStrength; //placeholder for security info
    }
}
