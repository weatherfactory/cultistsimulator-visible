using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

    public interface ICharacterInfoSubscriber
    {
        void ReceiveUpdate(Character character);
    }

