using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pong.Game
{
    interface IReactiveCollideable : ICollideable
    {
        void CollideWith(INonReactiveCollideable other);
    }
}
