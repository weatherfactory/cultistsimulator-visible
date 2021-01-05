using System;

namespace SecretHistories.Interfaces
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
