using Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestNpc : NPCController
{
    public Vector3 SetCmPos = new Vector3(0, 1, -2.2f);
    public Vector3 SetCmRot = Vector3.zero;
    List<QuestData> questDatas = new List<QuestData>();
    public NpcData npcData = null;
    public override void OpenNpc()
    {
        base.OpenNpc();
    }
    public override void CloseNpc()
    {
        base.CloseNpc();
    }
    public override void CameraSetting()
    {
        Vector3 cameraPos = transform.position + SetCmPos;
        Vector3 rotate = SetCmRot;

        Camera.main.GetComponent<CameraController>().PlayerToNpcMove(cameraPos, rotate, gameObject);
    }
    public override void OpenNpcUI()
    {
        if(questDatas.Count <= 0)
        {
            if (Managers.Data.NpcDict.TryGetValue(templateId, out npcData) == false)
                return;
            List<int> ids = npcData.npcQuestLists;
            foreach (int id in ids)
            {
                if (Managers.Data.QuestDict.TryGetValue(id, out QuestData questData) == false)
                    return;
                questDatas.Add(questData);
            }
        }
        Managers.UI.ShowPopupUI<UI_QuestDialogue_Popup>().Setting(this, questDatas);
    }
    public override void CloseNpcUI()
    {
        base.CloseNpcUI();
    }
}
