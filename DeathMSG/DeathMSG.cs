using System;
using System.IO;
using Fougerite;
using Fougerite.Events;
using UnityEngine;

namespace DeathMSG
{
    public class DeathMSG : Fougerite.Module
    {
        public IniParser Config;
        public IniParser Bodies;
        public IniParser Range;

        public string DeathMSGName;
        public string Bullet;
        public string animal;
        public string suicide;
        public string sleeper;
        public string huntingbow;
        public string banmsg;
        public string tpamsg;
        public string spike;
        public string explosion;
        public string bleeding;
        public string tpbackmsg;

        public const string red = "[color #FF0000]";
        public const string green = "[color #009900]";

        public int KillLog;
        public int ean;
        public int esn;
        public int autoban;
        public int essn;
        public int dkl;
        public int EnableConsoleKills;

        public System.IO.StreamWriter file;

        public override string Name
        {
            get { return "DeathMSG"; }
        }

        public override string Author
        {
            get { return "DreTaX"; }
        }

        public override string Description
        {
            get { return "DeathMSG"; }
        }

        public override Version Version
        {
            get { return new Version("1.2"); }
        }

        public override void Initialize()
        {
            if (!File.Exists(Path.Combine(ModuleFolder, "KillLog.log")))
            {
                File.Create(Path.Combine(ModuleFolder, "KillLog.log")).Dispose();
            }
            LoadConfig();
            Hooks.OnPlayerKilled += OnPlayerKilled;
            Hooks.OnPlayerSpawned += OnPlayerSpawned;
            Hooks.OnCommand += OnCommand;
        }

        public override void DeInitialize()
        {
            Hooks.OnPlayerKilled -= OnPlayerKilled;
            Hooks.OnCommand -= OnCommand;
            Hooks.OnPlayerSpawned -= OnPlayerSpawned;
        }

        public void OnPlayerSpawned(Fougerite.Player player, SpawnEvent se)
        {
            if (player != null)
            {
                if (DataStore.GetInstance().Get("tpfriendautoban", player.UID) != null) { DataStore.GetInstance().Remove("tpfriendautoban", player.UID);}
                if (DataStore.GetInstance().Get("homesystemautoban", player.UID) != null) { DataStore.GetInstance().Remove("homesystemautoban", player.UID);}
                if (DataStore.GetInstance().Get("DeathMSGBAN", player.UID) != null)
                {
                    string get = (string) DataStore.GetInstance().Get("DeathMSGBAN", player.UID);
                    Vector3 loc = Util.GetUtil().ConvertStringToVector3(get);
                    player.TeleportTo(loc);
                    player.MessageFrom(DeathMSGName, green + tpbackmsg);
                    DataStore.GetInstance().Remove("DeathMSGBAN", player.UID);
                }
            }
        }

        public void Log(string killer, string weapon, string distance, string victim, 
            string bodypart, string damage, int tp, string loca, string locv, string avg = "Not Available")
        {
            string line = DateTime.Now + " Killer: " + killer + " Gun: " + weapon + " Dist: " + distance + " Victim: " +
                          victim + " BodyP: " + bodypart + " DMG: " + damage + " LocA: " + loca + " LocV: " + locv
                          + " " + avg;
            if (tp == 1)
            {
                line = line + " WAS TELEPORTING";
            }
            file = new System.IO.StreamWriter(Path.Combine(ModuleFolder, "KillLog.log"), true);
            file.WriteLine(line);
            file.Close();
        }

        public void LoadConfig()
        {
            try
            {

                Config = new IniParser(Path.Combine(ModuleFolder, "Settings.ini"));
                Range = new IniParser(Path.Combine(ModuleFolder, "Range.ini"));
                Bodies = new IniParser(Path.Combine(ModuleFolder, "Bodies.ini"));
                DeathMSGName = Config.GetSetting("Settings", "DeathMSGName");
                Bullet = Config.GetSetting("Settings", "msg");
                KillLog = int.Parse(Config.GetSetting("Settings", "killog"));
                ean = int.Parse(Config.GetSetting("Settings", "enableanimalmsg"));
                animal = Config.GetSetting("Settings", "animalkill");
                esn = int.Parse(Config.GetSetting("Settings", "enablesuicidemsg"));
                suicide = Config.GetSetting("Settings", "suicide");
                autoban = int.Parse(Config.GetSetting("Settings", "autoban"));
                essn = int.Parse(Config.GetSetting("Settings", "enablesleepermsg"));
                dkl = int.Parse(Config.GetSetting("Settings", "displaykilllog"));
                sleeper = Config.GetSetting("Settings", "SleeperKill");
                huntingbow = Config.GetSetting("Settings", "huntingbow");
                banmsg = Config.GetSetting("Settings", "banmsg");
                tpamsg = Config.GetSetting("Settings", "TpaMsg");
                spike = Config.GetSetting("Settings", "spike");
                explosion = Config.GetSetting("Settings", "explosionmsg");
                bleeding = Config.GetSetting("Settings", "bmsg");
                tpbackmsg = Config.GetSetting("Settings", "tpbackmsg");
                EnableConsoleKills = int.Parse(Config.GetSetting("Settings", "EnableConsoleKills"));
            }
            catch (Exception ex)
            {
                Logger.LogError("[DeathMSG] Failed to read config! " + ex);
            }
        }

