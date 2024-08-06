using Data;
using DG.Tweening;
using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_QuestDialogue_Popup : UI_Popup
{
    List<UI_QuestList_Item> QuestList = new List<UI_QuestList_Item>();
    QuestNpc npc = null;
    List<QuestData> QuestDatas = new List<QuestData>();
    QuestData selectQuest = null;
    Tweener tweener = null;
    bool _init = false;
    bool QuestTrigger = false;
    bool QuestEnd = false;
    enum GameObjects
    {
        Content,
        ScrollView
    }
    enum Texts
    {
        DialogueText,
        NpcNameText
    }
    enum Buttons
    {
        YesBtn,
        NoBtn
    }
    public override void Init()
    {
        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));

        GetButton((int)Buttons.YesBtn).gameObject.BindEvent((e) => { AcceptQuest(); });
        GetButton((int)Buttons.NoBtn).gameObject.BindEvent((e) => { DeclineQuest(); });
        GetButton((int)Buttons.YesBtn).gameObject.SetActive(false);
        GetButton((int)Buttons.NoBtn).gameObject.SetActive(false);
        GetText((int)Texts.DialogueText).text = "";

        _init = true;
        selectQuest = null;

        RefreshUI();
    }
    bool QuestAccept = false;
    public void AcceptQuest()
    {
        GetText((int)Texts.DialogueText).text = "";
        GetButton((int)Buttons.YesBtn).gameObject.SetActive(false);
        GetButton((int)Buttons.NoBtn).gameObject.SetActive(false);
        QuestTrigger = false;
        QuestEnd = false;
        QuestAccept = false;
        GetText((int)Texts.DialogueText).DOText(selectQuest.questAcceptString, selectQuest.questAcceptString.Length * 0.02f).OnComplete(() => { QuestEnd = true; QuestAccept = true; });
    }
    public void DeclineQuest()
    {
        GetText((int)Texts.DialogueText).text = "";
        GetButton((int)Buttons.YesBtn).gameObject.SetActive(false);
        GetButton((int)Buttons.NoBtn).gameObject.SetActive(false);
        QuestTrigger = false;
        GetText((int)Texts.DialogueText).DOText(selectQuest.questRefuseString, selectQuest.questRefuseString.Length * 0.02f).OnComplete(() => { QuestEnd = true; });
    }
    public void Setting(QuestNpc questNpc, List<QuestData> questDatas)
    {
        npc = questNpc;
        QuestDatas = questDatas;

        RefreshUI();
    }
    public void RefreshUI()
    {
        if (_init == false || npc == null)
            return;
        QuestList.Clear();
        foreach (Transform child in GetObject((int)GameObjects.Content).transform)
            Destroy(child.gameObject);
        
        foreach (var quest in QuestDatas)
        {
            if ((quest.demandQuest != 0 && Managers.Quest.CheckIsFinishQuest(quest.demandQuest) == false) || 
                quest.demandLevel > Managers.Object.MyPlayer.Stat.Level) continue;
            GameObject go = Managers.Resource.Instantiate("UI/SubItem/UI_QuestList_Item", GetObject((int)GameObjects.Content).transform);
            UI_QuestList_Item questItem = go.GetComponent<UI_QuestList_Item>();
            questItem.Setting(quest, this);
            QuestList.Add(questItem);
        }

        if(QuestList.Count <= 0)
        {
            GetObject((int)GameObjects.ScrollView).SetActive(false);
            GetText((int)Texts.DialogueText).DOText(npc.npcData.questNonString, npc.npcData.questNonString.Length * 0.02f);
        }
        GetText((int)Texts.NpcNameText).text = npc.NpcName.text;
    }
    int count = 0;
    public void ClickQuest(QuestData quest)
    {
        selectQuest = quest;
        QuestList.Clear();
        foreach (Transform child in GetObject((int)GameObjects.Content).transform)
            Destroy(child.gameObject);
        GetObject((int)GameObjects.ScrollView).SetActive(false);
        Managers.Object.MyPlayer.QuestTrigger = true;
        count = 0;
        ShowText(count++);
    }
    public void Update()
    {
        if (QuestTrigger == true && Input.GetKeyDown(KeyCode.Space))
        {
            ShowText(count++);
        }
        else if(QuestEnd == true && Input.GetKeyDown(KeyCode.Space))
        {
            QuestEnd = false;
            if(QuestClear == true)
            {
                // 클리어 패킷
                C_ClearQuest clearQuestPacket = new C_ClearQuest();
                clearQuestPacket.NpcId = npc.Id;
                clearQuestPacket.QuestId = selectQuest.id;
                clearQuestPacket.QuestType = selectQuest.questType;
                Managers.Network.Send(clearQuestPacket);
            }
            else if(QuestAccept == true)
            {
                // 퀘스트 수락 패킷
                C_AddQuest addQuestPacekt = new C_AddQuest();
                addQuestPacekt.NpcId = npc.Id;
                addQuestPacekt.QuestId = selectQuest.id;
                Managers.Network.Send(addQuestPacekt);
            }
            Managers.Object.MyPlayer.QuestTrigger = false;
            npc.CloseNpc();
        }
    }
    public void ShowText(int cnt)
    {
        if (cnt >= selectQuest.questDescription.Count) return;
        QuestTrigger = true;
        tweener?.Kill();
        TextMeshProUGUI text = GetText((int)Texts.DialogueText);
        text.text = "";
        string npcText = selectQuest.questDescription[cnt];
        if(npcText == null)
        {
            Debug.LogError("non script");
            return;
        }
        tweener = text.DOText(npcText, npcText.Length * 0.02f).SetEase(Ease.Linear).OnComplete(() =>
        {
            if(cnt == selectQuest.questDescription.Count - 1)
            {
                GetButton((int)Buttons.YesBtn).gameObject.SetActive(true);
                GetButton((int)Buttons.NoBtn).gameObject.SetActive(true);
            }
        });
    }
    bool QuestClear = false;
    public void QuestFinish(QuestData quest)
    {
        GetText((int)Texts.DialogueText).text = ""; QuestList.Clear();
        foreach (Transform child in GetObject((int)GameObjects.Content).transform)
            Destroy(child.gameObject);
        Managers.Object.MyPlayer.QuestTrigger = true;
        GetObject((int)GameObjects.ScrollView).SetActive(false);
        GetButton((int)Buttons.YesBtn).gameObject.SetActive(false);
        GetButton((int)Buttons.NoBtn).gameObject.SetActive(false);
        selectQuest = quest;
        QuestTrigger = false;
        QuestEnd = false;
        QuestClear = false;
        GetText((int)Texts.DialogueText).DOText(quest.questClearString, quest.questClearString.Length * 0.02f).OnComplete(() => { QuestEnd = true; QuestClear = true; });
    }
    public void QuestNotYetFinish(QuestData quest)
    {
        GetText((int)Texts.DialogueText).text = ""; QuestList.Clear();
        foreach (Transform child in GetObject((int)GameObjects.Content).transform)
            Destroy(child.gameObject);
        Managers.Object.MyPlayer.QuestTrigger = true;
        GetObject((int)GameObjects.ScrollView).SetActive(false);
        GetButton((int)Buttons.YesBtn).gameObject.SetActive(false);
        GetButton((int)Buttons.NoBtn).gameObject.SetActive(false);
        QuestTrigger = false;
        GetText((int)Texts.DialogueText).DOText(quest.questNonClearString, quest.questNonClearString.Length * 0.02f).OnComplete(() => { QuestEnd = true; });
    }
}

