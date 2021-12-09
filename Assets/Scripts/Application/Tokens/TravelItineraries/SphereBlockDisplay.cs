using System.Collections;
using SecretHistories.Spheres;
using TMPro;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Tokens.TravelItineraries
{
    public class SphereBlockDisplay : MonoBehaviour
    {

        [SerializeField]
        private TextMeshProUGUI Path;
        [SerializeField]
        private TextMeshProUGUI BlockDirection;
        [SerializeField]
        private TextMeshProUGUI BlockReason;



        public void DisplaySphereBlock(SphereBlock sb)
        {
            Path.text = sb.AtSpherePath.ToString();

            BlockDirection.text = sb.BlockDirection.ToString();
            BlockReason.text = sb.BlockReason.ToString();
            gameObject.name = $"{sb.AtSpherePath}_{sb.BlockDirection}_{sb.BlockReason}";

        }
    }
}