using Data;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Quest : UI_Base
{
    bool _init = false;
    List<UI_Quest_Item> questItems = new List<UI_Quest_Item>();
    UI_Quest_Item currentItem = null;
    enum GameObjects
    {
        DetailBackground,
        QuestContent
    }
    enum Texts
    {
        QuestFullNameText,
        QuestDemandLevelText,
        QuestLocationText,
        QuestDetailText,
        QuestClearText
    }
    enum Images
    {
        QuestNpcImage,
        QuestClearItemImage,
    }
    enum Buttons
    {
        ProgressBtn,
        FinishBtn,
        ExitButton
    }
    public override void Init()
    {
        BindObject(typeof(GameObjects));
        BindImage(typeof(Images));
        BindText(typeof(Texts));
        BindButton(typeof(Buttons));

        GetButton((int)Buttons.ProgressBtn).gameObject.BindEvent(ViewProgressQuest);
        GetButton((int)Buttons.FinishBtn).gameObject.BindEvent(ViewFinishQuest);

        GetObject((int)GameObjects.DetailBackground).SetActive(false);

        GetButton((int)Buttons.ExitButton).gameObject.BindEvent((e) => {
            var ui = Managers.UI.SceneUI as UI_GameScene; ui.CloseUI("UI_Quest");
        });
        questItems.Clear();
        ViewProgressQuest(null);
        _init = true;
        RefreshUI();
    }
    public void ViewProgressQuest(PointerEventData point)
    {
        GetButton((int)Buttons.ProgressBtn).GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>("UI/Content/name_bar2");
        GetButton((int)Buttons.FinishBtn).GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>("UI/Content/name_bar3");
        GetObject((int)GameObjects.DetailBackground).SetActive(false);

        List<Quest> quests = Managers.Quest.GetAllQuest();
        QuestListUI(quests);
    }
    public void ViewFinishQuest(PointerEventData point)
    {
        GetButton((int)Buttons.FinishBtn).GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>("UI/Content/name_bar2");
        GetButton((int)Buttons.ProgressBtn).GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>("UI/Content/name_bar3");
        GetObject((int)GameObjects.DetailBackground).SetActive(false);

        List<Quest> quests = Managers.Quest.GetAllFinishQuest();
        QuestListUI(quests);
    }
    public void QuestListUI(List<Quest> quests)
    {
        Transform parent = GetObject((int)GameObjects.QuestContent).transform;
        questItems.Clear();
        foreach (Transform child in parent.transform)
            Destroy(child.gameObject);
        if (quests == null) return;
        foreach (Quest quest in quests)
        {
            GameObject go = Managers.Resource.Instantiate("UI/SubItem/UI_Quest_Item", parent);
            UI_Quest_Item questItem = go.GetComponent<UI_Quest_Item>();
            questItem.Settting(quest, this);
            questItems.Add(questItem);
        }
    }
    public void QuestDetailUI(QuestData questData, UI_Quest_Item item)
    {
        if (currentItem != null)
            currentItem.ResetColor();
        currentItem = item;
        GetText((int)Texts.QuestFullNameText).text = questData.questTitle;
        GetText((int)Texts.QuestDemandLevelText).text = $"레벨 {questData.demandLevel}이상";
        GetText((int)Texts.QuestLocationText).text = questData.questLocationString;
        GetImage((int)Images.QuestNpcImage).sprite = Managers.Resource.Load<Sprite>(questData.questNpcIconPath);
        GetText((int)Texts.QuestDetailText).text = questData.questDetailString;
        if (questData.questItemIconPath == null)
            GetImage((int)Images.QuestClearItemImage).color = new Color(1, 1, 1, 0);
        else
        {
            GetImage((int)Images.QuestClearItemImage).color = new Color(1, 1, 1, 1);
            GetImage((int)Images.QuestClearItemImage).sprite = Managers.Resource.Load<Sprite>(questData.questItemIconPath);
        }
        switch (item._quest.QuestType)
        {
            case Google.Protobuf.Protocol.QuestType.Battle:
                BattleQuest bq = (BattleQuest)item._quest;
                if(bq != null)
                {
                    int id = ((BattleQuestData)questData).goals[0].enemyId;
                    GetText((int)Texts.QuestClearText).text = $"{questData.goalText} {bq.countDict[id]} / {((BattleQuestData)questData).goals[0].count}";
                }
                break;
            case Google.Protobuf.Protocol.QuestType.Collection:
                CollectionQuest cq = (CollectionQuest)item._quest;
                if (cq != null)
                {
                    int id = ((CollectionQuestData)questData).goals[0].collectionId;
                    GetText((int)Texts.QuestClearText).text = $"{questData.goalText} {cq.countDict[id]} / {((CollectionQuestData)questData).goals[0].count}";
                }
                break;
            case Google.Protobuf.Protocol.QuestType.Enter:
                EnterQuestData enterQuestData = (EnterQuestData)questData;
                GetText((int)Texts.QuestClearText).text = enterQuestData.goalText;
                break;
        }
        GetObject((int)GameObjects.DetailBackground).SetActive(true);
    }
    public void RefreshUI()
    {
        if (_init == false) return;

    }
}
