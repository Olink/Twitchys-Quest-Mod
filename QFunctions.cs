﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using TShockAPI;
using Terraria;

namespace QuestSystemLUA
{
    public class QFunctions
    {
        public static bool AtXY(int x, int y, QPlayer Player, int radius = 1)
        {
            Rectangle rec, playerrec;
            rec = new Rectangle(x - radius, y - radius, radius * 2, radius * 2);
            playerrec = new Rectangle((int)Player.TSPlayer.X / 16, (int)Player.TSPlayer.Y / 16, 1, 1);
            return rec.Intersects(playerrec);
        }
        public static void TileEdit(int x, int y, string tile)
        {
            byte type;

            if (QTools.GetTileTypeFromName(tile, out type))
            {
                if (type < 253)
                {
                    Main.tile[x, y].type = (byte)type;
                    Main.tile[x, y].active = true;
                    Main.tile[x, y].liquid = 0;
                    Main.tile[x, y].skipLiquid = true;
                    Main.tile[x, y].frameNumber = 0;
                    Main.tile[x, y].frameX = -1;
                    Main.tile[x, y].frameY = -1;
                }
                else if (type == 253)
                {
                    Main.tile[x, y].active = false;
                    Main.tile[x, y].skipLiquid = false;
                    Main.tile[x, y].lava = false;
                    Main.tile[x, y].liquid = 255;
                    Main.tile[x, y].checkingLiquid = false;
                }
                else if (type == 254)
                {
                    Main.tile[x, y].active = false;
                    Main.tile[x, y].skipLiquid = false;
                    Main.tile[x, y].lava = true;
                    Main.tile[x, y].liquid = 255;
                    Main.tile[x, y].checkingLiquid = false;
                }
                if ((Main.tile[x, y].type == 53) || (Main.tile[x, y].type == 253) || (Main.tile[x, y].type == 254))
                    WorldGen.SquareTileFrame(x, y, false);
                QTools.UpdateTile(x, y);
            }
            else
                throw new Exception("Invalid Tile Name");
        }
        public static void WallEdit(int x, int y, string wall)
        {
            byte type;

            if (QTools.GetTileTypeFromName(wall, out type))
            {
                if (type < 255)
                {
                    Main.tile[x, y].wall = (byte)type;
                }
                QTools.UpdateTile(x, y);
            }
            else
                throw new Exception("Invalid Wall Name");
        }
        public static void DeleteBoth(int x, int y)
        {
            Main.tile[x, y].active = false;
            Main.tile[x, y].wall = 0;
            Main.tile[x, y].skipLiquid = true;
            Main.tile[x, y].liquid = 0;
            QTools.UpdateTile(x, y);
        }
        public static void DeleteWall(int x, int y)
        {
            Main.tile[x, y].wall = 0;
            QTools.UpdateTile(x, y);
        }
        public static void DeleteTile(int x, int y)
        {
            Main.tile[x, y].active = false;
            Main.tile[x, y].skipLiquid = true;
            Main.tile[x, y].liquid = 0;
            QTools.UpdateTile(x, y);
        }
        public static void Sleep(int time)
        {
            Thread.Sleep(time);
        }
        public static void Teleport(int x, int y, QPlayer Player)
        {
            Player.TSPlayer.Teleport(x, y + 3);
        }
        public static void ClearKillList(QPlayer Player)
        {
            lock (Player.KillNames)
                Player.KillNames.Clear();
        }
        public static void GoCollectItem(string name, int amount, QPlayer Player)
        {
            int count;
            do
            {
                count = 0;
                try
                {
                    foreach (Item slot in Player.Inventory)
                    {
                        if (slot != null)
                            if (slot.name.ToLower() == name.ToLower())
                                count += slot.stack;
                    }
                }
                catch (Exception e)
                {
                    Log.Info(e.Message);
                }
                Thread.Sleep(1);
            }
            while (count < amount);
        }
        public static void TakeItem(string qname, string iname, int amt, QPlayer Player)
        {
            if (amt > 0)
            {
                var aitem = new AwaitingItem(qname, amt, iname);
                Player.AwaitingItems.Add(aitem);
                if (amt > 1)
                    Player.TSPlayer.SendMessage(string.Format("Please drop {0} {1}'s, The excess will be returned.", amt, iname));
                else
                    Player.TSPlayer.SendMessage(string.Format("Please drop {0} {1}, The excess will be returned.", amt, iname));
                while (Player.AwaitingItems.Contains(aitem)) { Thread.Sleep(1); }
            }
        }
        public static int GetRegionTilePercentage(string tiletype, string regionname)
        {
            double amountofmatchedtiles = 0;
            double totaltilecount = 0;
            TShockAPI.DB.Region r;
            byte type;
            if (QTools.GetTileTypeFromName(tiletype, out type))
            {
                if ((r = TShock.Regions.ZacksGetRegionByName(regionname)) != null)
                {
                    for (int i = r.Area.X; i < (r.Area.X + r.Area.Width); i++)
                    {
                        for (int j = r.Area.Y; j < (r.Area.Y + r.Area.Height); j++)
                        {
                            if (Main.tile[i, j].active && Main.tile[i, j].type == type )
                                amountofmatchedtiles++;
                            totaltilecount++;
                        }
                    }
                }
            }
            if (totaltilecount != 0)
                return (int)((amountofmatchedtiles / totaltilecount) * 100);
            return 0;
        }
        public static int GetXYTilePercentage(string tiletype, int X, int Y, int Width, int Height)
        {
            double amountofmatchedtiles = 0;
            double totaltilecount = 0;
            byte type;
            if (QTools.GetTileTypeFromName(tiletype, out type))
            {
                for (int i = X; i < (X + Width); i++)
                {
                    for (int j = Y; j < (Y + Height); j++)
                    {
                        if (Main.tile[i, j].active && Main.tile[i, j].type == type)
                            amountofmatchedtiles++;
                        totaltilecount++;
                    }
                }
            }
            if (totaltilecount != 0)
                return (int)((amountofmatchedtiles / totaltilecount) * 100);
            return 0;
        }
        public static int GetRegionWallPercentage(string walltype, string regionname)
        {
            double amountofmatchedwalls = 0;
            double totalwallcount = 0;
            TShockAPI.DB.Region r;
            byte type;
            if (QTools.GetWallTypeFromName(walltype, out type))
            {
                if ((r = TShock.Regions.ZacksGetRegionByName(regionname)) != null)
                {
                    for (int i = r.Area.X; i < (r.Area.X + r.Area.Width); i++)
                    {
                        for (int j = r.Area.Y; j < (r.Area.Y + r.Area.Height); j++)
                        {
                            if (Main.tile[i, j].active && Main.tile[i, j].wall == type)
                                amountofmatchedwalls++;
                            totalwallcount++;
                        }
                    }
                }
            }
            if (totalwallcount != 0)
                return (int)((amountofmatchedwalls / totalwallcount) * 100);
            return 0;
        }
        public static int GetXYWallPercentage(string walltype, int X, int Y, int Width, int Height)
        {
            double amountofmatchedwalls = 0;
            double totalwallcount = 0;
            byte type;
            if (QTools.GetWallTypeFromName(walltype, out type))
            {
                for (int i = X; i < (X + Width); i++)
                {
                    for (int j = Y; j < (Y + Height); j++)
                    {
                        if (Main.tile[i, j].active && Main.tile[i, j].type == type)
                            amountofmatchedwalls++;
                        totalwallcount++;
                    }
                }
            }
            if (totalwallcount != 0)
                return (int)((amountofmatchedwalls / totalwallcount) * 100);
            return 0;
        }
        //Below = New in V1.2
        //Fixed/Working
        public static void Give(string name, QPlayer Player, int amount = 1)
        {
            Main.rand = new Random();
            Item item = TShock.Utils.GetItemByName(name)[0];
            Player.TSPlayer.GiveItem(item.type, item.name, item.width, item.height, amount);
        } //In Wiki
        public static void Private(string message, QPlayer Player, Color color)
        {
            Player.TSPlayer.SendMessage(message, color);
        } //In Wiki
        public static void Broadcast(string message, Color color)
        {
            TShock.Utils.Broadcast(message, color);
        } //In Wiki
        public static void StartQuest(string qname, QPlayer Player)
        {
            Player.NewQuest(QTools.GetQuestByName(qname), true);
        }
        public static string ReadNextChatLine(QPlayer Player, bool hide = false)
        {
            Player.AwaitingChat = true;
            Player.HideChat = hide;
            while (Player.AwaitingChat) { }
            Player.HideChat = false;
            return Player.LastChatMessage;
        }
        public static void Kill(string name, QPlayer Player, int amount = 1)
        {
            for (int i = 0; i < amount; i++)
            {
                Player.AwaitingKill = true;
                while (!Player.KillNames.Contains(name)) { Thread.Sleep(1); }
                Player.KillNames.Remove(name);
                Player.AwaitingKill = false;
            }
        } //In Wiki
        public static void KillNpc(int id)
        {
            Main.rand = new Random();
            Main.npc[id].StrikeNPC(99999, 0, 0);
            NetMessage.SendData((int)PacketTypes.NpcStrike, -1, -1, "", id, 99999, 0, 0);
        } //In Wiki
        public static List<int> SpawnMob(string name, int x, int y, int amount = 1)
        {
            List<int> Ids = new List<int>();
            NPC npc = TShock.Utils.GetNPCByName(name)[0];
            for (int i = 0; i < amount; i++)
            {
                int npcid;
                int spawnTileX;
                int spawnTileY;
                TShock.Utils.GetRandomClearTileWithInRange(x, y, 1, 1, out spawnTileX, out spawnTileY);
                npcid = QNPC.NewNPC(spawnTileX * 16, spawnTileY * 16, npc.type, 0);
                Main.npc[npcid].SetDefaults(npc.name);
                Main.npc[npcid].UpdateNPC(npcid);
                Ids.Add(npcid);
            }
            return Ids;
        } //In Wiki
        public static void SetNPCHealth(int id, int health)
        {
            Main.rand = new Random();
            Main.npc[id].life = health;
        } //In Wiki
    }
}