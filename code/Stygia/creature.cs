using System;
using System.Text;
using Stygia.Entity;
using Stygia.Space;
using Stygia.Session;
using libtcod;

namespace Stygia.Actor
{
    [Serializable]
    public class Creature
    {
        public enum CreatureType { None = -1, Humanoid = 0, Undead = 1, Construct = 2, Elemental = 3, Draconic = 4 };
        public enum CreatureFaction { None = -1, PC = 0, Ally = 1, Friendly = 2, Enemy = 3}
        public enum CreatureRace
        {
            None = -1, Human = 0, Kobold = 1, Goblin = 2, Ogre = 3, Trogg = 4,
            Troll = 5, Golem = 6, Vampire = 7, Wight = 8, Giant = 9, Drake = 10, Elemental = 11, 
        };
        public enum CreatureStatus { None = -1, Skip = 0, Alive = 1, Sleeping = 2, Dead = 3 }

        // public enum HumanoidType { None = -1, Warrior = 0, Bruiser = 1, Assassin = 2, Reaper = 3, Stalker = 4 };

        // Default Constructor
        public Creature()
        {
            // Set the internal values
            name = String.Empty;
            body = 0;
            mind = 0;
            finesse = 0;
            faction = CreatureFaction.None;
            type = CreatureType.None;
            Inventory = new int[27];
            for (int i = 1; i <= 26; i++)
            {
                Inventory[i] = -1;
            }
            graphic = ' ';
            colour = new TCODColor(0, 0, 0);
            status = CreatureStatus.None;
            attackDescription = string.Empty;
            speech = string.Empty;
            action = string.Empty;
            currenthp = 0;
            location = new Point(0, 0);
        }

        public Creature(CreatureFaction faction)
        {
            // Set the internal values
            Inventory = new int[27];
            for (int i = 0; i <= 26; i++)
            {
                Inventory[i] = -1;
            }

            if (faction == CreatureFaction.PC)
            {

                name = "You";
                body = 5;
                mind = 5;
                finesse = 5;
                hp = 13;
                currenthp = hp;
                mp = 0;
                faction = CreatureFaction.PC;
                type = CreatureType.Humanoid;
                graphic = '@';
                colour = new TCODColor(0, 0, 0);
                status = CreatureStatus.Alive;
                attackDescription = string.Empty;
                speech = string.Empty;
                action = string.Empty;
                location = new Point(0, 0);
            }
            else
            {
                name = String.Empty;
                body = 0;
                mind = 0;
                finesse = 0;
                faction = CreatureFaction.None;
                type = CreatureType.None;
                graphic = ' ';
                colour = new TCODColor(0, 0, 0);
                status = CreatureStatus.None;
                attackDescription = string.Empty;
                speech = string.Empty;
                action = string.Empty;
                location = new Point(0, 0);
            }
        }

