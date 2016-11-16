using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.CS.TabletopUI.Interfaces;

namespace Assets.Core.Commands
{
   public class Notification: INotification
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
