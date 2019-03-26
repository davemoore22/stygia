**stygia-7DRL**

*GENERAL INFORMATION*

This game is **Stygia**; it was my entry in the **2011** Seven-Day Roguelike Competition (7DRL).I started coding* at 6pm on Sunday 6th March 2011, and version 0.1 was compiled at 5.54pm on Suunday 13th March 2011, 167 hours and 54 minutes later. It is a complete and playable game, although I'm sure there are many bugs present, as I did not have time to playtest it properly during the competition. Subsequent to this, a new version, v0.12 was released a couple of days later with some minor bug fixes. Details are below

To get key/control information, press ? on starting a new game.

This program and code is released under the GPL v2 license, and is (c) Dave Moore 2011.

Stygia was written in C# using the Libtcod library and using Visual Studio 2010. This version of the code was compiled using, and requires v4 of the Microsoft .NET Framework to run.

*CODE STUFF*

Inside the code subdirectory is the VS project for this program. I've removed the files that come with the Libtcod.NET distribution, but they will be present in the Solution Explorer - just copy the appropriate files into the Stygia directory and you should be fine.

Yes, the code isn't particularily well written: it was my first serious application written in C#.

Prior to the commencement of the competition, I had a title screen and a @ walking around a map. All the other code has been written during the course of the 7DRL week.

Roguebasin page: <http://www.roguebasin.com/index.php?title=Stygia>
Dev blog: <https://stygiaroguelike.wordpress.com/>
Review: <https://blog.heroicfisticuffs.com/2011/03/7drl-2011-revirews-stygia.html>


![Screenshot](https://lh6.googleusercontent.com/-eE233fpOk_4/TY3268ntgMI/AAAAAAAAADo/xAMMnRfdyw8/s1600/stygia01_intro.png)

![Screenshot](https://stygiaroguelike.files.wordpress.com/2011/03/inventory1.png)

*CHANGELOG*

Stygia v0.12 - 15/03/2011
=========================
- Fixed an issue with the view not being centred on the player after 
  using a scroll of Teleportation.
- Added a version notifier to the title screen.
- Added a command to display version information in-game ('V').
- The Quit ('Q') command now works properly.

Stygia v0.11 - 14/03/2011
=========================
- Fixed an issue that would sometimes prevent monsters from moving and 
  doing stuff on their turns.
- Fixed an issue with monsters accidentally stacking on the same square 
  and resulting in one of them being undetectable/unattackable.

Stygia v0.1 - 13/03/2011
========================
Initial Release for the 7DRL Competition