        public void OnCommand(Fougerite.Player player, string cmd, string[] args)
        {
            if (cmd == "flushdeathmsg")
            {
                if (player.Admin)
                {
                    DataStore.GetInstance().Flush("DeathMSGAVG");
                    DataStore.GetInstance().Flush("DeathMSGAVG2");
                    player.MessageFrom(DeathMSGName, "Flushed!");
                }
            }
            else if (cmd == "deathmsgrd")
            {
                if (player.Admin)
                {
                    LoadConfig();
                    player.MessageFrom(DeathMSGName, "Reloaded!");
                }
            }
        }

        public int RangeOf(string weapon)
        {
            var data = Range.GetSetting("range", weapon);
            if (data == null)
            {
                return -1;
            }
            return int.Parse(data);
        }

        public void OnPlayerKilled(DeathEvent de)
        {
            if (de.DamageType != null && de.Attacker != null && de.Victim != null &&
                (de.AttackerIsPlayer || de.AttackerIsNPC || de.AttackerIsEntity))
            {
                Fougerite.Player victim = (Fougerite.Player) de.Victim;
                string victimname = victim.Name;
                if (de.AttackerIsNPC)
                {
                    string killername = ((NPC) de.Attacker).Name;
                    if (ean == 1)
                    {
                        string a = animal;
                        a = a.Replace("victim", victimname);
                        a = a.Replace("killer", killername);
                        Server.GetServer().BroadcastFrom(DeathMSGName, a);
                        if (EnableConsoleKills == 1)
                        {
                            Logger.Log("[Death] " + a);
                        }
                    }
                }
                else if (de.AttackerIsPlayer)
                {
                    Fougerite.Player attacker = (Fougerite.Player)de.Attacker;
                    ulong vid = victim.UID;
                    ulong aid = attacker.UID;
                    if (vid == aid && esn == 1)
                    {
                        string n = suicide;
                        n = n.Replace("victim", victimname);
                        Server.GetServer().BroadcastFrom(DeathMSGName, n);
                        if (EnableConsoleKills == 1)
                        {
                            Logger.Log("[Death] " + n);
                        }
                        return;
                    }
                    string weapon = de.WeaponName;
                    string bodyPart = Bodies.GetSetting("bodyparts", de.DamageEvent.bodyPart.ToString());
                    Vector3 killerloc = attacker.Location;
                    Vector3 location = victim.Location;
                    double distance = Math.Round(Vector3.Distance(killerloc, location));
                    double damage = Math.Round(de.DamageAmount);
                    string bleed = de.DamageType;
                    if (bleed == "Bullet")
                    {
                        string message;
                        if (de.Sleeper)
                        {
                            if (essn == 0)
                            {
                                return;
                            }
                            message = sleeper;
                        }
                        else
                        {
                            message = Bullet;
                        }
                        string n = message.Replace("victim", victimname);
                        n = n.Replace("killer", attacker.Name);
                        n = n.Replace("weapon", weapon);
                        n = n.Replace("damage", damage.ToString());
                        n = n.Replace("number", distance.ToString());
                        n = n.Replace("bodyPart", bodyPart);
                        int c = 0;
                        string calc = "Not Available";
                        if (bodyPart == "Head")
                        {
                            if (DataStore.GetInstance().Get("DeathMSGAVG", attacker.UID) == null)
                            {
                                DataStore.GetInstance().Add("DeathMSGAVG", attacker.UID, 1);
                                c = 1;
                            }
                            else
                            {
                                c = (int) DataStore.GetInstance().Get("DeathMSGAVG", attacker.UID) + 1;
                                DataStore.GetInstance().Add("DeathMSGAVG", attacker.UID, c);
                            }
                        }
                        else
                        {
                            if (DataStore.GetInstance().Get("DeathMSGAVG2", attacker.UID) == null)
                            {
                                DataStore.GetInstance().Add("DeathMSGAVG2", attacker.UID, 1);
                                c = 1;
                            }
                            else
                            {
                                c = (int)DataStore.GetInstance().Get("DeathMSGAVG2", attacker.UID) + 1;
                                DataStore.GetInstance().Add("DeathMSGAVG2", attacker.UID, c);
                            }

                        }
                        if (c >= 5)
                        {
                            object cd = DataStore.GetInstance().Get("DeathMSGAVG2", attacker.UID);
                            if (cd != null)
                            {
                                int cd2 = (int) cd;
                                double cc = c / cd2;
                                calc = Math.Round(cc).ToString();
                                n = n + " (HAvg: " + calc + "% )";
                            }
                        }
                        Server.GetServer().BroadcastFrom(DeathMSGName, n);
                        if (EnableConsoleKills == 1)
                        {
                            Logger.Log("[Death] " + n);
                        }
                        if (autoban == 1)
                        {
                            int range = RangeOf(weapon);
                            if (range == -1 || weapon.ToLower().Contains("spike"))
                            {
                                return;
                            }
                            if (distance > range)
                            {
                                var tpfriendteleport = DataStore.GetInstance().Get("tpfriendautoban", aid);
                                var hometeleport = DataStore.GetInstance().Get("homesystemautoban", aid);
                                if (hometeleport == null || string.IsNullOrEmpty((string)hometeleport)
                                    || (string)hometeleport == "none"
                                    || tpfriendteleport == null || string.IsNullOrEmpty((string)tpfriendteleport)
                                    || (string)tpfriendteleport == "none")
                                {
                                    string z = banmsg;
                                    z = z.Replace("killer", attacker.Name);
                                    if (distance >= 1000)
                                    {
                                        return;
                                    }
                                    if (KillLog == 1)
                                    {
                                        Log(attacker.Name, weapon, distance.ToString(), victimname, bodyPart,
                                            damage.ToString(),
                                            0, killerloc.ToString(), location.ToString(), calc);
                                    }
                                    DataStore.GetInstance().Add("DeathMSGBAN", vid, location);
                                    Server.GetServer().BroadcastFrom(DeathMSGName, red + z);
                                    Server.GetServer()
                                        .BanPlayer(attacker, "Console", "Range Ban: " + distance + " Gun: " +
                                                                        weapon);
                                }
                                else
                                {
                                    string t = tpamsg;
                                    t = t.Replace("killer", attacker.Name);
                                    if (distance >= 1000)
                                    {
                                        return;
                                    }
                                    if (KillLog == 1)
                                    {
                                        Log(attacker.Name, weapon, distance.ToString(), victimname, bodyPart,
                                            damage.ToString(),
                                            1, killerloc.ToString(), location.ToString(), calc);
                                    }
                                    Server.GetServer().BroadcastFrom(DeathMSGName, t);
                                    DataStore.GetInstance().Remove("tpfriendautoban", aid);
                                    DataStore.GetInstance().Remove("homesystemautoban", aid);
                                }
                                return;
                            }
                        }
                        if (KillLog == 1)
                        {
                            Log(attacker.Name, weapon, distance.ToString(), victimname, bodyPart,
                                damage.ToString(),
                                0, killerloc.ToString(), location.ToString(), calc);
                        }
                    }
                    else if (bleed == "Melee")
                    {
                        string hn = huntingbow;
                        if (weapon == "Hunting Bow")
                        {
                            if (de.Sleeper && essn == 1)
                            {
                                hn = sleeper;
                            }
                            else
                            {
                                return;
                            }
                            hn = hn.Replace("victim", victimname);
                            hn = hn.Replace("killer", attacker.Name);
                            hn = hn.Replace("damage", damage.ToString());
                            hn = hn.Replace("number", distance.ToString());
                            hn = hn.Replace("bodyPart", bodyPart);
                            Server.GetServer().BroadcastFrom(DeathMSGName, hn);
                            if (EnableConsoleKills == 1)
                            {
                                Logger.Log("[Death] " + hn);
                            }
                            if (autoban == 1)
                            {
                                int range = RangeOf(weapon);
                                if (range == -1 || weapon.ToLower().Contains("spike"))
                                {
                                    return;
                                }
                                if (distance > range)
                                {
                                    var tpfriendteleport = DataStore.GetInstance().Get("tpfriendautoban", aid);
                                    var hometeleport = DataStore.GetInstance().Get("homesystemautoban", aid);
                                    if (hometeleport == null || string.IsNullOrEmpty((string)hometeleport)
                                    || (string)hometeleport == "none"
                                    || tpfriendteleport == null || string.IsNullOrEmpty((string)tpfriendteleport)
                                    || (string)tpfriendteleport == "none")
                                    {
                                        string z = banmsg;
                                        z = z.Replace("killer", attacker.Name);
                                        if (distance >= 1000)
                                        {
                                            return;
                                        }
                                        if (KillLog == 1)
                                        {
                                            Log(attacker.Name, weapon, distance.ToString(), victimname, bodyPart,
                                                damage.ToString(),
                                                0, killerloc.ToString(), location.ToString());
                                        }
                                        DataStore.GetInstance().Add("DeathMSGBAN", vid, location);
                                        Server.GetServer().BroadcastFrom(DeathMSGName, red + z);
                                        Server.GetServer()
                                            .BanPlayer(attacker, "Console", "Range Ban: " + distance + " Gun: " +
                                                                            weapon);
                                    }
                                    else
                                    {
                                        string t = tpamsg;
                                        t = t.Replace("killer", attacker.Name);
                                        if (distance >= 1000)
                                        {
                                            return;
                                        }
                                        if (KillLog == 1)
                                        {
                                            Log(attacker.Name, weapon, distance.ToString(), victimname, bodyPart,
                                                damage.ToString(),
                                                1, killerloc.ToString(), location.ToString());
                                        }
                                        Server.GetServer().BroadcastFrom(DeathMSGName, t);
                                        DataStore.GetInstance().Remove("tpfriendautoban", aid);
                                        DataStore.GetInstance().Remove("homesystemautoban", aid);
                                    }
                                    return;
                                }
                            }
                            if (KillLog == 1)
                            {
                                Log(attacker.Name, weapon, distance.ToString(), victimname, bodyPart,
                                    damage.ToString(),
                                    0, killerloc.ToString(), location.ToString());
                            }
                        }
                        else if (weapon == "Spike Wall" && de.AttackerIsEntity)
                        {
                            string ownername = de.Entity.OwnerName;
                            string s = spike;
                            s = s.Replace("victim", victimname);
                            s = s.Replace("killer", ownername);
                            s = s.Replace("weapon", "Spike Wall");
                            Server.GetServer().BroadcastFrom(DeathMSGName, s);
                            if (EnableConsoleKills == 1)
                            {
                                Logger.Log("[Death] " + s);
                            }
                        }
                        else if (weapon == "Large Spike Wall" && de.AttackerIsEntity)
                        {
                            string ownername = de.Entity.OwnerName;
                            string s = spike;
                            s = s.Replace("victim", victimname);
                            s = s.Replace("killer", ownername);
                            s = s.Replace("weapon", "Spike Wall");
                            Server.GetServer().BroadcastFrom(DeathMSGName, s);
                            if (EnableConsoleKills == 1)
                            {
                                Logger.Log("[Death] " + s);
                            }
                        }
                        else
                        {
                            string n = Bullet;
                            n = n.Replace("victim", victimname);
                            n = n.Replace("killer", attacker.Name);
                            n = n.Replace("weapon", weapon);
                            n = n.Replace("damage", damage.ToString());
                            n = n.Replace("number", distance.ToString());
                            n = n.Replace("bodyPart", bodyPart);
                            Server.GetServer().BroadcastFrom(DeathMSGName, n);
                            if (EnableConsoleKills == 1)
                            {
                                Logger.Log("[Death] " + n);
                            }
                        }
                    }
                    else if (bleed == "Explosion")
                    {
                        string x = explosion;
                        x = x.Replace("killer", attacker.Name);
                        x = x.Replace("victim", victimname);
                        if (weapon == "F1 Grenade")
                        {
                            x = x.Replace("weapon", "F1 Grenade");
                        }
                        else if (weapon == "Explosive Charge")
                        {
                            x = x.Replace("weapon", "C4");
                        }
                        Server.GetServer().BroadcastFrom(DeathMSGName, x);
                        if (EnableConsoleKills == 1)
                        {
                            Logger.Log("[Death] " + x);
                        }
                    }
                    else if (bleed == "Bleeding")
                    {
                        string n = bleeding;
                        n = n.Replace("victim", victimname);
                        n = n.Replace("killer", attacker.Name);
                        Server.GetServer().BroadcastFrom(DeathMSGName, n);
                        if (EnableConsoleKills == 1)
                        {
                            Logger.Log("[Death] " + n);
                        }
                    }
                }
            }
        }
    }
}
