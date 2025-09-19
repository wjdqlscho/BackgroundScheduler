/*using UnityEngine;
using UnityEditor;
using System.Collections;


public class ObjectMover : MonoBehaviour
{
    // 인스펙터에서 받을 변수들
    public GameObject referenceObject; // 기준 오브젝트
    public GameObject primary;  // 이동할 첫 번째 오브젝트 (primary)
    public GameObject secondary;  // 이동할 두 번째 오브젝트 (secondary)
    
    public Vector3 targetPositionPrimary;   // 첫 번째 오브젝트 목표 위치 (primary)
    public Vector3 targetPositionSecondary;   // 두 번째 오브젝트 목표 위치 (secondary)
    
    public float targetRotationPrimaryY;    // 첫 번째 오브젝트 목표 회전 (primary, y)
    public float targetRotationSecondaryY;    // 두 번째 오브젝트 목표 회전 (secondary, y)

    // 기준 오브젝트를 두 오브젝트의 목표 위치와 회전으로 이동시키는 메서드
    public void MoveReferenceObject()
    {
        StartCoroutine(MoveWithDelay());
    }

    // 기준 오브젝트를 두 오브젝트의 목표 위치와 회전으로 이동시키는 메서드
    private IEnumerator MoveWithDelay()
    {
        Debug.Log("MoveReferenceObject() called");

        for (int i = 0; i < 10; i++) // 10번 반복
        {
            // 1. 회전 오차 계산 (primary가 secondary를 보는 방향)
        Vector3 direction = secondary.transform.position - primary.transform.position;
        float targetYaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        float rotationError = Mathf.DeltaAngle(referenceObject.transform.rotation.eulerAngles.y, targetYaw);

        // 2. 기준 오브젝트의 회전 적용
        referenceObject.transform.rotation = Quaternion.Euler(0, -rotationError, 0);

        // 한 프레임 대기
        yield return null;

        // 3. 위치 오차 계산 (기존 코드 유지)
        Vector3 positionErrorPrimary = targetPositionPrimary - primary.transform.position;
        Vector3 positionErrorSecondary = targetPositionSecondary - secondary.transform.position;
        Vector3 averagePositionError = (positionErrorPrimary + positionErrorSecondary) / 2;

        // 4. 기준 오브젝트를 위치 오차만큼 이동
        referenceObject.transform.position += averagePositionError;

        // 5. 이동값 출력
        Debug.Log("Iteration " + (i + 1));
        Debug.Log("rotationError: " + rotationError); // 수정: rotationError로 변경
        Debug.Log("Average Position Error: " + averagePositionError);
        }
    }
}

// CustomEditor는 Unity Editor의 기능이므로 UnityEditor 네임스페이스를 사용
[CustomEditor(typeof(ObjectMover))]
public class ObjectMoverEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 기본 Inspector 그리기
        DrawDefaultInspector();

        ObjectMover objectMover = (ObjectMover)target;

        // 버튼을 만들고 클릭하면 기준 오브젝트 이동
        if (GUILayout.Button("Move Reference Object"))
        {
            // 버튼 클릭 시 MoveReferenceObject() 메서드를 호출하여 기준 오브젝트 이동
            objectMover.MoveReferenceObject();
        }
    }
}


using UnityEngine;
using UnityEditor;
using System.Collections;

public class ObjectMover : MonoBehaviour
{
    // 인스펙터에서 받을 변수들
    public GameObject referenceObject;  // 기준 오브젝트
    public GameObject primary;           // 첫 번째 오브젝트 (primary)
    public GameObject secondary;         // 두 번째 오브젝트 (secondary)

    public Vector3 targetPositionPrimary;   // 첫 번째 오브젝트 목표 위치 (primary)
    public Vector3 targetPositionSecondary; // 두 번째 오브젝트 목표 위치 (secondary)

    public float targetRotationPrimaryY;    // 첫 번째 오브젝트 목표 회전 (primary, y)
    public float targetRotationSecondaryY;  // 두 번째 오브젝트 목표 회전 (secondary, y)

    // 기준 오브젝트를 두 오브젝트의 목표 위치와 회전으로 이동시키는 메서드
    public void MoveReferenceObject()
    {
        StartCoroutine(MoveWithDelay());
    }

    // 기준 오브젝트를 두 오브젝트의 목표 위치와 회전으로 이동시키는 코루틴
    private IEnumerator MoveWithDelay()
    {
        Debug.Log("MoveReferenceObject() called");

        // 10번 반복
        for (int i = 0; i < 10; i++)
        {
            // 1. 회전 오차 계산 (방향 오차)
            float rotationErrorPrimary = Mathf.DeltaAngle(primary.transform.rotation.eulerAngles.y, targetRotationPrimaryY); // primary 현재 Y축 회전값
            float rotationErrorSecondary = Mathf.DeltaAngle(secondary.transform.rotation.eulerAngles.y, targetRotationSecondaryY); // 목표로 지정한 Y축 회전값

            // 두 회전 오차의 평균 구하기(위에서 구한거)
            float averageRotationError = (rotationErrorPrimary + rotationErrorSecondary) / 2;

            // 2. 기준 오브젝트의 회전 적용
            referenceObject.transform.rotation = Quaternion.Euler(0, referenceObject.transform.rotation.eulerAngles.y + averageRotationError, 0);
            //현재 방향에서 평균 오차만큼 돌려서 보정하는 것

            // 한 프레임 대기 (회전 후 위치 계산을 다음 프레임에 하도록)
            yield return null; // 한 프레임 대기

            // 3. 위치 오차 계산 (위치 차이의 평균)a
            Vector3 positionErrorPrimary = targetPositionPrimary - primary.transform.position;
            Vector3 positionErrorSecondary = targetPositionSecondary - secondary.transform.position;

            // 두 위치 차이의 평균 구하기
            Vector3 averagePositionError = (positionErrorPrimary + positionErrorSecondary) / 2;

            // 4. 기준 오브젝트를 위치 오차만큼 이동
            referenceObject.transform.position += averagePositionError;

            // 5. 이동값 출력
            Debug.Log("Iteration " + (i + 1));
            Debug.Log("averageRotationError: " + averageRotationError);
            Debug.Log("Average Position Error: " + averagePositionError);
        }
    }
}

// CustomEditor는 Unity Editor의 기능이므로 UnityEditor 네임스페이스를 사용
[CustomEditor(typeof(ObjectMover))]
public class ObjectMoverEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 기본 Inspector 그리기
        DrawDefaultInspector();

        ObjectMover objectMover = (ObjectMover)target;

        // 버튼을 만들고 클릭하면 기준 오브젝트 이동
        if (GUILayout.Button("Move Reference Object"))
        {
            // 버튼 클릭 시 MoveReferenceObject() 메서드를 호출하여 기준 오브젝트 이동
            objectMover.MoveReferenceObject();
        }
    }
}
*/
using UnityEngine;
using UnityEditor;
using System.Collections;
using Valve.VR;

