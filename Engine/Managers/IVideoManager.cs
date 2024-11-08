﻿using LittleAGames.PFWolf.SDK.Components;

namespace Engine.Managers;

public interface IVideoManager
{
    void Initialize();
    void Update(Component component);
    void UpdateScreen();

    void Shutdown();
}