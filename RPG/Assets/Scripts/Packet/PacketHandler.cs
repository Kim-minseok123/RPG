using EasyTransition;
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Playables;

class PacketHandler
{
    public static void S_PingHandler(PacketSession session, IMessage packet)
    {
        C_Pong pongPacket = new C_Pong();
        Managers.Network.Send(pongPacket);
    }
    public static void S_ConnectedHandler(PacketSession session, IMessage packet)
    {
        C_Login loginPacket = new C_Login();
        loginPacket.AccountId = Managers.Network.AccountId;
        loginPacket.Token = Managers.Network.Token;
        Managers.Network.Send(loginPacket);
    }
    public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
    {
        S_LeaveGame leaveGameHandler = packet as S_LeaveGame;
        Managers.Object.Clear();
    }
    public static void S_LoginHandler(PacketSession session, IMessage packet)
	{
		S_Login loginPacket = (S_Login)packet;
		Debug.Log($"LoginOk({loginPacket.LoginOk})");
        Managers.Network.LobbyPlayerInfos.Clear();
        var lobbyPlayers = loginPacket.Players;

        foreach (var player in lobbyPlayers)
        {
            Managers.Network.LobbyPlayerInfos.Add(player);
        }
#if UNITY_SERVER
        C_CreatePlayer createPlayerPacket = new C_CreatePlayer();
        createPlayerPacket.Name = "Master";
        createPlayerPacket.IsMale = true;
        Managers.Network.Send(createPlayerPacket);

#else
        if (Managers.Network.Master == true) return;
        TransitionSettings ts = Managers.Resource.Load<TransitionSettings>("Trans/LinearWipe");
        TransitionManager.Instance().Transition(Define.Scene.Lobby, ts, 0);
        Managers.UI.CloseAllPopupUI();
#endif
    }
    public static void S_BanishHandler(PacketSession session, IMessage packet)
    {
        Managers.Network._session.Disconnect();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // 어플리케이션 종료
#endif
    }
    public static void S_CreatePlayerHandler(PacketSession session, IMessage packet)
    {
        S_CreatePlayer createOkPacket = (S_CreatePlayer)packet;
#if UNITY_SERVER
            Managers.Scene.LoadScene(Define.Scene.Game);
            C_EnterGame enterGamePacket = new C_EnterGame();
            enterGamePacket.Name = "Master";
            Managers.Network.Send(enterGamePacket);
#else
        if (createOkPacket.Player == null)
        {
            Managers.UI.FindPopupUI<UI_CreatePlayer_Popup>().isClick = false;
            Managers.UI.ShowPopupUI<UI_Confirm_Popup>().Setting("닉네임이 중복됩니다.\n 다른 닉네임을 사용해주세요.");
        }
        else
        {
            Managers.Network.LobbyPlayerInfos.Add(createOkPacket.Player);
            Managers.UI.FindPopupUI<UI_CreatePlayer_Popup>().EndCreate();
        }
#endif

    }
    public static void S_EnterGameHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame enterGamePacket = (S_EnterGame)packet;
        
