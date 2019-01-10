using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeodeInfoMenu.Menus
{
    /// <summary>
    /// Represents a menu that has a scrollbar.
    /// </summary>
    interface IScrollableMenu
    {
        /// <summary>
        /// Set the current scrollbar index
        /// </summary>
        /// <param name="index">Which index to use</param>
        void SetCurrentIndex(int index);

        /// <summary>
        /// Get the current scrollbar index
        /// </summary>
        /// <returns>The index of the scrollbar</returns>
        int GetCurrentIndex();
    }
}
