using Data;
using DG.Tweening;
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
    public void AcceptQuest()
    {

    }
    public void DeclineQuest()
    {
        GetText((int)Texts.DialogueText).text = "";
        GetButton((int)Buttons.YesBtn).gameObject.SetActive(false);
        GetButton((int)Buttons.NoBtn).gameObject.SetActive(false);
        GetText((int)Texts.DialogueText).DOText(selectQuest.questRefuseString, selectQuest.questRefuseString.Length * 0.02f).OnComplete(() => { StartCoroutine(CoEndDialogue()); });
    }
    IEnumerator CoEndDialogue()
    {
        yield return new WaitForSeconds(2f);
        QuestTrigger = false;
        Managers.Object.MyPlayer.QuestTrigger = false;
        npc.CloseNpc();
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
}

