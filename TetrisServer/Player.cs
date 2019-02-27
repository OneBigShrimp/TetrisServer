using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TetrisSupport;
using MyNetManager;
using System.IO;

namespace TetrisServer
{
    public class Player
    {
        static int randomRange = ConstData.ItemCount * Map.ColumnCount;


        public PlayerState State { get; set; }

        public CellInfo this[int row, int column]
        {
            get
            {
                return this._map[row][column];
            }
        }

        public byte TeamId { get; set; }

        public string Name { get; set; }

        public int CurShapeIndex { get; set; }

        byte _tbIndex;

        public byte TbIndex
        {
            get
            {
                return this._tbIndex;
            }
        }

        Map _map;
        Random _r;

        ILinker _linker;

        List<byte> _itemBag = new List<byte>();

        List<byte> tempBag = new List<byte>();

        Action _onGameFail;

        public Player(ILinker linker, byte tbIndex, Random r, Action onGameFail)
        {
            this._linker = linker;
            this._tbIndex = tbIndex;
            this._r = r;
            this._onGameFail = onGameFail;
            this._map = new Map();
        }

        public void ProcessReceiveMsg(Table tb)
        {
            this._linker.Tick(tb);
        }

        /// <summary>
        /// 将一个shape放置在地图上
        /// </summary>
        /// <param name="shape">被放置的shape</param>
        /// <param name="itemFlag">获得的道具标识</param>
        /// <returns>消除的层数,-1表示放置非法</returns>
        public int PutOneShape(Shape shape, ref byte itemFlag)
        {
            //File.AppendAllText("Test.txt", shape.ToString());
            if (_map.CheckShapeLegal(shape))
            {
                Position oldPos = shape.Pos;
                shape.Pos += Position.Down;
                if (!_map.CheckShapeLegal(shape))
                {
                    shape.Pos = oldPos;
                    int erasureCount = _map.ForcePutShape(shape, tempBag);
                    if (erasureCount > 0)
                    {
                        itemFlag = (byte)_r.Next(0, randomRange);
                        _map.ApplyItemFlagOnMap(ref itemFlag);

                        if (_itemBag.Count < ConstData.ItemBagCapacity && tempBag.Count > 0)
                        {
                            int getCount = tempBag.Count > (ConstData.ItemBagCapacity - _itemBag.Count) ? (ConstData.ItemBagCapacity - _itemBag.Count) : tempBag.Count;
                            SEatItems sei = new SEatItems() { Items = new byte[getCount] };
                            for (int i = 0; i < getCount; i++)
                            {
                                sei.Items[i] = tempBag[i];
                                _itemBag.Add(tempBag[i]);
                            }
                            this.SendMsg(sei);
                        }
                        tempBag.Clear();
                    }
                    return erasureCount;
                }
            }
            return -1;
        }

        public void GameStart()
        {
            this.CurShapeIndex = 0;
            this.State = PlayerState.Gaming;
            this._map.ClearMap();
            this._itemBag.Clear();
        }

        public void SendMsg(IProtocol p)
        {
            this._linker.SendMsg(p);
        }


        public int[] BinaryAllMap(out ItemDetail[] items)
        {
            List<ItemDetail> itemDetailList = new List<ItemDetail>();
            List<int> intList = new List<int>();
            for (byte i = 0; i < Map.RowCount; i++)
            {
                int curLine = this._map[i].ConvertToInt();
                if (curLine == 0)
                {
                    break;
                }
                if (this._map[i].HasItem)
                {
                    byte itemCol = this._map[i].ItemCol;
                    itemDetailList.Add(new ItemDetail() { RowIndex = i, ColIndex = itemCol, ItemCellInfo = (byte)this._map[i][itemCol] });
                }

                intList.Add(curLine);
            }
            items = itemDetailList.ToArray();
            return intList.ToArray();
        }

        public bool RemoveItem(byte itemType)
        {
            return _itemBag.Remove(itemType);
        }

        public bool ReceiveItem(byte itemType)
        {
            return this._map.ReceiveItem(itemType);
        }

        public void AddLine(byte addLine)
        {
            this._map.ChangeLine(addLine);
        }

        public void GameOver()
        {
            this._map.ClearMap();
            this.State = PlayerState.None;
        }
    }

}