        public Creature(int Level)
        {
            CreatureRace race = (CreatureRace)Level;

            // Set the internal values
            Inventory = new int[27];
            for (int i = 0; i <= 26; i++)
            {
                Inventory[i] = -1;
            }
            location = new Point(0, 0);
            
            body = Convert.ToInt32(race) * 2;
            mind = Convert.ToInt32(race) * 2;
            finesse = Convert.ToInt32(race) * 2;
            status = CreatureStatus.Sleeping;
            colour = new TCODColor(0, 0, 0);
            
            hp = body * 2;
            currenthp = hp;

            faction = CreatureFaction.Enemy;
            
            switch (race)
            {
                case CreatureRace.Kobold:
                    type = CreatureType.Humanoid;
                    colour = TCODColor.brass;
                    graphic = 'k';
                    attackDescription = "swings at";
                    speech = "I'm gonna kill you!  Yip yip!";
                    action = "picks a chunk of rotten meat from its teeth";
                    break;
                case CreatureRace.Goblin:
                    type = CreatureType.Humanoid;
                    colour = TCODColor.grey;
                    graphic = 'g';
                    attackDescription = "stabs at";
                    speech = "This is mine.  All mine!";
                    action = "scratches its bottom";
                    break;
                case CreatureRace.Ogre:
                    type = CreatureType.Humanoid;
                    colour = TCODColor.yellow;
                    graphic = 'o';
                    attackDescription = "swings at";
                    speech = "Me bash you gud!";
                    action = "scratches its head";
                    break;
                case CreatureRace.Trogg:
                    type = CreatureType.Humanoid;
                    colour = TCODColor.purple;
                    graphic = 't';
                    attackDescription = "bites";
                    speech = "Get away from there!";
                    action = "makes a loud woofing noise";
                    break;
                case CreatureRace.Troll:
                    type = CreatureType.Humanoid;
                    colour = TCODColor.grey;
                    graphic = 'T';
                    attackDescription = "claws at";
                    speech = "If you pay, me not kill you.  Den again, me might kill you anyway.  Do you feel lucky?";
                    action = "roars";
                    break;
                case CreatureRace.Golem:
                    type = CreatureType.Construct;
                    colour = TCODColor.blue;
                    graphic = 'G';
                    attackDescription = "mauls";
                    speech = "Never changing.  Never dying.";
                    action = "makea a creaking sound";
                    break;
                case CreatureRace.Vampire:
                    type = CreatureType.Undead;
                    colour = TCODColor.fuchsia;
                    graphic = 'v';
                    attackDescription = "swings at";
                    speech = "I'm going to suck your blud!";
                    action = "twirls its cape";
                    break;
                case CreatureRace.Wight:
                    type = CreatureType.Undead;
                    colour = TCODColor.white;
                    graphic = 'w';
                    attackDescription = "claws at";
                    speech = "Soon you'll be undead too.";
                    action = "leaps through the air at you!";
                    break;
                case CreatureRace.Giant:
                    type = CreatureType.Humanoid;
                    colour = TCODColor.grey;
                    graphic = 'G';
                    attackDescription = "punches";
                    speech = "Me jus' wanna go home now";
                    action = "stomps around, causing the floor to shake";
                    break;
                case CreatureRace.Elemental:
                    type = CreatureType.Elemental;
                    colour = TCODColor.flame;
                    graphic = 'E';
                    attackDescription = "lashes";
                    speech = "The secrets of the Elements are mine!";
                    action = "picks a chunk of rotten meat from its teeth";
                    break;
                case CreatureRace.Drake:
                    type = CreatureType.Draconic;
                    colour = TCODColor.red;
                    graphic = 'D';
                    attackDescription = "bites";
                    speech = "We're the only ones that are going to survive!";
                    action = "arches its back and spreads its wings";
                    break;
                default:
                    break;
            }

            name = Convert.ToString(race);

        }

        public string Value
        {
            get
            {
                StringBuilder buffer = new StringBuilder(name);
                buffer.Append(":");
                buffer.Append(body.ToString());
                buffer.Append(":");
                buffer.Append(mind.ToString());
                buffer.Append(":");
                buffer.Append(finesse.ToString());
                buffer.Append(":");
                buffer.Append(hp.ToString());
                buffer.Append(":");
                buffer.Append(currenthp.ToString());
                buffer.Append(":");
                buffer.Append(mp.ToString());
                buffer.Append(":");
                buffer.Append(graphic.ToString());
                buffer.Append(":");
                buffer.Append(colour.Red.ToString());
                buffer.Append(":");
                buffer.Append(colour.Green.ToString());
                buffer.Append(":");
                buffer.Append(colour.Blue.ToString());
                buffer.Append(":");
                for (int i = 1; i <= 26; i++)
                {
                    buffer.Append(Inventory[i].ToString());
                    buffer.Append(":");
                }
                buffer.Append(faction.ToString());
                buffer.Append(":");
                buffer.Append(type.ToString());
                buffer.Append(":");
                buffer.Append(status.ToString());
                buffer.Append(":");
                buffer.Append(attackDescription.ToString());
                buffer.Append(":");
                buffer.Append(speech.ToString());
                buffer.Append(":");
                buffer.Append(action.ToString());
                buffer.Append(":");
                buffer.Append(location.Value.ToString());
                
                return buffer.ToString();
            }
            set
            {
                int index = 0;
                string[] words = value.Split(':');
                name = words[index++];
                body = Convert.ToInt32(words[index++]);
                mind = Convert.ToInt32(words[index++]);
                finesse = Convert.ToInt32(words[index++]);
                hp = Convert.ToInt32(words[index++]);
                currenthp = Convert.ToInt32(words[index++]);
                mp = Convert.ToInt32(words[index++]);
                graphic = Convert.ToChar(words[index++]);
                colour.Red = Convert.ToByte(words[index++]);
                colour.Green = Convert.ToByte(words[index++]);
                colour.Blue = Convert.ToByte(words[index++]);
                for (int i = 1; i <= 26; i++)
                {
                    Inventory[i] = Convert.ToInt32(words[index++]);
                }
                faction = (CreatureFaction)Enum.Parse(typeof(CreatureFaction), words[index++]);
                type = (CreatureType)Enum.Parse(typeof(CreatureType), words[index++].Trim('\0'));
                status = (CreatureStatus)Enum.Parse(typeof(CreatureStatus), words[index++].Trim('\0'));
                attackDescription = words[index++];
                speech = words[index++];
                action = words[index++];
                location.Value = words[index++];
            }
        }

