using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Interfaces;


namespace SecretHistories.Fucine
{
    public class ContentImportLog
    {
        private List<ILogMessage> _contentImportMessages= new List<ILogMessage>();

        public bool ImportFailed()
        {
            return _contentImportMessages.Exists(m => m.MessageLevel > 1);
        }

        public void LogProblem(string problemDesc)
        {
            _contentImportMessages.Add(new NoonLogMessage(problemDesc));
        }

        public void LogWarning(string desc)
        {
            _contentImportMessages.Add(new NoonLogMessage(desc, 1));
        }

        public void LogInfo(string desc)
        {
            _contentImportMessages.Add(new NoonLogMessage(desc,0));
        }

        public IList<ILogMessage> GetMessages()
        {
            return new List<ILogMessage>(_contentImportMessages);

        }
    }
}
