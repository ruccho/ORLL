using MessagePack;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[MessagePackObject]
public class LocalizationUnit : ISerializationCallbackReceiver
{
    [Key(0)]
    public string Name { get; set; }
    [Key(1)]
    public Dictionary<string, string> Values { get; set; }

    public LocalizationUnit()
    {
        if(Values == null)
        {
            Values = new Dictionary<string, string>();
        }
    }

    public void OnAfterDeserialize()
    {
        if(Values == null)
        {
            Values = new Dictionary<string, string>();
        }
    }

    public void OnBeforeSerialize()
    {
        if (Values == null)
        {
            Values = new Dictionary<string, string>();
        }
    }
}