        public bool IsVowel()
        {
            char firstChar = name.ToLower()[0];
            return firstChar == 'a' || firstChar == 'e' ||
                firstChar == 'i' || firstChar == 'o' ||
                firstChar == 'u';
        }

        // Private data members
        private string name;
        private int body;
        private int mind;
        private int finesse;
        private int hp;
        private int currenthp;
        private int mp;
        private CreatureFaction faction;
        private CreatureType type;
        private char graphic;
        private TCODColor colour;
        private CreatureStatus status;
        private string attackDescription;      
        private string speech;
        private string action;
        private Point location;

        internal Point Location
        {
            get { return location; }
            set { location = value; }
        }

        public string Action
        {
            get { return "The " + name + " " + action; }
            set { action = value; }
        }

        public string AttackDescription
        {
            get { return attackDescription; }
            set { attackDescription = value; }
        }
        public string Speech
        {
            get { return "The " + name + " says '" + speech + "'"; }
            set { speech = value; }
        }

        public int Currenthp
        {
            get { return currenthp; }
            set { currenthp = value; }
        }
             
        // Inventory
        public int[] Inventory;

        // Get first empty slot
        public int FirstEmptySlot()
        {
            int FirstSlot = -1;
            for (int i = 1; i <= 26; i++)
            {
                if (Inventory[i] == -1)
                {
                    FirstSlot = i;
                    break;
                }
            }
            return FirstSlot;
        }

        public int AddItem(int ItemID)
        {
            int SlotToUse = FirstEmptySlot();
            if (SlotToUse != -1)
            {
                Inventory[SlotToUse] = ItemID;
                return SlotToUse;
            }
            else
            {
                return -1;
            }
        }

        public bool TakeDamage(int Damage)
        {
            // handle regen
            currenthp -= Damage;
            if (currenthp > HP)
            {
                currenthp = HP;
            }

            return (currenthp > 0);
        }

        public bool DropItem(int SlotID)
        {
            if (SlotID < 1 && SlotID > 26)
            {
                return false;
            }
            else
            {
                Inventory[SlotID] = -1;
                return true;
            }
        }
     
        // Publically accessible properties
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public int AttackBonus
        {
            get
            {
                if (faction == CreatureFaction.PC)
                {
                    return body;
                }
                else
                {
                    return body;
                }
            }
        }

        public int DefenseBonus
        {
            get
            {
                if (faction == CreatureFaction.PC)
                {
                    return finesse;
                }
                else
                {
                    return finesse;
                }
            }
        }

        public int Body
        {
            get { return body; }
            set { body = value; }
        }

        public int Mind
        {
            get { return mind; }
            set { mind = value; }
        }

        public int Finesse
        {
            get { return finesse; }
            set { finesse = value; }
        }

        public int HP
        {
            get { return hp; }
            set { hp = value; }
        }

        public int MP
        {
            get { return mp; }
            set { hp = value; }
        }

        public CreatureFaction Faction
        {
            get { return faction; }
            set { faction = value; }
        }

        public CreatureType Type
        {
            get { return type; }
            set { type = value; }
        }

        public char Graphic
        {
            get { return graphic; }
            set { graphic = value; }
        }

        public TCODColor Colour
        {
            get { return colour; }
            set { colour = value; }
        }

        public CreatureStatus Status
        {
            get { return status; }
            set { status = value; }
        }
    }
}
