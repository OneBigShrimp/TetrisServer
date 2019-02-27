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
            ServerNetManager.Instance.Regist(typeof(CPlayerReady), 1);
            ServerNetManager.Instance.Regist(typeof(CChangeTeam), 3);
            ServerNetManager.Instance.Regist(typeof(CPutShape), 4);
            ServerNetManager.Instance.Regist(typeof(CUseItem), 5);
            ServerNetManager.Instance.Regist(typeof(CReqFailure), 6);
            ServerNetManager.Instance.Regist(typeof(CSetName), 7);

            
            ServerNetManager.Instance.Regist(typeof(SEnter), 20);
            ServerNetManager.Instance.Regist(typeof(SAddPlayer), 21);
            ServerNetManager.Instance.Regist(typeof(SChangeTeam), 22);
            ServerNetManager.Instance.Regist(typeof(SPlayerReady), 23);
            ServerNetManager.Instance.Regist(typeof(SGameReady), 25);
            ServerNetManager.Instance.Regist(typeof(SGameStart), 26);
            ServerNetManager.Instance.Regist(typeof(SPutShape), 27);
            ServerNetManager.Instance.Regist(typeof(SRefreshHoleMap), 28);
            ServerNetManager.Instance.Regist(typeof(SUseItem), 29);
            ServerNetManager.Instance.Regist(typeof(SPlayerFailure), 30);
            ServerNetManager.Instance.Regist(typeof(SPlayerLeave), 31);
            ServerNetManager.Instance.Regist(typeof(SEatItems), 32);
            ServerNetManager.Instance.Regist(typeof(SGameOver), 33);
            ServerNetManager.Instance.Regist(typeof(SUnderAttack), 34);
            ServerNetManager.Instance.Regist(typeof(SSetName), 35);
        }
    }
}
