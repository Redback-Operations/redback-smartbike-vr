using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum PageState
{
    None = 0,
    Selected = 1,
    Locked = 2
}

[Serializable]
public class Page
{
    public PageState State;
    public Sprite Sprite;

    public bool Dirty { get; set; } = true;

    private Sprite _sprite;

    public void SetSprite(Sprite sprite)
    {
        _sprite = sprite;
        Dirty = false;
    }

    public Sprite GetSprite()
    {
        return _sprite;
    }
}
