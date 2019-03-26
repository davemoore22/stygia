using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using Stygia.Space;
using Stygia.Actor;
using Stygia.Entity;
using Stygia.Utils;
using libtcod;

namespace Stygia.Session
{
    public enum MessageType { None = -1, Standard = 0, System = 1, LowPriority = 2, Item = 3, Inventory = 4, Error = 5, Monster = 6 };
    public enum InventoryMode { None = -1, Standard = 0, Drop = 1, Wear = 2, Takeoff = 3, Activate };

    [Serializable] 
    class Message : IDisposable
    {
        public Message()
        {
            text = string.Empty;
            type = MessageType.None;
        }

        public Message(String message, MessageType typeOfMessage)
        {
            // Remember to remove/replace any delimiter characters
            text = message;
            type = typeOfMessage;
        }

        public virtual void Dispose()
        {
            // No need here for any cleanup
        }

        private String text;
        public String Text
        {
            get { return text; }
            set { text = value; }
        }

        private MessageType type;
        public MessageType Type
        {
            get { return type; }
            set { type = value; }
        }
       
    }

    [Serializable] 
    class Game
    {
        // Standard Constructor
        public Game(Window window, bool StartNewGame)
        {
            canQuit = false;

            temperatures = new String[8];
            temperatures[0] = "Temperate";
            temperatures[1] = "Mild";
            temperatures[2] = "Chilly";
            temperatures[3] = "Frosty";
            temperatures[4] = "Cold";
            temperatures[5] = "Freezing";
            temperatures[6] = "Glacial";
            temperatures[7] = "Frozen";

            items = new List<Item>();
            creatures = new List<Creature>();
            random = new TCODRandom(TCODRandomType.MersenneTwister);
           
            if (StartNewGame)
            {
                // Set up a new game
                int yloc = 1;
                ID = System.Guid.NewGuid();
                directionOffSet = new Point[8];
                directionOffSet[(int)CardinalDirection.N] = new Point(0, -1);
                directionOffSet[(int)CardinalDirection.NE] = new Point(1, -1);
                directionOffSet[(int)CardinalDirection.E] = new Point(1, 0);
                directionOffSet[(int)CardinalDirection.SE] = new Point(1, 1);
                directionOffSet[(int)CardinalDirection.S] = new Point(0, 1);
                directionOffSet[(int)CardinalDirection.SW] = new Point(-1, 1);
                directionOffSet[(int)CardinalDirection.W] = new Point(-1, 0);
                directionOffSet[(int)CardinalDirection.NW] = new Point(-1, -1);

                window.DisplayText("Starting new Game: ", 1, yloc, TCODColor.white, TCODColor.black, -1);
                window.DisplayText(ID.ToString(), 20, yloc++, TCODColor.yellow, TCODColor.black, -1);
                window.DisplayText(String.Empty, 1, yloc++, TCODColor.white, TCODColor.black, -1);
                window.DisplayText("Generating Player Information...", 1, yloc++, TCODColor.white, TCODColor.black, -1);
                Turns = 0;
                Level = 1;
                characterLoc = new Point(150, 100);
                Player = new Creature(Creature.CreatureFaction.PC);
                window.DisplayText("Generating Dungeon...", 1, yloc++, TCODColor.white, TCODColor.black, -1);
                mapLevel = new Map(MapType.Cave, 1);
                window.DisplayText("Generating View Information...", 1, yloc++, TCODColor.white, TCODColor.black, -1);
                ViewMap = new TCODMap(300, 200);
                mapLevel.BuildFOV(ViewMap, characterLoc.X, characterLoc.Y);
                viewPort = new MapViewPort(150, 100, 60, 26);
                window.DisplayText("Placing Items...", 1, yloc++, TCODColor.white, TCODColor.black, -1);
                GenerateItemsOnLevel(random, 12, Level);
                window.DisplayText("Placing Creatures...", 1, yloc++, TCODColor.white, TCODColor.black, -1);
                GenerateMonstersOnLevel(random, 12, Level);
                window.ClearScreen();
                RedrawMap(window, characterLoc, mapLevel);
                RedrawStatus(window);
                messages = new List<Message>();
                messages.Clear();
                Message startMessage1 = new Message("Welcome to Stygia.", MessageType.System);
                messages.Add(startMessage1);
                Message startMessage2 = new Message("The Hidden Depths await you...", MessageType.System);
                messages.Add(startMessage2);
                Message help = new Message("Press ? at any time for Help Informatiom or V to see what's new", MessageType.Standard);
                messages.Add(help);
                RedrawMessages(window);
            }
            else
            {
                // Load the old game
                int yloc = 1;
                directionOffSet = new Point[8];
                directionOffSet[(int)CardinalDirection.N] = new Point(0, -1);
                directionOffSet[(int)CardinalDirection.NE] = new Point(1, -1);
                directionOffSet[(int)CardinalDirection.E] = new Point(1, 0);
                directionOffSet[(int)CardinalDirection.SE] = new Point(1, 1);
                directionOffSet[(int)CardinalDirection.S] = new Point(0, 1);
                directionOffSet[(int)CardinalDirection.SW] = new Point(-1, 1);
                directionOffSet[(int)CardinalDirection.W] = new Point(-1, 0);
                directionOffSet[(int)CardinalDirection.NW] = new Point(-1, -1);

                window.DisplayText("Loading current Game: ", 1, yloc, TCODColor.white, TCODColor.black, -1);
                ID = System.Guid.NewGuid();
                mapLevel = new Map(MapType.None, 0);
                ViewMap = new TCODMap(300, 200);
                viewPort = new MapViewPort(0, 0, 0, 0);
                messages = new List<Message>();
                messages.Clear();
                Player = new Creature(Creature.CreatureFaction.PC);
                characterLoc = new Point(0, 0);
                Turns = 0;
                if (Load())
                {
                    window.DisplayText("Game Loaded Successfully...", 1, yloc++, TCODColor.white, TCODColor.black, -1);                  
                    Message loadedMessage = new Message("Loaded Game....", MessageType.System);
                    messages.Add(loadedMessage  );
                    Message reStartMessage = new Message("Welcome back to Stygia...", MessageType.System);
                    messages.Add(reStartMessage);
                    window.ClearScreen();
                    RedrawMap(window, characterLoc, mapLevel);
                    RedrawStatus(window);
                    RedrawMessages(window);
                }
                else
                {
                    window.DisplayText("Cannot load saved game or save game does not exist, aborting...", 1, yloc++, TCODColor.white, TCODColor.black, -1);
                    window.DisplayText("Press any key to return to the Main Menu", 1, 33, TCODColor.grey, TCODColor.black, -1);
                    window.WaitForAnyKeyPress();
                    canQuit = true;
                }
            }
        }

