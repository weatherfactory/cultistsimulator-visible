using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Core.Entities
{
    /// <summary>
    /// Illuminations are text annotations added to stacks to allow for unique alerts and messages.
    /// </summary>
    public class IlluminateLibrarian
    {
        private Dictionary<string, string> _currentIlluminations;
        private const string KEY_MANSUSJOURNAL = "mansusjournal";
        private const string KEY_CREDITSTEXT = "creditstext";



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

        public void AddCreditsText(string value)
        {
            _currentIlluminations.Add(KEY_CREDITSTEXT, value);
        }

        public string GetCreditsText()
        {
            if(_currentIlluminations.ContainsKey(KEY_CREDITSTEXT))
                return _currentIlluminations[KEY_CREDITSTEXT];
            return "¯\\_(ツ)_/¯";
        }

        public void AddMansusJournalEntry(string value)
        {
            _currentIlluminations.Add(KEY_MANSUSJOURNAL, value);
        }

        public string PopMansusJournalEntry()
        {
            if (_currentIlluminations.ContainsKey(KEY_MANSUSJOURNAL))
            {
                string je = _currentIlluminations[KEY_MANSUSJOURNAL];
                _currentIlluminations.Remove(KEY_MANSUSJOURNAL);
                return je;
            }

            return string.Empty;
        }
    }
}