public class ObjectMover : MonoBehaviour
{
    // 인스펙터에서 받을 변수들
    public GameObject referenceObject;  // 기준 오브젝트
    public GameObject primary;           // 첫 번째 오브젝트 (primary)
    public GameObject secondary;         // 두 번째 오브젝트 (secondary)

    public Vector3 targetPositionPrimary;   // 첫 번째 오브젝트 목표 위치 (primary)
    public Vector3 targetPositionSecondary; // 두 번째 오브젝트 목표 위치 (secondary)

    public float targetRotationPrimaryY;    // 첫 번째 오브젝트 목표 회전 (primary, y)
    public float targetRotationSecondaryY;  // 두 번째 오브젝트 목표 회전 (secondary, y)

    public SteamVR_Action_Pose poseAction;  // Primary 트래커의 Pose 액션
    public SteamVR_Behaviour_Pose primaryPose;  // Primary 트래커의 Pose 컴포넌트
    public SteamVR_Behaviour_Pose secondaryPose; // Secondary 트래커의 Pose 컴포넌트

    private Vector3 lastVelocityPrimary;
    private Vector3 lastVelocitySecondary;

    // 기준 오브젝트를 두 오브젝트의 목표 위치와 회전으로 이동시키는 메서드
    public void MoveReferenceObject()
    {
        StartCoroutine(MoveWithDelay());
    }

