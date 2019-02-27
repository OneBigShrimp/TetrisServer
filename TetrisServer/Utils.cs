using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TetrisSupport;

namespace TetrisServer
{
    public class Utils
    {
        public static Position Pos2Position(Pos pos)
        {
            return new Position() { X = pos.X, Y = pos.Y };
        }

        public static Pos Position2Pos(Position position)
        {
            return new Pos() { X = (byte)position.X, Y = (byte)position.Y };
        }
    }
}
