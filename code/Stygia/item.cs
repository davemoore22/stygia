using System;
using System.Text;
using libtcod;

namespace Stygia.Entity
{
    [Serializable]
    public class Item
    {
        public int BaseWeaponDamage = 4;
        public int BaseArmourClass = 3;

        public enum ItemType { None = -1, Weapon = 0, Armour = 1, Potion = 2, Scroll = 3, Wand = 4, Corpse = 5 }
        public enum ItemStatus { None = -1, Ground = 0, Carried = 1, Worn = 2, Wielded = 3 }
        public enum ItemPotionType { None = -1, Healing = 0, Fire = 1, Enlightenment = 2 }
        public enum ItemScrollType { None = -1, Teleport = 0, Fire = 1, EnchantArmor = 2, EnchantWeapon = 3 }
        public enum ItemWandType { None = -1, Healing = 0, FireBall = 1, Torment = 2, Doom = 3, Teleport = 4 }
        public enum ItemQuality { None = -1, Common = 0, Uncommon = 1, Superior = 2, Epic = 3 }
        public enum ItemStatistic { None = -1, Body = 0, Mind = 1, Finesse = 2, Attack = 3, Defense = 4 };
             
        public Item()
        {
            type = ItemType.None;
            name = string.Empty;
            status = ItemStatus.None;
            itemLevel = 0;
            attackBonus = 0;
            defenseBonus = 0;
            bodyBonus = 0;
            mindBonus = 0;
            finesseBonus = 0;
            charges = 0;
            quality = ItemQuality.None;
            potionType = ItemPotionType.None;
            scrollType = ItemScrollType.None;
            wandType = ItemWandType.None;
            itemGraphic = ' ';
            itemColour = new TCODColor(0, 0, 0);
        }

