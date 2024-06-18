using Data;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_SkillInfoCanvas : UI_Base
{
    Skill skillData;
    int curLevel;
    enum Texts
    {
        SkillInfoNameText,
        SummaryText,
        DescriptionText,
        MasterLevelText
    }
    enum Images
    {
        SkillInfoImage
    }
    enum GameObjects
    {
        SkillInfoObj
    }

    public override void Init()
    {
        BindObject(typeof(GameObjects));
        BindText(typeof(Texts));
        BindImage(typeof(Images));

        RefreshUI();

        var rectTransform = GetObject((int)GameObjects.SkillInfoObj).GetComponent<RectTransform>();

        Vector2 mousePosition = Input.mousePosition;
        Vector2 movePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform.parent as RectTransform, mousePosition, null, out movePos);

        rectTransform.anchoredPosition = movePos;
    }
    public void Setting(Skill data, int curLevel)
    {
        skillData = data;
        this.curLevel = curLevel;
        RefreshUI();
    }
    private void RefreshUI()
    {
        if (skillData == null) return;
        
        GetImage((int)Images.SkillInfoImage).sprite = Managers.Resource.Load<Sprite>($"Textures/Skill/{skillData.name}");
        GetText((int)Texts.SkillInfoNameText).text = skillData.name;

        GetText((int)Texts.DescriptionText).text = SkillDescription.MakeDescription(skillData, curLevel);
        GetText((int)Texts.MasterLevelText).text = $"[마스터 레벨 : {skillData.masterLevel}]";
        GetText((int)Texts.SummaryText).text = skillData.description;
    }
    public void Update()
    {
        var rectTransform = GetObject((int)GameObjects.SkillInfoObj).GetComponent<RectTransform>();

        Vector2 mousePosition = Input.mousePosition;
        Vector2 movePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform.parent as RectTransform, mousePosition, null, out movePos);

        rectTransform.anchoredPosition = movePos;
    }
}
