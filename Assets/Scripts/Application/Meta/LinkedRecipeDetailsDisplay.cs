using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using TMPro;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Meta
{
   public class LinkedRecipeDetailsDisplay: MonoBehaviour
   {
       [SerializeField] private TextMeshProUGUI _summary;

       public void Populate(LinkedRecipeDetails details)
       {
           _summary.text = details.Id;
       }
   }
}
