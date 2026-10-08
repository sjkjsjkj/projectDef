# 프로젝트 기본 맥락

사용자는 2026-10-08에 이 스크립트 폴더와 첨부한 클래스 다이어그램을 앞으로 이 프로젝트에서 대화할 때의 기본 맥락으로 삼아 달라고 요청했다.

## 답변과 작업 기준

- Unity / C#으로 제작 중인 삼국지 디펜스 게임이다. 이 폴더와 다이어그램은 게임의 일부분이며 전체 프로젝트의 범위로 단정하지 않는다.
- 설계 설명과 코드 제안은 아래 아키텍처 및 기존 스크립트의 책임과 연결 관계를 바탕으로 한다.
- 다이어그램은 설계 참고 자료이고 실제 구현과 차이가 있다. 현재 동작, 클래스명, API는 관련 최신 코드를 확인해 설명하고 설계와 구현을 구분한다. 다이어그램의 모든 기능이 구현되었다고 가정하지 않는다.
- 사용자의 후속 설명과 설계 변경을 반영한다. 아래 구현 메모는 최초 확인 시점의 요약이므로 최신 코드보다 우선하지 않는다.
- 한국어로 소통한다. 기존 명명과 코드 스타일을 참고하고, 요청과 무관한 구조 변경은 하지 않는다.

## 첨부 UML의 설계 개요

원본 이미지: `C:/Users/sjhh6/Downloads/sfx/삼국지 디펜스 게임 UML 아키텍처 다이어그램.png`
원본을 열 수 없는 경우에도 아래 요약을 기본 참고로 사용한다.

- 게임 진행: `GameManager`, `WaveManager`, `WaveData`, `EnemySpawner`가 게임 상태, 웨이브 데이터와 시작/종료, 적 생성 및 생존 적 관리를 담당한다.
- 장수/유닛: `Unit`이 `UnitData`를 참조하고, 기본 능력치 `UnitBaseStat`과 실행 중 능력치 `UnitStat`을 구분한다. `UnitCombatController`가 기본 공격 및 스킬을 제어하고 `UnitTargetSearcher`가 타깃을 탐색한다.
- 적: `Enemy`, `EnemyData`, `EnemyStat`, `EnemyMovement`, `EnemyPath`로 데이터, 체력/피해/사망, 경로 이동을 나눈다.
- 스킬: `AbilityData`는 정의 데이터, `AbilityInstance`는 개별 실행 상태와 쿨다운, `AbilityContext`는 시전자/대상/위치 등의 실행 맥락, `AbilityEffect`는 피해/회복/버프 등 효과를 담당한다.
- 배치: `IDropTarget`을 구현하는 `BattleSlot`, `WaitingSlot`과 `DragController`로 전투/대기 슬롯 배치, 드래그, 교환을 구성한다.
- UI/자원: `UIManager`, `UnitInfoPanel`, `EnemyInfoPanel`, `ResourcePanel`, `ResourceManager`로 정보 표시와 자원 관리를 나눈다.
- 공통 열거형 설계: `GameState`, `TargetType`, `TargetSelectionType`, `AbilityType`, `AbilityEffectType`.
- 관계도는 참조/사용, 합성, 집합, 상속/구현, 이벤트/콜백을 구분한다.

## 현재 스크립트에서 확인한 대응 구조 (2026-10-08)

- `Global/`: 공통 베이스, 싱글턴, 매니저, 부트스트랩, 이벤트 버스, 풀/팩토리, 인터페이스, 유틸리티 및 컨트롤러.
- `Data/`: ScriptableObject 기반 유닛/스킬 정의 등. `Logic/`: 유닛/적, 필드/슬롯, 스킬 실행, 플레이어 등. `Actions/`: 입력 디스패처.
- 유닛은 `Logic/Combat/Unit/Contoller/Unit.cs`, 데이터는 `Data/UnitData.cs`, 실행 중 능력치는 `Logic/Combat/Unit/UnitStats.cs`. 실제 폴더명은 `Contoller`이다.
- 전투 제어는 `Global/Controller/UnitCombatController.cs`에 있다.
- 스킬은 `Data/Skill/AbilityData.cs`, `Logic/Skill/AbilityInstance.cs`, `Logic/Skill/AbilityContext.cs`로 나뉘며, `TargetSelector`, `SkillCondition`, `SkillEffect`를 ScriptableObject로 구성한다. `UnitCombatController`는 기본 공격/스킬별 문맥과 인스턴스를 생성하고 실행 시도 및 Tick을 담당한다. `AbilityInstance`는 준비 상태 확인 → `AbilityData.TryExecute(context)` 호출 → 성공 시 쿨다운 설정을 담당한다. `AbilityData`는 사용 조건 → 타깃 선택 → 발동 조건 → 효과 적용을 실행하며 유닛별 상태를 저장하지 않는다. `AbilityContext`는 Owner/OwnerStats/Origin과 실행별 타깃 목록을 전달한다. `Unit.CurrentTarget`은 배치 슬롯이므로 공격 타깃과 구분한다. 컨트롤러와의 연동 상태는 변경 시 다시 확인한다.
- 배치는 `Global/Controller/DragDropController.cs`, `IDraggable`, `IDropTarget`, `Logic/Filed/Slot.cs`, `Logic/Filed/WaitField.cs`를 사용한다. 실제 폴더명은 `Filed`이다.
- 적 관련 구현은 `Logic/Combat/Enemy/`의 `EnemyHealth`, `EnemyMovement`, `EnemyPath`, `EnemySpawner` 등을 확인한다.
- 현재 `Global/Manager/GameManager.cs`는 전역 루트 접근과 씬 로드/부팅을 담당하므로 UML에 있는 웨이브/전투 상태 관리까지 구현된 것으로 간주하지 않는다.
- `Logic/Player` 및 다양한 입력/기록 이벤트도 존재한다. 디펜스 다이어그램에 없다는 이유만으로 불필요한 코드라고 판단하지 않는다.
