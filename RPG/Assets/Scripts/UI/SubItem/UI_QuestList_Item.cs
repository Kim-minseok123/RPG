using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class UI_QuestList_Item : UI_Base
{
    QuestData QuestData;
    UI_QuestDialogue_Popup dialogue_Popup;
    bool _init = false;
    enum Texts
    {
        QuestNameText
    }
    public override void Init()
    {
        BindText(typeof(Texts));

        GetText((int)Texts.QuestNameText).gameObject.BindEvent((e) =>
        {
            GetText((int)Texts.QuestNameText).fontStyle = TMPro.FontStyles.Underline;
        }, Define.UIEvent.PointerEnter); 
        GetText((int)Texts.QuestNameText).gameObject.BindEvent((e) =>
        {
            GetText((int)Texts.QuestNameText).fontStyle = TMPro.FontStyles.Normal;
        }, Define.UIEvent.PointerExit);
        GetText((int)Texts.QuestNameText).gameObject.BindEvent((e) =>
        {
            dialogue_Popup.ClickQuest(QuestData);
        });

        _init = true;
        RefreshUI();
    }
    public void Setting(QuestData questData, UI_QuestDialogue_Popup popup)
    {
        QuestData = questData;
        dialogue_Popup = popup;

        RefreshUI();
    }
    public void RefreshUI()
    {
        if (_init == false || QuestData == null) return;
        GetText((int)Texts.QuestNameText).text = QuestData.questTitle;
    }
}

