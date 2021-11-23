using System; 
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.UI;
using SecretHistories.Constants;
using UnityEngine;

namespace SecretHistories.Spheres.Angels
{
   public interface IAngel
   {
       /// <summary>
       /// highest authorityangels act first
       /// </summary>
       int Authority { get; }
       
       void Act(float seconds, float metaseconds);

       void SetWatch(Sphere sphere);

       bool MinisterToDepartingToken(Token token, Context context);
       
        bool MinisterToEvictedToken(Token token, Context context);

        void Retire();

        bool Defunct { get; }

        bool RequestingRetirement { get; }
        void ShowRelevantVisibleCharacteristic(List<VisibleCharacteristic> visibleCharacteristics);
        void HideRelevantVisibleCharacteristic(List<VisibleCharacteristic> visibleCharacteristics);
   }
}
