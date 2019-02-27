using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNetManager;

namespace TetrisServer
{
    public class Pos : ISerObj
    {
        public byte X;
        public byte Y;
    }

    public class MapCellInfo : ISerObj
    {
        public Pos CellPos;
        public byte CellInfo;
    }

    public class AllShapeInfo : ISerObj
    {
        public byte CurShapeFlag;
        public byte NextShapeFlag;
        public byte NextNextShapeFlag;
    }

    public class ItemDetail : ISerObj
    {
        public byte RowIndex;
        public byte ColIndex;
        public byte ItemCellInfo;
    }

    public enum EnterFailReason
    {
        None = 0,
        IsGaming = 1,
        TableIsFull = 2,
    }
}
