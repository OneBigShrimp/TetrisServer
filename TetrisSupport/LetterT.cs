using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TetrisSupport
{
    public class LetterT : Shape
    {
        public LetterT()
            : base(0, 1, -1, 0, 0, 0, 1, 0, ProductType.TurnByLastOne)
        {

        }
    }
}
