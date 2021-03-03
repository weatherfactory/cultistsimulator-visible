using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Elements;
using SecretHistories.UI;

namespace SecretHistories.Commands
{
    public class NullElementStackCreationCommand
    {
        public NullElementStack Execute(Context context)
        {
            return  NullElementStack.Create();
        }
    }
}
