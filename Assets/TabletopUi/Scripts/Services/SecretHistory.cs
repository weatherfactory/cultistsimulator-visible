using System.Collections.Generic;
using Noon;
using UnityEngine;

namespace Assets.TabletopUi.Scripts.Services
{
    //max lines
    //enforce max lines
    //autodisplay when warning
    //control to hide
    //we probably want multiple text lines
    //and there's likely an asset store thing
    public class SecretHistory:MonoBehaviour,ILogSubscriber
    {
        [SerializeField] private GameObject LogMessageEntryPrefab;
        [SerializeField] private Transform logMessageEntriesHere;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameObject canvas;

        //public bool IsVisible => canvasGroup.alpha > 0f;
        public bool IsVisible => canvas.activeInHierarchy;

        private List<SecretHistoryLogMessageEntry> entries=new List<SecretHistoryLogMessageEntry>();

        public void SetVisible(bool visible)
        {
            //if (visible)
            //    canvasGroup.alpha = 1f;
            //else
            //    canvasGroup.alpha = 0f;
            canvas.SetActive(visible);
        }

        
        public void AddMessage(NoonLogMessage message)
        {
            
            foreach(var e in entries)
                if (e.TryMatchMessage(message))
                    return;

            SecretHistoryLogMessageEntry entry = Instantiate(LogMessageEntryPrefab, logMessageEntriesHere).GetComponent<SecretHistoryLogMessageEntry>();
            entries.Add(entry);
            entry.DisplayMessage(message);
            //always display the log if an error has occurred
            if(message.MessageLevel==2)
                SetVisible(true);
        
        }


    }
}
