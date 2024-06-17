using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILoader<Key, Value>
{
    Dictionary<Key, Value> MakeDict();
}

public class DataManager
{
    public Dictionary<int, Data.Skill> SkillDict { get; private set; } = new Dictionary<int, Data.Skill>();
    public Dictionary<int, Data.Monster> MonsterDict { get; private set; } = new Dictionary<int, Data.Monster>();
    public Dictionary<int, Data.ItemData> ItemDict { get; private set; } = new Dictionary<int, Data.ItemData>();
    public Dictionary<int, Data.ExpData> ExpDict { get; private set; } = new Dictionary<int, Data.ExpData>();
	public  List<int> BeginnerSkillData { get; private set; } = new List<int>();

    public void Init()
    {
        SkillDict = LoadJson<Data.SkillData, int, Data.Skill>("SkillData").MakeDict();
        ItemDict = LoadJson<Data.ItemLoader, int, Data.ItemData>("ItemData").MakeDict();
        MonsterDict = LoadJson<Data.MonsterData, int, Data.Monster>("MonsterData").MakeDict();
        ExpDict = LoadJson<Data.ExpLoader, int, Data.ExpData>("ExpData").MakeDict();
        BeginnerSkillData.Add(3);
        BeginnerSkillData.Add(5);
	}

    Loader LoadJson<Loader, Key, Value>(string path) where Loader : ILoader<Key, Value>
    {
		TextAsset textAsset = Managers.Resource.Load<TextAsset>($"Data/{path}");
        return Newtonsoft.Json.JsonConvert.DeserializeObject<Loader>(textAsset.text);
	}
}
