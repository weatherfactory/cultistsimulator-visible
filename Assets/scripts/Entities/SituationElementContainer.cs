using System;

namespace Assets.scripts.Entities
{
    public class SituationElementContainer: BaseElementsContainer, IElementsContainer
    {
        public override void ModifyElementQuantity(string elementId, int quantityChange)
        {
            if (!_elements.ContainsKey(elementId))
            {
                _elements.Add(elementId, quantityChange);
            }
            else
            _elements[elementId] = _elements[elementId] + quantityChange;
            PublishElementQuantityUpdate(elementId, GetCurrentElementQuantity(elementId), 0);
        }

        public override void TriggerSpecialEvent(string endingId)
        {
            throw new NotImplementedException();
        }

    }
}

