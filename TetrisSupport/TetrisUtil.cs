using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TetrisSupport
{
    public class TetrisUtil
    {
        static Dictionary<byte, int> itemType2ChangeLine = new Dictionary<byte, int>();

        static TetrisUtil()
        {
            itemType2ChangeLine.Add((byte)ItemType.AddOneLine, 1);
            itemType2ChangeLine.Add((byte)ItemType.AddTwoLine, 2);
            itemType2ChangeLine.Add((byte)ItemType.AddThreeLine, 3);
            itemType2ChangeLine.Add((byte)ItemType.ReduceOneLine, -1);
            itemType2ChangeLine.Add((byte)ItemType.ReduceTwoLine, -2);
            itemType2ChangeLine.Add((byte)ItemType.ReduceThreeLine, -3);
            itemType2ChangeLine.Add((byte)ItemType.SpeedDown, 0);
            itemType2ChangeLine.Add((byte)ItemType.SpeedUp, 0);
        }

        public static int GetChangeLineByItemType(byte itemType)
        {
            return itemType2ChangeLine[itemType];
        }

        public static void AnalyseItemFlag(byte itemFlag, out int colIndex, out ItemType itemType)
        {
            colIndex = itemFlag % Map.ColumnCount;
            itemType = (ItemType)(itemFlag / Map.ColumnCount + 1);
        }

    }
}
