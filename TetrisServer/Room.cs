using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNetManager;
using TetrisSupport;

namespace TetrisServer
{
    public class Room
    {

        public static readonly Room Instance = new Room();

        Table _oneTb;
        public Room()
        {

        }

        public void Init(string ip)
        {
            _oneTb = new Table();
            ServerNetManager.Instance.Init(this.OnAddLinker, this.OnNetException);
            Regist();
            ServerNetManager.Instance.StartListen(ip, ConstData.GamePort);
        }

        void OnAddLinker(ILinker linker)
        {
            _oneTb.EnterTable(linker);
        }

        void OnNetException(ILinker linker, NetExceptionType excType, Exception ex)
        {
            _oneTb.OnLinkerClose(linker);
        }


        void Regist()
        {
            ServerNetManager.Instance.Regist(typeof(CPlayerReady));
            ServerNetManager.Instance.Regist(typeof(CChangeTeam));
            ServerNetManager.Instance.Regist(typeof(CPutShape));
            ServerNetManager.Instance.Regist(typeof(CUseItem));
            ServerNetManager.Instance.Regist(typeof(CReqFailure));
            ServerNetManager.Instance.Regist(typeof(CSetName));


            ServerNetManager.Instance.Regist(typeof(SEnter));
            ServerNetManager.Instance.Regist(typeof(SAddPlayer));
            ServerNetManager.Instance.Regist(typeof(SChangeTeam));
            ServerNetManager.Instance.Regist(typeof(SPlayerReady));
            ServerNetManager.Instance.Regist(typeof(SGameReady));
            ServerNetManager.Instance.Regist(typeof(SGameStart));
            ServerNetManager.Instance.Regist(typeof(SPutShape));
            ServerNetManager.Instance.Regist(typeof(SRefreshHoleMap));
            ServerNetManager.Instance.Regist(typeof(SUseItem));
            ServerNetManager.Instance.Regist(typeof(SPlayerFailure));
            ServerNetManager.Instance.Regist(typeof(SPlayerLeave));
            ServerNetManager.Instance.Regist(typeof(SEatItems));
            ServerNetManager.Instance.Regist(typeof(SGameOver));
            ServerNetManager.Instance.Regist(typeof(SUnderAttack));
            ServerNetManager.Instance.Regist(typeof(SSetName));
        }
    }
}
