﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TetrisSupport
{
    public class Two : Shape
    {
        public Two()
            : base(-1, 0, 0, 0, 0, -1, -1, -1, ProductType.TurnThenClone)
        {

        }
    }
}
