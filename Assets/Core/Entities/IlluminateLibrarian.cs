using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core.Entities
{
    public class IlluminateLibrarian
    {
        private Dictionary<string, string> _currentIlluminations;


        public IlluminateLibrarian(Dictionary<string, string> currentIlluminations)
        {
        _currentIlluminations = currentIlluminations;
        }


        public IlluminateLibrarian() : this(new Dictionary<string, string>())
        {
        }

    public Dictionary<string, string> GetCurrentIlluminations()
        {
            return new Dictionary<string, string>(_currentIlluminations);
        }

}
}
