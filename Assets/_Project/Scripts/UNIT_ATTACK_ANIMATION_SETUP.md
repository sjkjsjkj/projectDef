# 공격 전달 · 이펙트 · 기물 반동 설정

## 현재 적용된 내용

- 여포 프리팹 `Assets/_Project/02_Prefab/Units/Unit_LuBu.prefab`은 `assembled.png` 한 장을 사용합니다. 팔 회전 리깅을 제거하고 그림 전체를 대상 방향으로 약 0.07 unit 밀었다가 0.18초 안에 복귀합니다. 최대 기울기는 3도입니다.
- 기존 공통 기본 공격 SO는 근거리로 유지하고 베기/명중 샘플 이미지를 연결했습니다.
- 원거리 예제는 `Assets/_Project/Scripts/Resources/CombatEffects/AbilityData_RangedExample.asset`입니다. 원거리 장수의 `UnitData.BasicAttack`에 연결하거나 복제하여 사용하세요. 기존 장수의 병종만 보고 공격 방식을 일괄 변경하지는 않습니다.
- 공격/피격 이미지와 투사체는 풀을 통해 재사용합니다. 런타임 실행기는 공격이 처음 발생한 씬에 자동 생성되므로 씬에 매니저를 추가할 필요가 없습니다.

## AbilityData (스킬 SO) 설정

기본 공격과 스킬 모두 같은 설정을 사용합니다.

| 항목 | 용도 |
|---|---|
| Delivery Type | Melee(근거리) / Ranged(원거리) |
| Attack Effect | 근거리: 공격 이미지 / 원거리: 투사체 이미지 (Sprite) |
| Hit Effect | 대상의 위치에 표시할 피격 이미지 (Sprite) |
| Attack Effect Size / Hit Effect Size | 투명 캔버스를 포함한 이미지의 긴 변을 Unity unit으로 지정 |
| Melee Effect Offset | 근거리 공격 이미지를 발사 위치에서 대상 쪽으로 띄울 거리 |
| Melee Attack Duration | 근거리 공격 이미지가 재생되고 피격 단계로 넘어가기까지의 시간 |
| Hit Effect Duration | 피격 이미지 재생 시작부터 실제 효과 적용까지의 시간 |
| Projectile Speed | 초당 이동 거리 |
| Projectile Lifetime | 제한 시간 안에 명중하지 못한 투사체를 반환하는 시간 |
| Projectile Hit Radius | 대상 Collider2D 접촉 판정에 더하는 반경 |
| Attack Origin Offset | 유닛 루트에 더하는 월드 XY 발사 위치. 기본 (0, 0.45) |
| Attack Image Angle | 이미지 방향 보정 각도. 기본은 오른쪽(+X)을 향한 이미지 |

현재 이펙트 슬롯은 **Sprite 이미지**를 받습니다. 공격/피격 이미지는 짧게 확대되며 사라지고, 투사체 이미지는 목표 방향으로 회전하며 이동합니다. Animator/ParticleSystem 프리팹 슬롯은 아닙니다. 제공된 Slash / Impact / Projectile은 교체 가능한 간단한 샘플입니다.

이미지를 비워도 지정한 시간과 피해 흐름은 유지되며 시각 효과만 생략됩니다. Hit Effect Duration이 0이어도 피격 단계가 시작된 다음 Tick부터 피해를 적용합니다. 피해는 이펙트의 렌더링 성공 여부와 독립적으로 처리됩니다.

## 실행 순서

**근거리:** 조건 확인 및 대상 선택 → 공격 모션/공격 이미지 → Melee Attack Duration 경과 → 대상마다 피격 이미지 → Hit Effect Duration 경과 → SkillEffect 적용.

**원거리:** 조건 확인 및 대상 선택 → 공격 모션/대상마다 투사체 대여 → 지정 대상 추적 및 접촉 → 투사체 초기화·반환 + 피격 이미지 → Hit Effect Duration 경과 → SkillEffect 적용.

피해뿐 아니라 `ApplyStatusEffect`도 명중 단계에서 적용됩니다. 쿨다운은 공격을 시작할 때 소비합니다. 타깃이 없거나 조건을 만족하지 않으면 쿨다운과 모션을 시작하지 않습니다.

타깃 선택기가 여러 적을 선택하면 각 적에게 한 번씩 적용하며, 원거리는 대상마다 하나의 투사체를 발사합니다. 현재 투사체는 선택한 대상을 추적합니다. 경로상의 다른 적에게 가로막히거나 관통하는 방식은 포함하지 않습니다. 대상 루트에 활성 Collider2D가 있으면 가까운 접촉점으로 이동하며, Collider가 없으면 대상 위치와 Hit Radius로 판정합니다.

