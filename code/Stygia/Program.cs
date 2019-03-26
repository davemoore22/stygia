using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using libtcod;
using Stygia;
using Stygia.Session;

namespace Stygia
{
    class Program
    {
        static void Main(string[] args)
        {
            const int ScreenHeight = 35;
            const int ScreenWidth = 80;

            // Set up the main window          
            //Window rootWindow = new Window("Md_curses_16x16.png", ScreenWidth, ScreenHeight, "Stygia: The Hidden Depths");
            Window rootWindow = new Window("font_16x16.png", ScreenWidth, ScreenHeight, "Stygia: The Hidden Depths");
            
            // Display the title screen and wait for a keypress
            rootWindow.ClearScreen();
            rootWindow.DisplayTitleScreen();
            rootWindow.WaitForAnyKeyPress();
          
            // Then display the main menu
            rootWindow.ClearScreen();
            rootWindow.DisplayMainMenu();

            // Handle main menu keypresses
            bool canQuit = false;
            do
            {
                TCODConsole.flush();
                char keypress = rootWindow.HandleMainMenu();

                switch (keypress)
                {
                    case 'S':
                        rootWindow.ClearScreen();
                        rootWindow.DisplayIntro();
                        rootWindow.WaitForAnyKeyPress();
                        rootWindow.ClearScreen();
                        Game newgame = new Game(rootWindow, true);
                        newgame.Start(rootWindow);
                        rootWindow.DisplayMainMenu();
                        break;
                    case 'C':
                        rootWindow.ClearScreen();
                        Game oldgame = new Game(rootWindow, false);
                        oldgame.Start(rootWindow);
                        rootWindow.DisplayMainMenu();
                        break;
                    case 'Q':
                        rootWindow.ClearScreen();
                        canQuit = true;                      
                        break;
                }
            }
           while (!TCODConsole.isWindowClosed() && !canQuit);
        }
    }
}
