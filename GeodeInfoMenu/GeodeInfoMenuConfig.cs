using StardewModdingAPI;

namespace GeodeInfoMenu
{
    /// <summary>Configuration for the mod.</summary>
    public class GeodeInfoMenuConfig
    {
        /// <summary>Key that opens up the menu.</summary>
        public SButton ActivationKey { get; set; } = SButton.K;

        /// <summary>Number of geodes to show in the info menu.</summary>
        public int NumberOfNextGeodeDropsToShow { get; set; } = 20;

        /// <summary>Whether to show stars next to undonated items.</summary>
        public bool ShowStarsNextToMineralsAndArtifactsNotDonatedToTheMuseum { get; set; } = true;

        /// <summary>Whether to remember the menu state between openings.</summary>
        public bool RememberMenuStateAfterClose { get; set; } = true;

        /// <summary>Whether to remember the scrollbar positions if remembering the menu state.</summary>
        public bool IfRememberingMenuStateAlsoRememberScrollBarPositions { get; set; } = true;

        /// <summary>Whether a right click needs to be on the search box in order to clear its text.</summary>
        public bool RightClickOnOnlySearchBoxToClearText { get; set; } = false;

        /// <summary>Whether escape instantly closes the menu rather than unselecting the search box.</summary>
        public bool PressingEscapeWhileTypingInSearchBoxInstantlyClosesMenu { get; set; } = true;
    }
}
