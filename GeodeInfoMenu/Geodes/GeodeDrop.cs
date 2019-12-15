using System.Collections.Generic;

namespace GeodeInfoMenu
{
    /// <summary>Represents an item that can drop from a geode.</summary>
    public class GeodeDrop
    {
        /***
         * Public Properties
         ***/

        /// <summary>The underlying hardcoded item, if this is a hard coded item.</summary>
        public HardCodedGeodeDrop HardCodedDrop { get; }

        /// <summary>Whether or not this is a hardcoded drop.</summary>
        public bool IsHardCodedDrop { get; }

        /// <summary>The underlying item id, if this is not a hard coded item.</summary>
        public int ParentSheetIndex { get; }

        /***
         * Private Fields
         ***/

        /// <summary>A mapping of hard coded drops to their item ids.</summary>
        private static readonly IDictionary<HardCodedGeodeDrop, int> SItems = new Dictionary<HardCodedGeodeDrop, int> {
                { HardCodedGeodeDrop.Stone, 390}, { HardCodedGeodeDrop.Clay, 330}, { HardCodedGeodeDrop.EarthCrystal, 86}, { HardCodedGeodeDrop.FrozenTear, 84}, { HardCodedGeodeDrop.FireQuartz, 82}, { HardCodedGeodeDrop.Coal, 382}, { HardCodedGeodeDrop.CopperOre, 378},
                { HardCodedGeodeDrop.IronOre, 380}, { HardCodedGeodeDrop.GoldOre,384}, { HardCodedGeodeDrop.IridiumOre,386}, { HardCodedGeodeDrop.PrismaticShard, 74}
            };

        /***
         * Public Methods
         ***/

        /// <summary>Constructs an instance to be a hard coded drop.</summary>
        /// <param name="item">The hardcoded drop.</param>
        public GeodeDrop(HardCodedGeodeDrop item)
        {
            this.HardCodedDrop = item;
            this.ParentSheetIndex = SItems[item];
            this.IsHardCodedDrop = true;
        }

        /// <summary>Constructs an instance to not be a hard coded drop.</summary>
        /// <param name="parentSheetIndex">The item id.</param>
        public GeodeDrop(int parentSheetIndex)
        {
            this.ParentSheetIndex = parentSheetIndex;
            this.IsHardCodedDrop = false;
        }
    }
}