        public bool Start(Window window)
        {
            // Main loop
            Point currentPos = new Point();
            Point attackPos = new Point();
            bool canPassTurn = true;
            char key = ' ';
            do
            {
                currentPos.Set(characterLoc);
                attackPos.Set(characterLoc);
                key = window.WaitForKeyPress(KeyMode.Exploration);
                bool walkedIntoWall = false ;
                switch (key)
                {

                    case 'H': // West
                        attackPos.Add(directionOffSet[(int)CardinalDirection.W]);
                        if (!AttackMonster(window, attackPos, random))
                        {
                            currentPos.Add(directionOffSet[(int)CardinalDirection.W]);
                            if (mapLevel[currentPos.X, currentPos.Y].Walkable)
                            {
                                characterLoc.Add(directionOffSet[(int)CardinalDirection.W]);
                                viewPort.Add(directionOffSet[(int)CardinalDirection.W]);
                            }
                            else { walkedIntoWall = true; }
                        }
                        canPassTurn = true;
                        break;
                    case 'L': // East
                        attackPos.Add(directionOffSet[(int)CardinalDirection.E]);
                        if (!AttackMonster(window, attackPos, random))
                        {
                            currentPos.Add(directionOffSet[(int)CardinalDirection.E]);
                            if (mapLevel[currentPos.X, currentPos.Y].Walkable)
                            {
                                characterLoc.Add(directionOffSet[(int)CardinalDirection.E]);
                                viewPort.Add(directionOffSet[(int)CardinalDirection.E]);
                            }
                            else { walkedIntoWall = true; }
                        }
                        canPassTurn = true;
                        break;
                    case 'K': // North
                        attackPos.Add(directionOffSet[(int)CardinalDirection.N]);
                        if (!AttackMonster(window, attackPos, random))
                        {
                            currentPos.Add(directionOffSet[(int)CardinalDirection.N]);
                            if (mapLevel[currentPos.X, currentPos.Y].Walkable)
                            {
                                characterLoc.Add(directionOffSet[(int)CardinalDirection.N]);
                                viewPort.Add(directionOffSet[(int)CardinalDirection.N]);
                            }
                            else { walkedIntoWall = true; 
                            }
                        }
                        canPassTurn = true;
                        break;
                    case 'J': // South
                        attackPos.Add(directionOffSet[(int)CardinalDirection.S]);
                        if (!AttackMonster(window, attackPos, random))
                        {
                            currentPos.Add(directionOffSet[(int)CardinalDirection.S]);
                            if (mapLevel[currentPos.X, currentPos.Y].Walkable)
                            {
                                characterLoc.Add(directionOffSet[(int)CardinalDirection.S]);
                                viewPort.Add(directionOffSet[(int)CardinalDirection.S]);
                            }
                            else { walkedIntoWall = true; }
                        }
                        canPassTurn = true;
                        break;
                    case 'Y': // NorthWest
                        attackPos.Add(directionOffSet[(int)CardinalDirection.NW]);
                        if (!AttackMonster(window, attackPos, random))
                        {
                            currentPos.Add(directionOffSet[(int)CardinalDirection.NW]);
                            if (mapLevel[currentPos.X, currentPos.Y].Walkable)
                            {
                                characterLoc.Add(directionOffSet[(int)CardinalDirection.NW]);
                                viewPort.Add(directionOffSet[(int)CardinalDirection.NW]);
                            }
                            else { walkedIntoWall = true; }
                        }
                        canPassTurn = true;
                        break;
                    case 'U': // NorthEast
                        attackPos.Add(directionOffSet[(int)CardinalDirection.NE]);
                        if (!AttackMonster(window, attackPos, random))
                        {
                            currentPos.Add(directionOffSet[(int)CardinalDirection.NE]);
                            if (mapLevel[currentPos.X, currentPos.Y].Walkable)
                            {
                                characterLoc.Add(directionOffSet[(int)CardinalDirection.NE]);
                                viewPort.Add(directionOffSet[(int)CardinalDirection.NE]);
                            }
                            else { walkedIntoWall = true; }
                        }
                        canPassTurn = true;
                        break;
                    case 'B': // SouthWest 
                        attackPos.Add(directionOffSet[(int)CardinalDirection.SW]);
                        if (!AttackMonster(window, attackPos, random))
                        {
                            currentPos.Add(directionOffSet[(int)CardinalDirection.SW]);
                            if (mapLevel[currentPos.X, currentPos.Y].Walkable)
                            {
                                characterLoc.Add(directionOffSet[(int)CardinalDirection.SW]);
                                viewPort.Add(directionOffSet[(int)CardinalDirection.SW]);
                            }
                            else { walkedIntoWall = true; }
                        }
                        canPassTurn = true;
                        break;
                     case 'N': // SouthEast
                        attackPos.Add(directionOffSet[(int)CardinalDirection.SE]);
                        if (!AttackMonster(window, attackPos, random))
                        {
                            currentPos.Add(directionOffSet[(int)CardinalDirection.SE]);
                            if (mapLevel[currentPos.X, currentPos.Y].Walkable)
                            {
                                characterLoc.Add(directionOffSet[(int)CardinalDirection.SE]);
                                viewPort.Add(directionOffSet[(int)CardinalDirection.SE]);
                            }
                            else { walkedIntoWall = true; }
                        }
                        canPassTurn = true;
                        break;
                    case 'S': // Save Game
                        Message saveMessage = new Message("Saving Game....", MessageType.System);
                        messages.Add(saveMessage);                                  
                        this.Save();
                        canPassTurn = false;
                        Message savedMessage = new Message("Game saved OK!", MessageType.System);
                        messages.Add(savedMessage);
                        RedrawMessages(window);
                        break;
                    case 'G': // Get Item
                        if (GetAnItem(mapLevel[currentPos.X, currentPos.Y].Item))
                        {
                            mapLevel[currentPos.X, currentPos.Y].Item = -1;
                            canPassTurn = true;
                        }
                        break;
                    case '?': // Display Help
                        DisplayHelp(window);
                        canPassTurn = false;    
                        break;
                    case 'V': // Display Version Information
                        DisplayVersion(window);
                        canPassTurn = false;
                        break;
                    case 'I': // Display Inventory
                        do
                        {
                            DisplayInventory(window, InventoryMode.Standard);
                            key = window.WaitForKeyPress(KeyMode.Inventory);
                            if (key == '#')
                            {
                                // Quit Inventory window
                                canPassTurn = false;
                            }
                            else
                            {
                                // Display item
                                int turnIntoSlot = key - '@';
                                DisplayItemDetails(window, turnIntoSlot);
                                key = window.WaitForKeyPress(KeyMode.ItemDetails);
                                canPassTurn = false;
                            }
                        }
                        while (key != '#');
                        break;
                    case 'X': // Activate an item
                        do
                        {
                            DisplayInventory(window, InventoryMode.Activate);
                            key = window.WaitForKeyPress(KeyMode.Activate);
                            if (key == '#')
                            {
                                // Quit Inventory window
                                canPassTurn = false;
                            }
                            else
                            {
                                // Drop item if possible
                                int turnIntoSlot = key - '@';
                                if (Player.Inventory[turnIntoSlot] > -1)
                                {
                                    bool success = ActivateAnItem(turnIntoSlot);
                                    if (success)
                                    {
                                        // Remove the item from the inventory and destroy it
                                        Item item = items[Player.Inventory[turnIntoSlot]];
                                        item.Status = Item.ItemStatus.None;
                                        Player.Inventory[turnIntoSlot] = -1;
                                    }
                                    canPassTurn = success;
                                    key = '#';
                                }
                                else
                                {
                                    Message nothingToActivate = new Message("Nothing to activate!", MessageType.Error);
                                    messages.Add(nothingToActivate);
                                    canPassTurn = false;
                                    key = '#';
                                }
                            }
                        }
                        while (key != '#');
                        break;
                    case 'D': // Drop an item
                        do
                        {
                            DisplayInventory(window, InventoryMode.Drop);
                            key = window.WaitForKeyPress(KeyMode.Drop);
                            if (key == '#')
                            {
                                // Quit Inventory window
                                canPassTurn = false;
                            }
                            else
                            {
                                // Drop item if possible
                                int turnIntoSlot = key - '@';
                                if (Player.Inventory[turnIntoSlot] > -1)
                                {
                                    canPassTurn = DropAnItem(turnIntoSlot);
                                    key = '#';
                                }
                                else
                                {
                                    Message nothingToDrop = new Message("Nothing to drop!", MessageType.Error);
                                    messages.Add(nothingToDrop);
                                    canPassTurn = false;
                                    key = '#';
                                }
                            }
                        }
                        while (key != '#');
                        break;
                    case 'W': // wield/wear an item
                        do
                        {
                            DisplayInventory(window, InventoryMode.Wear);
                            key = window.WaitForKeyPress(KeyMode.Wear);
                            if (key == '#')
                            {
                                // Quit Inventory window
                                canPassTurn = false;
                            }
                            else
                            {
                                // wear item if possible
                                int turnIntoSlot = key - '@';
                                if (Player.Inventory[turnIntoSlot] > -1)
                                {
                                    canPassTurn = WearAnItem(turnIntoSlot);
                                    key = '#';
                                }
                                else
                                {
                                    Message nothingToDrop = new Message("Nothing to wear!", MessageType.Error);
                                    messages.Add(nothingToDrop);
                                    canPassTurn = false;
                                    key = '#';
                                }
                            }
                        }
                        while (key != '#');
                        break;
                    case 'T': // take off an item
                        do
                        {
                            DisplayInventory(window, InventoryMode.Takeoff);
                            key = window.WaitForKeyPress(KeyMode.Takeoff);
                            if (key == '#')
                            {
                                // Quit Inventory window
                                canPassTurn = false;
                            }
                            else
                            {
                                // wear item if possible
                                int turnIntoSlot = key - '@';
                                if (Player.Inventory[turnIntoSlot] > -1)
                                {
                                    canPassTurn = TakeOffAnItem(turnIntoSlot);
                                    key = '#';
                                }
                                else
                                {
                                    Message nothingToDrop = new Message("Nothing to take off!", MessageType.Error);
                                    messages.Add(nothingToDrop);
                                    canPassTurn = false;
                                    key = '#';
                                }
                            }
                        }
                        while (key != '#');
                        break;
                    case '>': // Go down stairs
                        if (mapLevel.Cells[characterLoc.X, characterLoc.Y].Terrain != TerrainType.StairsDown)
                        {
                            Message noStairs = new Message("You cannot do that here", MessageType.Error);
                            messages.Add(noStairs);
                        }
                        else
                        {
                            GoDownALevel(Level++);
                        }
                        canPassTurn = false;
                        break;
                    case '.': // Wait one turn
                        Message wait = new Message("You wait for a bit.", MessageType.LowPriority);
                        messages.Add(wait);
                        PassATurn(window, true, random);                       
                        break;
                    case 'Q': // Quit
                        Message saveQuitMessage = new Message("Saving Game....", MessageType.System);
                        messages.Add(saveQuitMessage);                                  
                        this.Save();
                        Message savedQuitMessage = new Message("Game saved OK!", MessageType.System);
                        messages.Add(savedQuitMessage);
                        canQuit = true;
                        canPassTurn = false;
                        break;
                }
                if (walkedIntoWall)
                {
                    Message walkedIntoWallMessage = new Message("This wall is impassable", MessageType.LowPriority);
                    messages.Add(walkedIntoWallMessage);
                }
                PassATurn(window, canPassTurn, random);
            }
            while (!TCODConsole.isWindowClosed() && (!canQuit));
            window.ClearScreen();
            return true;
        }

        public void GoDownALevel(int DungeonLevelToGoDown)
        {
            // Now...clear the current maplevel and recreate it
            Message goDownMessage = new Message("You climb down some stairs deeper into the Earth", MessageType.LowPriority);
            messages.Add(goDownMessage);

            // Get rid of any monsters currently on the level
            for (int x = 0; x < 300; x++)
            {
                for (int y = 0; y < 200; y++)
                {
                    if (mapLevel.Cells[x, y].Creature > -1)
                    {
                        Creature creature = creatures[mapLevel.Cells[x, y].Creature];
                        creature.Status = Creature.CreatureStatus.Skip;
                    }
                }
            }

            Map newMapLevel = new Map(MapType.Cave, DungeonLevelToGoDown);
            mapLevel = newMapLevel;
            characterLoc.Set(150, 100);
            Turns = 0;
            MapViewPort newviewPort = new MapViewPort(150, 100, 60, 26);
            viewPort.Value = newviewPort.Value;
            GenerateItemsOnLevel(random, 12, Level);
            GenerateMonstersOnLevel(random, 12, Level);
            mapLevel.BuildFOV(ViewMap, characterLoc.X, characterLoc.Y);
            Player.Currenthp = HP;
        }

        public void DisplayItemDetails(Window window, int InventorySlot)
        {
            int yloc = 1;
            window.ClearScreen();
            if (Player.Inventory[InventorySlot] == -1 || (InventorySlot < 1 || InventorySlot > 26)) 
            {
                window.DisplayText("You are carrying nothing in this slot", 1, yloc, TCODColor.grey, TCODColor.black, -1);
            }
            else
            {
                Item itemToDisplay = items[Player.Inventory[InventorySlot]];
                string[] words = itemToDisplay.Details().Split('=');
                foreach (string i in words)
                {
                    if (yloc < 3) { window.DisplayText(i, 1, yloc, TCODColor.white, TCODColor.black, -1); }
                    else { window.DisplayText(i, 1, yloc, TCODColor.silver, TCODColor.black, -1); };
                    yloc++;
                }               
            }

            window.DisplayText("Press [ESC] to return", 1, 30, TCODColor.white, TCODColor.black, -1);
        }

        public void DisplayInventory(Window window, InventoryMode mode)
        {
            int yloc = 1;
            char letter = 'a';
            window.ClearScreen();
            window.DisplayText("You are carrying:", 1, yloc, TCODColor.white, TCODColor.black, -1);
            yloc += 2;
            for (int index = 1; index <= 26; index++)
            {
                window.DisplayText("(" + (letter++) + ")", 1, yloc, TCODColor.silver, TCODColor.black, -1);
                if (Player.Inventory[index] != -1)
                {
                    Item item = items[Player.Inventory[index]];
                    window.DisplayText(item.Name, 5, yloc, item.ItemColour, TCODColor.black, -1);
                    if (item.Status == Item.ItemStatus.Wielded)
                    {
                        window.DisplayText("(wielded)", 5 + item.Name.Length + 2, yloc, TCODColor.white, TCODColor.black, -1);
                    }
                    else if (item.Status == Item.ItemStatus.Worn)
                    {
                        window.DisplayText("(worn)", 5 + item.Name.Length + 2, yloc, TCODColor.white, TCODColor.black, -1);
                    }
                }
                else
                {
                    window.DisplayText("(empty)", 5, yloc, TCODColor.darkGrey, TCODColor.black, -1);
                }
                yloc++;
            }
            if (mode == InventoryMode.Standard)
            {
                window.DisplayText("Press [a-z] to examine an item, [ESC] to return", 1, 30, TCODColor.white, TCODColor.black, -1);
            }
            else if (mode == InventoryMode.Drop)
            {
                window.DisplayText("Press [a-z] to drop an item, [ESC] to return", 1, 30, TCODColor.white, TCODColor.black, -1);
            }
            else if (mode == InventoryMode.Takeoff)
            {
                window.DisplayText("Press [a-z] to remove an item, [ESC] to return", 1, 30, TCODColor.white, TCODColor.black, -1);
            }
            else if (mode == InventoryMode.Wear)
            {
                window.DisplayText("Press [a-z] to wear/wield an item, [ESC] to return", 1, 30, TCODColor.white, TCODColor.black, -1);
            }
        }