        Managers.Object.Add(enterGamePacket.Player, myPlayer: true);
    }
    public static void S_SpawnHandler(PacketSession session, IMessage packet)
    {
        S_Spawn spawnPacket = (S_Spawn)packet;
        foreach (ObjectInfo obj in spawnPacket.Objects)
        {
            Managers.Object.Add(obj, myPlayer: false);
        }
    }
    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
        S_Despawn despawnPacket = packet as S_Despawn;
        foreach (int id in despawnPacket.ObjectIds)
        {
            Managers.Object.Remove(id);
        }
    }
    public static void S_MoveHandler(PacketSession session, IMessage packet)
    {
        S_Move movePacket = (S_Move)packet;

        GameObject go = Managers.Object.FindById(movePacket.ObjectId);
        if (go == null)
            return;
        BaseController bc = go.GetComponent<BaseController>();
        if (bc == null)
            return;
        GameObject mosterTarget = null;
        if (movePacket.TargetId != -1) mosterTarget = Managers.Object.FindById(movePacket.TargetId);
        Vector3 target = new Vector3(movePacket.DestPosInfo.Pos.PosX, movePacket.DestPosInfo.Pos.PosY, movePacket.DestPosInfo.Pos.PosZ);
        bc.MoveTarget(target, mosterTarget);
    }
    public static void S_StopMoveHandler(PacketSession session, IMessage packet)
    {
        S_StopMove stopMovePacket = (S_StopMove)packet;

        GameObject go = Managers.Object.FindById(stopMovePacket.ObjectId);
        if (go == null)
            return;
        BaseController bc = go.GetComponent<BaseController>();
        if (bc == null)
            return;
        if (stopMovePacket.PosOk == false) return;
        bc.StopMove(
            new Vector3(stopMovePacket.Rotate.RotateX, stopMovePacket.Rotate.RotateY, stopMovePacket.Rotate.RotateZ),
            new Vector3(stopMovePacket.Pos.PosX, stopMovePacket.Pos.PosY, stopMovePacket.Pos.PosZ)
            );
    }
    public static void S_ChangeHpHandler(PacketSession session, IMessage packet)
    {
        S_ChangeHp changeHpPacket = (S_ChangeHp)packet;

        GameObject go = Managers.Object.FindById(changeHpPacket.ObjectId);
        if (go == null)
            return;
        BaseController bc = go.GetComponent<BaseController>();
        if (bc == null)
            return;
        bc.ChangeHp(changeHpPacket.Hp, changeHpPacket.IsHeal, changeHpPacket.ChangeHp);
    }
    public static void S_SkillMotionHandler(PacketSession session, IMessage packet)
    {
        S_SkillMotion skillMotionPacket = (S_SkillMotion)packet;   

        GameObject go = Managers.Object.FindById(skillMotionPacket.ObjectId);
        if (go == null)
            return;
        BaseController bc = go.GetComponent<BaseController>();
        if (bc == null)
            return;
        bc.OnAttack(skillMotionPacket.Info);
    }
    public static void S_AddItemHandler(PacketSession session, IMessage packet)
    {
        S_AddItem itemList = (S_AddItem)packet;

        // 메모리에 아이템 정보 적용
        foreach (ItemInfo itemInfo in itemList.Items)
        {
            Item item = Item.MakeItem(itemInfo);
            Managers.Inven.Add(item);
        }
        if (itemList.Money != 0)
            Managers.Inven.AddMoney(itemList.Money);

        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        gameSceneUI.InvenUI.RefreshUI();
        gameSceneUI.StatUI.RefreshUI();
        gameSceneUI.EquipUI.RefreshUI();
        gameSceneUI.DrawQuickSlot();

        if (Managers.Object.MyPlayer != null)
            Managers.Object.MyPlayer.RefreshAdditionalStat();
        var uiNpc = Managers.UI.FindPopupUI<UI_NpcSell_Popup>();
        if (uiNpc != null)
            uiNpc.RefreshUI();
    }
    public static void S_DieHandler(PacketSession session, IMessage packet)
    {
        S_Die diePakcet = (S_Die)packet;

        GameObject go = Managers.Object.FindById(diePakcet.ObjectId);
        GameObject attacker = Managers.Object.FindById(diePakcet.AttackerId);
        if (go == null || attacker == null)
            return;
        BaseController bc = go.GetComponent<BaseController>();
        if (bc == null)
            return;
        bc.OnDead(attacker);
        if(bc.Id == Managers.Object.MyPlayer.Id)
        {
            if (Camera.main.GetComponent<AudioListener>() != null)
            {
                Camera.main.GetComponent<AudioListener>().enabled = true;
            }
            Managers.Object.MyPlayer.GetComponent<AudioListener>().enabled = false;
            Managers.Object.MyPlayer = null;
        }
           
        if (bc.GetComponent<PlayerController>() != null)
            Managers.Object.Remove(bc.Id, false);
    }
    public static void S_ItemListHandler(PacketSession session, IMessage packet)
    {
        S_ItemList itemList = (S_ItemList)packet;

        Managers.Inven.Clear();
        Managers.Inven.EquipClear();

        // 메모리에 아이템 정보 적용
        foreach (ItemInfo itemInfo in itemList.Items)
        {
            Item item = Item.MakeItem(itemInfo);
            if(item.Equipped == true)
            {
                Managers.Inven.EquipAdd(item.Slot, item);
            }
            else
            {
                Managers.Inven.Add(item);
            }
        }
        
        Managers.Inven.Money = itemList.Money;
        if (Managers.Object.MyPlayer != null)
            Managers.Object.MyPlayer.RefreshAdditionalStat();
    }
    public static void S_MotionOrEffectHandler(PacketSession session, IMessage packet)
    {
        S_MotionOrEffect motionPacket = (S_MotionOrEffect)packet;

        GameObject go = Managers.Object.FindById(motionPacket.ObjectId);
        if (go == null)
            return;
        PlayerController pc = go.GetComponent<PlayerController>();
        if (pc == null)
            return;
        pc.StartMotionOrEffect(motionPacket.ActionName);
    }
    public static void S_EquipItemListHandler(PacketSession session, IMessage packet)
    {
        S_EquipItemList equipItemList = (S_EquipItemList)packet;

        GameObject go = Managers.Object.FindById(equipItemList.ObjectId);
        if (go == null) return;
        PlayerController pc = go.GetComponent<PlayerController>();
        if (pc == null)
            return;
        foreach (var id in equipItemList.TemplateIds)
        {
            pc.EquipItem(id);
        }
    }
    public static void S_EquipItemHandler(PacketSession session, IMessage packet)
    {
        S_EquipItem equipItem = (S_EquipItem)packet;

        GameObject go = Managers.Object.FindById(equipItem.ObjectId);
        if (go == null)
            return;
        PlayerController pc = go.GetComponent<PlayerController>();
        if (pc == null)
            return;

        if (equipItem.Equipped)
        {
            if(Managers.Object.MyPlayer.Id == equipItem.ObjectId)
            {
                Item item = Managers.Inven.Get(equipItem.ItemDbId);
                Managers.Inven.Remove(item);
                Managers.Inven.EquipAdd(equipItem.Slot, item);
                item.Equipped = equipItem.Equipped;
                item.Slot = equipItem.Slot;
                if (Managers.Object.MyPlayer != null)
                    Managers.Object.MyPlayer.RefreshAdditionalStat();

                UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
                gameSceneUI.InvenUI.RefreshUI();
                gameSceneUI.StatUI.RefreshUI();
                gameSceneUI.EquipUI.RefreshUI();
                
            }
            pc.EquipItem(equipItem.TemplateId);
        }
        else
        {
            if (Managers.Object.MyPlayer.Id == equipItem.ObjectId)
            {
                Item item = Managers.Inven.EquipFind(i => i.ItemDbId == equipItem.ItemDbId);
                Managers.Inven.EquipRemove(item.Slot);
                Managers.Inven.Add(item);
                item.Equipped = equipItem.Equipped;
                UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
                item.Slot = equipItem.NextSlot;
                if (Managers.Object.MyPlayer != null)
                    Managers.Object.MyPlayer.RefreshAdditionalStat();

                gameSceneUI.InvenUI.RefreshUI();
                gameSceneUI.StatUI.RefreshUI();
                gameSceneUI.EquipUI.RefreshUI();
            }
            pc.Disarm(equipItem.Slot);
        }
    }
    public static void S_ChangeStatHandler(PacketSession session, IMessage packet)
    {
        S_ChangeStat changeStatPacket = (S_ChangeStat)packet;

        MyPlayerController myPlayer = Managers.Object.MyPlayer;
        if (myPlayer == null) return;

        myPlayer.SetStat(changeStatPacket.StatInfo);
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        gameSceneUI.StatUI.RefreshUI();
        gameSceneUI.SkillUI.RefreshUI();
        gameSceneUI.ChangeHpOrMp();
        gameSceneUI.ChangeExp();

    }
    public static void S_ChangeConsumableItemHandler(PacketSession session, IMessage packet)
    {
        S_ChangeConsumableItem cmItem = (S_ChangeConsumableItem)packet;

        Item item = Managers.Inven.Get(cmItem.ItemDbId);
        if (item == null) return;
        item.Count = cmItem.Count;

        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        gameSceneUI.InvenUI.RefreshUI();
        gameSceneUI.DrawQuickSlot();
    }
    public static void S_UseItemHandler(PacketSession session, IMessage packet)
    {
        S_UseItem useItemPacket = (S_UseItem)packet;

        Item item = Managers.Inven.Get(useItemPacket.ItemDbId);
        if(item == null) return;
        item.Count = useItemPacket.Count;
        if(item.Count <= 0)
        {
            Managers.Inven.Remove(item);
        }
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        Managers.Object.MyPlayer.SetStat(useItemPacket.StatInfo);
        gameSceneUI.ChangeHpOrMp();
        gameSceneUI.ChangeExp();
        gameSceneUI.DrawQuickSlot();
        gameSceneUI.InvenUI.RefreshUI();
    }
    public static void S_SkillListHandler(PacketSession session, IMessage packet)
    {
        S_SkillList skillList = (S_SkillList)packet;
        MyPlayerController myPlayer = Managers.Object.MyPlayer;

        if (myPlayer == null)
            return;
        myPlayer.HaveSkillData.Clear();
        foreach (SkillInfo skill in skillList.Skills)
        {
            myPlayer.HaveSkillData.Add(skill.SkillId, skill.Level);
        }
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        gameSceneUI.SkillUI.RefreshUI();
    }
    public static void S_SkillLevelUpHandler(PacketSession session, IMessage packet)
    {
        S_SkillLevelUp skillLevelUp = (S_SkillLevelUp)packet;
        MyPlayerController myPlayer = Managers.Object.MyPlayer;

        if(myPlayer == null) return;

        myPlayer.Stat.SkillPoint--;
        SkillInfo skillInfo = skillLevelUp.Skill;

        if (skillLevelUp.IsNew)
        {
            myPlayer.HaveSkillData.Add(skillInfo.SkillId, skillInfo.Level);
        }
        else
        {
            myPlayer.HaveSkillData[skillInfo.SkillId] = skillInfo.Level;
        }
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        gameSceneUI.SkillUI.RefreshUI();
    }
    public static void S_QuickSlotHandler(PacketSession session, IMessage packet)
    {
        S_QuickSlot quickSlot = (S_QuickSlot)packet;
        foreach (var info in quickSlot.Info)
        {
            if (Util.IsAllLetters(info.SlotName))
                (Managers.UI.SceneUI as UI_GameScene).RegisterQuickSlot(info.SlotName, info.TemplateId, true);
            else
                (Managers.UI.SceneUI as UI_GameScene).RegisterQuickSlot(info.SlotName, info.TemplateId, false);
        }
    }
    public static void S_ChangeItemSlotHandler(PacketSession session, IMessage packet)
    {
        S_ChangeItemSlot itemSlot = (S_ChangeItemSlot)packet;

        if(itemSlot.SlotOne == -1 && itemSlot.ItemDbIdTwo != -1)
        {
            Item item1 = Managers.Inven.Get(itemSlot.ItemDbIdOne);
            Item item2 = Managers.Inven.Get(itemSlot.ItemDbIdTwo);
            if (item1 == null || item2 == null) return;
            Managers.Inven.Remove(item1);
            item2.Count = itemSlot.CountTwo;
            item2.Slot = itemSlot.SlotTwo;
        }
        else
        {
            // 빈 곳에 아이템 옮김
            if(itemSlot.ItemDbIdTwo == -1)
            {
                Item item = Managers.Inven.Get(itemSlot.ItemDbIdOne);
                if (item == null) return;
                item.Count = itemSlot.CountOne;
                item.Slot = itemSlot.SlotOne;
            }
            else
            {
                Item item1 = Managers.Inven.Get(itemSlot.ItemDbIdOne);
                Item item2 = Managers.Inven.Get(itemSlot.ItemDbIdTwo);
                if (item1 == null || item2 == null) return;
                item1.Count = itemSlot.CountOne;
                item1.Slot = itemSlot.SlotOne;
                item2.Count = itemSlot.CountTwo;
                item2.Slot = itemSlot.SlotTwo;
            }
        }
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        gameSceneUI.InvenUI.RefreshUI();
    }
    public static void S_RemoveItemHandler(PacketSession session, IMessage packet)
    {
        S_RemoveItem removeItem = (S_RemoveItem)packet;

        foreach (var item in removeItem.Items)
        {
            if(item.Count <= 0)
            {
                Item reItem = Managers.Inven.Get(item.ItemDbId);
                if (reItem == null)
                    return;
                Managers.Inven.Remove(reItem);
            }
            else
            {
                Item reItem = Managers.Inven.Get(item.ItemDbId);
                if (reItem == null)
                    return;
                reItem.Count = item.Count;
            }
        }
        Managers.Inven.AddMoney(removeItem.Money);

        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        gameSceneUI.InvenUI.RefreshUI();
        gameSceneUI.EquipUI.RefreshUI();
        gameSceneUI.DrawQuickSlot();
        var uiNpc = Managers.UI.FindPopupUI<UI_NpcSell_Popup>();
        if (uiNpc != null)
            uiNpc.RefreshUI();
    }
    public static void S_MessageHandler(PacketSession session, IMessage packet)
    {
        S_Message messagePacket = (S_Message)packet;
        UI_SceneConfirm_Popup go = Managers.Resource.Instantiate("UI/Popup/UI_SceneConfirm_Popup").GetComponent<UI_SceneConfirm_Popup>();
        go.Setting(messagePacket.Message);
    }
    public static void S_ChangeMapHandler(PacketSession session, IMessage packet)
    {
        S_ChangeMap changeMapPacket = (S_ChangeMap)packet;
        Managers.UI.CloseAllPopupUI();

        TransitionSettings ts = Managers.Resource.Load<TransitionSettings>("Trans/LinearWipe");
        switch (changeMapPacket.MapName)
        {
            case "Lobby":
                Managers.Sound.Play("LoginBgm", Define.Sound.Bgm);
                TransitionManager.Instance().Transition(Define.Scene.Lobby, ts, 0);
                break;
            case "Game":
                Managers.NextScene = Define.Scene.Game;
                TransitionManager.Instance().Transition(Define.Scene.Loading, ts, 0);
                break;
            case "Boss":
                Managers.NextScene = Define.Scene.Boss;
                TransitionManager.Instance().Transition(Define.Scene.Loading, ts, 0);
                break;
        }
        Managers.Object.Clear();
    }
    public static void S_SetMasterClientHandler(PacketSession session, IMessage packet)
    {
        S_SetMasterClient masterClientPacket = (S_SetMasterClient)packet;
        MyPlayerController myPlayer = Managers.Object.MyPlayer;
        if (myPlayer != null  && myPlayer.Id == masterClientPacket.ObjectId)
        {
            myPlayer.isMaster = true;
        }
        else
            myPlayer.isMaster = false;
    }
    public static void S_MakeMeteorObjectHandler(PacketSession session, IMessage packet)
    {
        S_MakeMeteorObject meteorPakcet = (S_MakeMeteorObject)packet;

        Vector3 spawnPoint = Util.PositionsToVector3(meteorPakcet.Pos);

        GameObject go = Managers.Object.FindById(meteorPakcet.ObjectId);
        if (go != null)
        {
            RedDragon redDragon = go.GetComponent<RedDragon>();
            if (redDragon != null)
                redDragon.SkillMeteor(spawnPoint);
        }
    }
    public static void S_BossItemCutSceneHandler(PacketSession session, IMessage packet)
    {
        Managers.Object.Clear();
        GameObject go = GameObject.Find("EndBossController");
        if (go != null)
        {
            PlayableDirector pd = go.GetComponent<PlayableDirector>();
            if(pd != null)
            {
                pd.Play();
            }
        }
    }
    public static void S_EndBossItemCutSceneHandler(PacketSession session, IMessage packet)
    {

    }
    public static void S_AddQuestHandler(PacketSession session, IMessage packet)
    {
        S_AddQuest addQuestPacket = (S_AddQuest)packet;
        Quest quest = Quest.MakeQuest(addQuestPacket.QuestId);
        if (quest == null) return;
        Managers.Quest.AddQuest(quest);
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        if (gameSceneUI != null)
        {
            gameSceneUI.QuestUI.RefreshUI();
        }
    }
    public static void S_ClearQuestHandler(PacketSession session, IMessage packet)
    {
        S_ClearQuest clearQuestPacket = (S_ClearQuest)packet;
        Quest quest = Managers.Quest.GetQuest(clearQuestPacket.QuestId, clearQuestPacket.QuestType);
        if (quest == null) return;
        Managers.Quest.RemoveQuest(quest);
        Managers.Quest.FinishQuest(quest);
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        if (gameSceneUI != null)
        {
            gameSceneUI.QuestUI.ResetCurrentItem(quest);
            gameSceneUI.QuestUI.RefreshUI();
        }
    }
    public static void S_QuestChangeValueHandler(PacketSession session, IMessage packet)
    {
        S_QuestChangeValue questChangeValue = (S_QuestChangeValue)packet;
        Quest quest = Managers.Quest.GetQuest(questChangeValue.QuestId, questChangeValue.QuestType);
        if (quest == null) return;

        switch (quest.QuestType)
        {
            case QuestType.Battle:
                {
                    BattleQuest battleQuest = (BattleQuest)quest;
                    battleQuest?.Update(new Data.BattleQuestGoals() { enemyId = questChangeValue.TemplateId, count = questChangeValue.Count });
                    battleQuest.IsFinish = questChangeValue.IsFinish;
                }
                break;
            case QuestType.Collection:
                {
                    CollectionQuest collectionQuest = (CollectionQuest)quest;
                    collectionQuest?.Update(new Data.CollectionQuestGoals() { collectionId = questChangeValue.TemplateId, count = questChangeValue.Count });
                    collectionQuest.IsFinish = questChangeValue.IsFinish;
                }
                break;
            case QuestType.Enter:
                {
                    EnterQuest enterQuest = (EnterQuest)quest;
                    enterQuest.IsFinish = questChangeValue.IsFinish;
                }
                break;
        }
        if(quest.IsFinish == true)
        {
            UI_SceneConfirm_Popup go = Managers.Resource.Instantiate("UI/Popup/UI_SceneConfirm_Popup").GetComponent<UI_SceneConfirm_Popup>();
            go.Setting($"<color=green>(퀘스트)</color>\n\n{quest.QuestName} <color=green>완료</color>");
        }
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        if (gameSceneUI != null)
        {
            gameSceneUI.QuestUI.RefreshUI();
        }
    }
    public static void S_AllQuestListHandler(PacketSession session, IMessage packet)
    {
        S_AllQuestList allQuestList = (S_AllQuestList)packet;

        foreach (var info in allQuestList.QuestList)
        {
            Quest quest = Quest.MakeQuest(info.QuestId);
            if (quest == null)
                return;
            
            switch (quest.QuestType)
            {
                case QuestType.Battle:
                    BattleQuest battleQuest = (BattleQuest)quest;
                    foreach (var goal in info.QuestGoal)
                        battleQuest.Update(new Data.BattleQuestGoals() { enemyId = goal.Id, count = goal.Count });
                    break;
                case QuestType.Collection:
                    CollectionQuest collectionQuest = (CollectionQuest)quest;
                    foreach (var goal in info.QuestGoal)
                        collectionQuest.Update(new Data.CollectionQuestGoals() { collectionId = goal.Id, count = goal.Count });
                    break;
                case QuestType.Enter:
                    break;
            }
            quest.IsFinish = info.IsFinish;
            if(info.IsCleared == true)
                Managers.Quest.FinishQuest(quest);
            else
                Managers.Quest.AddQuest(quest);
        }
        List<Quest> all = Managers.Quest.GetAllQuest();
        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        if (gameSceneUI != null)
        {
            gameSceneUI.QuestUI.RefreshUI();
        }
    }
    public static void S_ChattingHandler(PacketSession session, IMessage packet)
    {
        S_Chatting chatPacekt = (S_Chatting)packet;
        GameObject go = Managers.Object.FindById(chatPacekt.ObjectId);
        PlayerController pc = go.GetComponent<PlayerController>();
        if (pc == null) return;
        pc.MarkOutChat(chatPacekt.Content);

        UI_GameScene gameSceneUI = Managers.UI.SceneUI as UI_GameScene;
        if (gameSceneUI != null)
        {
            gameSceneUI.AddChatMessage($"{pc.objectInfo.Name} : {chatPacekt.Content}");
        }
    }
    public static void S_LookHandler(PacketSession session, IMessage packet)
    {
        S_Look look = (S_Look)packet;
        GameObject go = Managers.Object.FindById(look.ObjectId);
        if(go == null) return;
        BaseController bc = go.GetComponent<BaseController>();
        if (bc == null) return;
        bc.SetPos(rotate: look.Rotate);
    }
}


