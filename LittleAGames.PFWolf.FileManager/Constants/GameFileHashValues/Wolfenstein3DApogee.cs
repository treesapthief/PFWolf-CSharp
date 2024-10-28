﻿namespace LittleAGames.PFWolf.FileManager.Constants.GameFileHashValues;

public class Wolfenstein3DApogee : GamePack
{
    /// <summary>
    /// Wolfenstein 3D Apogee MD5 Hash for AUDIOHED.wl6
    /// </summary>
    public const string AudioHedMd5 = "a41af25a2f193e7d4afbcc4301b3d1ce";
    
    /// <summary>
    /// Wolfenstein 3D Apogee file name for AUDIOHED.wl6
    /// </summary>
    public const string AudioHed = "audiohed.wl6";

    /// <summary>
    /// Wolfenstein 3D Apogee AUDIOT.wl6
    /// </summary>
    public const string AudioTMd5 = "2385b488b18f8721633e5b2bdf054853";

    /// <summary>
    /// Wolfenstein 3D Apogee file name for AUDIOT.wl6
    /// </summary>
    public const string AudioT = "audiot.wl6";
    
    /// <summary>
    /// Wolfenstein 3D Apogee GAMEMAPS.wl6
    /// </summary>
    public const string GamemapsMd5 = "a4e73706e100dc0cadfb02d23de46481";

    /// <summary>
    /// Wolfenstein 3D Apogee file name for GAMEMAPS.wl6
    /// </summary>
    public const string Gamemaps = "gamemaps.wl6";
    
    /// <summary>
    /// Wolfenstein 3D Apogee MAPHEAD.wl6
    /// </summary>
    public const string MapHeadMd5 = "b8d2a78bc7c50da7ec9ab1d94f7975e1";
        
    /// <summary>
    /// Wolfenstein 3D Apogee file name for MAPHEAD.wl6
    /// </summary>
    public const string MapHead = "maphead.wl6";
    
    /// <summary>
    /// Wolfenstein 3D Apogee VGADICT.wl6
    /// </summary>
    public const string VgaDictMd5 = "ccad1a688ebafad9856eca085a20dfc4";
        
    /// <summary>
    /// Wolfenstein 3D Apogee file name for VGADICT.wl6
    /// </summary>
    public const string VgaDict = "vgadict.wl6";
    
    /// <summary>
    /// Wolfenstein 3D Apogee VGAGRAPH.wl6
    /// </summary>
    public const string VgaGraphMd5 = "f18b07d6ba988b8505415f7446235366";

    /// <summary>
    /// Wolfenstein 3D Apogee file name for vgagraph.wl6
    /// </summary>
    public const string VgaGraph = "vgagraph.wl6";
    
    /// <summary>
    /// Wolfenstein 3D Apogee VGAHEAD.wl6
    /// </summary>
    public const string VgaHeadMd5 = "9059afb104a51140bd0c127b73717197";
        
    /// <summary>
    /// Wolfenstein 3D Apogee file name for VGAHEAD.wl6
    /// </summary>
    public const string VgaHead = "vgahead.wl6";
    
    /// <summary>
    /// Wolfenstein 3D Apogee VSWAP.wl6
    /// </summary>
    public const string VswapMd5 = "b8ff4997461bafa5ef2a94c11f9de001";

    /// <summary>
    /// Wolfenstein 3D Apogee file name for VSWAP.wl6
    /// </summary>
    public const string Vswap = "vswap.wl6";
    
    public override GamePacks Pack => GamePacks.Wolfenstein3DFullApogee;
    
    protected override List<GamePackFile> Files => [
        new(AudioHed, AudioHedMd5),
        new(AudioT, AudioTMd5),
        new(Gamemaps, GamemapsMd5),
        new(MapHead, MapHeadMd5),
        new(VgaDict, VgaDictMd5),
        new(VgaGraph, VgaGraphMd5),
        new(VgaHead, VgaHeadMd5),
        new(Vswap, VswapMd5)
    ];
}