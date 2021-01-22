using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecretHistories.Enums;
using SecretHistories.Spheres;
using SecretHistories.UI;

namespace SecretHistories.Abstract
{
    public interface IElementStackHost
    {
        public void Populate(ElementStack stack);
        bool Defunct { get; }
        Sphere Sphere { get; }
        void onElementStackQuantityChanged(ElementStack elementStack, Context context);
        void Remanifest(RetirementVFX cardTransformWhite);
        bool Retire(RetirementVFX none);
    }
}
