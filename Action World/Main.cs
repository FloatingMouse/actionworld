using BepInEx;
using UnityEngine;
using RWCustom;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;

[module: UnverifiableCode]
namespace Action_World
{
    [BepInPlugin("formallizard.actionworld", "Action World", "1.3.0")]
    public class Main : BaseUnityPlugin
    {
        Room lastRoom;
        int Timer;
        public int MinEnemies = 1;
        public int MaxEnemiesPerPipe = 1;
        public static Main Instance;
        public bool InCombat;
        public Music.MusicPlayer musicPlayer;
        List<ShortcutData> Dens;
        public List<CreatureTemplate.Type> Enemies = new List<CreatureTemplate.Type>
        {
            CreatureTemplate.Type.CyanLizard,
            CreatureTemplate.Type.PinkLizard,
            CreatureTemplate.Type.GreenLizard,
            CreatureTemplate.Type.YellowLizard,
            CreatureTemplate.Type.DropBug,
            CreatureTemplate.Type.WhiteLizard
        };
        public List<CreatureTemplate.Type> Helpfuls = new List<CreatureTemplate.Type> {
            CreatureTemplate.Type.TubeWorm
        };
        public List<AbstractPhysicalObject.AbstractObjectType> Items = new List<AbstractPhysicalObject.AbstractObjectType>{

        };
        public List<string> SongList = new List<string>
        {
            "RW_32 - Grey Cloud",
            "RW_34 - Slaughter",
            "RW_43 - Bio Engineering",
            "RW_29 - Lovely Arps",
            "RW_13 - Action Scene",
            "RW_10 - Noisy",
            "RW_18 - The Captain",
            "RW_39 - Lack of Comfort",
            "RW_43 - Albino",
            "RW_42 - Kayava"
        };
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += Initiate;
        }
        public void Initiate(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            Instance = this;
            On.Player.Update += PlayerUpdateHook;
            On.Player.ctor += PlayerHook;
            On.Creature.SuckedIntoShortCut += EnterHook;
        }
        bool IsInactive(AbstractCreature c, Creature self)
        {
            CreatureTemplate.Type Type = c.realizedCreature.Template.type;
            int tindex = 0;
            while (tindex<Enemies.Count)
            {
                if (Enemies[tindex]==Type)
                {
                    return c.realizedCreature.dead;
                }
                tindex++;
            }
            return true;
        }
        void EnterHook(On.Creature.orig_SuckedIntoShortCut orig, Creature self, IntVector2 entrancePos, bool carriedByOther)
        {
            if (self.room.game.IsStorySession)
            {
                if (!InCombat | self.room.shortcutData(entrancePos).shortCutType != ShortcutData.Type.RoomExit)
                {
                    orig(self, entrancePos, carriedByOther);
                }
                else
                {
                    self.enteringShortCut = null;
                    self.inShortcut = false;
                    self.Stun(5);
                    if (self.abstractCreature.abstractAI != null)
                    {
                        self.abstractCreature.abstractAI.SetDestination(new WorldCoordinate(self.room.abstractRoom.index, self.room.RandomTile().x, self.room.RandomTile().y, -1));
                    }
                    if (self.abstractCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
                    {
                        self.mainBodyChunk.vel.x += Random.Range(-30, 30);
                        self.mainBodyChunk.vel.y += Random.Range(-30, 30);
                    }
                    else
                    {
                        if (self.room.abstractRoom.creatures.Contains(self.abstractCreature))
                        {
                            self.room.abstractRoom.creatures.Remove(self.abstractCreature);
                        }
                        self.room.RemoveObject(self);
                        self.abstractCreature.Destroy();
                        self.Destroy();
                    }
                    
                    
                }
            }
            else
            {
                orig(self, entrancePos, carriedByOther);
            }
        }
        void Invade(Player self)
        {
            int Count = Random.Range(MinEnemies, (MaxEnemiesPerPipe * Dens.Count)+1);
            int index = 0;
            while (index<Count)
            {
                if (Random.Range(0,8) == 0)
                {
                    if (Random.Range(0, 2) == 0 & Helpfuls.Count>0)
                    {
                        WorldCoordinate Pos = new WorldCoordinate(self.room.abstractRoom.index, self.room.RandomTile().x, self.room.RandomTile().y, -1);
                        if (self.room.Tiles[self.room.RandomTile().x, self.room.RandomTile().y].Solid == false)
                        {
                            AbstractCreature abstractCreature =
                            new AbstractCreature(self.room.world, StaticWorld.GetCreatureTemplate(Helpfuls[Random.Range(0, Helpfuls.Count)]), null, Pos, self.room.game.GetNewID());
                            abstractCreature.RealizeInRoom();
                            self.room.abstractRoom.creatures.Add(abstractCreature);
                        }
                        
                    }
                    else if(Items.Count>0)
                    {
                        WorldCoordinate Pos = new WorldCoordinate(self.room.abstractRoom.index, self.room.RandomTile().x, self.room.RandomTile().y, -1);
                        if (self.room.Tiles[self.room.RandomTile().x, self.room.RandomTile().y].Solid == false)
                        {
                            AbstractPhysicalObject abstractObject =
                            new AbstractPhysicalObject(self.room.world, Items[Random.Range(0, Items.Count)], null, Pos, self.room.game.GetNewID());
                            abstractObject.RealizeInRoom();
                        }
                    }
                    
                }
                else if(Enemies.Count>0)
                {
                    WorldCoordinate Pos = Dens[Random.Range(0, Dens.Count)].startCoord;
                    AbstractCreature abstractCreature =
                    new AbstractCreature(self.room.world, StaticWorld.GetCreatureTemplate(Enemies[Random.Range(0, Enemies.Count)]), null, Pos, self.room.game.GetNewID());
                    abstractCreature.RealizeInRoom();
                    self.room.abstractRoom.creatures.Add(abstractCreature);
                }
                
                index++;
            }
        }
        void PlayerHook(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            lastRoom = null;
        }
        void PlayerUpdateHook(On.Player.orig_Update orig, Player self, bool eu)
        {
            orig(self, eu);
            if (self.room.game.IsStorySession)
            {
                Timer++;
                if (Timer >= 100)
                {
                    Timer = 0;
                    InCombat = false;
                    int index = 0;
                    while (index < self.room.abstractRoom.creatures.Count)
                    {
                        if (!IsInactive(self.room.abstractRoom.creatures[index], self))
                        {
                            InCombat = true;
                        }
                        index++;
                    }
                }
                if (lastRoom != self.room & self.room.shelterDoor == null)
                {
                    int index = 0;
                    Dens = new List<ShortcutData> { };
                    while (index<self.room.shortcuts.Length)
                    {
                        if (self.room.shortcuts[index].shortCutType.Equals(ShortcutData.Type.DeadEnd) | self.room.shortcuts[index].shortCutType.Equals(ShortcutData.Type.CreatureHole) | self.room.shortcuts[index].shortCutType.Equals(ShortcutData.Type.NPCTransportation))
                        {
                            Dens.Add(self.room.shortcuts[index]);
                        }
                        index++;
                    }
                    if (Dens.Count>0)
                    {
                        Invade(self);
                    }
                    lastRoom = self.room;
                }
                else if (self.room.shelterDoor != null)
                {
                    lastRoom = self.room;
                }
                if (((self.grasps[1] == null && (!(self.grasps[0] != null && self.grasps[0].grabbed is Spear))) || (self.grasps[0] == null && (!(self.grasps[1] != null && self.grasps[1].grabbed is Spear)))) && Random.Range(0, 100) == 1)
                {
                    AbstractSpear ent = new AbstractSpear(self.room.world, null, self.abstractCreature.pos, self.room.game.GetNewID(), Random.Range(0, 10) == 0);
                    ent.RealizeInRoom();
                    self.PickupPressed();
                }

                musicPlayer = self.room.world.game.rainWorld.processManager.musicPlayer;
                if (musicPlayer.song == null)
                {
                    musicPlayer.RequestArenaSong(SongList[Random.Range(0, 10)], 0f);
                }
            }
            
        }
    }
    
}
