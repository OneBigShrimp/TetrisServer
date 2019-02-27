using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TetrisSupport
{
    public class Stone : Shape
    {
        public Stone()
        {
            relativeOccupy[0] = new FourPos(new Position(-1, 0), new Position(0, 0), new Position(-1, -1), new Position(0, -1));
            relativeOccupy[1] = relativeOccupy[0].Clone();
            relativeOccupy[2] = relativeOccupy[0].Clone();
            relativeOccupy[3] = relativeOccupy[0].Clone();
        }
    }
}
