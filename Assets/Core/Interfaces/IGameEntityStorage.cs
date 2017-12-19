using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;

namespace Assets.Core.Interfaces
{
    public interface IGameEntityStorage
    {
        List<IDeckInstance> DeckInstances { get; set; }
        IDeckInstance GetDeckInstanceById(string id);
        string Name { get; set; }
        string Profession { get; set; }
        string PreviousCharacterName { get; set; }
        string ReplaceTextFor(string text);
    }
}