    // 기준 오브젝트를 두 오브젝트의 목표 위치와 회전으로 이동시키는 코루틴
    public IEnumerator MoveWithDelay()
    {
        Debug.Log("MoveReferenceObject() called");

        // 10번 반복
        for (int i = 0; i < 10; i++)
        {
            // 1. 회전 오차 계산 (방향 오차)
            float rotationErrorPrimary = Mathf.DeltaAngle(primary.transform.rotation.eulerAngles.y, targetRotationPrimaryY); // primary 현재 Y축 회전값
            float rotationErrorSecondary = Mathf.DeltaAngle(secondary.transform.rotation.eulerAngles.y, targetRotationSecondaryY); // 목표로 지정한 Y축 회전값

            // 두 회전 오차의 평균 구하기(위에서 구한거)
            float averageRotationError = (rotationErrorPrimary + rotationErrorSecondary) / 2;

            // 2. 기준 오브젝트의 회전 적용
            referenceObject.transform.rotation = Quaternion.Euler(0, referenceObject.transform.rotation.eulerAngles.y + averageRotationError, 0);
            // 현재 방향에서 평균 오차만큼 돌려서 보정하는 것

            // 한 프레임 대기 (회전 후 위치 계산을 다음 프레임에 하도록)
            yield return null; // 한 프레임 대기

            // 3. 위치 오차 계산 (위치 차이의 평균)
            Vector3 positionErrorPrimary = targetPositionPrimary - primary.transform.position;
            Vector3 positionErrorSecondary = targetPositionSecondary - secondary.transform.position;

            // 두 위치 차이의 평균 구하기
            Vector3 averagePositionError = (positionErrorPrimary + positionErrorSecondary) / 2;

            // 4. 기준 오브젝트를 위치 오차만큼 이동
            referenceObject.transform.position += averagePositionError;

            // 5. 이동값 출력
            Debug.Log("Iteration " + (i + 1));
            Debug.Log("averageRotationError: " + averageRotationError);
            Debug.Log("Average Position Error: " + averagePositionError);

            // 6. 속도와 각속도 추출
            if (primaryPose != null)
            {
                Vector3 velocityPrimary = primaryPose.GetVelocity();
                Vector3 angularVelocityPrimary = primaryPose.GetAngularVelocity();
                Debug.Log("Primary Pose Velocity: " + velocityPrimary);
                Debug.Log("Primary Pose Angular Velocity: " + angularVelocityPrimary);
            }
            else
            {
                Debug.LogError("Primary Pose is null!");
            }

            if (secondaryPose != null)
            {
                Vector3 velocitySecondary = secondaryPose.GetVelocity();
                Vector3 angularVelocitySecondary = secondaryPose.GetAngularVelocity();
                Debug.Log("Secondary Pose Velocity: " + velocitySecondary);
                Debug.Log("Secondary Pose Angular Velocity: " + angularVelocitySecondary);
            }
            else
            {
                Debug.LogError("Secondary Pose is null!");
            }

            poseAction = SteamVR_Input.GetAction<SteamVR_Action_Pose>("Pose");  // Pose 액션을 연결

            // 7. poseAction으로 속도와 각속도 값 추출 (시도)
            if (poseAction != null)
            {
                Vector3 velocity = poseAction[SteamVR_Input_Sources.Any].velocity;
                Vector3 angularVelocity = poseAction[SteamVR_Input_Sources.Any].angularVelocity;
                Debug.Log("poseAction Velocity: " + velocity);
                Debug.Log("poseAction Angular Velocity: " + angularVelocity);
            }
            else
            {
                Debug.LogError("poseAction is null!");
            }
        }
    }
}

// CustomEditor는 Unity Editor의 기능이므로 UnityEditor 네임스페이스를 사용
[CustomEditor(typeof(ObjectMover))]
public class ObjectMoverEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 기본 Inspector 그리기
        DrawDefaultInspector();

        ObjectMover objectMover = (ObjectMover)target;

        // 버튼을 만들고 클릭하면 기준 오브젝트 이동
        if (GUILayout.Button("Move Reference Object"))
        {
            // 버튼 클릭 시 MoveReferenceObject() 메서드를 호출하여 기준 오브젝트 이동
            objectMover.MoveReferenceObject();
        }
    }
}
