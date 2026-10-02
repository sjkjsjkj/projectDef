using UnityEngine;

/// <summary>
/// 카메라를 캐싱하고 기능을 지원하는 유틸리티입니다.
/// </summary>
public static class UCamera
{
    private static Camera _mainCamera;
    private static Camera _uiCamera;
    public static Camera MainCamera => _mainCamera;
    #region ─────────────────────────▶ 공개 멤버 ◀─────────────────────────
    ///// <summary>
    ///// 현재 씬에서 메인 카메라를 탐색해서 반환합니다.
    ///// </summary>
    //public static Camera MainCamera
    //{
    //    get
    //    {
    //        if (_mainCamera == null)
    //        {
    //            _mainCamera = UObject.FindComponent<Camera>(K.NAME_MAIN_CAMERA);
    //            if (_mainCamera == null)
    //            {
    //                UDebug.PrintOnce($"메인 카메라를 찾을 수 없습니다.", LogType.Assert);
    //                return null;
    //            }
    //            UObject.AddComponent<CameraAspectUpdater>(_mainCamera.gameObject);
    //            _mainCamera.orthographic = true; // 직교 카메라
    //            _mainCamera.depth = K.CAMERA_MAIN_DEPTH;
    //            UDebug.Print("메인 카메라를 탐색하고 크기를 설정했습니다.");
    //        }
    //        return _mainCamera;
    //    }
    //}

    /// <summary>
    /// 현재 씬에서 UI 카메라를 탐색해서 반환합니다.
    /// </summary>
    public static Camera UICamera
    {
        get
        {
            if (_uiCamera == null)
            {
                _uiCamera = UObject.FindComponent<Camera>("UI Camera");
                if (_uiCamera == null)
                {
                    //UDebug.PrintOnce($"UI 카메라를 찾을 수 없습니다.", LogType.Assert);
                    return null;
                }
                _uiCamera.orthographic = true; // 직교 카메라
                _uiCamera.depth = K.CAMERA_UI_DEPTH;
                // 이후 UI 카메라는 Canvas Scaler에게 의존한다.
                // 만약 캔버스 컴포넌트를 가져올 수 있다면 Plane Distance를 자동 설정한다.

                Canvas canvas = FindUiCanvas(_uiCamera);
                // 캔버스 가져오기 성공
                if (canvas != null)
                {
                    canvas.planeDistance = K.DEFAULT_PLANE_DISTANCE;
                    UDebug.Print($"UI 카메라가 사용하는 캔버스를 탐색 성공하여 Plane Distance를 설정합니다.");
                }
                // 캔버스를 가져오지 못함
                else
                {
                    UDebug.Print($"UI 카메라가 사용하는 캔버스는 자동으로 탐색하지 못했습니다.");
                }
            }
            return _uiCamera;
        }
    }

    /// <summary>
    /// 기기 비율에 맞춰 카메라의 Orthographic Size를 늘립니다.
    /// </summary>
    /// <param name="cam">카메라 오브젝트</param>
    public static bool SetCameraAspect(Camera cam, int screenWidth, int screenHeight)
    {
        if (cam == null)
        {
            return false;
        }
        cam.orthographicSize = K.CAMERA_MIN_HEIGHT / 2f; // 최소 높이
        // 현재 기기의 화면 비율
        float curAspect = (float)screenWidth / (float)screenHeight; // 값이 클수록 가로가 길다
        if (curAspect < K.CAMERA_ASPECT) // 가로가 좁아서 양쪽에 있는 오브젝트들이 보이지 않을 경우
        {
            cam.orthographicSize *= (K.CAMERA_ASPECT / curAspect); // 카메라를 그만큼 늘린다.
        }
        return true;
    }

    public static void SetCameraAspectRatio(Camera cam, int screenWidth, int screenHeight, float ratio)
    {
        if(SetCameraAspect(cam, screenWidth, screenHeight))
        {
            cam.orthographicSize = cam.orthographicSize * ratio;
        }
    }
    #endregion

    #region ─────────────────────────▶ 내부 메서드 ◀─────────────────────────
    // 씬에서 모든 캔버스를 탐색해서 UI 카메라와 연결된 캔버스를 찾습니다.
    private static Canvas FindUiCanvas(Camera uiCam)
    {
        // 정렬 모드 없음으로 수집하기
        Canvas[] canvasList = GameObject.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        int length = canvasList.Length;
        // 현재 씬의 모든 캔버스 순회
        for (int i = 0; i < length; ++i)
        {
            var canvas = canvasList[i];
            if (canvas == null)
            {
                continue;
            }
            // 탐색 성공
            if (canvas.worldCamera == uiCam)
            {
                return canvas;
            }
        }
        UDebug.Print($"UI 캔버스를 찾지 못했습니다.", LogType.Warning);
        return null;
    }

    // 씬 로드가 완료될 때마다 null 대입
    private static void ClearCache(OnSceneLoadEnd ctx)
    {
        _mainCamera = null;
        //_mainCamera = MainCamera;
        SetCameraAspect(_mainCamera, Screen.width, Screen.height); // 카메라를 새로 찾을 때마다 조절
        _uiCamera = null;
        _uiCamera = UICamera;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Subscriber()
    {
        // static이라서 구독 해제하지 않아도 OK
        EventBus<OnSceneLoadEnd>.Subscribe(ClearCache);
    }
#endregion
}
