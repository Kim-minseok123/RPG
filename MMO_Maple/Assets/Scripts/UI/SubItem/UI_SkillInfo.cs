using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UI_SkillInfo : UI_Base
{
    Skill skillData;
    GameObject description;
    GameObject dragIcon;
    GameObject dragObj;
    enum Images
    {
        IconImage,
        LockSkillImage
    }
    enum Texts
    {
        SkillNameText,
        SkillLevelText,
        SkillLevelUpText,
    }
    enum Buttons
    {
        SkillLevelUpBtn
    }
    bool _init = false;
    int templateId = -1;
    int skillLevel = 0;
    public override void Init()
    {
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));
        // 스킬 레벨업
        GetButton((int)Buttons.SkillLevelUpBtn).gameObject.BindEvent(
            e => 
            {
                if (Managers.Object.MyPlayer.Stat.SkillPoint <= 0) return;
                C_SkillLevelUp skillLevelUp = new C_SkillLevelUp() { Skill = new SkillInfo() };
                skillLevelUp.Skill.SkillId = templateId;
                skillLevelUp.Skill.Level = 1;
                Managers.Network.Send(skillLevelUp);
            });
        {
            // 스킬 설명 출력
            GetImage((int)Images.IconImage).gameObject.BindEvent((e) =>
            {
                if (skillData == null) return;
                description = Managers.Resource.Instantiate("UI/SubItem/UI_SkillInfoCanvas");
                description.GetComponent<UI_SkillInfoCanvas>().Setting(skillData, skillLevel);
            }, Define.UIEvent.PointerEnter);
            // 스킬 설명 삭제
            GetImage((int)Images.IconImage).gameObject.BindEvent((e) =>
            {
                if (skillData == null) return;
                if (description != null)
                    Managers.Resource.Destroy(description);
            }, Define.UIEvent.PointerExit);
        }
        {
            // 스킬 드래그 시작
            GetImage((int)Images.IconImage).gameObject.BindEvent((e) =>
            {
                if(skillData == null || skillLevel <= 0) return;
                dragObj = Managers.Resource.Instantiate("UI/UI_DragObj");
                dragIcon = Util.FindChild(dragObj, "Icon");
                if (dragIcon == null) return;
                dragIcon.GetComponent<Image>().sprite = GetImage((int)Images.IconImage).sprite;
            }, Define.UIEvent.DragEnter);
            // 스킬 드래그
            GetImage((int)Images.IconImage).gameObject.BindEvent((e) =>
            {
                if (skillData == null || skillLevel <= 0) return;
                if (dragObj == null) return; if (dragIcon == null) return;
                dragIcon.transform.position = e.position;
            }, Define.UIEvent.Drag);
            // 스킬 드래그 멈춤
            GetImage((int)Images.IconImage).gameObject.BindEvent((e) =>
            {
                if (skillData == null || skillLevel <= 0) return;
                if (dragObj == null || templateId < 0) return; if (dragIcon == null ) return;
                Managers.Resource.Destroy(dragObj);
                if(e.pointerCurrentRaycast.gameObject.name != null)
                    (Managers.UI.SceneUI as UI_GameScene).RequestQuickSlotUI(e.pointerCurrentRaycast.gameObject.name, templateId, true);
            }, Define.UIEvent.DragEnd);
        }
        _init = true;

        RefreshUI();
    }
    public void Setting(int templateId)
    {
        this.templateId = templateId;
    }
    public void RefreshUI()
    {
        if (_init == false) return;
        if (Managers.Data.SkillDict.TryGetValue(templateId, out skillData) == false)
            return;

        GetImage((int)Images.IconImage).sprite = Managers.Resource.Load<Sprite>($"Textures/Skill/{skillData.name}");
        GetText((int)Texts.SkillNameText).text = skillData.name;

        if(Managers.Object.MyPlayer.HaveSkillData.TryGetValue(templateId, out skillLevel) == false)
        {
            GetText((int)Texts.SkillLevelText).text = "0";
            GetImage((int)Images.LockSkillImage).color = new Color(1, 1, 1, 0.7f);
        }
        else
        {
            GetImage((int)Images.LockSkillImage).color = new Color(1, 1, 1, 0);
            GetText((int)Texts.SkillLevelText).text = skillLevel.ToString();
        }
        if(Managers.Object.MyPlayer.Stat.SkillPoint <= 0)
        {
            GetText((int)Texts.SkillLevelUpText).color = new Color(1, 1, 1, 0.5f);
            GetButton((int)Buttons.SkillLevelUpBtn).gameObject.GetComponent<Image>().color = new Color(1, 1, 1, 0.5f);
        }
    }
    public void InfoRemoveSkill()
    {
        if (description != null)
            Managers.Resource.Destroy(description);
    }
}
