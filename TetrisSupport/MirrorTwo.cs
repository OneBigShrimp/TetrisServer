using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TetrisSupport
{
    public class MirrorTwo : Shape
    {
        public MirrorTwo()
            : base(0, 0, 1, 0, -1, -1, 0, -1, ProductType.TurnThenClone)
        {

        }
    }
}
