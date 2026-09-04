---
updated: 2026-09-04
phase: 3
---

# 현재 상태

> **덮어쓰는 문서다.** 과거 기록을 쌓지 않는다. 이력은 git이 갖고 있다.
> 작업을 시작할 때 읽고, 마칠 때 갱신한다.

## 한 줄 요약

**Phase 3 완료.** 다음은 Phase 4 콘텐츠 확장이다. 맵 구조를 방 8개로 확정했다.

## 지금 하는 일

**Phase 3 는 끝났다.** 세 항목을 순서대로 마쳤다.

```
[x] 1. 오브젝트 풀링 (+ 프로파일러 전후 측정)
[x] 2. ScriptableObject 데이터 분리
[x] 3. 스탯 · 모디파이어 시스템 (+ EditMode 테스트 22개)
```

원래 4번이던 '적 종류 확장하면서 FSM 재검토' 는 Phase 4 콘텐츠 확장과
같은 작업이라 그쪽으로 넘겼다. 미리 나누지 않고 적이 실제로 늘 때 판단한다.

2번이 3번의 바닥을 깔았다. 적과 투사체 수치가 데이터 에셋으로 나왔고,
컴포넌트는 `Awake` 에서 그 값을 자기 필드로 복사한다.
**그 필드가 곧 버프를 곱할 자리다.** 근거는 [데이터 소유](../decisions/data-ownership.md)에 있다.

2번에서 미뤄뒀던 플레이어 수치는 3번에서 `PlayerStats` 로 모였다.
ScriptableObject 로 빼지는 않았다. 하나뿐인 것을 데이터로 빼는 것은
확장성 증명이 되지 않기 때문이다.

**3번에서 `JM2D.Logic` 어셈블리를 처음 썼고 asmdef 4분할이 증명됐다.**
`Stat`, `StatModifier`, `ModifierType` 이 `System` 과 `System.Collections.Generic`
만 참조하므로 테스트 22개가 씬도 플레이도 없이 1초 안에 끝난다.
근거는 [스탯 설계](../decisions/stat-modifier-design.md)에 있다.

ROADMAP 의 "이벤트 기반 UI 갱신"과 "FSM 으로 정리"는 Phase 2에서 이미 됐다.
UI 는 전부 이벤트로 돌고, 적은 처음부터 상태 머신으로 만들었다.

**각 기능은 착수 전 `docs/specs/`에 명세를 만든다.**

### 정리해야 할 것

- 폰트 에셋의 `Clear Dynamic Data on Build` 가 꺼져 있다. Phase 6 빌드 전에 켠다.
- 성능 측정을 에디터에서 했다. `EditorLoop` 이 85%를 차지한다.
  Phase 6 빌드 때 개발 빌드로 다시 잰다.
- **적을 풀링하면 `Health.InitializeMaxHealth` 호출이 깨진다.** `EnemyController.Awake`
  에서 부르고 있어 재사용될 때 다시 돌지 않는다. 투사체에서 `_damage` 를 `Launch` 로
  옮긴 것과 같은 문제다. **적 풀링을 넣는 Phase 4에서 함께 본다.**
- **디버그 키(`StatDebugKeys`)를 Phase 4에서 지운다.** 아이템이 생기면 역할이 끝난다.
  `Gameplay` 씬의 `_DebugKeys` 오브젝트째로 지우면 된다.
- **방을 크게 만들면 카메라가 플레이어를 따라가야 한다.** 지금은 고정이다.
  맵 구조 결정의 파급효과이며 Phase 4 방 작업에서 함께 본다.
- **수치 스케일이 작아 퍼센트 증가가 묻힌다.** 공격력 1에 +10% 는 반올림해도 1이다.
  기본 수치를 키워야 풀리므로 **Phase 4 밸런싱에서 다룬다.**

## 막힌 것 (Blockers)

없음.

## 확정된 것

바꾸려면 근거가 필요하다. 가볍게 뒤집지 않는다.

| 항목 | 결정 |
|---|---|
| 엔진 | Unity 6.3 LTS (6000.3.9f1), **고정** |
| 장르 | 2D 탑다운 액션 로그라이트 |
| 조작 | 수동 조준 + 대시 회피 |
| 정체성 | 가방 그리드 배치로 빌드가 갈린다 |
| 아트 | 도형 프로토타입 기본, 에셋은 Phase 5 |
| 협업 | 페어 모드 (CLAUDE.md 1번) |
| 렌더 파이프라인 | **Built-in** (URP 미도입) |
| 입력 | Input System 1.18.0 **전용** (구 Input Manager 비활성) |
| 입력 사용법 | `.inputactions` 에셋 + 생성 C# 래퍼 |
| 게임 제목 | **JM2D** |
| 어셈블리 | JM2D.Logic / .Runtime / .Editor / .Tests.EditMode |
| 맵 구조 | **방 8개 + 절차적 배치.** 방 하나는 화면 두세 개 분량으로 크게 |

### 설치된 패키지

2D 템플릿에서 이 프로젝트에 필요한 것만 남겼다.
버전은 에디터 권장값을 따랐다 (템플릿 기본값은 6.1 기준이라 6.3에서 컴파일 실패).

`2d.sprite` `2d.tilemap` `ide.visualstudio` `inputsystem` `test-framework` `ugui`

제외: URP, 2D animation, psdimporter, spriteshape, aseprite, timeline,
visualscripting, collab-proxy, multiplayer.center

## 열려 있는 질문

결론이 나면 여기서 지우고 `확정된 것` 또는 `docs/decisions/`로 옮긴다.

- **URP 도입 여부**: Phase 5에서 2D 라이트가 필요하면 그때 판단.
  스프라이트 기반이라 나중 전환 비용이 낮아 지금 결정하지 않았다.
- 아이템 20종이 데이터 추가만으로 되는지 → **Phase 4 에서 서너 종을 만들어 보고 점검.**
  안 되면 12종으로 축소. 일정이 계획보다 빨라 주차 기준은 의미가 없어졌다.
- GitHub 저장소 공개 여부 (제출 시점에 공개 필요)
