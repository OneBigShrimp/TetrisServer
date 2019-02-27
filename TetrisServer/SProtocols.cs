using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNetManager;

namespace TetrisServer
{
    public class SEnter : IProtocol
    {
        public byte TbIndex;
        public void Process(ILinker linker, object args)
        {
        }
    }

    public class SAddPlayer : IProtocol
    {
        public byte TbIndex;
        public string Name;
        public byte TeamId;
        public void Process(ILinker linker, object args)
        {
        }
    }

    public class SSetName : IProtocol
    {
        public byte TbIndex;
        public string Name;
        public void Process(ILinker linker, object args)
        {
        }
    }

    public class SChangeTeam : IProtocol
    {
        public byte TbIndex;
        public byte TeamId;

        public void Process(ILinker linker, object args)
        {
        }
    }

    public class SPlayerReady : IProtocol
    {
        public byte TbIndex;
        public byte IsReady;
        public void Process(ILinker linker, object args)
        {
        }
    }

    public class SGameReady : IProtocol
    {
        public void Process(ILinker linker, object args)
        {
        }
    }

    public class SGameStart : IProtocol
    {
        public AllShapeInfo AllShape;
        public void Process(ILinker linker, object args)
        {
        }
    }


    public class SPutShape : IProtocol
    {
        public byte TbIndex;
        public byte ShapeFlag;
        public Pos PutPos;
        public byte NextNextShapeFlag;
        public byte ItemFlag;
        public void Process(ILinker linker, object args)
        {
        }
    }

    public class SRefreshHoleMap : IProtocol
    {
        public int[] AllLines;
        public ItemDetail[] Items;
        public AllShapeInfo AllShape;
        public void Process(ILinker linker, object args)
        {
        }
    }

    public class SUseItem : IProtocol
    {
        public byte FromTbIndex;
        public byte TargetTbIndex;
        public byte ItemType;
        public void Process(ILinker linker, object args)
        {
        }
    }

    public class SEatItems : IProtocol
    {
        public byte[] Items;
        public void Process(ILinker linker, object args)
        {
        }
    }

    public class SUnderAttack : IProtocol
    {
        public byte[] TbIndexs;

        public byte AddLine;
        public void Process(ILinker linker, object args)
        {
        }
    }


    public class SPlayerFailure : IProtocol
    {
        public byte TbIndex;
        public void Process(ILinker linker, object args)
        {

        }
    }


    public class SPlayerLeave : IProtocol
    {
        public byte TbIndex;
        public void Process(ILinker linker, object args)
        {
        }
    }

    public class SGameOver : IProtocol
    {
        /// <summary>
        /// 胜利者,
        /// 如果胜利者是自由人,则把胜利者的座位号放在低四位
        /// 如果胜利者是一个队伍,则把胜利队伍Id放在高四位
        /// </summary>
        public byte Winner;

        public void Process(ILinker linker, object args)
        {
        }
    }
}
