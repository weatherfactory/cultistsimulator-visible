using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Fucine;
using UnityEngine;

namespace SecretHistories.Assets.Scripts.Application.Services
{
   public class UnityLogWrapper: ILogSubscriber
   {
       public VerbosityLevel Sensitivity { get; protected set; }
        public UnityLogWrapper()
        {
            if (UnityEngine.Application.isEditor)
                Sensitivity = VerbosityLevel.SystemChatter;
            else
                Sensitivity = VerbosityLevel.Significants;

        }
        public void AddMessage(ILogMessage message)
        {
            string formattedMessage =
                (message.VerbosityNeeded > 0 ? new String('>', message.VerbosityNeeded) + " " : "") + message.Description;
            switch (message.MessageLevel)
            {
                case 0:
                    Debug.Log(formattedMessage);
                    break;
                case 1:
                    Debug.LogWarning(formattedMessage);
                    break;
                case 2:
                    Debug.LogError(formattedMessage);
                    break;
                default:
                    Debug.LogError(formattedMessage);
                    break;

            }
        }
    }
}
