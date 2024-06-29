using UnityEngine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    MyPlayerController player;
    public float Yaxis;
    public float Xaxis;
    public bool NpcTrigger = false;
    Vector3 prevPos;
    Vector3 prevRotate;
    public void Awake()
    {

    }

    private float rotSensitive = 3f;
    public float dis = 15f;
    public float RotationMin = 20f;
    public float RotationMax = 50f;
    private float smoothTime = 0.12f;


    private Vector3 targetRotation;
    private Vector3 currentVel;

    void LateUpdate()
    {
        if (player == null) return;
        if (NpcTrigger) return;
        if (Input.GetMouseButton(2))
        {
            Yaxis += Input.GetAxis("Mouse X") * rotSensitive;
            Xaxis -= Input.GetAxis("Mouse Y") * rotSensitive;

            Xaxis = Mathf.Clamp(Xaxis, RotationMin, RotationMax);

            targetRotation = Vector3.SmoothDamp(targetRotation, new Vector3(Xaxis, Yaxis), ref currentVel, smoothTime);
            this.transform.eulerAngles = targetRotation;
        }
        transform.position = player.transform.position - transform.forward * dis;
        prevPos = transform.position;
        prevRotate = transform.rotation.eulerAngles;
    }
    public void SettingPlayer(MyPlayerController myPlayer)
    {
        player = myPlayer;
    }
    public void PlayerToNpcMove(Vector3 pos, Vector3 rotate, GameObject npc)
    {
        transform.DOMove(pos, 1).SetEase(Ease.OutQuad).OnComplete(() => { npc.transform.DOLookAt(transform.position, 0.5f, AxisConstraint.Y); });
        transform.DORotate(rotate, 1).SetEase(Ease.OutQuad);
    }
    public void NpcToPlayerMove()
    {
        transform.DOMove(prevPos, 1).SetEase(Ease.OutQuad);
        transform.DORotate(prevRotate, 1).SetEase(Ease.OutQuad).OnComplete(() => { NpcTrigger = false; });
    }
}
