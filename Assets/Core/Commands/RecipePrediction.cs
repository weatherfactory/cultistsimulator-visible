using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Core.Entities;
using Assets.Core.Services;

namespace Assets.Core.Commands
{
   public class RecipePrediction
    {
        public string Title { get; protected set; }
        public string DescriptiveText { get; protected set; }
        public string BurnImage { get; protected set; }
        public EndingFlavour SignalEndingFlavour { get; protected set; }

        public RecipePrediction(string title,string descriptiveText,string burnImage, EndingFlavour signalEndingFlavour,IAspectsDictionary aspectsAvailable)
        {
            Title = title;
            DescriptiveText = descriptiveText;
            BurnImage = burnImage;
            SignalEndingFlavour = signalEndingFlavour;
            TextRefiner tr = new TextRefiner(aspectsAvailable);
            DescriptiveText = tr.RefineString(DescriptiveText);

        }
    }
}