        public Item(TCODRandom rnd, ItemType desiredItemType, ItemQuality desiredItemQuality, int desiredItemLevel, ItemStatus desiredItemStatus)
        {
            // Set defaults
            type = desiredItemType;
            status = desiredItemStatus;
            itemLevel = desiredItemLevel;
            itemLevel = 0;
            attackBonus = 0;
            defenseBonus = 0;
            bodyBonus = 0;
            mindBonus = 0;
            finesseBonus = 0;
            charges = 0;
            quality = desiredItemQuality;
            potionType = ItemPotionType.None;
            scrollType = ItemScrollType.None;
            wandType = ItemWandType.None;
            itemGraphic = ' ';
            itemColour = new TCODColor(0, 0, 0);

            // Generate random numbers in case we need to use it later
            ItemStatistic randomStatistic1 = (ItemStatistic)rnd.getInt(0, 2);
            ItemStatistic randomStatistic2 = (ItemStatistic)rnd.getInt(0, 2);

            StringBuilder nameBuilder = new StringBuilder(string.Empty);

            // Generate items based upon their specified type, quality and level
            switch (desiredItemType)
            {
                case ItemType.Weapon:
                    {
                        int typeOfWeapon = rnd.getInt(1, 6);
                        switch (typeOfWeapon)
                        {
                            case 1:
                              nameBuilder.Append("Longsword ");
                              break;
                            case 2:
                              nameBuilder.Append("Polearm ");
                              break;
                            case 3:
                              nameBuilder.Append("Spear ");
                              break;
                            case 4:
                              nameBuilder.Append("Mace ");
                              break;
                            case 5:
                              nameBuilder.Append("Warhammer ");
                              break;
                            case 6:
                              nameBuilder.Append("Halberd ");
                              break;
                        }
                        itemGraphic = ')';
                        switch (desiredItemQuality)
                        {
                            case ItemQuality.Common:
                                attackBonus = BaseWeaponDamage;
                                nameBuilder.Append("+0");
                                itemColour = TCODColor.lightestGrey; 
                                break;
                            case ItemQuality.Uncommon:
                                attackBonus = BaseWeaponDamage + desiredItemLevel;
                                nameBuilder.Append("+");
                                nameBuilder.Append(desiredItemLevel.ToString());
                                itemColour = TCODColor.green; 
                                break;
                            case ItemQuality.Superior:
                                attackBonus = BaseWeaponDamage + desiredItemLevel;
                                nameBuilder.Append("+");
                                nameBuilder.Append(desiredItemLevel.ToString());
                                itemColour = TCODColor.blue;
                                switch (randomStatistic1)
	                            {
                                    case ItemStatistic.Body:
                                        nameBuilder.Append(" (+");
                                        nameBuilder.Append(desiredItemLevel.ToString());
                                        nameBuilder.Append("B)");
                                        bodyBonus = desiredItemLevel;
                                        break;
                                    case ItemStatistic.Mind:
                                        nameBuilder.Append(" (+");
                                        nameBuilder.Append(desiredItemLevel.ToString());
                                        nameBuilder.Append("M)");
                                        mindBonus = desiredItemLevel;
                                        break;
                                    case ItemStatistic.Finesse:
                                        nameBuilder.Append(" (+");
                                        nameBuilder.Append(desiredItemLevel.ToString());
                                        nameBuilder.Append("F)");
                                        finesseBonus = desiredItemLevel;
                                        break;
	                            }                               
                                break;
                            case ItemQuality.Epic:
                                attackBonus = BaseWeaponDamage + desiredItemLevel;
                                nameBuilder.Append("+");
                                nameBuilder.Append(desiredItemLevel.ToString());
                                itemColour = TCODColor.purple;
                                switch (randomStatistic1)
                                {
                                    case ItemStatistic.Body:
                                        nameBuilder.Append(" (+");
                                        nameBuilder.Append(desiredItemLevel.ToString());
                                        nameBuilder.Append("B)");
                                        bodyBonus = desiredItemLevel;
                                        break;
                                    case ItemStatistic.Mind:
                                        nameBuilder.Append(" (+");
                                        nameBuilder.Append(desiredItemLevel.ToString());
                                        nameBuilder.Append("M)");
                                        mindBonus = desiredItemLevel;
                                        break;
                                    case ItemStatistic.Finesse:
                                        nameBuilder.Append(" (+");
                                        nameBuilder.Append(desiredItemLevel.ToString());
                                        nameBuilder.Append("F)");
                                        finesseBonus = desiredItemLevel;
                                        break;
                                }
                                switch (randomStatistic2)
                                {
                                    case ItemStatistic.Body:
                                        nameBuilder.Append(" (+");
                                        nameBuilder.Append(desiredItemLevel.ToString());
                                        nameBuilder.Append("B)");
                                        bodyBonus += desiredItemLevel;
                                        break;
                                    case ItemStatistic.Mind:
                                        nameBuilder.Append(" (+");
                                        nameBuilder.Append(desiredItemLevel.ToString());
                                        nameBuilder.Append("M)");
                                        mindBonus += desiredItemLevel;
                                        break;
                                    case ItemStatistic.Finesse:
                                        nameBuilder.Append(" (+");
                                        nameBuilder.Append(desiredItemLevel.ToString());
                                        nameBuilder.Append("F)");
                                        finesseBonus += desiredItemLevel;
                                        break;
                                }      
                                break;
                        }
                    }
                    break;
                case ItemType.Armour:
                    {
                        int typeOfArmour = rnd.getInt(1, 4);
                        switch (typeOfArmour)
                        {
                            case 1:
                                nameBuilder.Append("Chainmail ");
                                break;
                            case 2:
                                nameBuilder.Append("Plate Armour ");
                                break;
                            case 3:
                                nameBuilder.Append("Leather Armour ");
                                break;
                            case 4:
                                nameBuilder.Append("Scale Armour ");
                                break;
                        }
                        itemGraphic = '[';
                        switch (desiredItemQuality)
                        {
                            case ItemQuality.Common:
                                defenseBonus = BaseArmourClass;
                                nameBuilder.Append("+0");
                                itemColour = TCODColor.grey;
                                break;
                            case ItemQuality.Uncommon:
                                defenseBonus = BaseArmourClass + desiredItemLevel;
                                nameBuilder.Append("+");
                                nameBuilder.Append(desiredItemLevel.ToString());
                                itemColour = TCODColor.green;
                                break;
                            case ItemQuality.Superior:
                                defenseBonus = BaseArmourClass + desiredItemLevel;
                                nameBuilder.Append("+");
                                nameBuilder.Append(desiredItemLevel.ToString());
                                itemColour = TCODColor.blue;
                                switch (randomStatistic1)
	                            {
                                    case ItemStatistic.Body:
                                        nameBuilder.Append(" (+");
                                        nameBuilder.Append(desiredItemLevel.ToString());
                                        nameBuilder.Append("B)");
                                        bodyBonus = desiredItemLevel;
                                        break;
                                    case ItemStatistic.Mind:
                                        nameBuilder.Append(" (+");
                                        nameBuilder.Append(desiredItemLevel.ToString());
                                        nameBuilder.Append("M)");
                                        mindBonus = desiredItemLevel;
                                        break;
                                    case ItemStatistic.Finesse:
                                        nameBuilder.Append(" (+");
                                        nameBuilder.Append(desiredItemLevel.ToString());
                                        nameBuilder.Append("F)");
                                        finesseBonus = desiredItemLevel;
                                        break;
	                            }                               
                                break;
                            case ItemQuality.Epic:
                                defenseBonus = BaseArmourClass + desiredItemLevel;
                                nameBuilder.Append("+");
                                nameBuilder.Append(desiredItemLevel.ToString());
                                itemColour = TCODColor.purple;
                                switch (randomStatistic1)
                                {
                                    case ItemStatistic.Body:
                                        nameBuilder.Append(" (+");
                                        nameBuilder.Append(desiredItemLevel.ToString());
                                        nameBuilder.Append("B)");
                                        bodyBonus = desiredItemLevel;
                                        break;
                                    case ItemStatistic.Mind:
                                        nameBuilder.Append(" (+");
                                        nameBuilder.Append(desiredItemLevel.ToString());
                                        nameBuilder.Append("M)");
                                        mindBonus = desiredItemLevel;
                                        break;
                                    case ItemStatistic.Finesse:
                                        nameBuilder.Append(" (+");
                                        nameBuilder.Append(desiredItemLevel.ToString());
                                        nameBuilder.Append("F)");
                                        finesseBonus = desiredItemLevel;
                                        break;
                                }
                                switch (randomStatistic2)
                                {
                                    case ItemStatistic.Body:
                                        nameBuilder.Append(" (+");
                                        nameBuilder.Append(desiredItemLevel.ToString());
                                        nameBuilder.Append("B)");
                                        bodyBonus += desiredItemLevel;
                                        break;
                                    case ItemStatistic.Mind:
                                        nameBuilder.Append(" (+");
                                        nameBuilder.Append(desiredItemLevel.ToString());
                                        nameBuilder.Append("M)");
                                        mindBonus += desiredItemLevel;
                                        break;
                                    case ItemStatistic.Finesse:
                                        nameBuilder.Append(" (+");
                                        nameBuilder.Append(desiredItemLevel.ToString());
                                        nameBuilder.Append("F)");
                                        finesseBonus += desiredItemLevel;
                                        break;
                                }      
                                break;
                        }
                    }
                    break;
                case ItemType.Potion:
                    nameBuilder.Append("Potion of ");
                    itemGraphic = '!';
                    ItemPotionType randomPotionType = (ItemPotionType)rnd.getInt(0, 2);
                    potionType = randomPotionType;
                    nameBuilder.Append(randomPotionType.ToString());
                    itemColour = TCODColor.white;
                    break;
                case ItemType.Scroll:
                    nameBuilder.Append("Scroll of ");
                    itemGraphic = '?';
                    ItemScrollType randomScrollType = (ItemScrollType)rnd.getInt(0, 3);
                    scrollType = randomScrollType;
                    nameBuilder.Append(randomScrollType.ToString());
                    itemColour = TCODColor.white;
                    break;
                case ItemType.Wand:
                    nameBuilder.Append("Wand of ");
                    itemGraphic = '/';
                    ItemWandType randomWandType = (ItemWandType)rnd.getInt(0, 4);
                    wandType = randomWandType;
                    nameBuilder.Append(randomWandType.ToString());
                    charges = rnd.getInt(10, 20);
                    itemColour = TCODColor.brass;
                    break;
                case ItemType.Corpse:
                    nameBuilder.Append("frozen corpse");
                    itemGraphic = '%';
                    itemColour = TCODColor.grey;
                    break;
            }

            name = nameBuilder.ToString().TrimEnd();
        }
  
