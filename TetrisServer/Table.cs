using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyNetManager;
using TetrisSupport;
using System.Threading;

namespace TetrisServer
{
    public class Table : IEnumerable<Player>
    {

        const byte shapeCountOneGame = 80;
        /// <summary>
        /// 所有的玩家,
        /// </summary>
        Player[] players = new Player[ConstData.PlayerCount];

        object enterLock = new object();

        GameState state = GameState.Waiting;

        Random r;

        static int shapeFlagRange = ConstData.ColorCount * ConstData.ShapeCount * ConstData.ShapeTurnCount;

        byte[] shapeFlags = new byte[shapeCountOneGame];

        AutoResetEvent receiveEvent = new AutoResetEvent(true);

        Thread gameThd;

        public Table()
        {
            r = new Random();
            gameThd = new Thread(this.GameLoop);
            gameThd.IsBackground = true;
            gameThd.Start();
        }

        /// <summary>
        /// 与进入桌子处理访问相同资源的,需要上锁,因为这个处理并不是在当前Table的Tick线程引发的
        /// </summary>
        /// <param name="linker"></param>
        /// <returns></returns>
        public bool EnterTable(ILinker linker)
        {
            byte tbIndex = byte.MaxValue;
            lock (enterLock)
            {
                if (state == GameState.Waiting)
                {
                    for (byte i = 0; i < players.Length; i++)
                    {
                        if (players[i] == null)
                        {
                            linker.Id = tbIndex = i;
                            players[i] = new Player(linker, i, r, this.CheckGameOver);
                            linker.ReceiveEvent = receiveEvent;
                            break;
                        }
                    }
                }
            }
            //TbIndex为0表示进入桌子失败
            linker.SendMsg(new SEnter() { TbIndex = tbIndex });
            if (tbIndex == byte.MaxValue)
            {
                linker.Close();
            }
            else
            {
                foreach (var item in this)
                {
                    if (item.TbIndex != tbIndex)
                    {
                        //通知我其他在座玩家都有谁
                        linker.SendMsg(new SAddPlayer() { TbIndex = item.TbIndex, Name = item.Name, TeamId = item.TeamId });
                        //通知其他在座玩家我来了
                        item.SendMsg(new SAddPlayer() { TbIndex = tbIndex, Name = "", TeamId = 0 });
                    }
                }
            }
            return false;
        }

        public void SetPlayerName(ILinker linker, string name)
        {
            Player p = this.players[linker.Id];
            if (p == null)
            {
                return;
            }
            SSetName csn = new SSetName() { TbIndex = p.TbIndex, Name = name };
            p.Name = name;
            SendToAll(csn);
        }

        public void ChangeTeam(ILinker linker, byte teamId)
        {
            if (state != GameState.Waiting)
            {
                return;
            }
            Player p = this.players[linker.Id];
            if (p.State != PlayerState.None)
            {
                return;
            }
            p.TeamId = teamId;
            SendToAll(new SChangeTeam() { TbIndex = p.TbIndex, TeamId = teamId });
        }

        public void PlayerReady(ILinker linker, bool isReady)
        {
            if (state != GameState.Waiting)
            {
                return;
            }
            Player p = this.players[linker.Id];
            if (p != null)
            {
                if (isReady && p.State == PlayerState.None)
                {
                    p.State = PlayerState.Ready;
                }
                else if (!isReady && p.State == PlayerState.Ready)
                {
                    p.State = PlayerState.None;
                }
                else
                {
                    return;
                }
                SPlayerReady sps = new SPlayerReady();
                sps.TbIndex = p.TbIndex;
                sps.IsReady = (byte)(isReady ? 1 : 0);
                foreach (var onePlayer in this)
                {
                    onePlayer.SendMsg(sps);
                }
                lock (enterLock)
                {
                    if (!CheckGameReady())
                    {
                        return;
                    }
                    //将游戏状态更新为准备必须在enterLock锁内部,防止游戏进入准备阶段有玩家进入游戏
                    state = GameState.Ready;
                }
                GameReady();
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("PlayerReady() GetPlayer 失败");
            }
        }

