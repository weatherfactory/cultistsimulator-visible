using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Entities;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SecretHistories
{
    public class ReqFrame: MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        
        [SerializeField] private Image icon;
        [SerializeField] private Image highlight;
        [SerializeField] private TextMeshProUGUI criterionText;

        
        public void Populate(Element element, string criterion,bool requirementIsMatched)
        {
            
            criterionText.text = criterion;
            DisplayIcon(element);
            gameObject.name = $"Req_{element.Id}_{criterion}";

            highlight.gameObject.SetActive(requirementIsMatched);
        }

        private void DisplayIcon(Element element)
        {
            Sprite aspectSprite;
            if (element.IsAspect) //it may be a concrete element rather than just an aspect
                aspectSprite = ResourcesManager.GetSpriteForAspect(element.Icon);
            else
                aspectSprite = ResourcesManager.GetSpriteForElement(element.Icon);

            icon.sprite = aspectSprite;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            throw new NotImplementedException();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
           //
        }

        public void OnPointerExit(PointerEventData eventData)
        {
           //
        }
    }
}
