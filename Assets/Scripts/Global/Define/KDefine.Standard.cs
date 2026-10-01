#pragma warning disable IDE1006

public static partial class K
{
    // 카메라 / UI 동작 방식 정의
    public static readonly int SCREEN_WIDTH = 1920;
    public static readonly int SCREEN_HEIGHT = 1080;
    public static readonly float CAMERA_ASPECT = SCREEN_WIDTH / SCREEN_HEIGHT;
    // 카메라 / 이동 속도 / 충돌 크기 등의 기준점
    // public static readonly float UNIT_SCALE = 0.01f;
    // 현재 CAMERA_MIN_HEIGHT에 의존중이므로 우선 주석 처리
    public static readonly float SMALL_DISTANCE = 0.0001f;
    // 바닥 거리 통일 → 모든 UI가 카메라와 두는 거리 통일
    public static readonly float DEFAULT_PLANE_DISTANCE = 5f;
    // 카메라 최소 크기 (세로 기준)
    public static readonly float CAMERA_MIN_HEIGHT = 16f;
    public static readonly int CAMERA_MAIN_DEPTH = -1;
    public static readonly int CAMERA_UI_DEPTH = 0;
    public static readonly int CAMERA_OVERLAY_DEPTH = 1;
    // 매니저 우선순위 크기
    public static readonly int MANAGER_PRIORITY_SIZE = 11;
    public static readonly int MANAGER_FRAMEABLE_SIZE = 10;
}
