using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TetrisSupport
{
    public class Map
    {
        /// <summary>
        /// 地图总列数,这个不能随便改,整个地图消息二进制化时认定一行最多10个
        /// </summary>
        public const int ColumnCount = 10;
        public const int RowCount = 20;
        public static Position StartPos = new Position(5, 15);
        public event Action OnLineChange;


        Line[] allLines;

        public Line this[int rowIndex]
        {
            get
            {
                return allLines[rowIndex];
            }
        }

        public Map()
        {
            allLines = new Line[RowCount];
            for (int i = 0; i < RowCount; i++)
            {
                allLines[i] = new Line(i);
            }
        }

        public byte ForcePutShape(Shape shape, List<byte> getItems)
        {
            byte result = 0;
            Position[] occupy = shape.OccupyPoses.Poses;
            for (int i = 0; i < occupy.Length; i++)
            {
                allLines[occupy[i].Y][occupy[i].X] = shape.Info;
            }

            for (int i = shape.MaxY; i >= shape.MinY; i--)
            {
                if (allLines[i].IsFull)
                {
                    byte item = ErasureOneLine(i);
                    if (getItems != null && item > 0)
                    {
                        getItems.Add(item);
                    }
                    result++;
                }
            }

            return result;
        }

        public bool CheckShapeLegal(Shape shape)
        {
            Position[] occupyPoses = shape.OccupyPoses.Poses;
            for (int i = 0; i < occupyPoses.Length; i++)
            {
                int x = occupyPoses[i].X;
                int y = occupyPoses[i].Y;
                if (x < 0 || x >= ColumnCount)
                {
                    return false;
                }
                if (y < 0 || y >= RowCount)
                {
                    return false;
                }
                CellInfo cell = allLines[y][x];
                if (cell != CellInfo.None)
                {
                    return false;
                }
            }
            return true;
        }

        public void ApplyItemFlagOnMap(ref byte itemFlag)
        {
            if (itemFlag == byte.MaxValue)
            {
                return;
            }

            int colIndex;
            ItemType itemType;
            TetrisUtil.AnalyseItemFlag(itemFlag, out colIndex, out itemType);
            CellInfo itemInfo = (CellInfo)((byte)itemType + 10);
            for (int i = 0; i < Map.RowCount; i++)
            {
                if (!allLines[i].HasItem)
                {
                    if (allLines[i][colIndex] != CellInfo.None)
                    {
                        allLines[i][colIndex] = itemInfo;
                        return;
                    }
                    else
                    {
                        itemFlag = byte.MaxValue;
                    }
                }
                else if (allLines[i].IsEmpty)
                {
                    break;
                }
            }
            itemFlag = byte.MaxValue;
        }

        public bool ReceiveItem(byte itemType)
        {
            int changeLine = TetrisUtil.GetChangeLineByItemType(itemType);
            if (changeLine != 0)
            {
                return this.ChangeLine(changeLine);
            }
            else
            {
                return false;
            }
        }

        public bool ChangeLine(int changeCount)
        {
            if (changeCount < 0)
            {
                changeCount *= -1;
                for (int i = 0; i < changeCount; i++)
                {
                    ErasureOneLine(0);
                }
                return false;
            }
            else
            {
                bool gameOver = false;
                Line[] topLines = new Line[changeCount];

                int startRow = RowCount - changeCount;

                for (int i = 0; i < changeCount; i++)
                {
                    Line line = this[i + startRow];
                    if (!line.IsEmpty)
                    {
                        gameOver = true;
                    }
                    line.UnderAttack(i % 2);
                    topLines[i] = line;
                }

                for (int i = startRow - 1; i >= 0; i--)
                {
                    SetRowIndex(allLines[i], i + changeCount);
                }
                for (int i = 0; i < changeCount; i++)
                {
                    SetRowIndex(topLines[i], i);
                }
                return gameOver;
            }
        }

        public void ClearMap()
        {
            for (int i = 0; i < allLines.Length; i++)
            {
                allLines[i].Erasure();
            }
        }

        private byte ErasureOneLine(int rowIndex)
        {
            Line line = allLines[rowIndex];
            byte getItem = line.Erasure();
            for (int i = rowIndex + 1; i < allLines.Length; i++)
            {
                SetRowIndex(allLines[i], i - 1);
            }
            SetRowIndex(line, RowCount - 1);
            return getItem;
        }

        private void SetRowIndex(Line line, int targetIndex)
        {
            allLines[targetIndex] = line;
            line.RowIndex = targetIndex;
        }

    }

    public class Line
    {
        CellInfo[] _infos;

        int _rowIndex = -1;

        int _cellCount = 0;
        public bool IsFull
        {
            get
            {
                return this._cellCount == Map.ColumnCount;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return this._cellCount == 0;
            }
        }

        int _itemColIndex = -1;

        public byte ItemCol
        {
            get
            {
                return HasItem ? (byte)_itemColIndex : byte.MaxValue;
            }
        }

        public bool HasItem
        {
            get
            {
                return this._itemColIndex > -1;
            }
        }

        public int RowIndex
        {
            get
            {
                return this._rowIndex;
            }
            set
            {
                this._rowIndex = value;
                if (_eventBinding != null)
                {
                    _eventBinding.OnRowIndexChange(value);
                }
            }
        }

        public CellInfo this[int index]
        {
            get
            {
                return _infos[index];
            }
            set
            {
                if (value == _infos[index])
                {
                    return;
                }

                if ((byte)value > 10)
                {
                    _itemColIndex = index;
                }
                else
                {
                    _cellCount++;
                }

                _infos[index] = value;
                if (_eventBinding != null)
                {
                    _eventBinding.OnCellChange(index, value);
                }
            }
        }

        private LineEventBinding _eventBinding;


        public Line(int initRowIndex)
        {
            this._infos = new CellInfo[Map.ColumnCount];
            this._rowIndex = initRowIndex;
        }

        public void SetEventBinding(LineEventBinding binding)
        {
            this._eventBinding = binding;
        }

        public int ConvertToInt()
        {
            int result = 0;
            for (byte i = 0; i < Map.ColumnCount; i++)
            {
                byte curInfo = (byte)this._infos[i];
                if (curInfo > 10)
                {//大于10的表示是道具,道具另处理
                    curInfo = 1;
                }
                //每3位表示一个格子的颜色
                result |= (curInfo << (i * 3));
            }
            return result;
        }

        public void ParseByInt(int value)
        {
            for (int i = 0; i < Map.ColumnCount; i++)
            {
                this[i] = (CellInfo)(value & 7);
                value >>= 3;
            }
        }

        public void UnderAttack(int flag)
        {
            for (int i = 0; i < Map.ColumnCount; i++)
            {
                this[i] = (i % 2) == flag ? CellInfo.Gray : CellInfo.None;
            }
        }


        public byte Erasure()
        {
            byte getItem = 0;
            for (int i = 0; i < this._infos.Length; i++)
            {
                if (getItem == 0)
                {
                    int temp = (int)this._infos[i] - 10;
                    if (temp > 0)
                    {
                        getItem = (byte)temp;
                    }
                }
                this._infos[i] = CellInfo.None;
            }
            this._cellCount = 0;
            this._itemColIndex = -1;
            if (_eventBinding != null)
            {
                _eventBinding.OnErasureLine();
            }
            return getItem;
        }

        public override string ToString()
        {
            return string.Join("-", new List<CellInfo>(_infos).ConvertAll(t => ((byte)t).ToString()).ToArray());
        }
    }

    public interface LineEventBinding
    {
        void OnRowIndexChange(int newIndex);

        void OnCellChange(int cellIndex, CellInfo newInfo);

        void OnErasureLine();
    }

    /// <summary>
    /// 单元格信息，None表示格子没有被占用
    /// </summary>
    public enum CellInfo
    {
        None = 0,
        Blue = 1,
        Green = 2,
        Purple = 3,
        Red = 4,

        Gray = 5,

        SpeedUp = 11,
        AddOneLine = 12,
        AddTwoLine = 13,
        AddThreeLine = 14,
        SpeedDown = 15,
        ReduceOneLine = 16,
        ReduceTwoLine = 17,
        ReduceThreeLine = 18,
    }

    public enum ItemType
    {
        SpeedUp = 1,
        AddOneLine = 2,
        AddTwoLine = 3,
        AddThreeLine = 4,

        SpeedDown = 5,
        ReduceOneLine = 6,
        ReduceTwoLine = 7,
        ReduceThreeLine = 8,
    }
}
