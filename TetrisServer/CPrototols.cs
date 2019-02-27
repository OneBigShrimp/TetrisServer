using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNetManager;

namespace TetrisServer
{

    public class CSetName : IProtocol
    {
        public string Name;
        public void Process(ILinker linker, object args)
        {
            Table tb = args as Table;
            tb.SetPlayerName(linker, this.Name);
        }
    }

    public class CPlayerReady : IProtocol
    {
        public byte IsReady;
        public void Process(ILinker linker, object args)
        {
            Table tb = args as Table;
            tb.PlayerReady(linker, this.IsReady == 1);
        }
    }


    public class CChangeTeam : IProtocol
    {
        public byte TeamId;

        public void Process(ILinker linker, object args)
        {
            Table tb = args as Table;
            tb.ChangeTeam(linker, TeamId);
        }
    }


    public class CPutShape : IProtocol
    {
        public byte TurnIndex;
        public Pos PutPos;
        public void Process(ILinker linker, object args)
        {
            Table tb = args as Table;
            tb.PlayerPutShape(linker, this);
        }
    }

    public class CUseItem : IProtocol
    {
        public byte TargetTbIndex;
        public byte ItemType;
        public void Process(ILinker linker, object args)
        {
            Table tb = args as Table;
            tb.PlayerUseItem(linker, this);
        }
    }

    public class CReqFailure : IProtocol
    {
        public void Process(ILinker linker, object args)
        {
            Table tb = args as Table;
            tb.ReqFailure(linker);
        }
    }

}
