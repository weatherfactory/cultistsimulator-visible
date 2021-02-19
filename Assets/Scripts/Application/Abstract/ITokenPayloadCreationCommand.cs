using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Commands;
using SecretHistories.Spheres;

namespace SecretHistories.Abstract
{
    public interface ITokenPayloadCreationCommand
    {
        public ITokenPayload Execute(Context context);
        public List<SphereCreationCommand> SphereCreationCommands { get; }
    }
}
