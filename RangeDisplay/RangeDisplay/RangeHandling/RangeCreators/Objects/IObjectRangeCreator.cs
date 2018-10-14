using SObject = StardewValley.Object;

namespace RangeDisplay.RangeHandling.RangeCreators.Objects
{
    internal interface IObjectRangeCreator : IRangeCreator
    {
        bool CanHandle(SObject sObject);
    }
}