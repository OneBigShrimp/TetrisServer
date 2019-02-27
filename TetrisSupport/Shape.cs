using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TetrisSupport
{
    public abstract class Shape
    {

        protected enum ProductType
        {
            /// <summary>
            /// 根据前一个索引逆时针旋转90度
            /// </summary>
            TurnByLastOne = 1,
            /// <summary>
            /// 第1个通过旋转第0个获得，然后第2个克隆第0个，第3个克隆第1个
            /// </summary>
            TurnThenClone = 2,
        }

        private Position _pos;

        public Position Pos
        {
            get
            {
                return _pos;
            }
            set
            {
                this._pos = value;
            }
        }

        public CellInfo Info { get; private set; }

        private int _curTurnIndex = 0;
        public int CurTurnIndex
        {
            get
            {
                return this._curTurnIndex;
            }
            set
            {
                value %= 4;
                if (value < 0)
                {
                    value += 4;
                }
                this._curTurnIndex = value;
            }
        }
        public FourPos LocalOccupyPoses
        {
            get
            {
                return relativeOccupy[_curTurnIndex];
            }
        }

        public FourPos OccupyPoses
        {
            get
            {
                return relativeOccupy[_curTurnIndex] + Pos;
            }
        }

        public int MinY
        {
            get
            {
                return relativeOccupy[_curTurnIndex].MinY + Pos.Y;
            }
        }


        public int MaxY
        {
            get
            {
                return relativeOccupy[_curTurnIndex].MaxY + Pos.Y;
            }
        }


        protected FourPos[] relativeOccupy = new FourPos[4];

        protected Shape(int x0, int y0, int x1, int y1, int x2, int y2, int x3, int y3, ProductType pt)
        {
            relativeOccupy[0] = new FourPos(new Position(x0, y0), new Position(x1, y1), new Position(x2, y2), new Position(x3, y3));
            relativeOccupy[1] = relativeOccupy[0].GetTurn90Degree();
            if (pt == ProductType.TurnByLastOne)
            {
                relativeOccupy[2] = relativeOccupy[1].GetTurn90Degree();
                relativeOccupy[3] = relativeOccupy[2].GetTurn90Degree();
            }
            else
            {
                relativeOccupy[2] = relativeOccupy[0].Clone();
                relativeOccupy[3] = relativeOccupy[1].Clone();
            }
        }


        protected Shape()
        {

        }

        public static Shape Create(byte shapeFlag)
        {
            int turnIndex = shapeFlag % ConstData.ShapeTurnCount;
            int typeAndCellInfo = shapeFlag / ConstData.ShapeTurnCount;
            ShapeType type = (ShapeType)(typeAndCellInfo % ConstData.ShapeCount + 1);
            CellInfo info = (CellInfo)(typeAndCellInfo / ConstData.ShapeCount + 1);

            Shape result;
            switch (type)
            {
                case ShapeType.Stick:
                    result = new Stick();
                    break;
                case ShapeType.Stone:
                    result = new Stone();
                    break;
                case ShapeType.LetterT:
                    result = new LetterT();
                    break;
                case ShapeType.Two:
                    result = new Two();
                    break;
                case ShapeType.MirrorTwo:
                    result = new MirrorTwo();
                    break;
                case ShapeType.Seven:
                    result = new Seven();
                    break;
                case ShapeType.MirrorSeven:
                    result = new MirrorSeven();
                    break;
                default:
                    throw new Exception("ShapeType not support : " + type);
            }
            result._pos = new Position();
            result.Info = info;
            result.CurTurnIndex = turnIndex;
            return result;
        }




        public override string ToString()
        {
            return string.Format("造型: {0} 旋转: {1} 坐标: {2}", base.ToString(), this.CurTurnIndex, this.Pos.ToString());
        }
    }

    public enum ShapeType : byte
    {
        Stick = 1,
        Stone = 2,
        LetterT = 3,
        Two = 4,
        MirrorTwo = 5,
        Seven = 6,
        MirrorSeven = 7,
    }


    /// <summary>
    /// 一个坐标逆时针旋转90度，相当于x放在y的位置，y取相反数放在x的位置
    /// </summary>
    public struct Position
    {
        public int X { get; set; }
        public int Y { get; set; }

        public static Position Down
        {
            get
            {
                return new Position(0, -1);
            }
        }

        public static Position Left
        {
            get
            {
                return new Position(-1, 0);
            }
        }

        public static Position Right
        {
            get
            {
                return new Position(1, 0);
            }
        }

        public Position(int x, int y)
            : this()
        {
            this.X = x;
            this.Y = y;
        }

        internal Position GetTurn90Degree()
        {
            return new Position(-this.Y, this.X);
        }

        public static Position operator +(Position p1, Position p2)
        {
            return new Position(p1.X + p2.X, p1.Y + p2.Y);
        }

        public override string ToString()
        {
            return string.Format("[{0}, {1}]", this.X, this.Y);
        }
    }

    public class FourPos
    {
        public Position[] Poses { get; private set; }


        public int MinY { get; private set; }
        public int MaxY { get; private set; }

        public static FourPos operator +(FourPos self, Position pos)
        {
            return new FourPos(self.Poses[0] + pos, self.Poses[1] + pos, self.Poses[2] + pos, self.Poses[3] + pos);
        }

        public FourPos(Position[] source)
            : this(source[0], source[1], source[2], source[3])
        {

        }

        public FourPos(Position p0, Position p1, Position p2, Position p3)
        {
            Poses = new Position[] { p0, p1, p2, p3 };
            int minY = 3;
            int maxY = 0;
            for (int i = 0; i < Poses.Length; i++)
            {
                int curY = Poses[i].Y;
                if (curY < minY)
                {
                    minY = curY;
                }
                if (curY > maxY)
                {
                    maxY = curY;
                }
            }
            this.MinY = minY;
            this.MaxY = maxY;
        }

        public FourPos GetTurn90Degree()
        {
            return new FourPos(this.Poses[0].GetTurn90Degree(), this.Poses[1].GetTurn90Degree(), this.Poses[2].GetTurn90Degree(), this.Poses[3].GetTurn90Degree());
        }

        public FourPos Clone()
        {
            return (FourPos)this.MemberwiseClone();
        }


    }
}