        // Private data members
        private ItemType type;
        private String name;
        private ItemStatus status;
        private int itemLevel;
        private int attackBonus;     
        private int defenseBonus;
        private int bodyBonus;
        private int mindBonus;
        private int finesseBonus;
        private int charges;
        private ItemQuality quality;
        private ItemPotionType potionType;       
        private ItemScrollType scrollType;
        private ItemWandType wandType;
        private char itemGraphic;
        private TCODColor itemColour;

        public string Value
        {
            get
            {
                StringBuilder buffer = new StringBuilder(type.ToString());
                buffer.Append(":");
                buffer.Append(name);
                buffer.Append(":");
                buffer.Append(status.ToString());
                buffer.Append(":");
                buffer.Append(itemLevel.ToString());
                buffer.Append(":");
                buffer.Append(attackBonus.ToString());
                buffer.Append(":");
                buffer.Append(defenseBonus.ToString());
                buffer.Append(":");
                buffer.Append(bodyBonus.ToString());
                buffer.Append(":");
                buffer.Append(mindBonus.ToString());
                buffer.Append(":");
                buffer.Append(finesseBonus.ToString());
                buffer.Append(":");
                buffer.Append(charges.ToString());
                buffer.Append(":");
                buffer.Append(quality.ToString());
                buffer.Append(":");
                buffer.Append(potionType.ToString());
                buffer.Append(":");
                buffer.Append(scrollType.ToString());
                buffer.Append(":");
                buffer.Append(wandType.ToString());
                buffer.Append(":");
                buffer.Append(itemGraphic.ToString());
                buffer.Append(":");
                buffer.Append(itemColour.Red.ToString());
                buffer.Append(":");
                buffer.Append(itemColour.Blue.ToString());
                buffer.Append(":");
                buffer.Append(itemColour.Green.ToString());
              
                return buffer.ToString();
            }
            set
            {
                int index = 0;
                string[] words = value.Split(':');
                type = (ItemType)Enum.Parse(typeof(ItemType), words[index++]);
                name = words[index++];
                status = (ItemStatus)Enum.Parse(typeof(ItemStatus), words[index++]);
                itemLevel = Convert.ToInt32(words[index++]);
                attackBonus = Convert.ToInt32(words[index++]);
                defenseBonus = Convert.ToInt32(words[index++]);
                bodyBonus = Convert.ToInt32(words[index++]);
                mindBonus = Convert.ToInt32(words[index++]);
                finesseBonus = Convert.ToInt32(words[index++]);
                charges = Convert.ToInt32(words[index++]);
                quality = (ItemQuality)Enum.Parse(typeof(ItemQuality), words[index++]);
                potionType = (ItemPotionType)Enum.Parse(typeof(ItemPotionType), words[index++]);
                scrollType = (ItemScrollType)Enum.Parse(typeof(ItemScrollType), words[index++]);
                wandType = (ItemWandType)Enum.Parse(typeof(ItemWandType), words[index++]);
                itemGraphic = Convert.ToChar(words[index++]);
                itemColour.Red = Convert.ToByte(words[index++]);
                itemColour.Green = Convert.ToByte(words[index++]);
                itemColour.Blue = Convert.ToByte(words[index++]);
            }
        }

