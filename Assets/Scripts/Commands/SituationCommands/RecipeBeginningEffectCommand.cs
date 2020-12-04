using System.Collections.Generic;

namespace Assets.Core
{
    public interface IRecipeSlotHolder
    {
        void ClearRecipeThresholds();
        void AddRecipeThreshold(SlotSpecification spec);
    }

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

        public void PopulateRecipeSlots(IRecipeSlotHolder slotHolder)
        {
            if(OngoingSlots.Count>0) //only execute if there are any relevant slot instructions. We don't want to clear existing slots with a recipe that doesn't specify them
            {
                slotHolder.ClearRecipeThresholds();
                foreach (var spec in OngoingSlots)
                    slotHolder. AddRecipeThreshold(spec);

                OngoingSlots.Clear();
            }
        }
    }
}