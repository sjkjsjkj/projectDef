# 전체 유닛 스탯 효과

`GlobalUnitStatManager`는 시너지나 이벤트가 부여한 효과를 모아 전체 유닛에 적용하는 관리자입니다. 효과 하나의 스탯 보정값은 `UnitStatModifier`라고 부릅니다.

## 사용 예시

```csharp
var stats = GlobalUnitStatManager.Ins;

// 같은 출처 ID를 다시 설정하면 이전 목록 전체를 교체합니다.
stats.SetEffect("Synergy:Wei",
    new UnitStatModifier(UnitStatType.AttackPower, flat: 10f, multiplier: 1.2f),
    new UnitStatModifier(UnitStatType.AttackSpd, multiplier: 1.1f),
    new UnitStatModifier(UnitStatType.GoldGain, flat: 2f));

// 다른 출처는 함께 적용됩니다.
stats.SetEffect("Event:AttackBoost",
    new UnitStatModifier(UnitStatType.AttackPower, flat: 5f, multiplier: 1.5f));

stats.RemoveEffect("Event:AttackBoost");

// 현재 통합된 보정값 조회
UnitStatModifier attack = stats.GetModifier(UnitStatType.AttackPower);

// 같은 씬에서 새 게임을 시작할 때 호출
stats.ClearEffects();
```

## 계산 규칙

- 스탯 종류: `MaxHp`, `AttackPower`, `Defense`, `AttackRange`, `AttackSpd`, `GoldGain`.
- 효과가 없으면 고정값 0, 배율 1입니다. 고정값은 음수도 가능하며 배율은 0 이상입니다.
- 여러 효과의 고정값은 합산하고 배율은 곱산합니다.
- 최종값 = `(현재 레벨의 기본 스탯 × 배율들의 곱) + 고정값들의 합`. 결과가 음수면 0으로 제한합니다.
- 예: 공격력 100에 위 두 공격 효과를 적용하면 `100 × (1.2 × 1.5) + (10 + 5) = 195`입니다.
- 체력, 공격력, 방어력, 사거리, 공격속도는 기존 레벨 성장을 먼저 계산합니다. 획득 금화는 레벨 성장 없이 `UnitData.GoldGain`을 기본값으로 사용합니다.
- 현재 체력은 최대 체력 대비 비율을 유지합니다. 최대 체력이 일시적으로 0이 되어도 마지막 비율을 보존합니다.
- `UnitData`와 기존 `UnitStats` 객체 참조를 유지하므로 공유 SO를 수정하거나 기존 스킬 문맥을 교체하지 않습니다.

## 등록과 수명

- `Unit.InitializeForSpawn`, `Awake`, 첫 스탯 조회에서 자동 등록합니다. 신규 획득 즉시 기존 효과가 적용되고, 레벨업 시에도 유지됩니다.
- 전투/대기 슬롯, 비활성 여부와 관계없이 등록된 유닛 모두 갱신합니다. 등록된 유닛이 파괴되면 해제합니다. 아직 초기화되지 않은 비활성 씬 유닛은 초기화 시 적용됩니다.
- 기존 `Singleton<T>` 구조를 사용하므로 `Ins`에 처음 접근할 때 관리자가 자동 생성됩니다. 수동 배치한다면 씬에 하나만 둡니다.
- 이 관리자는 `DontDestroyOnLoad`를 사용하지 않으며, 관리자가 있는 씬을 벗어나면 효과도 제거됩니다. 같은 씬의 게임 재시작은 게임 흐름에서 `ClearEffects()`를 호출하세요.
- 현재는 플레이어/진영별 구분 없이 초기화된 모든 `Unit`을 대상으로 합니다.

## 실제 전투 및 금화 지급과의 관계

- 공격 효과는 기존 `DamageEffect`가 보정된 `AttackPower`를 읽어 반영합니다.
- `AttackSpd` 값은 보정되지만, 기존 `AbilityData.GetCooldown()`은 고정 쿨다운을 반환합니다. 공격속도 기반 쿨다운과 방어력 기반 피해 경감은 별도 전투 공식 연결이 필요합니다.
- `UnitData`의 새 `Gold Gain` 기본값은 0입니다. 유닛별 획득 금화 기본량을 설정하고, 보상 처리에서 `unit.Stat.GoldGain`을 읽어 지급하세요. 정수 반올림과 지급 시점은 보상 로직의 책임입니다.
- `ResourceManager.AddGold()`에는 자동 보정을 넣지 않았습니다. 해당 API는 구매 실패 환불에도 쓰이므로 유닛 보상과 동일하게 보정하면 환불액까지 달라지기 때문입니다.

## 검증

- 프로젝트의 Unity 참조 DLL로 전체 런타임 코드 컴파일 확인.
- 실제 관리자/유닛/스탯 코드를 Unity API 대역과 함께 실행해 계산, 중첩, 동일 ID 교체, 제거, 신규 유닛, 비활성 유닛, 레벨업, 공유 데이터 불변, 체력 비율, 등록 해제 등을 검증.
- Unity 플레이 모드의 씬 전환과 실제 구매/전투 동작은 별도 확인이 필요합니다.
