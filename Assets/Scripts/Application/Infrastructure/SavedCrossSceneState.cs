using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SecretHistories.Entities;

namespace SecretHistories.Infrastructure
{
    /// <summary>
    /// POCO for passing saved extragame state around
    /// </summary>
    public class SavedCrossSceneState
    {
        public Ending CurrentEnding;
        public List<Legacy> AvailableLegacies = new List<Legacy>();
        public Character DefunctCharacter;

    }
}
