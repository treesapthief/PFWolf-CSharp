﻿using System.Reflection;
using Engine;
using Engine.Managers;
using LittleAGames.PFWolf.Common;
using LittleAGames.PFWolf.Common.GamePacks;

var fileLoader = new FileLoader();
var gamePack = new Wolfenstein3DApogee();
var directory = "D:\\projects\\Wolf3D\\Wolf3D_Games\\wolf3d-v1.4-apogee";
var isValidPack = fileLoader.Validate(gamePack, directory);
if (!isValidPack)
{
    Console.WriteLine($"Pack: {gamePack.PackName} not found in directory: {directory}");
    return;
}

GameConfiguration gameConfig = new()
{
    BaseDirectory = directory,
    GamePalette = "wolfpal" // TODO: Move this into the pk3 file as other games will want to use custom palettes
};
var assetManager = new AssetManager(fileLoader);

assetManager.AddGamePack(gamePack, directory);
assetManager.AddModPack("D:\\projects\\Wolf3D\\PFWolf\\PFWolf-Assets", "pfwolf.pk3");
assetManager.AddAssembly(typeof(LittleAGames.PFWolf.ExternalPk3ModPack.NoOp).Assembly);
Console.WriteLine($"Assets loaded: {assetManager.AssetCount}");

var mapManager = new MapManager(assetManager); // TODO: Choose map handler from game pack?

var inputManager = new SDLInputManager();

var videoManager = new SDLVideoManager(assetManager, gameConfig);

new GameManager(
    assetManager,
    videoManager,
    inputManager,
    mapManager,
    gameConfig).Start();