        public bool IsVowel()
        {
            char firstChar = name.ToLower()[0];
            return firstChar == 'a' || firstChar == 'e' ||
                firstChar == 'i' || firstChar == 'o' ||
                firstChar == 'u';
        }

        public string Details()
        {
            StringBuilder buffer = new StringBuilder(string.Empty);
            if (IsVowel())
            {
                buffer.Append("An ");
                buffer.Append(Name);
            }
            else
            {
                buffer.Append("A ");
                buffer.Append(Name);
            }
            buffer.Append("=");
            buffer.Append("=");
            switch (Type)
            {
                case ItemType.Weapon:
                    buffer.Append("A robust weapon, enough to make a sizable dent in your foes.");
                    buffer.Append("=");
                    buffer.Append("It is of ");
                    buffer.Append(quality.ToString());
                    buffer.Append(" quality.");
                    buffer.Append("=");
                    buffer.Append("=");
                    buffer.Append("It provides the following bonus to attack:");
                    buffer.Append("=");
                    buffer.Append("Attack Bonus: ");
                    buffer.Append(attackBonus.ToString());
                    buffer.Append("=");
                    buffer.Append("=");
                    if (bodyBonus > 0) 
                    { 
                        buffer.Append("Wearing it also increases your Body by ");
                        buffer.Append(bodyBonus.ToString());
                        buffer.Append("=");
                    }
                    if (mindBonus > 0) 
                    { 
                        buffer.Append("Wearing it also increases your Mind by ");
                        buffer.Append(mindBonus.ToString());
                        buffer.Append("=");
                    }
                    if (finesseBonus > 0) 
                    { 
                        buffer.Append("Wearing it also increases your Finesse by ");
                        buffer.Append(finesseBonus.ToString());
                        buffer.Append("=");
                    }
                    break;
                case ItemType.Armour:
                    buffer.Append("A full set of solid armor, enough to protect your entire body.");
                    buffer.Append("=");
                    buffer.Append("It is of ");
                    buffer.Append(quality.ToString());
                    buffer.Append(" quality.");
                    buffer.Append("=");
                    buffer.Append("=");
                    buffer.Append("It provides the following protection:");
                    buffer.Append("=");
                    buffer.Append("Defense Bonus: ");
                    buffer.Append(defenseBonus.ToString());
                    buffer.Append("=");
                    buffer.Append("=");
                    if (bodyBonus > 0) 
                    { 
                        buffer.Append("Wearing it also increases your Body by ");
                        buffer.Append(bodyBonus.ToString());
                        buffer.Append("=");
                    }
                    if (mindBonus > 0) 
                    { 
                        buffer.Append("Wearing it also increases your Mind by ");
                        buffer.Append(mindBonus.ToString());
                        buffer.Append("=");
                    }
                    if (finesseBonus > 0) 
                    { 
                        buffer.Append("Wearing it also increases your Finesse by ");
                        buffer.Append(finesseBonus.ToString());
                        buffer.Append("=");
                    }
                    break;
                case ItemType.Potion:
                    buffer.Append("A stoppered glass vial containing a glowing liquid.");
                    buffer.Append("=");
                    buffer.Append("Drinking it will result in a"); 
                    buffer.Append(PotionType.ToString());
                    buffer.Append(" spell being cast upon you.");
                    buffer.Append("=");
                    buffer.Append("=");
                    buffer.Append("There is only enough liquid for one dose.");
                    break;
                case ItemType.Scroll:
                    buffer.Append("A piece of paper containing the");
                    buffer.Append(ScrollType.ToString());
                    buffer.Append(" spell written on it in glyphs.");
                    buffer.Append("=");
                    buffer.Append("This item can only be used once, and then the glyphs will magically disappear.");
                    break;
                case ItemType.Wand:
                    buffer.Append("A wooden wand containing the ");
                    buffer.Append(WandType.ToString());
                    buffer.Append(" spell.");
                    buffer.Append("=");
                    buffer.Append("It currently has ");
                    buffer.Append(charges.ToString());
                    buffer.Append(" stored.");
                    break;
                case ItemType.Corpse:
                    buffer.Append("A frozen corpse of a long-dead traveller.");
                    break;
            }
            return buffer.ToString();
        }