        // Attempt to get an item
        public bool GetAnItem(int ItemID)
        {
            // If no item present
            if (ItemID == -1)
            {
                Message noItemMessage = new Message("There is nothing here to get", MessageType.Error);
                messages.Add(noItemMessage);
                return false;
            }
            else
            {
                // Try to add the item to the player's inventory
                int SlotAddedTo = Player.AddItem(ItemID);

                // Check to see what slot its been added to
                if (SlotAddedTo == -1)
                {
                    Message noSpaceMessage = new Message("You can't carry any more", MessageType.Error);
                    messages.Add(noSpaceMessage);
                    return false;
                }
                else
                {
                    // Report back success to the player & change the item status
                    Item itemToAdd = items[ItemID];
                    itemToAdd.Status = Item.ItemStatus.Carried;                 
                    StringBuilder outputText = new StringBuilder("You have picked up ");
                    if (itemToAdd.IsVowel()) { outputText.Append("an "); }
                        else { outputText.Append("a "); }
                    outputText.Append(itemToAdd.Name);
                    char slotKey = Convert.ToChar(96 + SlotAddedTo);
                    outputText.Append(" (");
                    outputText.Append(slotKey.ToString());
                    outputText.Append(")");
                    Message addedMessage = new Message(outputText.ToString(), MessageType.Inventory);
                    messages.Add(addedMessage);
                    itemToAdd.Status = Item.ItemStatus.Carried;
                    return true;
                }
            }
        }

        // Attempt to drop an item
        public bool DropAnItem(int SlotID)
        {
            // Check if an item already exists on this tile, if so don't drop
            if (mapLevel[characterLoc.X, characterLoc.Y].Item != -1)
            {
                Message cannotDropItem = new Message("You cannot drop this item here", MessageType.Error);
                messages.Add(cannotDropItem);
                return false;
            }
            else
            {
                // Remove the item from the player's inventory
                int itemToDrop = Player.Inventory[SlotID];
                Player.Inventory[SlotID] = -1;
                mapLevel[characterLoc.X, characterLoc.Y].Item = itemToDrop;

                // Set the item status
                Item item = items[itemToDrop];
                item.Status = Item.ItemStatus.Ground;
                Message dropItem = new Message("You have dropped an item", MessageType.Item);
                messages.Add(dropItem);

                // Indicate success
                return true;
            }                   
        }

        // Attempt to activate an item
        public bool ActivateAnItem(int SlotID)
        {
            // First check if the item selected is of the correct type, scroll or potion
            Item itemToCheck = items[Player.Inventory[SlotID]];
            if (itemToCheck.Type == Item.ItemType.Potion || itemToCheck.Type == Item.ItemType.Scroll)
            {
                string itemEffect = string.Empty;
                if (itemToCheck.Type == Item.ItemType.Potion)
                {
                    // Check the potion type
                    switch (itemToCheck.PotionType)
                    {
                        case Item.ItemPotionType.Healing:
                            // Heals wounds
                            Player.Currenthp = Player.HP;
                            itemEffect = "You drink the potion, and feel much healthier";
                            break;
                        case Item.ItemPotionType.Fire:
                            // Resets the turn count for the level
                            itemEffect = "You drink the potion, and feel warmer";
                            Turns = 0;
                            break;
                        case Item.ItemPotionType.Enlightenment:
                            // Adds +1 to all stats
                            itemEffect = "You drink the potion, and feel more powerful!";
                            Player.Body++;
                            Player.Finesse++;
                            Player.Mind++;
                            break;
                    }

                    Message effectMessage = new Message(itemEffect.ToString(), MessageType.Item);
                    messages.Add(effectMessage);
                    return true;
                }
                else if (itemToCheck.Type == Item.ItemType.Scroll)
                {
                    switch (itemToCheck.ScrollType)
                    {
                        case Item.ItemScrollType.Teleport:
                            // Teleport to a random location on the level
                            Point newLoc = new Point(mapLevel.GetRandomTerrain(TerrainType.CorridorFloor, random));
                            characterLoc.Set(newLoc);
                            // Change viewport
                            MapViewPort newViewPort = new MapViewPort(newLoc.X, newLoc.Y, 60, 26);
                            viewPort = newViewPort;
                            itemEffect = "You read the scroll, and are suddenly teleported elsewhere!";
                            break;
                        case Item.ItemScrollType.Fire:
                            // Resets the turn count for the level
                            itemEffect = "You read the scroll, and feel warmer";
                            Turns = 0;
                            break;
                        case Item.ItemScrollType.EnchantArmor:
                            // Add bonuses to your current weapon, if any
                            int slotIDOfAlreadyWornArmour = GetItemStatusFromInventory(Item.ItemStatus.Worn);
                            if (slotIDOfAlreadyWornArmour == -1)
                            {
                                // Not wearing armour, so alert!
                                Message errorUsingMessage = new Message("You must wear the armour you wish to enchant!", MessageType.Error);
                                messages.Add(errorUsingMessage);
                                return false;
                            }
                            else
                            {
                                itemEffect = "You read the scroll, and your armor glows warmly for a moment!";
                                Item itemToEnchant = items[Player.Inventory[slotIDOfAlreadyWornArmour]];
                                itemToEnchant.DefenseBonus++;
                            }
                            break;
                        case Item.ItemScrollType.EnchantWeapon:
                            // Add bonuses to your current weapon, if any
                            int slotIDOfAlreadyWieldedWeapon = GetItemStatusFromInventory(Item.ItemStatus.Wielded);
                            if (slotIDOfAlreadyWieldedWeapon == -1)
                            {
                                // Not wearing armour, so alert!
                                Message errorUsingMessage = new Message("You must wield the weapon you wish to enchant!", MessageType.Error);
                                messages.Add(errorUsingMessage);
                                return false;
                            }
                            else
                            {
                                itemEffect = "You read the scroll, and your weapoin glows warmly for a moment!";
                                Item itemToEnchant = items[Player.Inventory[slotIDOfAlreadyWieldedWeapon]];
                                itemToEnchant.AttackBonus++;
                            }
                            break;
                    }

                    Message effectMessage = new Message(itemEffect.ToString(), MessageType.Item);
                    messages.Add(effectMessage);
                    return true;
                }
                else return false;
            }
            else
            {
                // Incorrect type of item
                Message cannotWearItem = new Message("This item cannot be used", MessageType.Error);
                messages.Add(cannotWearItem);
                return false;
            }      
        }

