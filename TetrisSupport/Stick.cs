using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TetrisSupport
{
    public class Stick : Shape
    {
        public Stick()
            : base(-2, 0, -1, 0, 0, 0, 1, 0, ProductType.TurnThenClone)
        {

        }
    }
}