        // Publically accessible properties
        public ItemType Type
        {
            get { return type; }
            set { type = value; }
        }

        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        public ItemStatus Status
        {
            get { return status; }
            set { status = value; }
        }

        public int ItemLevel
        {
            get { return itemLevel; }
            set { itemLevel = value; }
        }

        public int AttackBonus
        {
            get { return attackBonus; }
            set { attackBonus = value; }
        }

        public int DefenseBonus
        {
            get { return defenseBonus; }
            set { defenseBonus = value; }
        }

        public int BodyBonus
        {
            get { return bodyBonus; }
            set { bodyBonus = value; }
        }

        public int MindBonus
        {
            get { return mindBonus; }
            set { mindBonus = value; }
        }

        public int FinesseBonus
        {
            get { return finesseBonus; }
            set { finesseBonus = value; }
        }

        public int Charges
        {
            get { return charges; }
            set { charges = value; }
        }

        public ItemQuality Quality
        {
            get { return quality; }
            set { quality = value; }
        }

        public ItemPotionType PotionType
        {
            get { return potionType; }
            set { potionType = value; }
        }

        public ItemScrollType ScrollType
        {
            get { return scrollType; }
            set { scrollType = value; }
        }

        public ItemWandType WandType
        {
            get { return wandType; }
            set { wandType = value; }
        }

        public char ItemGraphic
        {
            get { return itemGraphic; }
            set { itemGraphic = value; }
        }

        public TCODColor ItemColour
        {
            get { return itemColour; }
            set { itemColour = value; }
        }        
    }
}