        public void PlayerPutShape(ILinker linker, CPutShape cps)
        {
            if (state != GameState.Gaming)
            {
                return;
            }
            Player p = this.players[linker.Id];
            if (p != null)
            {
                if (p.State != PlayerState.Gaming)
                {
                    return;
                }
                byte itemFlag = byte.MaxValue;
                int curUseIndex = this.GetUseIndex(p.CurShapeIndex++);
                byte putShapeFlag = shapeFlags[curUseIndex];
                putShapeFlag = (byte)(putShapeFlag / ConstData.ShapeTurnCount * ConstData.ShapeTurnCount + cps.TurnIndex);
                Shape shape = Shape.Create(putShapeFlag);
                shape.Pos = Utils.Pos2Position(cps.PutPos);
                int erasureCount = p.PutOneShape(shape, ref itemFlag);
                if (erasureCount != -1)
                {
                    SPutShape sps = new SPutShape();
                    sps.TbIndex = p.TbIndex;
                    sps.ShapeFlag = putShapeFlag;
                    sps.PutPos = cps.PutPos;
                    sps.NextNextShapeFlag = shapeFlags[this.GetUseIndex(curUseIndex + 3)];
                    sps.ItemFlag = itemFlag;
                    SendToAll(sps);

                    if (erasureCount > 2)
                    {
                        List<byte> underAtkSeats = new List<byte>();
                        byte addLine = (byte)(erasureCount - 1);
                        foreach (var onePlayer in this)
                        {
                            if (onePlayer.State != PlayerState.Gaming || onePlayer == p || (onePlayer.TeamId == p.TeamId && onePlayer.TeamId != 0))
                            {//不自动攻击自己或者队友
                                continue;
                            }
                            underAtkSeats.Add(onePlayer.TbIndex);
                            onePlayer.AddLine(addLine);
                        }
                        SendToAll(new SUnderAttack() { TbIndexs = underAtkSeats.ToArray(), AddLine = addLine });
                    }
                }
                else
                {
                    //下发整个地图,和AllShapeInfo
                    p.CurShapeIndex--;
                    SRefreshHoleMap srhm = new SRefreshHoleMap();
                    srhm.AllShape = new AllShapeInfo()
                    {
                        CurShapeFlag = shapeFlags[p.CurShapeIndex],
                        NextShapeFlag = shapeFlags[GetUseIndex(p.CurShapeIndex + 1)],
                        NextNextShapeFlag = shapeFlags[GetUseIndex(p.CurShapeIndex + 2)]
                    };
                    ItemDetail[] items;
                    srhm.AllLines = p.BinaryAllMap(out items);
                    srhm.Items = items;
                    p.SendMsg(srhm);
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("PlayerPutShape() GetPlayer 失败");
            }
        }

        public void PlayerUseItem(ILinker linker, CUseItem cui)
        {
            if (state != GameState.Gaming)
            {
                return;
            }
            Player p = this.players[linker.Id];
            if (p != null)
            {
                if (p.RemoveItem(cui.ItemType))
                {
                    if (cui.TargetTbIndex >= ConstData.PlayerCount)
                    {
                        return;
                    }
                    Player target = this.players[cui.TargetTbIndex];
                    if (target != null)
                    {
                        target.ReceiveItem(cui.ItemType);
                        SUseItem sui = new SUseItem() { FromTbIndex = p.TbIndex, TargetTbIndex = cui.TargetTbIndex, ItemType = cui.ItemType };
                        SendToAll(sui);
                    }
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show(string.Format("PlayerUseItem() Player {0} has no item {1}", linker.Id, (ItemType)cui.ItemType));
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("PlayerUseItem() GetPlayer 失败");
            }
        }

        public void ReqFailure(ILinker linker)
        {
            Player p = this.players[linker.Id];
            if (p != null)
            {
                if (state != GameState.Gaming || p.State != PlayerState.Gaming)
                {
                    return;
                }
                p.State = PlayerState.Failure;
                SendToAll(new SPlayerFailure() { TbIndex = p.TbIndex });
                CheckGameOver();
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("ReqFailure() GetPlayer 失败");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="linker"></param>
        public void OnLinkerClose(ILinker linker)
        {
            lock (enterLock)
            {
                if (linker.Id != -1)
                {
                    players[linker.Id] = null;

                    SPlayerLeave spl = new SPlayerLeave();
                    spl.TbIndex = (byte)linker.Id;
                    SendToAll(spl);
                    CheckGameOver();
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("linker remove failure");
                }
            }
        }

        void GameLoop()
        {
            while (true)
            {
                receiveEvent.WaitOne();
                lock (enterLock)
                {
                    foreach (var item in this)
                    {
                        item.ProcessReceiveMsg(this);
                    }
                }
            }
        }

        bool CheckGameReady()
        {
            byte checkTeamId = byte.MaxValue;
            bool hasDifTeam = false;
            foreach (var item in this)
            {
                if (item.State != PlayerState.Ready)
                {
                    return false;
                }
                if (!hasDifTeam)
                {
                    if (checkTeamId == byte.MaxValue)
                    {
                        checkTeamId = item.TeamId;
                    }
                    else
                    {
                        if (checkTeamId * item.TeamId == 0 || checkTeamId != item.TeamId)
                        {//遇到一个自由人队伍,或者与之前保存的队伍不相同,则说明存在不同队伍
                            hasDifTeam = true;
                        }
                    }
                }
            }
            return hasDifTeam;
        }

        void GameReady()
        {
            foreach (var item in this)
            {
                item.State = PlayerState.Ready;
            }
            SendToAll(new SGameReady());
            Thread.Sleep(ConstData.GameReadyDuration * 1000);
            GameStart();
        }

        void GameStart()
        {
            state = GameState.Gaming;
            for (int i = 0; i < shapeCountOneGame; i++)
            {
                shapeFlags[i] = (byte)r.Next(0, shapeFlagRange);
            }
            SGameStart p = new SGameStart()
            {
                AllShape = new AllShapeInfo()
                {
                    CurShapeFlag = shapeFlags[0],
                    NextShapeFlag = shapeFlags[1],
                    NextNextShapeFlag = shapeFlags[2]
                }
            };

            foreach (var item in this)
            {
                item.GameStart();
            }
            SendToAll(p);
        }

        void SendToAll(IProtocol p)
        {
            foreach (var item in this)
            {
                item.SendMsg(p);
            }
        }

        void CheckGameOver()
        {
            if (state != GameState.Gaming)
            {
                return;
            }
            byte winTeam = byte.MaxValue;
            byte winTbIndex = byte.MaxValue;
            foreach (var item in this)
            {
                if (item.State != PlayerState.Gaming)
                {
                    continue;
                }
                if (winTeam == byte.MaxValue)
                {
                    winTeam = item.TeamId;
                    winTbIndex = item.TbIndex;
                }
                else
                {
                    if (item.TeamId != winTeam || item.TeamId * winTeam == 0)
                    {
                        return;
                    }
                }
            }
            state = GameState.Waiting;
            foreach (var item in this)
            {
                item.GameOver();
            }
            byte winner = (byte)(winTeam == byte.MaxValue ? winTbIndex : winTeam << 4);
            SendToAll(new SGameOver() { Winner = winner });
        }

        int GetUseIndex(int srcIndex)
        {
            return srcIndex % shapeCountOneGame;
        }

        enum GameState
        {
            Waiting = 1,
            Ready = 2,
            Gaming = 3,
        }


        public IEnumerator<Player> GetEnumerator()
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] != null)
                {
                    yield return players[i];
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }


}
