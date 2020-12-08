using System; 
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.CS.TabletopUI;
using Assets.TabletopUi.Scripts.Infrastructure;

namespace Assets.Scripts.Spheres.Angels
{
   public interface IAngel
   {
       void Act(float interval);

       void SetMinisterTo(Sphere sphere);
       
       void SetWatch(Sphere sphere);
   }
}
