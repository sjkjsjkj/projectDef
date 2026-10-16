# 프로토타입 설정

## 사용

- MainScene의 기존 **설정** 버튼이 활성화됩니다. 디스플레이 / 사운드 / 게임 탭으로 구성됩니다.
- StageScene에는 실행 시 우측 상단 **설정** 버튼이 자동 생성됩니다. 마스터 볼륨 / 배경음 / 효과음 / 피해량 표시를 한 화면에서 조절합니다.
- 전투 설정창을 열면 시간이 정지하고, 닫으면 열기 전 배속으로 복원됩니다. 설정창 뒤의 UI와 장수 드래그는 차단됩니다.
- 설정은 즉시 적용됩니다. 슬라이더 변경 후 0.4초, 창 닫기, 앱 일시정지/종료 때 저장합니다.
- 언어는 한국어 / 영어를 지원하며, 이번 적용 범위는 **메인 메뉴와 설정창**입니다. 능력 강화, 객잔, 전투 정보, 이벤트, 결과 화면의 번역은 포함하지 않습니다.

## 연결 구조

- `DataManager`가 기존 `OptionData`, `ScreenData`, `VolumeData`를 공유하고 PlayerPrefs 키 `Monoeye.Options.v1`에 저장합니다.
- `SettingsPanel.Show(false, font)`는 메인 3탭, `SettingsPanel.Show(true, font)`는 전투 설정창을 엽니다. 기본 진입점은 이미 연결되어 있어 씬/프리팹의 추가 배치가 필요 없습니다.
- `BattleSettingsButton`은 StageScene 로드 시 UI와 `DamageNumbersView`를 생성합니다. 씬을 나가면 함께 제거됩니다.
- `USound`로 재생한 BGM/SFX는 현재 재생 중인 소리까지 볼륨을 갱신합니다. 별도의 AudioSource에서 직접 재생하는 소리가 있다면 해당 재생 경로도 USound에 연결해야 합니다.
- `EnemyHealth.OnDamageTaken`을 구독해 피해 숫자를 표시합니다. 최대 64개를 재사용하며, 피해량 표시를 끄면 이미 떠 있는 숫자도 숨깁니다.
- 해상도 목록은 모니터 지원 목록, 현재 해상도, 저장된 해상도를 중복 없이 구성합니다. 화면 모드는 ExclusiveFullScreen / Windowed / FullScreenWindow에 대응합니다. 실제 전체화면 동작은 플랫폼에 따라 다르므로 Windows 빌드에서도 확인하세요.

## 세이브 초기화

게임 탭에서 초기화 버튼을 누르면 확인창을 엽니다. 확인한 경우에만 `MetaProgressionManager.TryResetSave`를 호출합니다.
현재 영구 저장 대상인 메타포인트 / 영구 강화 / 정산 기록과 백업을 초기화하며, 화면·사운드·피해량 표시·언어 설정은 유지합니다.
실제 파일 저장이 성공한 뒤 런타임 성장 수치를 교체합니다. 저장이 실패하면 오류를 보여줍니다.

## 검증

Unity Test Runner의 EditMode에서 다음 테스트를 실행할 수 있습니다.

- `SettingsTests`: 옵션 직렬화와 구버전 기본값, 유효하지 않은 값 정규화, 화면 모드 매핑, 초기화 후 백업 복구.
- `SettingsPlayModeTests`: Play 모드 진입 후 메인 설정 버튼/탭/드롭다운, 영어 전환, 초기화 취소, 옵션 저장, StageScene 단일 화면 공유, 일시정지/배속 복원.

수동 확인: 각 해상도/화면 모드의 Windows 빌드 동작, BGM/SFX 재생 중 각 슬라이더와 0% 음소거, 피해 숫자 표시/숨김, 재실행 후 설정 유지, 초기화 확인 후 재실행 시 성장 0 확인.

2026-10-16: Unity 참조를 사용한 스크립트 컴파일을 검증했습니다. 별도 임시 프로젝트에서 자동 테스트 실행을 시도했으나 배치 Unity의 라이선스 오류(`No valid Unity Editor license found`)로 실행 결과는 확보하지 못했습니다.
