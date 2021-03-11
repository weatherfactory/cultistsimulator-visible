using System; 
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.UI;
using SecretHistories.Constants;

namespace SecretHistories.Spheres.Angels
{
   public interface IAngel
   {
       /// <summary>
       /// highest authorityangels act first
       /// </summary>
       int Authority { get; }
       
       void Act(float interval);

       void SetWatch(Sphere sphere);

       bool MinisterToDepartingToken(Token token, Context context);

        bool MinisterToEvictedToken(Token token, Context context);

    }
}
