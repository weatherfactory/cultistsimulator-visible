using System;

namespace SecretHistories.Fucine
{

        public interface ILogMessage
        {
            string Description { get; set; }
            int MessageLevel { get; }
            int VerbosityNeeded { get; }
            Exception LoggedException { get; set; }
            string ToString();
        }
    
}
