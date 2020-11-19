using System.Collections.Generic;

namespace Assets.Core
{
    public class RecipeBeginningEffectCommand
    {
        public List<SlotSpecification> OngoingSlots=new List<SlotSpecification>();
        public string BurnImage;

        public RecipeBeginningEffectCommand(List<SlotSpecification> ongoingSlots,string burnImage)
        {
            OngoingSlots = ongoingSlots;
            BurnImage = burnImage;
        }

        public RecipeBeginningEffectCommand()
        {
            OngoingSlots = new List<SlotSpecification>();
            BurnImage = null;
        }
    }
}