
using NLog;
using Sandbox.Game.World;
using System;
using Sandbox.Game.GameSystems.BankingAndCurrency;
using Torch;
using Torch.API;
using Torch.Commands;
using Torch.Commands.Permissions;
using VRage.Game.ModAPI;
using Torch.Session;
using System.Timers;
using System.Threading.Tasks;
using System.Runtime.Remoting.Contexts;
using System.Collections;
using Torch.API.Managers;
using Torch.API.Session;
using System.IO;
using Sandbox.Game.Entities;
using VRage.Game;
using System.Collections.Concurrent;
using VRage.Groups;
using Sandbox.ModAPI;
using VRageMath;
using Sandbox.Engine.Multiplayer;
using Torch.Managers.ChatManager;
using Sandbox.Game.Entities.Cube;
using Sandbox.ModAPI.Ingame;

namespace TechEvent
{
    public class TechEventPlugin : TorchPluginBase
    {
        private static string gridName;
        public static Logger Log = LogManager.GetCurrentClassLogger();

        private static Timer bTimer = new Timer();
        private static bool eventStarted = false;
        private static bool billion = false;
          private static Timer aTimer = new Timer();
        private static bool cancelled = false;
        private static int count;
        /// <inheritdoc />
        public override void Init(ITorchBase torch)
        {
            base.Init(torch);
            Log.Info("GIB ME THE TECH");
            var sessionManager = Torch.Managers.GetManager<TorchSessionManager>();
            aTimer.Interval = 60000;
            aTimer.Elapsed += OnTimedEventA;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;

           
            

        }
        private static bool CheckUptime()
        {
            bool isTime = false;
            if (TorchBase.Instance.CurrentSession.KeenSession.ElapsedPlayTime.TotalHours >= 1 && !eventStarted)
            {
               if (MySession.Static.Players.GetOnlinePlayerCount() < 10)
                {
                    if (!cancelled)
                    {
                     
                        SendMessageTemp("Tech Event", "Not enough players, cancelling.", Color.Red);
                        cancelled = true;
                    }
                    return false;
                }
                if (!eventStarted)
                {
                    Random rnd = new Random();
                    int gridNum = rnd.Next(0, 5);
                    switch (gridNum)
                    {
                        case 1:
                            gridName = "Event Location 1";
                            break;
                        case 2:
                            gridName = "Event Location 2";
                            break;
                        case 3:
                            gridName = "Event Location 3";
                            break;
                        case 4:
                            gridName = "Event Location 4";
                            break;
                        default:
                            gridName = "Event Location 1";
                            break;
                    }
                    SendMessageTemp("PvP Event", "Event is starting at - " + gridName, Color.Red);
                }
                aTimer.Enabled = false;
                aTimer.Close();
                eventStarted = true;
       
                bTimer.Interval = 60000;
                bTimer.Elapsed += OnTimedEventB;
                bTimer.AutoReset = true;
                bTimer.Enabled = true;
            }
            return isTime;
        }
        private static void OnTimedEventA(Object source, System.Timers.ElapsedEventArgs e)
        {
            Task.Run(() =>
            {
                CheckUptime();
            });
        }
        private static void FillContainer()
        {
            if (count == 16)
            {
                SendMessageTemp("PvP Event", "Event is over", Color.Red);
                bTimer.Close();
                return;
               
            }
            count++;
            MyCubeGrid TechGrid = null;
            ConcurrentBag<MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Group> groups = GridFinder.FindGridGroup(gridName);
            Parallel.ForEach(MyCubeGridGroups.Static.Physical.Groups, group =>
            {
                foreach (MyGroups<MyCubeGrid, MyGridPhysicalGroupData>.Node groupNodes in group.Nodes)
                {
                    MyCubeGrid grid = groupNodes.NodeData;
                    //does the grid have the same name as the input
                    bool found = false;
     
                    foreach (VRage.Game.ModAPI.IMySlimBlock block in grid.GetBlocks())
                    {
                        if (block != null && block.BlockDefinition.Id.SubtypeName.Contains("PrizeboxD") && grid.DisplayName.Equals(gridName))
                        {
                            TechGrid = grid;    
                            found = true;
                        }
                    }
                    if (!found)
                    {
                        continue;
                    }
                    break;
                }
            });
            if (TechGrid != null)
            {
                Sandbox.ModAPI.IMyTerminalBlock container = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(TechGrid).GetBlockWithName("Tech Container") as IMyTerminalBlock;
                Random random = new Random();
                int num = random.Next(100);
                if (num <= 1)
                {
                    if (!billion) { 
                    container.GetInventory().AddItems(VRage.MyFixedPoint.DeserializeStringSafe("50000000"), new MyObjectBuilder_PhysicalObject() { SubtypeName = "SpaceCredit" });
                    SendMessageTemp("PvP " + gridName, "50 Million SC.", Color.Red);

                        billion = true;
                }
                    else
                    {
                        container.GetInventory().AddItems(VRage.MyFixedPoint.DeserializeStringSafe("10000000"), new MyObjectBuilder_PhysicalObject() { SubtypeName = "SpaceCredit" });
                        SendMessageTemp("PvP " + gridName, "10 Million SC.", Color.Red);
                        return;
                    }
                    return;
                }
                if (num <= 50)
                {
                    container.GetInventory().AddItems(VRage.MyFixedPoint.DeserializeStringSafe("10000000"), new MyObjectBuilder_PhysicalObject() { SubtypeName = "SpaceCredit" });
                    SendMessageTemp("PvP " + gridName, "10 Million SC.", Color.Red);
                    return;
                }
                else
                {
                    num = random.Next(100);
                    if (num <= 10)
                    {
                        int num2 = random.Next(100);
                        if (num2 <= 10)
                        {
                            container.GetInventory().AddItems(VRage.MyFixedPoint.DeserializeStringSafe("50"), new MyObjectBuilder_Component() { SubtypeName = "Tech8x" });
                            SendMessageTemp("PvP " + gridName, "50 Exotic", Color.Red);
                        }
                        else
                        {
                            container.GetInventory().AddItems(VRage.MyFixedPoint.DeserializeStringSafe("10"), new MyObjectBuilder_Component() { SubtypeName = "Tech8x" });
                            SendMessageTemp("PvP " + gridName, "10 Exotic", Color.Red);
                        }
                        return;
                    }
                    if (num <= 30)
                    {
                        int num2 = random.Next(100);
                        if (num2 <= 40)
                        {
                            container.GetInventory().AddItems(VRage.MyFixedPoint.DeserializeStringSafe("75"), new MyObjectBuilder_Component() { SubtypeName = "Tech4x" });
                            SendMessageTemp("PvP " + gridName, "75 Rare", Color.Red);
                        }
                        else
                        {
                            container.GetInventory().AddItems(VRage.MyFixedPoint.DeserializeStringSafe("25"), new MyObjectBuilder_Component() { SubtypeName = "Tech4x" });
                            SendMessageTemp("PvP " + gridName, "25 Rare", Color.Red);
                        }
                        return;
                    }
                    if (num <= 100)
                    {
                        int num2 = random.Next(100);
                        if (num2 <= 50)
                        {
                            container.GetInventory().AddItems(VRage.MyFixedPoint.DeserializeStringSafe("100"), new MyObjectBuilder_Component() { SubtypeName = "Tech2x" });
                            SendMessageTemp("PvP " + gridName, "100 Common", Color.Red);
                        }
                        else
                        {
                            container.GetInventory().AddItems(VRage.MyFixedPoint.DeserializeStringSafe("50"), new MyObjectBuilder_Component() { SubtypeName = "Tech2x" });
                            SendMessageTemp("PvP " + gridName, "50 Common", Color.Red);
                        }
                        return;
                    }
                }



            }
        }

        public static void SendMessageTemp(string author, string message, Color color)
        {
            Logger _chatLog = LogManager.GetLogger("Chat");
            ScriptedChatMsg scriptedChatMsg1 = new ScriptedChatMsg();
            scriptedChatMsg1.Author = author;
            scriptedChatMsg1.Text = message;
            scriptedChatMsg1.Font = "White";
            scriptedChatMsg1.Color = color;
            scriptedChatMsg1.Target = 0L;
            ScriptedChatMsg scriptedChatMsg2 = scriptedChatMsg1;
            MyMultiplayerBase.SendScriptedChatMessage(ref scriptedChatMsg2);
            _chatLog.Info($"{author} (to {ChatManagerServer.GetMemberName(0)}): {message}");

        }
        private static void OnTimedEventB(Object source, System.Timers.ElapsedEventArgs e)
        {
            Task.Run(() =>
            {
                FillContainer();
            });
        }
    }
}
