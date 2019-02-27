using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TetrisSupport
{
    public class MirrorSeven : Shape
    {
        public MirrorSeven()
            : base(0, 1, 1, 1, 0, 0, 0, -1, ProductType.TurnByLastOne)
        {

        }
    }
}
