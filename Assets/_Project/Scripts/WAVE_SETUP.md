# 웨이브 설정

## 씬 연결

1. 기존 `EnemySpawner`의 `Enemy Path`에 경로 오브젝트를 연결합니다. 경로의 자식 Transform 순서가 이동 순서입니다.
2. `Create > ScriptableObjects > Wave > WaveData`로 웨이브 에셋을 만듭니다.
   - `Id`: 비어 있지 않은 식별자(예: `wave_01`).
   - `Wave Index`: 표시할 웨이브 번호.
   - `Duration`: 웨이브 제한 시간(초).
   - `Spawn Interval`: 적 한 마리마다 기다릴 소환 간격(초). 0이면 프레임당 한 마리입니다.
   - `Enemy Spawn Datas`: 적 프리팹(`EnemyMovement` + `EnemyHealth`)과 해당 프리팹의 총 소환 수 `Count`. 목록 순서대로 소환합니다.
3. 씬에 `WaveManager`를 하나 추가합니다.
   - `Enemy Spawner`: 위 스포너를 연결합니다.
   - `Wave Datas`: 웨이브 에셋을 진행 순서대로 연결합니다.
   - `Max Enemy Count`: 맵 전체 적 수의 게임 오버 기준입니다. 100이면 100마리까지 진행하고 101마리부터 패배합니다.
   - `Start Game On Start`: 체크하면 씬 시작 시 게임을 시작합니다. 해제하면 `GameManager.Ins.StartGame()`으로 시작합니다.
4. `GameManager`는 기존 Bootstrapper가 생성합니다. 씬에 중복으로 추가하지 않습니다. `WaveManager`가 자동으로 등록됩니다.

`EnemySpawner.Spawn Points`는 선택 사항입니다. 비워 두면 경로의 첫 지점에서 생성하고, 지정하면 생성 위치들을 번갈아 사용하여 첫 웨이포인트로 이동합니다. 기존 `Enemy Prefab` 필드는 단일 수동 소환 API와의 호환용이며, 웨이브에서는 `WaveData`의 프리팹을 사용합니다.

## 진행 규칙

- 첫 적은 웨이브 시작 후 첫 소환 Update에서 생성됩니다. 이후 `Spawn Interval` 간격으로 생성합니다.
- 총 소환 수를 채워도 제한 시간이 끝날 때까지 현재 웨이브를 유지합니다.
- 제한 시간이 끝나면 남은 적은 유지하고 다음 웨이브를 즉시 시작합니다. 아직 소환하지 않은 이전 웨이브의 수량은 취소합니다.
- 적은 마지막 웨이포인트에서 첫 웨이포인트로 이동하며 반복합니다. 경로 끝 도달로 삭제되지 않습니다.
- `EnemyHealth`가 있는 활성 생존 적 전체를 집계합니다. 이전 웨이브의 적, 다른 스포너의 적, 미리 배치한 적도 포함하며, 사망/비활성화/파괴 시 제외됩니다.
- 적 수 상한은 소환 제한이 아닙니다. 초과한 순간 `GameState.Defeat`으로 전환하고 웨이브 타이머와 소환을 중단합니다.
- 마지막 웨이브 시간이 끝난 후 모든 적을 처치하면 `GameState.Victory`로 전환합니다.
- 게임 종료 시 적 오브젝트는 유지합니다. 결과 UI나 전투 전체 정지는 `GameManager.OnGameStateChanged`에 연결할 수 있습니다.
- `StartGame()`으로 재시작하면 연결된 스포너가 생성한 적을 정리하고 첫 웨이브부터 진행합니다.

## 상태 확인

- `GameManager`: `State`, `CurrentWave`, `CurrentEnemyCount`. 인스펙터의 게임 진행 필드는 런타임 확인용입니다.
- `WaveManager`: `RemainingTime`, `IsRunning`, `IsComplete`, `OnWaveStart`, `OnWaveEnd`.
- `EnemySpawner.ActiveEnemyCount`는 해당 스포너가 추적하는 적 수입니다. 게임 오버 판정에는 `GameManager.CurrentEnemyCount`의 전체 집계를 사용합니다.
- `TestManager`에는 기존 아군 대기 필드 배치만 남아 있습니다. Space 키 적 소환은 제거했습니다.

## 플레이 모드 확인 항목

1. 두 웨이브의 Duration을 짧게 설정하고 적을 처치하지 않은 채 첫 타이머가 끝나도 기존 적이 남고 두 번째 적이 추가되는지 확인합니다.
2. Max Enemy Count를 3으로 설정하고 3마리에서는 진행, 4번째 생성 시 패배 및 추가 소환 중단을 확인합니다.
3. 두 번째 스포너 또는 미리 배치한 적도 전체 수에 포함되는지 확인합니다. 적의 사망과 비활성화 시 각각 한 번만 차감되어야 합니다.
4. 적이 경로 끝에서 첫 지점으로 돌아가며 전체 수가 줄지 않는지 확인합니다.
5. 마지막 웨이브의 시간이 끝난 뒤 남은 적을 모두 처치했을 때 승리하는지 확인합니다.
6. 재시작 시 기존 소환 적이 정리되고 첫 웨이브가 한 번만 시작하는지 확인합니다.
