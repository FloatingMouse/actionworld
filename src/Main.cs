    using BepInEx;
    using UnityEngine;
    using RWCustom;
using Menu.Remix.MixedUI;

using System.Collections.Generic;
    using System.Security;
    using System.Security.Permissions;
using System;
using MoreSlugcats;
using Random = UnityEngine.Random;
using Action_World;

namespace Action_World
{
    [BepInPlugin("mills888.actionworld", "Action World", "1.3.0")]
    public class Main : BaseUnityPlugin
    {
        Room lastRoom;
        int Timer;
        public int MinEnemies = 1;
        private bool initialized;
        private OptionsMenu1 optionsMenuInstance;
        public int MaxEnemiesPerPipe = 1;
        public static Main Instance;


        public bool InCombat = false;
        public Music.MusicPlayer musicPlayer;
        List<ShortcutData> Dens;
        public List<CreatureTemplate.Type> Enemies = new List<CreatureTemplate.Type>
            {
            };
        public List<CreatureTemplate.Type> Helpfuls = new List<CreatureTemplate.Type> {
                CreatureTemplate.Type.TubeWorm
            };
        public List<AbstractPhysicalObject.AbstractObjectType> Items = new List<AbstractPhysicalObject.AbstractObjectType>{
               AbstractPhysicalObject.AbstractObjectType.Spear,
               AbstractPhysicalObject.AbstractObjectType.ScavengerBomb,
               AbstractPhysicalObject.AbstractObjectType.BlinkingFlower,
               AbstractPhysicalObject.AbstractObjectType.Mushroom,
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


            orig.Invoke(self);
            bool flag2 = this.initialized;
            if (!flag2)
            {


                this.initialized = true;
                this.optionsMenuInstance = new OptionsMenu1(this);

                optionsMenuInstance = new OptionsMenu1(this);
                try
                {
                    MachineConnector.SetRegisteredOI("mills888.actionworld", this.optionsMenuInstance);
                }
                catch (Exception ex)
                {
                    Debug.Log($"Remix Menu Template examples: Hook_OnModsInit options failed init error {optionsMenuInstance}{ex}");
                    Logger.LogError(ex);
                    Logger.LogMessage("WHOOPS");
                }
            }
            /*vanila creature decipher XD
 * 1 : pink lizard
 * 2 : greenlizard
 * 3 : blue lizard
 * 4 : yellow lizard
 * 5 : white lizard
 * 6 : Red Lizard
 * 7 : black lizard
 * 8 : salamander
 * 9 : cyan lizard
 * 10 : vulture(exotic)
 * 11 : cicadaA
 * 12 : cicadaB
 * 13 : DLL
 * 14 : BLL
 * 15 : Red Centi
 * 16 : Big Spider
 * 17 : spitter spider
 * 18 : king vulter
 */


        //    Enemies.Clear();
      


















            orig(self);
            Instance = this;
            On.Player.Update += PlayerUpdateHook;
            On.Player.ctor += PlayerHook;
            On.Creature.SuckedIntoShortCut += EnterHook;
        }
        bool IsInactive(AbstractCreature c, Creature self)
        {

            Enemies.Clear();


            CreatureTemplate.Type Type = c.realizedCreature.Template.type;
            int tindex = 0;
            while (tindex < Enemies.Count)
            {
                if (Enemies[tindex] == Type)
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
                if (!InCombat || self.room.shortcutData(entrancePos).shortCutType != ShortcutData.Type.RoomExit)
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



        void spearReward(Player self, WorldCoordinate Pos)
        {
            Color explodeColor = new Color(1f, 0.4f, 0.3f);
            Vector2 vector = new Vector2(Pos.x, Pos.y);


            self.room.AddObject(new Explosion.ExplosionLight(vector, 280f, 1f, 7, explodeColor));
            self.room.AddObject(new Explosion.ExplosionLight(vector, 230f, 1f, 3, new Color(1f, 1f, 1f)));
            self.room.AddObject(new ExplosionSpikes(self.room, vector, 14, 30f, 9f, 7f, 170f, explodeColor));
            //self.room.AddObject(new ShockWave(vector, 330f, 0.045f, 5, false));

            AbstractSpear abstractSpear = new AbstractSpear(self.room.world, null, Pos, self.room.game.GetNewID(), false);
             self.room.abstractRoom.AddEntity(abstractSpear);
            abstractSpear.pos = Pos;
            abstractSpear.RealizeInRoom();

        }

        void KarmaReweard(Player self, WorldCoordinate Pos)
        {
            Color explodeColor = new Color(1f, 0.4f, 0.3f);
            Vector2 vector = new Vector2(Pos.x, Pos.y);


            self.room.AddObject(new Explosion.ExplosionLight(vector, 280f, 1f, 7, explodeColor));
            self.room.AddObject(new Explosion.ExplosionLight(vector, 230f, 1f, 3, new Color(1f, 1f, 1f)));
            self.room.AddObject(new ExplosionSpikes(self.room, vector, 14, 30f, 9f, 7f, 170f, explodeColor));
            self.room.AddObject(new ShockWave(vector, 330f, 0.045f, 5, false));

            AbstractPhysicalObject abstractObject = new AbstractConsumable(self.room.world, AbstractPhysicalObject.AbstractObjectType.KarmaFlower, null, Pos, self.room.game.GetNewID(), -1, -1, null);

            abstractObject.RealizeInRoom();


        }

        void FoodReward(Player self, WorldCoordinate Pos)
        {
            Color explodeColor = new Color(1f, 0.4f, 0.3f);
            Vector2 vector = new Vector2(Pos.x, Pos.y);


            self.room.AddObject(new Explosion.ExplosionLight(vector, 280f, 1f, 7, explodeColor));
            self.room.AddObject(new Explosion.ExplosionLight(vector, 230f, 1f, 3, new Color(1f, 1f, 1f)));
            self.room.AddObject(new ExplosionSpikes(self.room, vector, 14, 30f, 9f, 7f, 170f, explodeColor));
            self.room.AddObject(new ShockWave(vector, 330f, 0.045f, 5, false));

            AbstractPhysicalObject abstractObject = new AbstractConsumable(self.room.world, AbstractPhysicalObject.AbstractObjectType.DangleFruit, null, Pos, self.room.game.GetNewID(), -1, -1, null);

            abstractObject.RealizeInRoom();

        }





        void reward(Player self)
        {

            IntVector2 randomTile = GetRandomFreeTile(self.room, self);
            WorldCoordinate Pos = new WorldCoordinate(self.room.abstractRoom.index, randomTile.x, randomTile.y, -1);
          //  self.room.AddObject(new ShockWave(new Vector2(randomTile.x, randomTile.y), 100f, 0.07f, 6, false));
          if(optionsMenuInstance.Spear.Value)
            {
                spearReward(self, Pos);

            }
          if(optionsMenuInstance.Kama.Value)
            {
                KarmaReweard(self, Pos);

            }
          if(optionsMenuInstance.Food.Value)
            {
                FoodReward(self, Pos);
            }





        }



        bool checkInvalideTile(Player self, IntVector2 randomTiles)
        {
            bool check = self.room.Tiles[randomTiles.x, randomTiles.y].Solid;
            return check;
        }

        void Invade(Player self)
        {
            int Count = Random.Range(MinEnemies, (MaxEnemiesPerPipe * Dens.Count) + 1);
            int index = 0;
            while (index < Count)
            {
                if (Random.Range(0, 8) == 0)
                {
                    reward(self);

                    if (Random.Range(0, 2) == 0 && Helpfuls.Count > 0)
                    {
                        IntVector2 randomTile = GetRandomFreeTile(self.room, self);
                        if (!checkInvalideTile(self, randomTile))
                        {
                            WorldCoordinate Pos = new WorldCoordinate(self.room.abstractRoom.index, randomTile.x, randomTile.y, -1);
                            AbstractCreature abstractCreature = new AbstractCreature(self.room.world, StaticWorld.GetCreatureTemplate(Helpfuls[Random.Range(0, Helpfuls.Count)]), null, Pos, self.room.game.GetNewID());
                            abstractCreature.RealizeInRoom();
                            self.room.abstractRoom.AddEntity(abstractCreature);
                        }
                    }
                    else if (Items.Count > 0)
                    {
                        IntVector2 randomTile = GetRandomFreeTile(self.room, self);
                        if (!checkInvalideTile(self, randomTile))
                        {
                            WorldCoordinate Pos = new WorldCoordinate(self.room.abstractRoom.index, randomTile.x, randomTile.y, -1);
                            
                            AbstractPhysicalObject abstractObject = new AbstractPhysicalObject(self.room.world, Items[Random.Range(0, Items.Count)], null, Pos, self.room.game.GetNewID());
                            abstractObject.RealizeInRoom();
                        }
                    }

                }
                
                else if (Enemies.Count > 0)
                {
                    if (Dens.Count > 0)
                    {
                        ShortcutData den = Dens[Random.Range(0, Dens.Count)];
                        WorldCoordinate Pos = den.startCoord;
                        AbstractCreature abstractCreature = new AbstractCreature(self.room.world, StaticWorld.GetCreatureTemplate(Enemies[Random.Range(0, Enemies.Count)]), null, Pos, self.room.game.GetNewID());
                        abstractCreature.RealizeInRoom();
                        self.room.abstractRoom.AddEntity(abstractCreature);

                    }
                }
                index++;
            }
        }

        // Custom method to find a random free tile in the room
        IntVector2 GetRandomFreeTile(Room room, Player self)
        {
            return self.room.RandomTile();
        }

        void PlayerHook(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            lastRoom = self.room;

        }
        void PlayerUpdateHook(On.Player.orig_Update orig, Player self, bool eu)
        {
            eu = true;
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
                            while (index < self.room.shortcuts.Length)
                            {
                                if (self.room.shortcuts[index].shortCutType.Equals(ShortcutData.Type.DeadEnd) | self.room.shortcuts[index].shortCutType.Equals(ShortcutData.Type.CreatureHole) | self.room.shortcuts[index].shortCutType.Equals(ShortcutData.Type.NPCTransportation))
                                {
                                    Dens.Add(self.room.shortcuts[index]);
                                }
                                index++;
                            }
                            if (Dens.Count > 0)
                            {
                              Invade(self);
                        reward(self);
                            }
                            lastRoom = self.room;
                        }
                        else if (self.room.shelterDoor != null)
                        {
                            lastRoom = self.room;
                        }
                        /* if (((self.grasps[1] == null && (!(self.grasps[0] != null && self.grasps[0].grabbed is Spear))) || (self.grasps[0] == null && (!(self.grasps[1] != null && self.grasps[1].grabbed is Spear)))) && Random.Range(0, 100) == 1)
                         {
                           //  AbstractSpear ent = new AbstractSpear(self.room.world, null, self.abstractCreature.pos, self.room.game.GetNewID(), Random.Range(0, 10) == 0);
                           //  ent.RealizeInRoom();
                             //self.PickupPressed();
                         }*/

                        musicPlayer = self.room.world.game.rainWorld.processManager.musicPlayer;
                        if (musicPlayer.song == null)
                        {
                            musicPlayer.RequestArenaSong(SongList[Random.Range(0, 10)], 0f);
                        }
                        //vanilla
                        if (optionsMenuInstance.PinkLizard.Value)
                        {
                            Enemies.Add(CreatureTemplate.Type.PinkLizard);
                        }
                        if (optionsMenuInstance.GreenLizard.Value)
                        {
                            Enemies.Add(CreatureTemplate.Type.GreenLizard);
                        }
                        if (optionsMenuInstance.BlueLizard.Value)
                        {
                            Enemies.Add(CreatureTemplate.Type.BlueLizard);
                        }
                        if (optionsMenuInstance.YellowLizard.Value)
                        {
                            Enemies.Add(CreatureTemplate.Type.YellowLizard);
                        }
                        if (optionsMenuInstance.WhiteLizard.Value)
                        {
                            Enemies.Add(CreatureTemplate.Type.WhiteLizard);
                        }
                        if (optionsMenuInstance.RedLizard.Value)
                        {
                            Enemies.Add(CreatureTemplate.Type.RedLizard);
                        }

                        //break

                        if (optionsMenuInstance.BlackLizard.Value)
                        {
                            Enemies.Add(CreatureTemplate.Type.BlackLizard);
                        }
                        if (optionsMenuInstance.Salamander.Value)
                        {
                            Enemies.Add(CreatureTemplate.Type.Salamander);
                        }
                        if (optionsMenuInstance.CyanLizard.Value)
                        {
                            Enemies.Add(CreatureTemplate.Type.CyanLizard);
                        }
                        if (optionsMenuInstance.Spider.Value)
                        {
                            Enemies.Add(CreatureTemplate.Type.Spider);
                        }
                        if (optionsMenuInstance.DropBug.Value)
                        {
                            Enemies.Add(CreatureTemplate.Type.DropBug);
                        }

                        //break

                        if (optionsMenuInstance.Centiwing.Value)
                        {
                            Enemies.Add(CreatureTemplate.Type.Centiwing);
                        }
                        if (optionsMenuInstance.DLL.Value)
                        {
                            Enemies.Add(CreatureTemplate.Type.DaddyLongLegs);
                        }
                        if (optionsMenuInstance.BLL.Value)
                        {
                            Enemies.Add(CreatureTemplate.Type.BrotherLongLegs);
                        }
                        if (optionsMenuInstance.RedCenti.Value)
                        {
                            Enemies.Add(CreatureTemplate.Type.RedCentipede);
                        }
                        if (optionsMenuInstance.BigSpider.Value)
                        {
                            Enemies.Add(CreatureTemplate.Type.BigSpider);
                        }
                        if (optionsMenuInstance.SpitterSpider.Value)
                        {
                            Enemies.Add(CreatureTemplate.Type.SpitterSpider);
                        }
                        if (optionsMenuInstance.Centipede.Value)
                        {
                            Enemies.Add(CreatureTemplate.Type.Centipede);
                        }
                /*
                 *         public readonly Configurable<bool> SpitLizard;
public readonly Configurable<bool> EelLizard;
public readonly Configurable<bool> MotherSpider;
public readonly Configurable<bool> TLL;
public readonly Configurable<bool> AquaCenti;
public readonly Configurable<bool> ZoopLizard;
public readonly Configurable<bool> TrainLizard;
public readonly Configurable<bool> HLL;*/

                //msc
                if (optionsMenuInstance.SpitLizard.Value)
                {
                    Enemies.Add(MoreSlugcatsEnums.CreatureTemplateType.SpitLizard);
                }
                if (optionsMenuInstance.MotherSpider.Value)
                {
                    Enemies.Add(MoreSlugcatsEnums.CreatureTemplateType.MotherSpider);
                }
                if (optionsMenuInstance.TLL.Value)
                {
                    Enemies.Add(MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs);
                }
                if (optionsMenuInstance.AquaCenti.Value)
                {
                    Enemies.Add(MoreSlugcatsEnums.CreatureTemplateType.AquaCenti);
                }
                if (optionsMenuInstance.ZoopLizard.Value)
                {
                    Enemies.Add(MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard);
                }   
                if (optionsMenuInstance.TrainLizard.Value)
                {
                    Enemies.Add(MoreSlugcatsEnums.CreatureTemplateType.TrainLizard);
                }
                if (optionsMenuInstance.HLL.Value)
                {
                    Enemies.Add(MoreSlugcatsEnums.CreatureTemplateType.HunterDaddy);
                }
                if (optionsMenuInstance.EelLizard.Value)
                {
                    Enemies.Add(MoreSlugcatsEnums.CreatureTemplateType.EelLizard);
                }
            }




    //}
    //catch (NullReferenceException ex)
    //{
      //  Debug.LogError($"NullReferenceException in PlayerUpdateHook: {ex}");

    //}

}
}





public class OptionsMenu1 : OptionInterface
{
//  public static Configurable<bool> gourmandFlightActive;
public OptionsMenu1(Main plugin)
{
    //vanila
    PinkLizard = this.config.Bind<bool>("PinkLizardConfig", true);
    GreenLizard = this.config.Bind<bool>("GreenLizardConfig", true);
    BlueLizard = this.config.Bind<bool>("BlueLizardConfig", true);
    YellowLizard = this.config.Bind<bool>("YellowLizardConfig", true);
    WhiteLizard = this.config.Bind<bool>("WhiteLizardConfig", false);
    RedLizard = this.config.Bind<bool>("RedLizardConfig", false);
    BlackLizard = this.config.Bind<bool>("BlackLizardConfig", false);
    Salamander = this.config.Bind<bool>("SalamanderConfig", false);
    CyanLizard = this.config.Bind<bool>("CyanLizardConfig", true);
    Spider = this.config.Bind<bool>("SpiderConfig", false);
    DropBug = this.config.Bind<bool>("DropBugonfig", true);
    Centiwing = this.config.Bind<bool>("CentiwingConfig", true);
    DLL = this.config.Bind<bool>("DLLConfig", false);
    BLL = this.config.Bind<bool>("BLLConfig", false);
    RedCenti = this.config.Bind<bool>("RedCentiConfig", false);
    BigSpider = this.config.Bind<bool>("BigSpiderConfig", false);
    SpitterSpider = this.config.Bind<bool>("SpitterSpiderConfig", false);
    Centipede = this.config.Bind<bool>("CentipedeConfig", false);
    //msc
    SpitLizard = this.config.Bind<bool>("SpitLizardConfig", false);
    EelLizard = this.config.Bind<bool>("EelLizardConfig", false);
    MotherSpider = this.config.Bind<bool>("MotherSpiderConfig", false);
    TLL = this.config.Bind<bool>("TLLConfig", false);
    AquaCenti = this.config.Bind<bool>("AquaCentiConfig", false);
    ZoopLizard = this.config.Bind<bool>("ZoopLizardConfig", false);
    TrainLizard = this.config.Bind<bool>("TrainLizardConfig", false);
    HLL = this.config.Bind<bool>("HLLConig", false);
    //reward
    Kama = this.config.Bind<bool>("KamaConfig", false);
    Food = this.config.Bind<bool>("FoodConfig", false);
    Spear = this.config.Bind<bool>("SpearConfig", false);


    //rewards
    // Food = this.config.Bind<bool>("FoodConfig", true);
    //   Karma = this.config.Bind<bool>("KarmaConfig", true);

}
public override void Initialize()
{
    var opTab1 = new OpTab(this, "Default Canvas");
    this.Tabs = new[] { opTab1 }; // Add the tabs into your list of tabs. If there is only a single tab, it will not show the flap on the side because there is not need to.

    // Tab 1
    OpContainer tab1Container = new OpContainer(new Vector2(0, 0));
    opTab1.AddItems(tab1Container);
    // You can put sprites with effects in the Remix Menu by using an OpContainer

    UIelement[] UIArrayElements = new UIelement[] // Labels in a fixed box size + alignment
    {
    new OpCheckBox(PinkLizard, 25f, 500f),
    new OpLabel(25f, 550f, "Pink Lizard"),

    new OpCheckBox(GreenLizard, 100f, 500f),
    new OpLabel(100f, 550f, "Green Lizard"),

    new OpCheckBox(BlueLizard, 175f, 500f),
    new OpLabel(175f, 550f, "Blue Lizard"),

    new OpCheckBox(YellowLizard, 250f, 500f),
    new OpLabel(250f, 550f, "Yellow Lizard"),

    new OpCheckBox(WhiteLizard, 325f, 500f),
    new OpLabel(325f, 550f, "White Lizard"),

    new OpCheckBox(RedLizard, 400f, 500f),
    new OpLabel(400f, 550f, "Red Lizard"),

    //break

    new OpCheckBox(BlackLizard, 25f, 400f),
    new OpLabel(25f, 450f, "Black Lizard"),

    new OpCheckBox(Salamander, 100f, 400f),
    new OpLabel(100f, 450f, "Salamander"),

    new OpCheckBox(CyanLizard, 175f, 400f),
    new OpLabel(175f, 450f, "Cyan Lizard"),

    new OpCheckBox(Spider, 250f, 400f),
    new OpLabel(250f, 450f, "Vulture"),

    new OpCheckBox(DropBug, 325f, 400f),
    new OpLabel(325f, 450f, "Drop Wig"),

    new OpCheckBox(Centiwing, 400f, 400f),
    new OpLabel(400f, 450f, "Cicada B"),

    //break

    new OpCheckBox(DLL, 25f, 300f),
    new OpLabel(25f, 350f, "DLL"),

    new OpCheckBox(BLL, 100f, 300f),
    new OpLabel(100f, 350f, "BLL"),

    new OpCheckBox(RedCenti, 175f, 300f),
    new OpLabel(155f, 350f, "Red Centipede"),

    new OpCheckBox(BigSpider, 250f, 300f),
    new OpLabel(250f, 350f, "Big Spider"),

    new OpCheckBox(SpitterSpider, 325f, 300f),
    new OpLabel(315f, 350f, "Spitter Spider"),

    new OpCheckBox(Centipede, 400f, 300f),
    new OpLabel(400f, 350f, "King Vulture"),

    //break

    new OpLabel(250f, 250f, "Msc Config and easier settings"),

    //break

  new OpCheckBox(SpitLizard, 25, 100f),
    new OpLabel(25, 150f, "Spit lizard"),

    new OpCheckBox(EelLizard, 100, 100f),
    new OpLabel(100, 150f, "Eellizard"),

    new OpCheckBox(MotherSpider, 175, 100f),
    new OpLabel(175, 150f, "Mother Spi"),

    new OpCheckBox(TLL, 250, 100f),
    new OpLabel(250, 150f, "TLL"),

    new OpCheckBox(AquaCenti, 325, 100f),
    new OpLabel(325, 150f, "Aqua centi"),

    new OpCheckBox(ZoopLizard, 400, 100f),
    new OpLabel(400f, 150f, "Zoop lizard"),

    new OpCheckBox(TrainLizard, 25, 10f),
    new OpLabel(25, 50f, "Train lizard"),

    new OpCheckBox(HLL, 100, 10f),
    new OpLabel(100, 50f, "HLL"),

    //rewards not new line break XD

    new OpCheckBox(Kama, 165, 10f),
    new OpLabel(135, 50f, "Karmic reward"),

    new OpCheckBox(Food, 250, 10f),
    new OpLabel(250, 50f, "food reward"),

    new OpCheckBox(Spear, 325, 10f),
    new OpLabel(325, 50f, "SPEARS"),




//        new OpLabel(250f, 250f, "rewards"),

//          new OpCheckBox(Karma, 125f, 150f),
//            new OpLabel(125f, 200f, "karmic reward"),

  //  new OpCheckBox(Food, 375f, 150f),
    //new OpLabel(375f, 200f, "Food reward"),



    };
    opTab1.AddItems(UIArrayElements);

    /*  UIelement[] UIArrayElements2 = new UIelement[] //create an array of ui elements
      {
          //new OpSlider(testFloatSlider, new Vector2(50, 400), 100){max = 100, hideLabel = false}, // Using "hideLabel = true" makes the number disappear but the shadow of where the number would be still appears, why lol.

      };*/
            }