## 드래그와 일시정지

클릭만 했을 때는 시간이 정상적으로 흐릅니다. 마우스 이동이 Drag Threshold를 넘어 실제 드래그가 시작되면 다음을 정지합니다.

- 해당 유닛의 기본 공격/스킬 Tick, 쿨다운, 실패 후 재시도 대기 시간
- 신규 기본 공격과 수동/자동 스킬 사용
- 해당 유닛의 반동 모션과 아직 완료되지 않은 근거리 공격/피격 대기 시간

드롭·취소·입력 컨트롤러 비활성화·포커스 상실 시 드래그 상태를 해제합니다. 이미 발사된 투사체와 그 명중 처리는 시전자가 드래그되어도 계속 진행됩니다.

설정창·라운드 이벤트 등 `GameManager.IsCombatPaused`가 참일 때는 모든 공격 실행, 투사체, 이펙트와 반동 모션이 멈춥니다. 시간 배속은 `Time.deltaTime`을 따라갑니다.

## 책임과 안전한 재사용

- `AbilityData`: 공유 정의와 조건/타깃 선택. 실행 중 상태를 보관하지 않습니다.
- `AbilityInstance`: 쿨다운 및 준비 상태. 드래그 중 Tick/발동을 차단합니다.
- `AbilityExecutionRunner`: 씬별 지연 타격과 이펙트/투사체 풀 관리.
- `AbilityProjectile`: 목표 추적, 접촉/수명 만료, 상태 초기화 및 반환.
- `AbilityContext`: 발사 시 공격력과 대상별 독립 타깃 목록을 보관합니다. 이후 다른 공격·레벨업으로 이미 발사한 피해가 바뀌지 않습니다.
- `AbilityTargetHandle`: 사망·파괴·비활성 대상과 풀에서 새로 생성된 적을 구분합니다. `EnemyHealth.SpawnVersion`이 바뀌면 이전 공격은 취소됩니다.
- `UnitCombatController.AttackStarted`: 기본 공격과 스킬이 접수되면 목표 위치/공격 간격을 전달합니다.

대상이 사라지면 피해 없이 투사체를 반환합니다. 비행 중 최대 수명이 지나도 반환합니다. 반환 시 대상·콜백·이미지·색상·위치·회전·크기·타이머를 초기화합니다. 시전자가 사라지면 준비 중인 근거리 공격은 취소되지만 이미 발사된 투사체는 독립적으로 진행합니다. 씬 종료/실행기 비활성화 시 대기 공격과 풀을 모두 정리합니다.

## 다른 기물의 모션 설정

```text
Unit (Unit / UnitCombatController / Collider2D / UnitAttackAnimation)
└─ Visual                         # 움직이는 그림 루트
   └─ Assembled (SpriteRenderer)
```

`UnitAttackAnimation.Visual`에 그림 루트를 연결하세요. 배치 루트와 Collider는 이동시키지 않습니다. Duration, Move Distance, Tilt Angle을 조정할 수 있고, 공격 간격이 짧으면 다음 공격 전에 복귀하도록 모션 시간이 줄어듭니다. 프리팹 모드에서 **공격 미리보기**로 오른쪽 방향 반동을 확인할 수 있습니다. 실제 전투에서는 선택된 첫 대상 방향을 사용합니다.

여포 원본은 1254×1254px이며 PPU 1000을 유지했습니다. 전체 캔버스는 1.254 unit이고, 발 정렬을 위해 Assembled의 Local Position을 (0.157, 0.583, 0)으로 두었습니다.

## 에디터 메뉴와 검증

- `Window > Tools > Unit Animation > Setup Combat Effects`: 샘플 이미지/원거리 예제 SO/공통 머티리얼 생성. 기존 수동 이미지 연결은 보존합니다.
- `Window > Tools > Unit Animation > Create Lu Bu Example`: 여포 완성 이미지와 반동 모션 연결. 이전 AttackPivot/Body 조립 구조를 교체합니다.
- Test Runner: `AbilityExecutionTests`, `AttackPipelinePlayModeTests`, `LuBuAnimationTests`, `UnitInfoTests`.

원본 여포 이미지는 Git에서 제외된 `DontCommitFolder`에 있습니다. 다른 PC에서도 같은 이미지와 GUID를 준비해야 프리팹 참조가 유지됩니다. 샘플 이펙트 이미지는 프로젝트 Resources 폴더에 포함됩니다.
