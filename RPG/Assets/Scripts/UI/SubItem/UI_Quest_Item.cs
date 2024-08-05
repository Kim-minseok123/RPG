using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Quest_Item : UI_Base
{
    public Quest _quest;
    QuestData _questData;
    UI_Quest parent;
    bool _init = false;
    enum Buttons
    {
        QuestDetailBtn
    }
    enum Texts
    {
        QuestNameText
    }
    enum Images
    {
        FinishImage
    }
    public override void Init()
    {
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));

        GetButton((int)Buttons.QuestDetailBtn).gameObject.BindEvent(ClickQuest);

        _init = true;
        RefesthUI();
    }
    public void ClickQuest(PointerEventData data)
    {
        if (parent != null)
            parent.OpenQuestDetailUI(_questData, this);
        Managers.Sound.Play("ButtonClick");
    }
    public void SetColor()
    {
        GetButton((int)Buttons.QuestDetailBtn).GetComponent<Image>().color = Util.HexColor("#391010");
    }
    public void ResetColor()
    {
        GetButton((int)Buttons.QuestDetailBtn).GetComponent<Image>().color = Util.HexColor("#262021");
        Debug.Log("asd");
    }
    public void Setting(Quest quest, UI_Quest uiQuest)
    {
        if (Managers.Data.QuestDict.TryGetValue(quest.TemplateId, out QuestData questData) == false) return;
        _quest = quest;
        _questData = questData;
        parent = uiQuest;
        RefesthUI();
    }
    public void RefesthUI()
    {
        if (_quest == null || _init == false) return;

        GetText((int)Texts.QuestNameText).text = _questData.questTitle;
        if(_quest.IsFinish)
            GetImage((int)Images.FinishImage).sprite = Managers.Resource.Load<Sprite>("Textures/Quest/Finish");
        else
            GetImage((int)Images.FinishImage).sprite = Managers.Resource.Load<Sprite>("Textures/Quest/Progress");
        
    }
}
