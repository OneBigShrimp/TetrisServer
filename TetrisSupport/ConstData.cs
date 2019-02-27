using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TetrisSupport
{
    public class ConstData
    {
        public const byte PlayerCount = 6;
        public const byte ItemCount = 8;

        public const byte ItemBagCapacity = 10;
        /// <summary>
        /// 颜色数量(不包括灰色),这个数不能超过7,整个地图消息二进制化时认定颜色总种类不超过8种
        /// </summary>
        public const byte ColorCount = 4;
        public const byte ShapeCount = 7;
        public const byte ShapeTurnCount = 4;
        public const int GamePort = 20010;
        public const int GameReadyDuration = 3;
    }
}
