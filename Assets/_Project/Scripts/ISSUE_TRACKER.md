# 이슈 기록

## ISSUE-001 — 유닛 구매 시 빈 첫 대기 슬롯을 건너뜀

- 접수: 2026-10-15
- 상태: 코드 수정 및 컴파일 확인 완료 / Unity 실행 검증 대기
- 증상: 유닛 구매 시 간헐적으로 첫 슬롯이 비어 보이는데 다음 슬롯에 배치됨.

### 확인한 원인

`Assets/Scenes/StageScene.unity`의 `TestManager`는 `unitTestFlag: 1`이며, `units`에 씬 인스턴스가 아니라 `Unit_ZhaoYun.prefab` 원본을 참조하고 있었다(GUID `2e325a539d1693241be320f7f2a8d072`).

기존 `TestManager.UnitTest()`는 생성 없이 원본을 `WaitField.TryAddUnit()`에 전달했다. `Slot.OnDrop()`도 원본 여부를 검사하지 않아 첫 슬롯의 `_unit`에 프리팹을 저장할 수 있었다. 프리팹 원본은 씬에 표시되지 않지만 `IsEmpty`는 false이므로 이후 신규 구매는 다음 슬롯을 선택한다.

이 잘못된 등록 경로와 씬 설정은 코드 및 에셋에서 확인했다. 사용자가 보고한 간헐성의 정확한 실행 조건은 아직 플레이 모드에서 재현하지 않았다.

### 수정

- `Global/Manager/TestManager.cs`: 프리팹이면 `Instantiate()` 후 등록한다. 씬 유닛은 그대로 등록한다. 배치 실패 시 새 인스턴스를 제거하며, 대기 필드가 없으면 즉시 반환한다.
- `Logic/Filed/Slot.cs`: `CanDrop()`과 `OnDrop()`에서 씬에 속하지 않은 유닛을 거부한다. 원본의 위치 및 배치 참조를 변경하지 않는다.
- `Editor/SlotPlacementTests.cs`: 프리팹 등록 거부, 거부 후 첫 슬롯 사용, 테스트 유닛 실제 생성, 씬 유닛 재사용 및 비워진 첫 슬롯 재사용을 검증하는 EditMode 테스트 4개를 추가했다.

### 검증 및 완료 조건

- 프로젝트의 참조 DLL과 컴파일 상수를 사용한 Roslyn 컴파일: `Assembly-CSharp`, `Assembly-CSharp-Editor` 모두 성공. 기존 미사용/인스펙터 할당 필드 경고 존재.
- `git diff --check`: 통과.
- Unity Test Runner의 EditMode에서 `SlotPlacementTests` 실행 필요. 테스트 실행 및 실제 구매 동작 검증은 아직 수행하지 않았다.
- 플레이 모드를 새로 시작해 테스트 유닛이 첫 슬롯에 보이는지 확인한다. 테스트 유닛을 다른 슬롯으로 옮긴 뒤 **미보유 장수**를 구매하면 첫 빈 슬롯을 사용해야 한다. 이미 보유한 장수를 구매하면 기존 설계에 따라 레벨업한다.
- 빈 대기석으로 시작하려면 씬의 `TestManager > Unit Test Flag`를 끈다. 현재 설정을 유지하면 테스트 유닛이 실제로 첫 슬롯을 차지하므로 다음 신규 장수는 두 번째 슬롯에 배치되는 것이 정상이다.
