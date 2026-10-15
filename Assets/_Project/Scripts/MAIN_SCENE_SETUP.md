# 메인 화면과 씬 전환

## 실행 흐름

`MainScene(Loading...) → 전역 부팅 완료 → 메인 메뉴 → 게임시작 → 페이드 아웃 → StageScene 비동기 로드 → 페이드 인 → 전투 시작`

- `Assets/Scenes/MainScene.unity`가 Build Settings의 첫 씬입니다. `StageScene`도 활성 등록하고 기존 `SampleScene`은 뒤에 유지했습니다.
- 별도의 로딩 씬 대신 최초 MainScene 위에 전역 로딩 오버레이를 표시합니다. Bootstrapper는 첫 씬의 Awake 전에 실행됩니다.
- 에디터에서는 어떤 씬을 편집 중이든 Play를 누르면 `MainScene`에서 시작합니다. `Editor/MainScenePlayModeSetup.cs`가 Unity의 `playModeStartScene`을 자동 설정하며 Play 종료 후에는 원래 편집하던 씬으로 돌아옵니다. 씬을 수동으로 전환하거나 저장하지 않습니다.
- MainSceneController가 카메라, 메뉴 Canvas, Input System용 EventSystem을 씬 내부에 생성합니다. 카메라와 UI를 직접 배치하면 기존 객체를 사용합니다.
- 게임시작은 SceneFlowManager.StartStage()를 호출합니다. 전환이 끝난 후 GameManager.StartGame()을 호출합니다.
- WaveManager의 자동 시작도 전환 완료를 기다리므로 검은 화면 뒤에서 웨이브 시간이 소모되거나 이중 시작되지 않습니다. StageScene 단독 실행에서는 기존 startGameOnStart 설정을 따릅니다.
- 게임 종료는 빌드에서는 앱 종료, 에디터에서는 Play 종료입니다. 능력 강화와 설정은 표시만 하며 비활성 상태입니다.

## 객체 수명과 소유권

| 범위 | 객체 | 책임 |
| --- | --- | --- |
| 앱 전체 | Bootstrapper / BootManager | 최초 1회 서비스 생성. 전역 프리팹까지 초기화한 후 완료 통지 |
| 앱 전체 | InputManager / DatabaseManager / DataManager / FrameManager | 공용 입력, 정의 데이터, 옵션, 프레임 실행 |
| 앱 전체 | GameManager | 전투 상태 및 현재 씬의 WaveManager 연결. 실제 전투 객체는 소유하지 않음 |
| 앱 전체 | SceneFlowManager / FadeCanvas | 씬 로딩, 전환 상태, 페이드, 로딩 표시, 중복 요청 차단 |
| MainScene | MainSceneController / MainMenuCanvas / Camera / EventSystem | 메인 화면. StageScene으로 넘어갈 때 제거 |
| StageScene | WaveManager / EnemySpawner / EnemyPath / EnemyPool | 한 판의 웨이브, 적, 경로, 적 풀 |
| StageScene | ResourceManager / InnManager / UnitSpawner / 필드·슬롯·유닛 / 전투 UI | 한 판의 골드, 영입, 편성 및 UI |
| StageScene | GlobalUnitStatManager | 이름의 Global은 모든 전투 유닛에 적용한다는 뜻. 앱 영구 저장이 아님 |

메인에서 전투 객체를 생성해 DontDestroyOnLoad로 넘기지 않습니다. 다음 판은 StageScene을 다시 로드해 구성합니다. 향후 메타 포인트/영구 강화의 저장 서비스는 앱 수명으로 추가하고, 판 시작 시 저장된 강화 값을 StageScene의 능력치에 적용하세요. 현재 ResourceManager의 골드나 GlobalUnitStatManager의 런타임 효과를 영구 재화/저장 데이터로 재사용하지 않습니다. 결과 보상, 저장, 강화 구매, 설정 화면은 이번 범위에 포함하지 않았습니다.

`Resources/BootPrefab`, `Resources/CanvasPrefab`은 기존 전역 로더가 유지합니다. 이 경로에는 앱 전체에서 필요한 프리팹만 넣고 스테이지 UI/웨이브/필드 프리팹은 넣지 마세요. 씬에 전역 매니저를 다시 배치할 필요가 없습니다.

## 전환 API와 이벤트

- `SceneFlowManager.State`: Booting / Ready / FadingOut / Loading / FadingIn.
- `IsTransitioning`: 부팅 시작부터 최초 페이드 인, 이후 각 전환의 페이드 인 종료까지 true.
- `StartStage()`: 메인에서 게임 시작. `ReturnToMain()`: 향후 결과 화면의 메인 복귀 연결점.
- `TryLoadScene(...)`: 승인되면 true, 중복 요청 또는 로드 불가능한 씬이면 false. 잘못된 씬은 현재 화면을 유지하고 LastError/OnLoadFailed로 알립니다.
- `OnSceneLoadStart`: 이전/목적지의 논리 ID. `OnSceneLoadEnd`: 새 씬의 Start 및 페이드 인까지 마친 뒤 통지합니다. 최초 부팅 완료 때도 1회 통지합니다.
- `EScene`은 빌드 인덱스가 아닙니다. SceneCatalog가 이름/경로를 ID로 변환하며 미등록 이름은 Unknown입니다. 기존 enum 값은 유지하고 Stage를 추가했습니다.
- 기존 GameManager.LoadScene* 호출 형태는 호환용으로 남겼으며 SceneFlowManager에 위임합니다. 구형 동기 LoadScene도 이제 비동기로 동작합니다. 완료 후 작업은 callback 또는 OnSceneLoadEnd에서 수행하세요.
- 페이드는 기본 0.2초씩이며 unscaled time을 사용합니다. Single 전환이 성공하면 Time.timeScale을 1로 복구합니다.
- ApplicationLifetime이 종료 상태를 실행마다 초기화하므로 Domain Reload를 끈 에디터의 반복 Play에서도 싱글턴 생성이 막히지 않습니다.

## 확인 항목

1. MainScene Play: 로딩 후 네 버튼과 한글 텍스트가 표시되는지 확인합니다.
2. 게임시작을 연속 클릭: StageScene이 한 번만 로드되고 페이드가 끝난 뒤 웨이브가 시작되는지 확인합니다.
3. 전환 동안 FadeCanvas가 화면과 포인터 입력을 가리고, 완료 후 객잔/드래그 입력이 복구되는지 확인합니다.
4. 전역 매니저 루트는 하나이며 MainMenuCanvas/메인 카메라는 StageScene에 남지 않는지 확인합니다.
5. StageScene을 편집하다가 Play를 눌러도 MainScene에서 시작하고, 종료하면 StageScene으로 돌아오는지 확인합니다. Play를 반복해도 동일하게 동작해야 합니다.
6. 향후 복귀 버튼을 연결할 때 ReturnToMain() 후 재시작하여 골드/적/영입 목록/시너지가 새 판으로 초기화되는지 확인합니다.
7. StageScene 빌드 등록을 임시 해제한 경우 오류 메시지 후 메뉴 입력이 유지되는지 확인합니다.

코드 변경 후 프로젝트의 Unity 컴파일 응답 파일을 사용해 런타임 및 에디터 어셈블리 컴파일을 통과했습니다. MainScene의 스크립트 GUID 연결, 첫 씬 순서와 빌드 씬 경로도 검사했습니다. 이 작업 환경에서는 Unity 창 활성화/캡처가 시간 초과되어 실제 Play 화면과 클릭 동작은 아직 검증하지 못했습니다.
