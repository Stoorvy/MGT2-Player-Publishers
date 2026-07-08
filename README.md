# Player Publishers

A BepInEx mod for Mad Games Tycoon 2.

## Features

- Multiplayer publisher negotiations
- Counter offers
- Negotiation menu

## Installation

IF you don't know how to install BepInEx just install PlayerPublishers.zip or PlayerPublishers-WithoutStoorvyStuff.zip and put the files into game directory.

IF you know how to install BepInEx (or you already have it) install PlayerPublishers.dll or PlayerPublishers-DLLWithStoorvyStuff.zip and put the dll in BepInEx/plugins then put other folder in main game directory.

## Notes

If you encounter any bugs relating this mod please write a comment about it. I only tested this with 2 players but it should work with any number of players.

### What is **Stoorvy Stuff**?
- I added myself as a company in the mod. If you don't want that you can just install without Stoorvy stuff versions.

### Why does it say payment 0$ in publisher menu?
- Because once you click to a player publisher you'll see a menu and you can offer any amount you want.

### How does profit share work?
- Publisher automaticly pays set percent of profit every week. Developer will see a message like "Player sent you 10000$". Mod uses game's sending money feature to process this.

### Weekly sells updates after next week
- On developer's side weekly sells may show different numbers. It'll fix itself after receiving true info from publisher. It keeps updating weekly.

### Updating game and hyping works fine
- Developer can update and hype the game just fine. Publisher can also hype the game.

## Dependencies

- BepInEx 5.4.23.5
- Harmony

## Screenshots

![Offer Menu](screenshots/OfferMenu.png)

![Offer List](screenshots/OfferList.png)

![Player Publisher](screenshots/PlayerPublisher.png)
