using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Entities;
using SecretHistories.Enums;
using SecretHistories.Spheres;
using SecretHistories.UI;

namespace SecretHistories.Assets.Scripts.Application.Spheres
{
    [IsEmulousEncaustable(typeof(Sphere))]
    public class SkySphere: Sphere
    {
        public override SphereCategory SphereCategory => SphereCategory.World;


        public virtual void Start()
        {
            //_background.onClicked += HandleOnTableClicked;

            //var roomSpheres = GetComponentsInChildren<RoomSphere>();
            //foreach (var r in roomSpheres)
            //{
            //    var roomSphereSpec = r.gameObject.GetComponent<PermanentSphereSpec>();
            //    roomSphereSpec.ApplySpecToSphere(r);
            //    r.SetContainer(this);
            //  Watchman.Get<HornedAxe>().RegisterSphere(r);
            //}
        }
    }
}
