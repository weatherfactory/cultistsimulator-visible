using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Abstract;
using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.Spheres;

namespace SecretHistories.Commands
{
    public class SphereCreationCommand: IEncaustment
    {
        public SphereSpec GoverningSphereSpec { get; set; }
        public FucinePath Path { get; set; }
        public List<TokenCreationCommand> Tokens { get; set; }

        public Sphere Execute(Context context)
        {
        //    public Sphere InstantiateSphere(SphereSpec spec, FucinePath parentPath)
        return null;
        }
    }
}
