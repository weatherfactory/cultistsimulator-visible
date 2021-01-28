using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Entities.Verbs;

namespace SecretHistories.Commands
{
    public class DropzoneCreationCommand: ITokenPayloadCreationCommand
    {
        public ITokenPayload Execute(Context context)
        {
            return new Dropzone();
        }
    }
}
