using System.Collections.Generic;
using Assets.CS.TabletopUI.Interfaces;
using UnityEngine;

namespace Assets.CS.TabletopUI
{
    
    public class Notifier : MonoBehaviour,IElementCardSubscriber {
        [Header("Prefabs")]
        [SerializeField]
        ElementDetailsWindow elementDetailWindowPrefab;
        [SerializeField]
        Transform windowHolderFixed;

        private int _maxNumElementWindows;
        private List<ElementDetailsWindow> elementWindows = new List<ElementDetailsWindow>();

        public Notifier(int maxNumElementWindows)
        {
            _maxNumElementWindows = maxNumElementWindows;
        }




        public void ShowElementDetails(ElementCard card)
        {

            if (_maxNumElementWindows > 0 && elementWindows.Count == _maxNumElementWindows)
                HideElementDetails(elementWindows[0].GetElementCard());

            //		PutTokenInAir(card.transform as RectTransform);

            var window = BuildElementDetailsWindow();
            //		window.transform.position = card.transform.position;
            window.SetElementCard(card);
            elementWindows.Add(window);
        }

       public void HideElementDetails(ElementCard card)
        {
            //if (Draggable.itemBeingDragged == null || Draggable.itemBeingDragged.gameObject != card.gameObject)
            //	PutTokenOnTable(card.transform as RectTransform); // remove card from details window before hiding it, so it isn't removed, if we're not already dragging it

            elementWindows.Remove(card.detailsWindow);
            card.detailsWindow.Hide();
        }

        public ElementDetailsWindow BuildElementDetailsWindow()
        {
            var window = Instantiate(elementDetailWindowPrefab) as ElementDetailsWindow;
            window.transform.SetParent(windowHolderFixed);
            window.transform.localPosition = Vector3.zero;
            window.transform.localScale = Vector3.one;
            window.transform.localRotation = Quaternion.identity;
            return window;
        }



        public void ElementPickedUp(ElementCard elementCard)
        {
            throw new System.NotImplementedException();
        }

        public void HideAllElementDetails()
        {
            for (int i = 0; i < elementWindows.Count; i++)
                HideElementDetails(elementWindows[i].GetElementCard());
        }
    }
}
