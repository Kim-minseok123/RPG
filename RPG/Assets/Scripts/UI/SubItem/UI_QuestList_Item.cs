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
    enum Images
    {
        Image
    }
    public override void Init()
    {
        BindText(typeof(Texts));
        BindImage(typeof(Images));

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
            Quest quest = Managers.Quest.GetQuest(QuestData.id, QuestData.questType);
            if(quest == null)
            {
                dialogue_Popup.ClickQuest(QuestData);
                return;
            }
            if (Managers.Quest.CheckClearQuest(QuestData.id, QuestData.questType))
            {
                // 퀘스트를 클리어 경우
                dialogue_Popup.QuestFinish(QuestData);
            }
            else
            {
                // 아직 퀘스트를 클리어하지 못했을 경우
                dialogue_Popup.QuestNotYetFinish(QuestData);
            }
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
        if (Managers.Quest.CheckClearQuest(QuestData.id, QuestData.questType))
        {
            GetImage((int)Images.Image).color = Util.HexColor("#15A55C");
        }
        else
        {
            GetImage((int)Images.Image).color = Util.HexColor("#A51516");
        }
    }

}

