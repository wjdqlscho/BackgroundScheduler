using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using Valve.VR;
using Valve.VR.InteractionSystem;  // VelocityEstimator가 이 네임스페이스에 있습니다.


#if UNITY_EDITOR
using UnityEditor;
#endif

public class TrackerTcpSender : MonoBehaviour
{
    [Header("Tracker Objects")]
    public GameObject primary;   // Left Foot Tracker
    public GameObject secondary; // Right Foot Tracker

    [Header("Network Settings")]
    public string serverIP = "192.168.66.226"; // 서버 IP
    public int serverPort = 9000;              // 서버 포트

    [Header("IMU Settings")]
    public VelocityEstimator primaryVelocityEstimator;  // Primary의 VelocityEstimator
    public VelocityEstimator secondaryVelocityEstimator; // Secondary의 VelocityEstimator

    [Header("Debug Settings")]
    public bool debugMode = true;  // 상세 로그 출력

    private TcpClient client;
    private NetworkStream stream;
    public bool isSending = false;

    private int updateCounter = 0;  // 로그 빈도 제어

    void Start()
    {
        // 기본 검증
        if (primary == null || secondary == null)
        {
            Debug.LogError("Please assign Primary and Secondary Tracker objects in the Inspector!");
            return;
        }

        // VelocityEstimator 컴포넌트 가져오기
        if (primaryVelocityEstimator == null)
        {
            primaryVelocityEstimator = primary.GetComponent<VelocityEstimator>();
            if (primaryVelocityEstimator == null)
            {
                Debug.LogError("Primary VelocityEstimator component is missing on primary tracker object.");
                return;
            }
        }

        if (secondaryVelocityEstimator == null)
        {
            secondaryVelocityEstimator = secondary.GetComponent<VelocityEstimator>();
            if (secondaryVelocityEstimator == null)
            {
                Debug.LogError("Secondary VelocityEstimator component is missing on secondary tracker object.");
                return;
            }
        }

        if (string.IsNullOrEmpty(serverIP) || serverPort <= 0 || serverPort > 65535)
        {
            Debug.LogError("Invalid server IP or port settings!");
            return;
        }

        Debug.Log("Tracker setup completed. Waiting for SteamVR connection...");
    }

    void Update()
    {
        updateCounter++;

        if (isSending && client != null && client.Connected)
        {
            // 트래커 위치 가져오기
            Vector3 posPrimary = primary != null ? primary.transform.position : Vector3.zero;
            Vector3 posSecondary = secondary != null ? secondary.transform.position : Vector3.zero;

            // 속도, 각속도, 가속도 추출 (VelocityEstimator에서 추출)
            Vector3 velocityPrimary = primaryVelocityEstimator.GetVelocityEstimate();
            Vector3 angularVelocityPrimary = primaryVelocityEstimator.GetAngularVelocityEstimate();
            Vector3 accelerationPrimary = primaryVelocityEstimator.GetAccelerationEstimate();

            Vector3 velocitySecondary = secondaryVelocityEstimator.GetVelocityEstimate();
            Vector3 angularVelocitySecondary = secondaryVelocityEstimator.GetAngularVelocityEstimate();
            Vector3 accelerationSecondary = secondaryVelocityEstimator.GetAccelerationEstimate();

            // 좌표 변환 (SteamVR 좌표계 -> Unity 좌표계)
            Vector3 convertedPrimary = new Vector3(posPrimary.x, posPrimary.z, posPrimary.y);
            Vector3 convertedSecondary = new Vector3(posSecondary.x, posSecondary.z, posSecondary.y);

            // 데이터 포맷팅
            string timestamp = DateTime.UtcNow.ToString("o");
            string message = string.Format("{0} | Primary Position:{1:F3},{2:F3},{3:F3} | Secondary Position:{4:F3},{5:F3},{6:F3} | " +
                "Primary Velocity:{7:F3},{8:F3},{9:F3} | Primary Angular Velocity: {10:F3},{11:F3},{12:F3} | Primary Acceleration: {13:F3},{14:F3},{15:F3} | " +
                "Secondary Velocity:{16:F3},{17:F3},{18:F3} | Secondary Angular Velocity: {19:F3},{20:F3},{21:F3} | Secondary Acceleration: {22:F3},{23:F3},{24:F3}",
                timestamp,
                convertedPrimary.x, convertedPrimary.y, convertedPrimary.z,
                convertedSecondary.x, convertedSecondary.y, convertedSecondary.z,
                velocityPrimary.x, velocityPrimary.y, velocityPrimary.z,
                angularVelocityPrimary.x, angularVelocityPrimary.y, angularVelocityPrimary.z,
                accelerationPrimary.x, accelerationPrimary.y, accelerationPrimary.z,
                velocitySecondary.x, velocitySecondary.y, velocitySecondary.z,
                angularVelocitySecondary.x, angularVelocitySecondary.y, angularVelocitySecondary.z,
                accelerationSecondary.x, accelerationSecondary.y, accelerationSecondary.z);

            byte[] data = Encoding.UTF8.GetBytes(message + "\n");

            // 데이터 전송
            try
            {
                stream.Write(data, 0, data.Length);
                if (debugMode && updateCounter % 30 == 0)
                {
                    Debug.Log("Data sent: " + message);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to send data: " + e.Message);
                isSending = false;
                CloseConnection();
            }
        }
    }

    public void OnSendButtonClick()
    {
        try
        {
            if (client == null || !client.Connected)
            {
                client = new TcpClient(serverIP, serverPort);
                stream = client.GetStream();
                Debug.Log("TCP connection established: " + serverIP + ":" + serverPort);
            }
            isSending = true;
            Debug.Log("Started sending data...");
        }
        catch (Exception e)
        {
            Debug.LogError("TCP connection failed: " + e.Message);
        }
    }

    private void CloseConnection()
    {
        if (stream != null)
        {
            stream.Close();
            stream = null;
        }
        if (client != null)
        {
            client.Close();
            client = null;
        }
        Debug.Log("TCP connection closed");
    }

    void OnApplicationQuit()
    {
        isSending = false;
        CloseConnection();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TrackerTcpSender))]
public class TrackerTcpSenderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        TrackerTcpSender trackerSender = (TrackerTcpSender)target;
        if (GUILayout.Button(trackerSender.isSending ? "Stop Sending" : "Start Sending"))
        {
            trackerSender.OnSendButtonClick();
        }
    }
}
#endif
