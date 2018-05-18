using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;

namespace Assets.Core.Commands
{
   public class RecipePrediction
    {
        public string Title { get; set; }
        public string DescriptiveText { get; set; }
        public string BurnImage { get; set; }
        public EndingFlavour SignalEndingFlavour { get; set; }
    }
}
