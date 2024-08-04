using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Util
{
    public static T GetOrAddComponent<T>(GameObject go) where T : UnityEngine.Component
    {
        T component = go.GetComponent<T>();
		if (component == null)
            component = go.AddComponent<T>();
        return component;
	}
    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive);
        if (transform == null)
            return null;
        
        return transform.gameObject;
    }
    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
    {
        if (go == null)
            return null;

        if (recursive == false)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform transform = go.transform.GetChild(i);
                if (string.IsNullOrEmpty(name) || transform.name == name)
                {
                    T component = transform.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }
		}
        else
        {
            foreach (T component in go.GetComponentsInChildren<T>())
            {
                if (string.IsNullOrEmpty(name) || component.name == name)
                    return component;
            }
        }

        return null;
    }
    public static Vector3 PositionsToVector3(Positions positions)
    {
        return new Vector3(positions.PosX, positions.PosY, positions.PosZ);
    }
    public static Positions Vector3ToPositions(Vector3 vector)
    {
        return new Positions() { PosX = vector.x, PosY = vector.y, PosZ = vector.z };
    }
    public static bool IsAllLetters(string input)
    {
        return input.All(char.IsLetter);
    }
    public static string ChagneClassType(ClassTypes classTypes)
    {
        switch (classTypes)
        {
            case ClassTypes.Beginner:
                return "초보자";
            case ClassTypes.Warrior:
                return "전사";
            case ClassTypes.Archer:
                return "궁수";
            default:
                return "";
        }
    }
    public static Color HexColor(string hexCode)
    {
        Color color;
        if (ColorUtility.TryParseHtmlString(hexCode, out color))
        {
            return color;
        }

        Debug.LogError("[UnityExtension::HexColor]invalid hex code - " + hexCode);
        return Color.white;
    }
}
public class SkillDescription
{
    public static string MakeDescription(Skill skill, int curLevel)
    {
        string result = "";

        switch (skill.id)
        {
            case 3:
                AttackSkill attackSkill = (AttackSkill)skill;
                if (curLevel == 0)
                {
                    result = $"[다음레벨 1]\n" +
                        $"MP {skill.mpConsume}를 소모, 전방의 모든 적을 " +
                        $"{attackSkill.skillDatas[0].damage + attackSkill.skillDatas[0].skillLevelInc * curLevel}% 데미지 후 " +
                        $"{attackSkill.skillDatas[1].damage + attackSkill.skillDatas[1].skillLevelInc * curLevel}% 데미지";
                }
                else if (curLevel == skill.masterLevel)
                {
                    result = $"[현재레벨 {curLevel}]\n" +
                        $"MP {skill.mpConsume}를 소모, 전방의 모든 적을 " +
                        $"{attackSkill.skillDatas[0].damage + attackSkill.skillDatas[0].skillLevelInc * (curLevel - 1)}% 데미지 후 " +
                        $"{attackSkill.skillDatas[1].damage + attackSkill.skillDatas[1].skillLevelInc * (curLevel - 1)}% 데미지";
                }
                else
                {
                    result = $"[현재레벨 {curLevel}]\n" +
                        $"MP {skill.mpConsume}를 소모, 전방의 모든 적을 " +
                        $"{attackSkill.skillDatas[0].damage + attackSkill.skillDatas[0].skillLevelInc * (curLevel - 1)}% 데미지 후 " +
                        $"{attackSkill.skillDatas[1].damage + attackSkill.skillDatas[1].skillLevelInc * (curLevel - 1)}% 데미지\n" +
                        $"[다음레벨 {curLevel + 1}]\n" +
                        $"MP {skill.mpConsume}를 소모, 전방의 모든 적을 " +
                        $"{attackSkill.skillDatas[0].damage + attackSkill.skillDatas[0].skillLevelInc * curLevel}% 데미지 후 " +
                        $"{attackSkill.skillDatas[1].damage + attackSkill.skillDatas[1].skillLevelInc * curLevel}% 데미지";
                }
                return result;
            case 5:
                BuffSkill buffSkill = (BuffSkill)skill;
                if (curLevel == 0)
                {
                    result = $"[다음레벨 1]\n" +
                        $"MP {skill.mpConsume}를 소모, " +
                        $"지속 시간 {10 * (curLevel + 1)}초, " +
                        $"물리 공격력 {(int)(buffSkill.skillLevelInc * (curLevel + 1) / 2)} 상승";
                }
                else if (curLevel == skill.masterLevel)
                {
                    result = $"[현재레벨 {curLevel}]\n" +
                        $"MP {skill.mpConsume}를 소모, " +
                        $"지속 시간 {10 * curLevel}초, " +
                        $"물리 공격력 {(int)(buffSkill.skillLevelInc * curLevel / 2)} 상승";
                }
                else
                {
                    result = $"[현재레벨 {curLevel}]\n" +
                        $"MP {skill.mpConsume}를 소모, " +
                        $"지속 시간 {10 * curLevel}초, " +
                        $"물리 공격력 {(int)((buffSkill.skillLevelInc + curLevel) / 2)} 상승\n" +
                        $"[다음레벨 {curLevel + 1}]\n" +
                        $"MP {skill.mpConsume}를 소모, " +
                        $"지속 시간 {10 * (curLevel + 1)}초, " +
                        $"물리 공격력 {(int)((buffSkill.skillLevelInc + (curLevel + 1)) / 2)} 상승";
                }
                return result;
            default:
                break;
        }
        return null;
    }
    public static string GetSkillNameKorToEng(string name)
    {
        string result = null;
        switch (name)
        {
            case "파천강기":
                return "HeavenCleavingForce";
            case "분노":
                return "Anger";
            default:
                break;
        }
        return result;
    }
}