# 상태이상 사용법

`StatusEffectData`는 공유 정의, `StatusEffectInstance`는 대상별 시간 상태입니다.
`StatusEffectController`에는 Update가 없고, `StatusEffectSystem`이 효과가 있는 대상만 갱신합니다.
적에게는 `EnemyStatusController` 하나를 고정해서 사용합니다. 기존 프리팹에 없다면 생성/스폰 초기화 때 한 번 보완합니다.

## 스킬에 연결

1. `Data/StatusEffect/Presets`의 8종 예제 에셋을 사용하거나 Project 창에서
   `Create > ScriptableObjects > Combat > StatusEffect > EnemyStatusEffect`로 정의를 만듭니다.
2. `Create > ScriptableObjects > Combat > SkillEffect > ApplyStatusEffect`로 부여 효과를 만듭니다.
   기존 SkillEffect의 ID와 Status Effect 참조를 설정합니다.
3. 해당 AbilityData의 Effects 목록에 부여 효과를 추가합니다.

Effects는 목록 순서대로 실행됩니다. 화상을 부여한 그 타격부터 추가 피해를 적용하려면
ApplyStatusEffect를 DamageEffect 앞에 두고, 다음 타격부터 적용하려면 뒤에 둡니다.
코드에서는 `enemy.GetComponent<EnemyStatusController>().TryApply(data)`로 부여합니다.

## 규칙

시간 단위는 초입니다. Duration은 전체 지속 시간이고 Tick Interval은 주기 발동 간격입니다.
첫 주기 효과는 부여 즉시가 아니라 Tick Interval 경과 후 발동합니다.

| 종류 | 동작 |
| --- | --- |
| Poison / 독 | 매 간격마다 Damage만큼 고정 피해 |
| Bleeding / 출혈 | 매 간격마다 Damage만큼 고정 피해 |
| Burn / 화상 | 직접 피격마다 Damage만큼 추가 피해 |
| Paralysis / 마비 | 매 간격마다 0.1초 이동 정지 |
| Chill / 오한 | 지속 시간 동안 Slow Ratio만큼 감속 |
| Fracture / 골절 | 지속 시간 동안 Slow Ratio만큼 감속 |
| Freeze / 빙결 | 지속 시간 동안 이동 정지 |
| Stun / 기절 | 지속 시간 동안 이동 정지 |

- 예제 값은 지속 3초, 간격 1초, 피해 5, 감속 30%이며 밸런스 확정값이 아닙니다.
- 빈 ID는 상태이상 종류별 기본 ID를 사용합니다. 같은 ID 재부여는 최신 정의로 교체하고
  지속 시간을 갱신합니다. 중첩 수는 쌓지 않으며 기존 주기 타이머를 보존합니다.
- 같은 종류를 별개 효과로 중첩하려면 서로 다른 ID를 사용합니다.
- 서로 다른 감속은 배율을 곱합니다. 30%와 50% 감속이 겹치면 속도는 35%입니다.
- 빙결/기절/마비가 겹치면 이동 제한이 하나라도 남아 있는 동안 멈춥니다.
  현재 적은 이동만 하므로 공격/스킬 차단은 구현하지 않습니다.
- 독/출혈의 `TakeStatusDamage`는 화상을 발동하지 않습니다. 기존 `TakeDamage`는 직접 피격입니다.
  화상 피해는 원래 타격에 합쳐 한 번 처리하므로 추가 피격을 재귀적으로 발생시키지 않습니다.
- 긴 프레임에서 놓친 주기는 만료 시각까지만 처리합니다. 만료 시각과 같은 주기도 포함합니다.
  이동 정지는 프레임 단위로 반영되므로 이미 지나간 마비 구간의 이동을 소급 보정하지 않습니다.
- GameManager의 전투 일시정지와 Time.deltaTime을 따릅니다.
- 사망/비활성화/풀 재사용 시 효과, 이동 배율, 화상 피해와 중앙 등록을 초기화합니다.
- `RemoveEffect(id)`는 개별 해제, `ClearEffects()`는 전체 해제입니다.

## 확장

새 동작은 `StatusEffectData`를 상속해서 정의할 수 있습니다. 중앙 갱신기는 적 전용 열거형을 사용하지 않습니다.
대상별 상태는 SO가 아닌 인스턴스에 보관합니다. 향후 아군은 `StatusEffectController`를 상속한 어댑터로
아군 체력/스탯과 연결할 수 있습니다. 현재 아군 버프나 공격력/방어력 보정 기능은 추가하지 않았습니다.

Unity Test Runner의 EditMode에서 `StatusEffectTests`와 기존 `EnemySpawnTests`를 실행해
피해 주기, 중복 부여, 이동 제한 합산, 일시정지, 사망과 풀 반환을 확인할 수 있습니다.
