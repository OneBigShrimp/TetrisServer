using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TetrisSupport
{

    public enum PlayerState
    {
        None = 0,
        Ready = 1,
        Gaming = 2,
        Failure = 3,
        GameOver = 4,
    }
}
