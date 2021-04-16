using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.Fucine;

namespace SecretHistories.Commands
{
   public class Notification: INotification
    {
        public Notification(string title, string description)
        {
            Title = title;
            Description = description;
        }

        public string Title { get; set; }
        public string Description { get; set; }
        public bool Additive => true;
    }
}
