using System; 
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.UI;
using SecretHistories.Infrastructure;

namespace SecretHistories.Spheres.Angels
{
   public interface IAngel
   {
       void Act(float interval);

       void SetMinisterTo(Sphere sphere);
       
       void SetWatch(Sphere sphere);
   }
}
