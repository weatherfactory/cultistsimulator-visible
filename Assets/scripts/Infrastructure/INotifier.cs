using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


    public interface INotifier
    {
        void Log(string message, Style style);
        void Notify(string aside, string message, INotifyLocator notifyingGameObject);
    }

