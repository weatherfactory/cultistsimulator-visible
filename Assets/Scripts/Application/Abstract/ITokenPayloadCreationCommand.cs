using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretHistories.Abstract
{
    public interface ITokenPayloadCreationCommand
    {
        public ITokenPayload Execute(Context context);
    }
}
