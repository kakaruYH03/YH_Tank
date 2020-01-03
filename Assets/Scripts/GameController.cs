using SocketIO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] Transform targetObject;       // 타켓 포인트
    [SerializeField] GameObject tankPrefab;        // 탱크 Prefab

    private Tank playerTank;                       // 플레이어 탱크
    private Tank otherTank;                        // 상대방 탱크

    private float maxWidth = 100f;
    private float maxHeight = 100f;

    private SocketIOComponent socket;               // SocketIo 객체
    private ClientInfo clientInfo;                  // Client ID & 

    public enum GameState { None, Play, Over,}
    private GameState currentState;
    public GameState CurrentState
    {
        get
        {
            return currentState;
        }
        set
        {
            switch (value)
            {
                case GameState.None:
                    break;

                case GameState.Play:
                    CreatePlayerTank();
                    break;

                case GameState.Over:
                    break;
            }

            currentState = value;
        }
    }

    private void Start()
    {
        CurrentState = GameState.None;
        InitSocket();
    }

    void Update()
    {
        if (CurrentState == GameState.Play)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;
                if (Physics.Raycast(ray.origin, ray.direction, out hitInfo))
                {
                    Vector3 targetPosition = hitInfo.point;
                    targetObject.position = targetPosition;
                    playerTank.TargetPostion = targetPosition;

                    // TODO: 상대방에게 Player 탱크의 목적지를 알려준다

                    JSONObject positionData = new JSONObject();
                    positionData.AddField("x", targetPosition.x);
                    positionData.AddField("z", targetPosition.z);

                    JSONObject data = new JSONObject();
                    data.AddField("roomId", this.clientInfo.roomId);
                    data.AddField("clientId", this.clientInfo.clientId);
                    data.AddField("position", positionData);

                    socket.Emit("req_movetank", data);
                }
            }
        }
    }

    public bool StartGame(ClientInfo clientInfo)
    {
        if (clientInfo == null) return false;
        else
        {
            this.clientInfo = clientInfo;
            CurrentState = GameState.Play;

            return true;
        }
    }

    private void InitSocket()
    {
        GameObject socketObject = GameObject.Find("SocketIO");
        socket = socketObject.GetComponent<SocketIOComponent>();

        socket.On("res_othercreatetank", EventOtherCreateTank);
        socket.On("res_othermovetank", EventOtherMoveTank);
    }

    private void EventOtherCreateTank(SocketIOEvent e)
    {
        string clientId = e.data.GetField("clientId").str;
        JSONObject positionData = e.data.GetField("position");

        float x = 0f, z = 0f;

        positionData.GetField(ref x, "x");
        positionData.GetField(ref z, "z");

        otherTank = CreateTank(new Vector3(x, 0, z));
    }

    private void EventOtherMoveTank(SocketIOEvent e)
    {
        string clientId = e.data.GetField("clientId").str;
        JSONObject positionData = e.data.GetField("position");

        float x = 0f, z = 0f;

        positionData.GetField(ref x, "x");
        positionData.GetField(ref z, "z");

        if (otherTank)
        {
            otherTank.TargetPostion = new Vector3(x, 0, z);
        }
    }

    private void CreatePlayerTank()
    {
        Vector3 randomPostion = GetRandomPostion();
        playerTank = CreateTank(randomPostion);

        JSONObject positionData = new JSONObject();
        positionData.AddField("x", randomPostion.x);
        positionData.AddField("z", randomPostion.z);

        JSONObject data = new JSONObject();
        data.AddField("roomId", this.clientInfo.roomId);
        data.AddField("clientId", this.clientInfo.clientId);
        data.AddField("position", positionData);

        socket.Emit("req_createtank", data);
    }

    private Tank CreateTank(Vector3 postion)
    {
        Tank tank = Instantiate(tankPrefab, postion, Quaternion.identity).GetComponent<Tank>();
        return tank;
    }

    private Vector3 GetRandomPostion()
    {
        float randomX = Random.Range(-maxWidth / 2, maxWidth / 2);
        float randomZ = Random.Range(-maxHeight / 2, maxHeight / 2);

        return new Vector3(randomX, 0, randomZ);
    }
}