        // Wearwield an item
        public bool WearAnItem(int SlotID)
        {
            // First check if the item selected is of the correct type, armour or weapon
            Item itemToCheck = items[Player.Inventory[SlotID]];
            if (itemToCheck.Type == Item.ItemType.Armour || itemToCheck.Type == Item.ItemType.Weapon)
            {
                // Check we're not actually wearing this item already
                if (itemToCheck.Status == Item.ItemStatus.Wielded)
                {
                    // Incorrect type of item
                    Message cannotWearItem = new Message("You're already wearing this!", MessageType.Error);
                    messages.Add(cannotWearItem);
                    return false;
                }
                else if (itemToCheck.Status == Item.ItemStatus.Worn)
                {
                    // Incorrect type of item
                    Message cannotWearItem = new Message("You're already wielding this!", MessageType.Error);
                    messages.Add(cannotWearItem);
                    return false;
                }
                else
                {
                    // Now check if we are already wearing or wielding something, and if so, swap
                    if (itemToCheck.Type == Item.ItemType.Armour)
                    {
                        int slotIDOfAlreadyWornArmour = GetItemStatusFromInventory(Item.ItemStatus.Worn);
                        if (slotIDOfAlreadyWornArmour == -1)
                        {
                            // Not wearing armour, so wear this armour
                            itemToCheck.Status = Item.ItemStatus.Worn;
                            StringBuilder outputText = new StringBuilder("You wear ");
                            if (itemToCheck.IsVowel()) { outputText.Append("an "); }
                            else { outputText.Append("a "); }
                            outputText.Append(itemToCheck.Name);
                            Message wearMessage = new Message(outputText.ToString(), MessageType.Inventory);
                            messages.Add(wearMessage);
                            return true;
                        }
                        else
                        {
                            // Already wearing armour, so swap
                            Item itemToRemove = items[Player.Inventory[slotIDOfAlreadyWornArmour]];
                            StringBuilder outputText = new StringBuilder("You remove ");
                            if (itemToRemove.IsVowel()) { outputText.Append("an "); }
                            else { outputText.Append("a "); }
                            outputText.Append(itemToRemove.Name);
                            Message removeArmourMessage = new Message(outputText.ToString(), MessageType.Inventory);
                            messages.Add(removeArmourMessage);
                            itemToRemove.Status = Item.ItemStatus.Carried;

                            itemToCheck.Status = Item.ItemStatus.Worn;
                            StringBuilder wearText = new StringBuilder("You wear ");
                            if (itemToCheck.IsVowel()) { outputText.Append("an "); }
                            else { wearText.Append("a "); }
                            wearText.Append(itemToCheck.Name);
                            Message wearMessage = new Message(wearText.ToString(), MessageType.Inventory);
                            messages.Add(wearMessage);
                            return true;
                        }
                    }
                    else if (itemToCheck.Type == Item.ItemType.Weapon)
                    {
                        int slotIDOfAlreadyWieldedWeapon = GetItemStatusFromInventory(Item.ItemStatus.Wielded);
                        if (slotIDOfAlreadyWieldedWeapon == -1)
                        {
                            // Not wearing armour, so wear this armour
                            itemToCheck.Status = Item.ItemStatus.Wielded;
                            StringBuilder outputText = new StringBuilder("You wield ");
                            if (itemToCheck.IsVowel()) { outputText.Append("an "); }
                            else { outputText.Append("a "); }
                            outputText.Append(itemToCheck.Name);
                            Message wearMessage = new Message(outputText.ToString(), MessageType.Inventory);
                            messages.Add(wearMessage);
                            return true;
                        }
                        else
                        {
                            // Already wearing armour, so swap
                            Item itemToRemove = items[Player.Inventory[slotIDOfAlreadyWieldedWeapon]];
                            StringBuilder outputText = new StringBuilder("You unwield ");
                            if (itemToRemove.IsVowel()) { outputText.Append("an "); }
                            else { outputText.Append("a "); }
                            outputText.Append(itemToRemove.Name);
                            Message removeArmourMessage = new Message(outputText.ToString(), MessageType.Inventory);
                            messages.Add(removeArmourMessage);
                            itemToRemove.Status = Item.ItemStatus.Carried;

                            itemToCheck.Status = Item.ItemStatus.Wielded;
                            StringBuilder wearText = new StringBuilder("You wield ");
                            if (itemToCheck.IsVowel()) { outputText.Append("an "); }
                            else { wearText.Append("a "); }
                            wearText.Append(itemToCheck.Name);
                            Message wearMessage = new Message(wearText.ToString(), MessageType.Inventory);
                            messages.Add(wearMessage);
                            return true;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                // Incorrect type of item
                Message cannotWearItem = new Message("This item cannot be worn or wielded", MessageType.Error);
                messages.Add(cannotWearItem);
                return false;
            }          
        }

        // Wearwield an item
        public bool TakeOffAnItem(int SlotID)
        {
            // First check if the item selected is of the correct type, armour or weapon
            Item itemToCheck = items[Player.Inventory[SlotID]];
            if (itemToCheck.Type == Item.ItemType.Armour || itemToCheck.Type == Item.ItemType.Weapon)
            {
                // Check we're not actually wearing this item already
                if (itemToCheck.Status != Item.ItemStatus.Wielded && itemToCheck.Status == Item.ItemStatus.Worn)
                {
                    // The item isn't currently worn or wielded
                    Message cannotTakeOffItem = new Message("You're not wearing or wielding this!", MessageType.Error);
                    messages.Add(cannotTakeOffItem);
                    return false;
                }
                else
                {
                    // If we are wearing/wielding an item
                    if (itemToCheck.Type == Item.ItemType.Armour)
                    {
                        // Not wearing armour, so wear this armour
                        itemToCheck.Status = Item.ItemStatus.Carried;
                        StringBuilder outputText = new StringBuilder("You take off ");
                        if (itemToCheck.IsVowel()) { outputText.Append("an "); }
                        else { outputText.Append("a "); }
                        outputText.Append(itemToCheck.Name);
                        Message wearMessage = new Message(outputText.ToString(), MessageType.Inventory);
                        messages.Add(wearMessage);
                        return true;
                    }
                    else if (itemToCheck.Type == Item.ItemType.Weapon)
                    {
                        // Not wearing armour, so wear this armour
                        itemToCheck.Status = Item.ItemStatus.Carried;
                        StringBuilder outputText = new StringBuilder("You unwield ");
                        if (itemToCheck.IsVowel()) { outputText.Append("an "); }
                        else { outputText.Append("a "); }
                        outputText.Append(itemToCheck.Name);
                        Message wearMessage = new Message(outputText.ToString(), MessageType.Inventory);
                        messages.Add(wearMessage);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                // Incorrect type of item
                Message cannotWearItem = new Message("This item cannot be worn or wielded for you to take off!", MessageType.Item);
                messages.Add(cannotWearItem);
                return false;
            }
        }

        // Find item status
        public int GetItemStatusFromInventory(Item.ItemStatus statusToLookFor)
        {
            for (int index = 1; index <= 26; index++)
            {
                if (Player.Inventory[index] != -1)
                {
                    Item item = items[Player.Inventory[index]];
                    if (item.Status == statusToLookFor)
                    {
                        return index;
                    }
                }
            }

            return -1;
        }

        public void PassATurn(Window window, bool canIncrementTurnCount, TCODRandom rnd)
        {
            if (canIncrementTurnCount) { 
                Turns++;
                if (!DoStuff(window, rnd))
                {
                    // Death!
                    DisplayDeath(window);
                    canQuit = true;
                    return;
                }
            }

            // Do health regen, but only on fire squares
            if (Turns % 10 == 0 && mapLevel[characterLoc.X, characterLoc.Y].AmbientLight > 0)
            {
                Player.TakeDamage(-1);
            }
            else if (Turns % 10 == 0 && mapLevel[characterLoc.X, characterLoc.Y].AmbientLight == 0)
            {
                // small chance of losing health due to heat loss
                int chance = rnd.getInt(0, 100);
                if (chance < Convert.ToInt32(Progress * 7))
                {
                    if (!Player.TakeDamage(1))
                    {
                        // Death!
                        DisplayDeath(window);
                        canQuit = true;
                        return;
                    }
                    else
                    {
                        Message coldMessage = new Message("The cold is freezing you! Find heat immediately!", MessageType.Standard);
                        messages.Add(coldMessage);
                    }
                }
            }

            mapLevel.BuildFOV(ViewMap, characterLoc.X, characterLoc.Y);
            DisplayItemText(characterLoc.X, characterLoc.Y, window);
            RedrawMap(window, characterLoc, mapLevel);
            RedrawStatus(window);
            RedrawMessages(window);
        }

        public bool DoStuff(Window window, TCODRandom rnd)
        {
            // Main AI Loop

            // Now, we loop through the monster list, checking for currently alive monsters, and
            // monsters that are on the same level as the PC
            int index = -1;
            foreach (Creature creature in creatures)
            {
                index++;
                if (creature.Status == Creature.CreatureStatus.Alive || creature.Status == Creature.CreatureStatus.Sleeping)
                {                   
                    // Awake sleeping monsters
                    Point creatureLoc = new Point(creature.Location.X, creature.Location.Y);

                    // Just make sure, as a paranoia check that the creature's own record of its
                    // location is consistent with the dungeons:
                    if (mapLevel.Cells[creatureLoc.X, creatureLoc.Y].Creature != index)
                    {
                        creature.Status = Creature.CreatureStatus.Skip;
                    }

                    if (creature.Status == Creature.CreatureStatus.Sleeping)
                    {
                        if (mapLevel.Cells[creatureLoc.X, creatureLoc.Y].Visible == Visiblity.Visible)
                        {
                            creature.Status = Creature.CreatureStatus.Alive;
                            if (creature.IsVowel())
                            {
                                Message newMonsterMessage = new Message("You see an " + creature.Name + "!", MessageType.Monster);
                                messages.Add(newMonsterMessage);
                            }
                            else
                            {
                                Message newMonsterMessage = new Message("You see a " + creature.Name + "!", MessageType.Monster);
                                messages.Add(newMonsterMessage);
                            }
                        }
                    }
                    else if (creature.Status == Creature.CreatureStatus.Alive)
                    {
                        // Now, check the distance between the creature and the player
                        int Distance = characterLoc.Dist(creatureLoc);
                        if (Distance > 1)
                        {
                            // Move towards the player, and have a small chance of saying something
                            int chance = rnd.getInt(0, 10);
                            if (chance == 0)
                            {
                                Message speechMessage = new Message(creature.Speech, MessageType.Standard);
                                messages.Add(speechMessage);
                            }
                            else if (chance == 1)
                            {
                                Message actionMessage = new Message(creature.Action, MessageType.Standard);
                                messages.Add(actionMessage);
                            }

                            // Build a local map for pathfinding
                            TCODMap pathfindingMap = new TCODMap(300, 200);
                            pathfindingMap.clear(false, false);
                            for (int dx = 0; dx < 300; dx++)
                            {                             
                                for (int dy = 0; dy < 200; dy++)
                                {
                                    pathfindingMap.setProperties(dx, dy, mapLevel.Cells[dx, dy].Walkable, mapLevel.Cells[dx, dy].Walkable);
                                    if (mapLevel.Cells[dx, dy].Creature > -1)
                                    {
                                        pathfindingMap.setProperties(dx, dy, false, false);
                                    }
                                }
                            }

                            // Calculate the path
                            TCODPath AStrPath = new TCODPath(pathfindingMap, 1);
                            AStrPath.compute(creatureLoc.X, creatureLoc.Y, characterLoc.X, characterLoc.Y);

                            // Get the location of the next cell if possible
                            if (AStrPath.size() > 0)
                            {
                                int nextX = 0;
                                int nextY = 0;
                                AStrPath.get(0, out nextX, out nextY);
                                Point nextStep = new Point(nextX, nextY);

                                // Now move the monster to this square
                                mapLevel.Cells[nextX, nextY].Creature = mapLevel.Cells[creatureLoc.X, creatureLoc.Y].Creature;
                                mapLevel.Cells[creatureLoc.X, creatureLoc.Y].Creature = -1;
                                creature.Location.X = Convert.ToByte(nextX);
                                creature.Location.Y = Convert.ToByte(nextY);
                            }
                        }
                        else if (Distance == 1)
                        {                                   
                            // Combat!
                            int damageTaken = Attacks(creature, Player, rnd);
                            if (damageTaken > 0)
                            {
                                Message attackSuccessfulMessage = new Message("The " + creature.Name + " " + creature.AttackDescription +
                                    " " + "you and hits for " + damageTaken.ToString() + " points of damage!", MessageType.Monster);
                                messages.Add(attackSuccessfulMessage);
                                if (!Player.TakeDamage(damageTaken))
                                {
                                    // We're dead!
                                    Message deadMessage = new Message("YOU ARE DEAD!", MessageType.Monster);
                                    messages.Add(deadMessage);
                                    return false;
                                }

                            }
                            else
                            {
                                Message attackUnSuccessfulMessage = new Message("The " + creature.Name + " " + creature.AttackDescription +
                                    " " + "you but misses!", MessageType.Monster);
                                messages.Add(attackUnSuccessfulMessage);
                            }
                        }
                        else
                        {
                            // Distance 0, which means a bug has occurred
                            mapLevel.Cells[creatureLoc.X, creatureLoc.Y].Creature = -1;
                            creature.Status = Creature.CreatureStatus.Dead;
                        }
                    }
                }              
            }
            return true;
        }

        // Attack a monster: return true if combat occurs
        public bool AttackMonster(Window window, Point locationToAttack, TCODRandom rnd)
        {
            if (mapLevel[locationToAttack.X, locationToAttack.Y].Creature > -1)
            {
                Creature creatureToAttack = creatures[mapLevel[locationToAttack.X, locationToAttack.Y].Creature];
                int damageCaused = Attacks(Player, creatureToAttack, rnd);
                if (damageCaused == 0)
                {
                    Message missMessage = new Message("You attack the " + creatureToAttack.Name + " but miss!", MessageType.Standard);
                    messages.Add(missMessage);
                }
                else
                {
                    Message hitMessage = new Message("You attack the " + creatureToAttack.Name + " and hit, causing " + damageCaused.ToString() + " points of damage!", MessageType.Standard);
                    messages.Add(hitMessage);

                    if (damageCaused > creatureToAttack.Currenthp)
                    {
                        Message killMessage = new Message("You have killed the " + creatureToAttack.Name + "!", MessageType.Standard);
                        messages.Add(killMessage);
                        mapLevel[locationToAttack.X, locationToAttack.Y].Creature = -1;
                        creatureToAttack.Status = Creature.CreatureStatus.Dead;
                    }
                }

                return true;
            }
            else
            {
                return false;
            }          
        }

        public void RedrawMap(Window window, Point characterLoc, Space.Map map)
        {
            int dx;
            int dy;
            TCODColor torchColour = new TCODColor();
            TCODColor wallColour = new TCODColor();
            int temperatureCategory = Convert.ToInt32(Progress * 7);
            TCODColor temperatureColour = new TCODColor();
            temperatureColour = TCODColor.Interpolate(TCODColor.yellow, TCODColor.darkBlue, Progress);

            window.ClearScreen();
            window.rootConsole.setBackgroundColor(TCODColor.black);          
            for (int x = viewPort.TopLeft.X; x <= viewPort.TopRight.X; x++)
            {
                for (int y = viewPort.TopLeft.Y; y <= viewPort.BottomLeft.Y; y++)
                {
                    dx = (x - viewPort.TopLeft.X) + 1;
                    dy = (y - viewPort.TopLeft.Y);
                    if (map.InBounds(x, y))
                    {
                        //window.DisplayText(characterLoc.X.ToString() + "/" + characterLoc.Y.ToString(), 70, 3, 
                        //    TCODColor.white, TCODColor.black, -1);

                        switch (map.Cells[x, y].Visible)
                        {                                  
                            case Visiblity.Previously:                              
                                window.rootConsole.setForegroundColor(TCODColor.darkGrey);
                                switch (map.Cells[x, y].Terrain)
	                            {
		                            case TerrainType.HardRock:
                                        window.rootConsole.putChar(dx, dy, 177); // 177 // 233
                                        break;
                                    case TerrainType.SoftRock:
                                        window.rootConsole.putChar(dx, dy, 177); // 177 // 233
                                        break;
                                    case TerrainType.RoomFloor:
                                        window.rootConsole.putChar(dx, dy, 250); // 150
                                        break;
                                    case TerrainType.CorridorFloor:
                                        window.rootConsole.putChar(dx, dy, 250); // 150
                                        break;
                                    case TerrainType.OpenDoor:
                                        window.rootConsole.putChar(dx, dy, '\\');
                                        break;
                                    case TerrainType.ClosedDoor:
                                        window.rootConsole.putChar(dx, dy, '+');
                                        break;
                                    case TerrainType.StairsDown:
                                        if (random.getInt(0, 1) == 0) { window.rootConsole.setForegroundColor(TCODColor.purple); }
                                        else { window.rootConsole.setForegroundColor(TCODColor.fuchsia); }
                                        window.rootConsole.putChar(dx, dy, '>');
                                        break;
                                    case TerrainType.ElementalGate:
                                        if (random.getInt(0, 2) == 0) { window.rootConsole.setForegroundColor(TCODColor.red); }                                      
                                        else { window.rootConsole.setForegroundColor(TCODColor.flame); }
                                        window.rootConsole.putChar(dx, dy, '#');
                                        break;
	                            }
                                window.rootConsole.setCharBackground(dx, dy, map.Cells[x, y].AmbientLightColour, TCODBackgroundFlag.Add);                               
                                break;
                            case Visiblity.Visible:
                                // Get the colour of the torchoverlay for this cell
                                torchColour = TCODColor.Interpolate(TCODColor.black, TCODColor.yellow, map.Cells[x, y].LightIntensity);   
                                
                                // Get the colour of the lit walls
                                wallColour = TCODColor.Interpolate(TCODColor.white, temperatureColour, map.Cells[x, y].LightIntensity);

                                // Set the cells according to these colours and any ambient lighting
                                window.rootConsole.setCharBackground(dx, dy, torchColour, TCODBackgroundFlag.Set);   
                                window.rootConsole.setCharBackground(dx, dy, map.Cells[x, y].AmbientLightColour, TCODBackgroundFlag.Add);
                                window.rootConsole.setForegroundColor(wallColour);
                                switch (map.Cells[x, y].Terrain)
                                {
                                    case TerrainType.HardRock:
                                        //window.rootConsole.setCharBackground(dx, dy, TCODColor.brass, TCODBackgroundFlag.Add);                                    
                                        window.rootConsole.putChar(dx, dy, 177);
                                        break;
                                    case TerrainType.SoftRock:
                                        //window.rootConsole.setCharBackground(dx, dy, TCODColor.brass, TCODBackgroundFlag.Add);
                                        window.rootConsole.putChar(dx, dy, 177);
                                        break;
                                    case TerrainType.RoomFloor:
                                        window.rootConsole.setForegroundColor(TCODColor.grey);
                                        window.rootConsole.putChar(dx, dy, 250);
                                        break;
                                    case TerrainType.CorridorFloor:
                                        window.rootConsole.setCharBackground(dx, dy, TCODColor.darkestGrey, TCODBackgroundFlag.Add);
                                        window.rootConsole.setForegroundColor(TCODColor.grey);
                                        window.rootConsole.putChar(dx, dy, 250);
                                        break;
                                    case TerrainType.OpenDoor:
                                        window.rootConsole.putChar(dx, dy, '\\');
                                        break;
                                    case TerrainType.ClosedDoor:
                                        window.rootConsole.putChar(dx, dy, '+');
                                        break;
                                    case TerrainType.StairsDown:
                                        if (random.getInt(0, 1) == 0) { window.rootConsole.setForegroundColor(TCODColor.purple); }
                                        else { window.rootConsole.setForegroundColor(TCODColor.fuchsia); }
                                        window.rootConsole.putChar(dx, dy, '>');
                                        break;
                                    case TerrainType.ElementalGate:
                                        if (random.getInt(0, 2) == 0) { window.rootConsole.setForegroundColor(TCODColor.red); }
                                        else { window.rootConsole.setForegroundColor(TCODColor.flame); }
                                        window.rootConsole.putChar(dx, dy, '#');
                                        break;
                                }
                                if (map.Cells[x, y].Item > -1)
                                {
                                    // We have an item here, which we should also display its glyph
                                    Item itemToDisplay = items[map.Cells[x, y].Item];
                                    window.rootConsole.setForegroundColor(itemToDisplay.ItemColour);
                                    window.rootConsole.putChar(dx, dy, itemToDisplay.ItemGraphic);
                                }
                                if (map.Cells[x, y].Creature > -1)
                                {
                                    // We have a creature which we should also display its glyph
                                    Creature creatureToDisplay = creatures[map.Cells[x, y].Creature];
                                    window.rootConsole.setForegroundColor(creatureToDisplay.Colour);
                                    window.rootConsole.putChar(dx, dy, creatureToDisplay.Graphic);
                                }

                                break;
                            case Visiblity.NotVisible:
                                break;
                        }

                        if (x == characterLoc.X && y == characterLoc.Y)
                        {                        
                            window.rootConsole.setForegroundColor(TCODColor.white);
                            window.rootConsole.putChar(dx, dy, 64);
                        }
                    }
                }
            }


        }

        public void RedrawMessages(Window window)
        {
            List<Message> messagesToDisplay = new List<Message>();
            messagesToDisplay.AddRange(messages.Skip(Math.Max(0, messages.Count() - 5)).Take(5));
            int yLoc = 28;
            TCODColor foregroundColor = new TCODColor();
                     
            foreach (Message m in messagesToDisplay)
            {
                switch (m.Type)
                {
                    case MessageType.Standard:
                        foregroundColor = TCODColor.white;
                        break;
                    case MessageType.System:
                        foregroundColor = TCODColor.yellow;
                        break;
                    case MessageType.LowPriority:
                        foregroundColor = TCODColor.grey;
                        break;
                    case MessageType.Item:
                        foregroundColor = TCODColor.cyan;
                        break;
                    case MessageType.Inventory:
                        foregroundColor = TCODColor.green;
                        break;
                    case MessageType.Error:
                        foregroundColor = TCODColor.flame;
                        break;
                    case MessageType.Monster:
                        foregroundColor = TCODColor.white;
                        break;
                    default:
                        foregroundColor = TCODColor.grey;
                        break;
                }
                window.DisplayText(m.Text, 1, yLoc++, foregroundColor, TCODColor.black, -1);
            }
        }

        public void RedrawStatus(Window window)
        {
            // Figure out the current temperature as a function of time spent on the level      
            string temperature = string.Empty;
            int temperatureCategory = Convert.ToInt32(Progress * 7);
            if (temperatureCategory > 7) { temperatureCategory = 7; }
            if (temperatureCategory < 0) { temperatureCategory = 0; }

            int xLoc = 62;
            int yLoc = 1;

            // Display the temperature/level
            TCODColor barColor = new TCODColor();
            barColor = TCODColor.Interpolate(TCODColor.lightestYellow, TCODColor.darkBlue, Progress);
            window.DisplayText("Temperature: ", xLoc, yLoc++, TCODColor.white, TCODColor.black, -1);
            window.DisplayText(temperatures[temperatureCategory], xLoc, yLoc, barColor, TCODColor.black, -1);
            yLoc += 2;
            window.DisplayText("Depth: ", xLoc, yLoc, TCODColor.white, TCODColor.black, -1);
            window.DisplayText((Level * 100).ToString() + " feet", xLoc + 7, yLoc, TCODColor.red, TCODColor.black, -1);
            yLoc += 2;
            //window.DisplayText("Turns: ", xLoc, yLoc, TCODColor.white, TCODColor.black, -1);
            //window.DisplayText(Turns.ToString(), xLoc + 7, yLoc, TCODColor.yellow, TCODColor.black, -1);
            //yLoc += 2;

            // Now display the character information
            window.DisplayText("You: @ (level " + Level + ")", xLoc, yLoc, TCODColor.white, TCODColor.black, -1);
            yLoc += 2;
            window.DisplayText("Health:", xLoc, yLoc, TCODColor.green, TCODColor.black, -1);
            if (Player.Currenthp < 3)
            {
                window.DisplayText(Player.Currenthp.ToString() + "/" + HP.ToString(), xLoc + 8, yLoc, TCODColor.yellow, TCODColor.black, -1);
            }
            else if (Player.Currenthp <= 0)
            {
                window.DisplayText(Player.Currenthp.ToString() + "/" + HP.ToString(), xLoc + 8, yLoc, TCODColor.red, TCODColor.black, -1);
            }
            else
            {
                window.DisplayText(Player.Currenthp.ToString() + "/" + HP.ToString(), xLoc + 8, yLoc, TCODColor.green, TCODColor.black, -1);
            }
            //yLoc++;
            //window.DisplayText("Mana:", xLoc, yLoc, TCODColor.fuchsia, TCODColor.black, -1);
            //window.DisplayText(Player.MP.ToString() + "/" + ((Player.Mind + getItemBonus(Item.ItemStatistic.Mind)) * 2).ToString(), xLoc + 8, yLoc, TCODColor.fuchsia, TCODColor.black, -1);
            yLoc += 2;
            window.DisplayText("Body:", xLoc, yLoc, TCODColor.white, TCODColor.black, -1);
            window.DisplayText(B.ToString(), xLoc + 6, yLoc, TCODColor.green, TCODColor.black, -1);
            yLoc++;
            window.DisplayText("Finesse:", xLoc, yLoc, TCODColor.white, TCODColor.black, -1);
            window.DisplayText(F.ToString(), xLoc + 9, yLoc, TCODColor.green, TCODColor.black, -1);
            yLoc++;
            window.DisplayText("Mind:", xLoc, yLoc, TCODColor.white, TCODColor.black, -1);
            window.DisplayText(M.ToString(), xLoc + 6, yLoc, TCODColor.green, TCODColor.black, -1);
            yLoc += 2;
            
            // Statistics
            window.DisplayText("Attack:", xLoc, yLoc, TCODColor.white, TCODColor.black, -1);
            window.DisplayText(Att.ToString(), xLoc + 8, yLoc, TCODColor.yellow, TCODColor.black, -1);
            yLoc++;
            window.DisplayText("Defense:", xLoc, yLoc, TCODColor.white, TCODColor.black, -1);
            window.DisplayText(Def.ToString(), xLoc + 9, yLoc, TCODColor.yellow, TCODColor.black, -1);
            yLoc += 2;

            // Items & Magic
            window.DisplayText("Wielding:", xLoc, yLoc, TCODColor.white, TCODColor.black, -1);
            yLoc++;

            int slotIDOfAlreadyWornArmour = GetItemStatusFromInventory(Item.ItemStatus.Wielded);
            if (slotIDOfAlreadyWornArmour != -1)
            {
                // Not wearing armour, so wear this armour
                Item itemToCheck = items[Player.Inventory[slotIDOfAlreadyWornArmour]];
                window.DisplayText(itemToCheck.Name, xLoc, yLoc, itemToCheck.ItemColour, TCODColor.black, -1);
            }
            else
            {
                window.DisplayText("(nothing)", xLoc, yLoc, TCODColor.grey, TCODColor.black, -1);
            }
            yLoc += 2;
            window.DisplayText("Wearing:", xLoc, yLoc, TCODColor.white, TCODColor.black, -1);
            yLoc++;

            int slotIDOfAlreadyWieldedWeapon = GetItemStatusFromInventory(Item.ItemStatus.Worn);
            if (slotIDOfAlreadyWieldedWeapon != -1)
            {
                // Not wearing armour, so wear this armour
                Item itemToCheck = items[Player.Inventory[slotIDOfAlreadyWieldedWeapon]];
                window.DisplayText(itemToCheck.Name, xLoc, yLoc, itemToCheck.ItemColour, TCODColor.black, -1);
            }
            else
            {
                window.DisplayText("(nothing)", xLoc, yLoc, TCODColor.grey, TCODColor.black, -1);
            }
            yLoc += 2;

            //window.DisplayText("Casting:", xLoc, yLoc, TCODColor.white, TCODColor.black, -1);
            //yLoc++;
            //window.DisplayText("(nothing)", xLoc, yLoc, TCODColor.grey, TCODColor.black, -1);
            //yLoc += 3;

            TCODColor heatColour = new TCODColor();
            string heatText = string.Empty;
            if (DisplayHeatText(characterLoc.X, characterLoc.Y, out heatText, out heatColour))
            {
                window.DisplayText(heatText, xLoc, yLoc, heatColour, TCODColor.black, -1);
            }
            
        }

        public void SaveElement(string filename, string data)
        {
            using (FileStream fs = File.Create(filename))
            {
                Byte[] info = new UTF8Encoding(true).GetBytes(data);
                fs.Write(info, 0, info.Length);
            }

            // Compress the text file
            FileInfo fi = new FileInfo(filename);
            Compress(fi);
        }

        public string LoadElement(string appPath, string filename)
        {
            // Load a datafile into a string
            string dataFile = appPath + filename + ".dat";
            string dataFileTemp = appPath + filename + ".tmp";
            FileInfo fiData = new FileInfo(dataFile);
            Decompress(fiData);
            FileInfo fiDataTemp = new FileInfo(dataFileTemp);
                
            StringBuilder dataBuffer = new StringBuilder(string.Empty);
            using (FileStream fsData = File.OpenRead(dataFileTemp))
            {
                byte[] bData = new byte[1024];
                UTF8Encoding tempUTFEncodingData = new UTF8Encoding(true);
                while (fsData.Read(bData, 0, bData.Length) > 0)
                {
                    dataBuffer.Append(tempUTFEncodingData.GetString(bData).Trim('\0'));
                    for (int i = 0; i < 1024; i++)
                    {
                        bData[i] = Convert.ToByte('\0');
                    }

                }
            }
            fiDataTemp.Delete();
            return dataBuffer.ToString().Trim('\0');
        }

        public bool Load()
        {
            // first find the subdirectory
            StringBuilder appPath = new StringBuilder(Path.GetDirectoryName(Application.ExecutablePath));
            appPath.Append("\\save\\");

            if (!Directory.Exists(appPath.ToString()))
            {
                return false;
            }
            else
            {
                // First check that all the files exist, and if so decompress them
                List<string> fileList = new List<string>();
                fileList.Add(appPath.ToString() + "levelData.dat");
                fileList.Add(appPath.ToString() + "lightData.dat");
                fileList.Add(appPath.ToString() + "gameData.dat");
                fileList.Add(appPath.ToString() + "messageData.dat");
                fileList.Add(appPath.ToString() + "characterData.dat");
                fileList.Add(appPath.ToString() + "itemData.dat");
                fileList.Add(appPath.ToString() + "creatureData.dat");

                bool fileNotFound = false;
                foreach (string s in fileList)
                {
                    if (!File.Exists(s))
                    {
                        fileNotFound = true;
                    }
                }
                if (fileNotFound)
                {
                    return false;
                }
                else
                {
                    // Now decompress each file in turn to a temporary file and read it in to memory
                    // before parsing it          
                    string levelData = LoadElement(appPath.ToString(), "levelData");
                    string gameData = LoadElement(appPath.ToString(), "gameData");
                    string itemData = LoadElement(appPath.ToString(), "itemData");
                    string lightsourceData = LoadElement(appPath.ToString(), "lightData");
                    string characterData = LoadElement(appPath.ToString(), "characterData");
                    string messageData = LoadElement(appPath.ToString(), "messageData");
                    string creatureData = LoadElement(appPath.ToString(), "creatureData");
                                                       
                    // At this point we now have the save data in memory, so we now have to parse it
                    // and replace the extant data with the data
                    int index = 0;

                    // Level Data
                    mapLevel.Value = levelData;
                    
                    // Game Data
                    string[] gameWords = gameData.Split('=');
                    ID = System.Guid.Parse(gameWords[index++]);
                    viewPort.Value = gameWords[index++];
                    characterLoc.Value = gameWords[index++];
                    Turns = Convert.ToInt32(gameWords[index++]);
                    Level = Convert.ToInt32(gameWords[index++]);

                    // Item Data
                    string[] tempItems = itemData.Split('=');
                    List<string> itemWords = new List<string>();
                    foreach (string i in tempItems)
                    {
                        itemWords.Add(i);
                    }
                    foreach (string itemText in itemWords)
                    {
                        if (itemText.Trim('\0').Length > 0)
                        {
                            Item itemToAdd = new Item();
                            itemToAdd.Value = itemText;
                            items.Add(itemToAdd);
                        }
                    }

                    // Creature Data
                    string[] tempCreature = creatureData.Split('=');
                    List<string> creatureWords = new List<string>();
                    foreach (string c in tempCreature)
                    {
                        creatureWords.Add(c);
                    }
                    foreach (string creatureText in creatureWords)
                    {
                        if (creatureText.Trim('\0').Length > 0)
                        {
                            Creature creatureToAdd = new Creature();
                            creatureToAdd.Value = creatureText;
                            creatures.Add(creatureToAdd);
                        }
                    }

                    // Message Data 
                    string[] temp = messageData.Split('=');
                    List<string> messageWords = new List<string>();
                    foreach (string m in temp)
                    {
                        messageWords.Add(m);
                    }
                    messageWords.Remove(messageWords.Last());

                    foreach (string m in messageWords)
                    {
                        string[] messageComponents = m.Split('^');
                        Message message = new Message();
                        message.Text = messageComponents[0].Trim('\0');
                        message.Type = (MessageType)Enum.Parse(typeof(MessageType), messageComponents[1].Trim('\0'));
                        messages.Add(message);                    
                    }

                    // Lightsource Data
                    string[] lsTemp = lightsourceData.Split('=');
                    List<string> lsWords = new List<string>();
                    foreach (string ls in lsTemp)
                    {
                        lsWords.Add(ls);
                    }
                    lsWords.Remove(lsWords.Last());

                    foreach (string ls in lsWords)
                    {
                        string[] lsWordsComponents = ls.Split(':');
                        index = 0;
                        Point lspos = new Point();
                        lspos.Value = lsWordsComponents[index++];
                        int radius = Convert.ToInt32(lsWordsComponents[index++]);
                        TCODColor lsColour = new TCODColor();
                        lsColour.setHSV((float)Convert.ToDouble(lsWordsComponents[index++]),
                                            (float)Convert.ToDouble(lsWordsComponents[index++]),
                                            (float)Convert.ToDouble(lsWordsComponents[index++]));
                        float lsIntensity = (float)Convert.ToDouble(lsWordsComponents[index++]);

                        LightSource lightSource = new LightSource(lspos, radius, lsColour, lsIntensity, mapLevel);
                        mapLevel.lightSources.Add(lightSource);                     
                    }

                    // Character Data
                    Player.Value = characterData;

                    return true;
                }              
            }
        }

        public void Save()
        {
            // First find or create a subdirectory
            StringBuilder appPath = new StringBuilder(Path.GetDirectoryName(Application.ExecutablePath));
            appPath.Append("\\save\\");
            
            if (!Directory.Exists(appPath.ToString()))
            {
                Directory.CreateDirectory(appPath.ToString());
            }

            // First, create and save the current level map as a compressed text file
            string levelData = this.mapLevel.Value;
            string mapFile = appPath.ToString() + "levelData";
            SaveElement(mapFile, levelData);

            // Now write out the lightsource data         
            StringBuilder lightDataBuffer = new StringBuilder(string.Empty);
            foreach (LightSource ls in mapLevel.lightSources)
            {
                lightDataBuffer.Append(ls.Value);
                lightDataBuffer.Append("=");
            }
            string lightData = lightDataBuffer.ToString().Trim('\0');
            string lightFile = appPath.ToString() + "lightData";
            SaveElement(lightFile, lightData);

            // Now the item data
            StringBuilder itemDataBuffer = new StringBuilder(string.Empty);
            foreach (Item i in items)
            {
                itemDataBuffer.Append(i.Value);
                itemDataBuffer.Append("=");
            }
            string itemData = itemDataBuffer.ToString().Trim('\0');          
            string itemFile = appPath.ToString() + "itemData";
            SaveElement(itemFile, itemData);

            // Now the creature data
            StringBuilder creatureDataBuffer = new StringBuilder(string.Empty);
            foreach (Creature c in creatures)
            {
                creatureDataBuffer.Append(c.Value);
                creatureDataBuffer.Append("=");
            }
            string creatureData = creatureDataBuffer.ToString().Trim('\0');
            string creatureFile = appPath.ToString() + "creatureData";
            SaveElement(creatureFile, creatureData);
      
            // Now write out all the other game data as a seperate  file
            StringBuilder gameDataBuffer = new StringBuilder(ID.ToString());
            gameDataBuffer.Append("=");
            gameDataBuffer.Append(viewPort.Value);
            gameDataBuffer.Append("=");
            gameDataBuffer.Append(characterLoc.Value);
            gameDataBuffer.Append("=");
            gameDataBuffer.Append(Turns.ToString());
            gameDataBuffer.Append("=");
            gameDataBuffer.Append(Level.ToString());
            string gameData = gameDataBuffer.ToString();
            string gameFile = appPath.ToString() + "gameData";
            SaveElement(gameFile, gameData);        

            // Now write out the game messages         
            StringBuilder messageDataBuffer = new StringBuilder(string.Empty);
            foreach (Message m in messages)
            {
                messageDataBuffer.Append(m.Text);
                messageDataBuffer.Append("^");
                messageDataBuffer.Append(m.Type.ToString());
                messageDataBuffer.Append("=");
            }
            string messageData = messageDataBuffer.ToString().Trim('\0');
            string messageFile = appPath.ToString() + "messageData";
            SaveElement(messageFile, messageData); 
         
            // And finally Character Info           
            StringBuilder characterDataBuffer = new StringBuilder(string.Empty);
            characterDataBuffer.Append(Player.Value);
            string characterData = characterDataBuffer.ToString();
            string characterFile = appPath.ToString() + "characterData";
            SaveElement(characterFile, characterData);          
        }

        public void GenerateItemsOnLevel(TCODRandom rnd, int numberItems, int dungeonLevel)
        {
            // Generate and place a number of items on the level of each type
            for (int i = 0; i < numberItems; i++)
            {
                int itemIndex = items.Count;
                int chance = rnd.getInt(0, 100);
                Item.ItemQuality itemQuality = Item.ItemQuality.None;
                Item.ItemType itemType = Item.ItemType.None;

                // Randomly generate the item quality and type, with better items rarer
                if (chance >= 95) { itemQuality = Item.ItemQuality.Epic; }
                else if (chance >= 80) { itemQuality = Item.ItemQuality.Superior; }
                else if (chance >= 55) { itemQuality = Item.ItemQuality.Uncommon; }
                else { itemQuality = Item.ItemQuality.Common; }

                chance = rnd.getInt(0, 100);
                if (chance >= 80) { itemType = Item.ItemType.Armour; }
                else if (chance >= 60) { itemType = Item.ItemType.Weapon; }
                else if (chance >= 50) { itemType = Item.ItemType.Scroll; }
                else if (chance >= 40) { itemType = Item.ItemType.Potion; }
                // else if (chance >= 35) { itemType = Item.ItemType.Wand; } // Removed Wands
                else { itemType = Item.ItemType.Corpse; }

                Item item = new Item(random, itemType, itemQuality, dungeonLevel, Item.ItemStatus.Ground);

                // now get a random empty, walkable location to place the item
                Point newPoint = mapLevel.GetRandomTerrain(TerrainType.CorridorFloor, rnd);
                mapLevel.Cells[newPoint.X, newPoint.Y].Item = itemIndex;
                items.Add(item);
            }
        }

        public void GenerateMonstersOnLevel(TCODRandom rnd, int numberCreatures, int dungeonLevel)
        {
            // Deactivate all current monsters
            foreach (Creature c in creatures)
            {
                c.Status = Creature.CreatureStatus.Dead;
            }

            // Generate and place a number of creatures on the level of each type
            for (int i = 0; i < numberCreatures; i++)
            {
                int creatureIndex = creatures.Count;
                // small chance of OOD monsters
                int chance = rnd.getInt(0, 100);
                int creaturelevel;

                if (chance < 10 && dungeonLevel < 11)
                {
                    creaturelevel = dungeonLevel + 1;
                }
                else
                {
                     creaturelevel = dungeonLevel;
                }

                Creature creature = new Creature(creaturelevel);
                creature.Status = Creature.CreatureStatus.Sleeping;

                // now get a random empty, walkable location to place the item
                Point newPoint = mapLevel.GetRandomTerrain(TerrainType.CorridorFloor, rnd);
                mapLevel.Cells[newPoint.X, newPoint.Y].Creature = creatureIndex;
                creature.Location.X = newPoint.X;
                creature.Location.Y = newPoint.Y;
                creatures.Add(creature);
            }
        }

        public void DisplayItemText(int x, int y, Window window)
        {
            if (mapLevel[x, y].Item > -1)
            {
                
                Item itemToDisplay = items[mapLevel.Cells[x, y].Item];
                StringBuilder outputText = new StringBuilder(itemToDisplay.Name);
                if (itemToDisplay.IsVowel())
                {
                    outputText.Insert(0, "You see an ");
                    outputText.Append(" here");
                }
                else
                {
                    outputText.Insert(0, "You see a ");
                    outputText.Append(" here");
                }

                Message itemMessage = new Message(outputText.ToString(), MessageType.Item);
                messages.Add(itemMessage);
            }
            else if (mapLevel.Cells[characterLoc.X, characterLoc.Y].Terrain == TerrainType.StairsDown)
            {
                Message seeStairs = new Message("Hot air rises from a passageway deeper into the Earth", MessageType.Error);
                messages.Add(seeStairs);
            }
            else if (mapLevel.Cells[characterLoc.X, characterLoc.Y].Terrain == TerrainType.ElementalGate)
            {
                Message wonMessage = new Message("You have found the elemental gate. YOU HAVE WON!", MessageType.System);
                messages.Add(wonMessage);
                DisplayOutro(window);
                canQuit = true;
            }
        }

        // Display the help screen
        public void DisplayHelp(Window window)
        {
            int yloc = 1;
            window.ClearScreen();
            window.DisplayText("HELP INFORMATION", 1, yloc++, TCODColor.yellow, TCODColor.black, -1);
            yloc++;
            window.DisplayText("The aim of the game is to find the elemental gate '#'.", 1, yloc++, TCODColor.white, TCODColor.black, -1);
            window.DisplayText("Use items found through the dungeon to help you to do this.", 1, yloc++, TCODColor.white, TCODColor.black, -1);
            window.DisplayText("Kill monsters that impede your progress.", 1, yloc++, TCODColor.white, TCODColor.black, -1);
            window.DisplayText("Due to the cold, you will gradually lose health as you progress deeper.", 1, yloc++, TCODColor.white, TCODColor.black, -1);
            window.DisplayText("Standing in proximity to fires will prevent this.", 1 , yloc++, TCODColor.white, TCODColor.black, -1);
            yloc++;
            window.DisplayText("KEYS/CONTROLS", 1, yloc++, TCODColor.yellow, TCODColor.black, -1);
            yloc++;
            window.DisplayText("Movement Commands", 1, yloc++, TCODColor.green, TCODColor.black, -1);
            window.DisplayText("To move in a particular direction or to attack a creature", 1, yloc++, TCODColor.white, TCODColor.black, -1);
            window.DisplayText("use the Numpad or Vi keys", 1, yloc++, TCODColor.white, TCODColor.black, -1);
            window.DisplayText(string.Empty, 1, yloc++, TCODColor.white, TCODColor.black, -1);  
            yloc++;
            window.DisplayText("1 2 3       y k u", 35, yloc++, TCODColor.white, TCODColor.black, -1);
            window.DisplayText("4 @ 6       h @ l", 35, yloc++, TCODColor.white, TCODColor.black, -1);
            window.DisplayText("7 8 9       b j n", 35, yloc++, TCODColor.white, TCODColor.black, -1);
            yloc++;
            window.DisplayText("Inventory/Item Commands", 1, yloc++, TCODColor.green, TCODColor.black, -1);
            window.DisplayText("i: Display Inventory", 1 , yloc++, TCODColor.white, TCODColor.black, -1);
            window.DisplayText("g: Get an item (from the floor)", 1, yloc++, TCODColor.white, TCODColor.black, -1);
            window.DisplayText("d: Drop an item", 1, yloc++, TCODColor.white, TCODColor.black, -1);
            window.DisplayText("w: Wear/wield an item", 1, yloc++, TCODColor.white, TCODColor.black, -1);
            window.DisplayText("t: Take off a item", 1, yloc++, TCODColor.white, TCODColor.black, -1);
            window.DisplayText("x: Use an item (i.e. quaff/read)", 1, yloc++, TCODColor.white, TCODColor.black, -1);
            yloc++;
            window.DisplayText("Other Commands", 1, yloc++, TCODColor.green, TCODColor.black, -1);
            window.DisplayText("?: Display this information", 1, yloc++, TCODColor.white, TCODColor.black, -1);
            window.DisplayText("S: Save the game", 1, yloc++, TCODColor.white, TCODColor.black, -1);
            window.DisplayText("Q: Save and Quit the game", 1, yloc++, TCODColor.white, TCODColor.black, -1);
            window.DisplayText(">: Go down stairs", 1, yloc++, TCODColor.white, TCODColor.black, -1);
            window.DisplayText(".: Wait a turn", 1, yloc++, TCODColor.white, TCODColor.black, -1);

            window.DisplayText("Press any key to return...", -1, 33, TCODColor.grey, TCODColor.black, -1);

            window.WaitForAnyKeyPress();
            window.ClearScreen();
        }

        // Display the version information
        public void DisplayVersion(Window window)
        {
            // Get the text file
            StringBuilder versionFileName = new StringBuilder(Path.GetDirectoryName(Application.ExecutablePath));
            versionFileName.Append("\\doc\\version.txt");
            if(!File.Exists(versionFileName.ToString()))
            {
                Message cannotFindFile = new Message("Cannot find \\doc\\version.txt", MessageType.Error);
                messages.Add(cannotFindFile);
            }
            else
            {
                int yloc = 1;
                window.ClearScreen();
                using (StreamReader sr = new StreamReader(versionFileName.ToString()))
                {
                    string line = string.Empty;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.Length > 0)
                        {
                            if (line[0] == '=')
                            {
                                window.DisplayText(line, 1, yloc++, TCODColor.white, TCODColor.black, -1);
                            }
                            else if (line[0] == 'S')
                            {
                                window.DisplayText(line, 1, yloc++, TCODColor.silver, TCODColor.black, -1);
                            }
                            else
                            {
                                window.DisplayText(line, 1, yloc++, TCODColor.grey, TCODColor.black, -1);
                            }
                        }
                        else
                        {
                            yloc++;
                        }
                    }
                }               
            }
                       
            window.DisplayText("Press any key to return...", -1, 33, TCODColor.grey, TCODColor.black, -1);

            window.WaitForAnyKeyPress();
            window.ClearScreen();
        }

        // Display the outro
        public void DisplayOutro(Window window)
        {
            window.ClearScreen();
            string[] Outro = new string[16];

            // Initialise the Outro
            Outro[0] = "With your discovery of the gate to the";
            Outro[1] = "  Elemental Fire plane, it seems as if";
            Outro[2] = "  your world is saved.";
            Outro[3] = String.Empty; 
            Outro[4] = "All races put aside their differences";
            Outro[5] = "  to drive back the creatures of cold";
            Outro[6] = "  and together harness this new source";
            Outro[7] = "  of live-giving heat.";
            Outro[8] = String.Empty;
            Outro[9] = "Of course, you can't help but have a";
            Outro[10] = "  nagging feeling that the servant of";
            Outro[11] = "  elemental fire that you defeated was";                
            Outro[12] = "  merely an ominous foretaste of things";
            Outro[13] = "  to come...";
            Outro[14] = String.Empty;
            Outro[15] = "THE END.";

  
            int XLoc = 1;

            foreach (string s in Outro)
            {
                window.DisplayText(s, 34, ++XLoc, TCODColor.white, TCODColor.black, -1);
            }

            window.DisplayText("Press any key to exit...", -1, 33, TCODColor.grey, TCODColor.black, -1);

            // Setup the logo image
            TCODImage leftImage = new TCODImage("end.bmp");

            // Blit the image to the background
            leftImage.blit2x(window.rootConsole, 1, 1);
            window.WaitForAnyKeyPress();
            window.ClearScreen();

            canQuit = true;
        }

        // Display death
        public void DisplayDeath(Window window)
        {
            Message deadMessage = new Message("YOU ARE DEAD!", MessageType.Error);
            messages.Add(deadMessage);
            RedrawMap(window, characterLoc, mapLevel);
            RedrawMessages(window);
            RedrawStatus(window);
            window.WaitForAnyKeyPress();
            window.ClearScreen();
            string[] Death = new string[9];

            // Initialise the Outro
            Death[0] = "With your death, the last home to save";
            Death[1] = "  your people is extinguished.";
            Death[2] = String.Empty;
            Death[3] = "The cold grows more intense, and the";
            Death[4] = "  last remnants of civilisation";
            Death[5] = "  huddle together for warm but die";
            Death[6] = "  a cold lonely and dark death..";
            Death[7] = String.Empty;
            Death[8] = "THE END.";

            int XLoc = 1;

            foreach (string s in Death)
            {
                window.DisplayText(s, 34, ++XLoc, TCODColor.white, TCODColor.black, -1);
            }

            window.DisplayText("Press any key to exit...", -1, 33, TCODColor.grey, TCODColor.black, -1);

            // Setup the logo image
            TCODImage leftImage = new TCODImage("failure.bmp");

            // Blit the image to the background
            leftImage.blit2x(window.rootConsole, 1, 1);
            window.WaitForAnyKeyPress();
            window.ClearScreen();

            // Delete the contents of the save directory           
            StringBuilder appPath = new StringBuilder(Path.GetDirectoryName(Application.ExecutablePath));
            appPath.Append("\\save\\");
            if (Directory.Exists(appPath.ToString()))
            {
                Directory.Delete(appPath.ToString(), true);
            }
            canQuit = true;
        }

        public bool DisplayHeatText(int x, int y, out string outputText, out TCODColor outputTextColour)
        {
            outputTextColour = mapLevel[x, y].AmbientLightColour.Plus(TCODColor.grey);
            if (mapLevel[x, y].AmbientLight > 0.5f)
            {
                outputText = "You feel warm";
                return true;
            }
            else if  (mapLevel[x, y].AmbientLight > 0)
            {
                outputText = "You feel warm";
                return true;
            }
            else
            {
                outputText = string.Empty;
                return false;

            }
        }

        private int getItemBonus(Item.ItemStatistic statToUse)
        {
            Item item = new Item();
            int runningTotal = 0;

            for (int i = 1; i <=26; i++)
            {
                if (Player.Inventory[i] > -1)
                {
                    item = items[Player.Inventory[i]];
                    if (item.Status == Item.ItemStatus.Wielded || item.Status == Item.ItemStatus.Worn)
                    {
                        switch (statToUse)
                        {
                            case Item.ItemStatistic.Body:
                                runningTotal += item.BodyBonus;
                                break;
                            case Item.ItemStatistic.Mind:
                                runningTotal += item.MindBonus;
                                break;
                            case Item.ItemStatistic.Finesse:
                                runningTotal += item.FinesseBonus;
                                break;
                            case Item.ItemStatistic.Attack:
                                runningTotal += item.AttackBonus;
                                break;
                            case Item.ItemStatistic.Defense:
                                runningTotal += item.DefenseBonus;
                                break;
                        }
                    }
                }
            }

            return runningTotal;
        }

        // From http://msdn.microsoft.com/en-us/library/ms404280.aspx
        public static void Compress(FileInfo fi)
        {
            // Get the stream of the source file.
            using (FileStream inFile = fi.OpenRead())
            {
                // Prevent compressing hidden and 
                // already compressed files.
                if ((File.GetAttributes(fi.FullName)
                    & FileAttributes.Hidden)
                    != FileAttributes.Hidden & fi.Extension != ".dat")
                {
                    // Create the compressed file.
                    using (FileStream outFile =
                                File.Create(fi.FullName + ".dat"))
                    {
                        using (GZipStream Compress =
                            new GZipStream(outFile,
                            CompressionMode.Compress))
                        {
                            // Copy the source file into 
                            // the compression stream.
                            inFile.CopyTo(Compress);                        
                        }
                    }                   
                }
            }

            // Delete the uncompressed file
            fi.Delete();
        }

        // From http://msdn.microsoft.com/en-us/library/ms404280.aspx
        public static void Decompress(FileInfo fi)
        {
            // Get the stream of the source file.
            using (FileStream inFile = fi.OpenRead())
            {
                // Get original file extension, for example
                // "doc" from report.doc.gz.
                string curFile = fi.FullName;
                string origName = curFile.Remove(curFile.Length - 
                	fi.Extension.Length);
                origName = origName + ".tmp";

                //Create the decompressed file.
                using (FileStream outFile = File.Create(origName))
                {
                    using (GZipStream Decompress = new GZipStream(inFile,
                            CompressionMode.Decompress))
                    {
                        // Copy the decompression stream 
                        // into the output file.
            		    Decompress.CopyTo(outFile);
            		    
                        //Console.WriteLine("Decompressed: {0}", fi.Name);

                    }
                }
            }
        }

        public float Progress
        {
            get
            {
                float progress = 1 - ((float)Turns / 1000);
                if (progress < 0) { progress = 0; }
                progress = 1 - progress;
                return progress;
            }
        }

        // These are all crap and will need to be rewritten soon and in the
        // proper class (creature!)
        public int Att
        {
            get
            {
                int result = Player.Finesse + Level;
                result += getItemBonus(Item.ItemStatistic.Attack);
                return result;
            }

        }

        public int Def
        {
            get
            {
                int result = Player.Finesse + Level;
                result += getItemBonus(Item.ItemStatistic.Defense);
                return result;
            }

        }

        public int B
        {
            get
            {
                int result = Player.Body + Level;
                result += getItemBonus(Item.ItemStatistic.Body);
                return result;
            }

        }

        public int HP
        {
            get
            {
                int result = B * 2 + Level;
                result += getItemBonus(Item.ItemStatistic.Body);
                Player.HP = result;
                return result;
            }

        }

        public int MP
        {
            get
            {
                int result = M * 2;
                result += getItemBonus(Item.ItemStatistic.Mind);
                return result;
            }

        }

        public int M
        {
            get
            {
                int result = Player.Mind + Level;
                result += getItemBonus(Item.ItemStatistic.Mind);
                return result;
            }

        }

        public int F
        {
            get
            {
                int result = Player.Finesse + Level;
                result += getItemBonus(Item.ItemStatistic.Finesse);
                return result;
            }

        }

        public int Attacks(Creature creatureAttack, Creature creatureDefend, TCODRandom rnd)
        {
            // Let one creature try and attack the other
            int attackDice = rnd.getInt(1, 20);
            int defenseDice = rnd.getInt(1, 20);
            if (creatureAttack.Faction == Creature.CreatureFaction.PC)
            {
                attackDice += Att;
            }
            else
            {
                attackDice += creatureAttack.AttackBonus;
            }
            if (creatureDefend.Faction == Creature.CreatureFaction.PC)
            {
                defenseDice += Def;
            }
            else
            {
                defenseDice += creatureAttack.DefenseBonus;
            }

            // Now see if we have a hit
            int damage = 0;
            if (attackDice > defenseDice)
            {
                damage = attackDice - defenseDice;
                damage += creatureAttack.AttackBonus;
            }
            else
            {
                damage = 0;
            }

            // Limit the damage to avoid one-shot kills of the player
            if (creatureDefend.Faction == Creature.CreatureFaction.PC)
            {
                if (damage >= HP)
                {
                    damage = HP - 1;
                }
            }

            return damage;
        }

        public Map mapLevel;
        private MapViewPort viewPort;
        private Point characterLoc;
        private int Turns;
        private Creature Player;
        private TCODMap ViewMap;
        public Guid ID;
        public Point[] directionOffSet;
        public List<Message> messages;
        public string[] temperatures;
        private bool canQuit;
        public List<Item> items;
        public List<Creature> creatures;
        public int Level;

        TCODRandom random;
    }
}