        // Configurable values. They are bound to the config in constructor, and then passed to UI elements.
        // They will contain values set in the menu. And to fetch them in your code use their NAME.Value. For example to get the boolean testCheckBox.Value, to get the integer testSlider.Value
        //public readonly Configurable<TYPE> NAME;
        //vanila creature list
        public readonly Configurable<bool> PinkLizard;
        public readonly Configurable<bool> GreenLizard;
        public readonly Configurable<bool> BlueLizard;
        public readonly Configurable<bool> YellowLizard;
        public readonly Configurable<bool> WhiteLizard;
        public readonly Configurable<bool> RedLizard;
        public readonly Configurable<bool> BlackLizard;
        public readonly Configurable<bool> Salamander;
        public readonly Configurable<bool> CyanLizard;
        public readonly Configurable<bool> Spider;
        public readonly Configurable<bool> DropBug;
        public readonly Configurable<bool> Centiwing;
        public readonly Configurable<bool> DLL;
        public readonly Configurable<bool> BLL;
        public readonly Configurable<bool> RedCenti;
        public readonly Configurable<bool> BigSpider;
        public readonly Configurable<bool> SpitterSpider;
        public readonly Configurable<bool> Centipede;

        //MSC
        public readonly Configurable<bool> SpitLizard;
        public readonly Configurable<bool> EelLizard;
        public readonly Configurable<bool> MotherSpider;
        public readonly Configurable<bool> TLL;
        public readonly Configurable<bool> AquaCenti;
        public readonly Configurable<bool> ZoopLizard;
        public readonly Configurable<bool> TrainLizard;
        public readonly Configurable<bool> HLL;


        //reward list
        public readonly Configurable<bool> Kama;
        public readonly Configurable<bool> Food;
        public readonly Configurable<bool> Spear;



        /*  public readonly Configurable<bool> Food;
          public readonly Configurable<bool> Karma;*/
        //MSC list
    }


}



/*vanila creature decipher XD
 * 1 : pink lizard
 * 2 : greenlizard
 * 3 : blue lizard
 * 4 : yellow lizard
 * 5 : white lizard
 * 6 : Red Lizard
 * 7 : black lizard
 * 8 : salamander
 * 9 : cyan lizard
 * 10 : vulture(exotic)
 * 11 : cicadaA
 * 12 : cicadaB
 * 13 : DLL
 * 14 : BLL
 * 15 : Red Centi
 * 16 : Big Spider
 * 17 : spitter spider
 * 18 : king vulter
 */