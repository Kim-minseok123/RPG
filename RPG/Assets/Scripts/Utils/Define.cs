using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Define
{
    public enum Scene
    {
        Unknown,
        Login,
        Lobby,
        Loading,
        Game,
        Boss,
    }

    public enum Sound
    {
        Bgm,
        Effect,
        MaxCount,
    }

    public enum UIEvent
    {
        Click,
        Drag,
        DragEnter,
        DragEnd,
        PointerEnter,
        PointerExit,
    }
}
[Serializable]
public class SoundData
{
    public float bgmVolume = 1f;
    public float eftVolume = 1f;
}