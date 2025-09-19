using UnityEngine;
using UnityEditor;
using System;
using System.Net.Sockets;
using System.Text;
using Valve.VR;

public class TrackerTcpSender : MonoBehaviour
{
    public GameObject primary;   // 첫 번째 트래커
    public GameObject secondary; // 두 번째 트래커

    public string serverIP = "192.168.66.226"; // 서버 IP
    public int serverPort = 9000;              // 서버 포트

    private TcpClient client;
    private NetworkStream stream;
    private bool isSending = false;

    public SteamVR_Behaviour_Pose primaryPose;  // Primary 트래커의 Pose 컴포넌트
    public SteamVR_Behaviour_Pose secondaryPose; // Secondary 트래커의 Pose 컴포넌트

    void Start()
    {
        // Tracker 상태 확인
        if (primaryPose != null)
        {
            primaryPose.inputSource = SteamVR_Input_Sources.Any; // 필요시 LeftHand 등으로 변경
            if (primaryPose.isValid)
                Debug.Log("Primary Tracker is valid and tracked.");
            else
                Debug.LogWarning("Primary Tracker is not valid! Check SteamVR settings or Tracker role.");
        }
        else
        {
            Debug.LogError("Primary Pose component is missing!");
        }

        if (secondaryPose != null)
        {
            secondaryPose.inputSource = SteamVR_Input_Sources.Any; // 필요시 RightHand 등으로 변경
            if (secondaryPose.isValid)
                Debug.Log("Secondary Tracker is valid and tracked.");
            else
                Debug.LogWarning("Secondary Tracker is not valid! Check SteamVR settings or Tracker role.");
        }
        else
        {
            Debug.LogError("Secondary Pose component is missing!");
        }

        // OpenVR로 device index 확인 (디버깅용)
        var system = OpenVR.System;
        if (system != null)
        {
            uint leftHandIndex = system.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.LeftHand);
            uint rightHandIndex = system.GetTrackedDeviceIndexForControllerRole(ETrackedControllerRole.RightHand);
            Debug.Log($"LeftHand Device Index: {leftHandIndex}, RightHand Device Index: {rightHandIndex}");
        }
        else
        {
            Debug.LogError("OpenVR System not initialized! Ensure SteamVR is running.");
        }
    }

    void Update()
    {
        if (isSending && client != null && client.Connected)
        {
            // 위치 변환
            Vector3 posPrimary = primary.transform.position;
            Vector3 posSecondary = secondary.transform.position;
            Vector3 convertedPrimary = new Vector3(posPrimary.x, posPrimary.z, posPrimary.y);
            Vector3 convertedSecondary = new Vector3(posSecondary.x, posSecondary.z, posSecondary.y);

            // 타임스탬프
            string timestamp = DateTime.UtcNow.ToString("o");

            // Primary IMU 데이터
            Vector3 velocityPrimary = Vector3.zero;
            Vector3 angularVelocityPrimary = Vector3.zero;
            if (primaryPose != null && primaryPose.isValid)
            {
                velocityPrimary = primaryPose.GetVelocity();
                angularVelocityPrimary = primaryPose.GetAngularVelocity();
                if (velocityPrimary == Vector3.zero && angularVelocityPrimary == Vector3.zero)
                {
                    Debug.LogWarning("Primary IMU is zero - Ensure Tracker is moving and set to 'Held in Hand' in SteamVR.");
                }
            }
            else
            {
                Debug.LogWarning("Primary Pose is invalid or not assigned.");
            }

            // Secondary IMU 데이터
            Vector3 velocitySecondary = Vector3.zero;
            Vector3 angularVelocitySecondary = Vector3.zero;
            if (secondaryPose != null && secondaryPose.isValid)
            {
                velocitySecondary = secondaryPose.GetVelocity();
                angularVelocitySecondary = secondaryPose.GetAngularVelocity();
                if (velocitySecondary == Vector3.zero && angularVelocitySecondary == Vector3.zero)
                {
                    Debug.LogWarning("Secondary IMU is zero - Ensure Tracker is moving and set to 'Held in Hand' in SteamVR.");
                }
            }
            else
            {
                Debug.LogWarning("Secondary Pose is invalid or not assigned.");
            }

            // 메시지 포맷
            string message = string.Format("{0} | Primary:{1:F3},{2:F3},{3:F3} | Secondary:{4:F3},{5:F3},{6:F3} | Primary Velocity: {7} | Primary Angular: {8} | Secondary Velocity: {9} | Secondary Angular: {10}",
                timestamp,
                convertedPrimary.x, convertedPrimary.y, convertedPrimary.z,
                convertedSecondary.x, convertedSecondary.y, convertedSecondary.z,
                velocityPrimary.ToString("F2"),
                angularVelocityPrimary.ToString("F2"),
                velocitySecondary.ToString("F2"),
                angularVelocitySecondary.ToString("F2"));

            byte[] data = Encoding.UTF8.GetBytes(message + "\n");

            Debug.Log("전송할 데이터: " + message);

            try
            {
                stream.Write(data, 0, data.Length);
                Debug.Log("데이터 전송 성공");
            }
            catch (Exception e)
            {
                Debug.LogError("데이터 전송 실패: " + e.Message);
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
                Debug.Log("TCP 연결 성공");
            }
            isSending = true;
            Debug.Log("전송 시작");
        }
        catch (Exception e)
        {
            Debug.LogError("TCP 연결 실패: " + e.Message);
        }
    }

    void OnApplicationQuit()
    {
        if (stream != null) stream.Close();
        if (client != null) client.Close();
        Debug.Log("TCP 연결 종료");
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
        if (GUILayout.Button("전송 시작"))
        {
            trackerSender.OnSendButtonClick();
        }
    }
}
#endif
