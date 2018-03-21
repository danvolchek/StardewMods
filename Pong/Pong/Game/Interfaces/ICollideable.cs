using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pong.Game
{
    interface ICollideable : IUpdateable
    {
        Rectangle GetBoundingBox();
    }
}
