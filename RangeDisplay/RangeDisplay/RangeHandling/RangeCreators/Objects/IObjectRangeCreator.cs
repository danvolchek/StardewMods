using SObject = StardewValley.Object;

namespace RangeDisplay.RangeHandling.RangeCreators
{
    internal interface IObjectRangeCreator : IRangeCreator
    {
        bool CanHandle(SObject sObject);
    }
}