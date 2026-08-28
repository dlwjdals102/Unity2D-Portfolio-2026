---
date: 2026-08-28
status: accepted
superseded_by:
related: []
---

# 어셈블리 정의(asmdef) 구조를 Logic / Runtime / Editor / Tests 4분할로 한다

## 문제

**`Assembly-CSharp`(asmdef가 없는 기본 어셈블리)는 asmdef에서 참조할 수 없다.**

asmdef를 하나도 두지 않으면 게임 코드가 전부 `Assembly-CSharp`에 들어가고,
테스트 어셈블리가 이를 참조할 방법이 없다. CLAUDE.md 5번이 정의한
"순수 로직에 EditMode 테스트를 작성한다"는 계획이 **성립 자체를 못 한다.**

나중에 바꾸면 폴더와 네임스페이스를 전부 건드리게 되므로 첫 테스트 전에 정해야 했다.

## 검토한 대안

### 안 A — asmdef를 쓰지 않는다
- 장점: 가장 단순. 설정이 없다.
- 단점: **테스트가 불가능하다.** 계획의 전제가 무너진다. → 탈락

### 안 B — 단일 `Game` + Editor + Tests (3개)
- 장점: 단순하면서 테스트는 가능해진다.
- 단점: 순수 로직과 MonoBehaviour 의존 코드의 경계가 강제되지 않는다.
  테스트 대상이 어디까지인지 규율로만 지켜야 한다.

### 안 C — 도메인별 분할 (Player / Enemy / Combat / Data / UI / Core)
- 장점: 의존 방향이 드러나고 순환 참조가 원천 차단된다. 부분 재컴파일.
- 단점: **1인 3~4개월 프로젝트에 과설계.** 초반에 클래스를 옮길 때마다
  참조를 고쳐야 한다. CLAUDE.md 2번(단순함 우선) 위반.

### 안 D — Logic / Runtime / Editor / Tests (4개)
- 장점: **테스트 대상이 어셈블리로 물리적으로 분리된다.**
- 단점: 무엇을 Logic에 둘지 판단이 필요하다.

## 선택과 이유

**안 D를 택했다.**

결정적인 근거는 CLAUDE.md 5번이 이미 "EditMode 테스트 대상 =
MonoBehaviour 의존 없는 순수 로직"이라고 규정해 뒀다는 점이다.
그 경계를 폴더 규칙이 아니라 **어셈블리로 강제**하면,
테스트 가능한 코드가 구조적으로 분리된다.

안 C를 버린 이유는 어셈블리 6개의 참조 관리 비용이
이 규모에서 얻는 이득보다 크기 때문이다.
안 B를 버린 이유는 경계가 규율에만 의존해서, 마감이 급해지면 반드시 무너지기 때문이다.

### 구조

| 어셈블리 | 위치 | 참조 | 역할 |
|---|---|---|---|
| `Game.Logic` | `_Project/Scripts/Logic/` | 없음 | 순수 로직. **테스트 대상** |
| `Game.Runtime` | `_Project/Scripts/` | Logic, InputSystem, UI | MonoBehaviour 등 Unity 의존 |
| `Game.Editor` | `_Project/Scripts/Editor/` | Runtime, Logic | 에디터 툴, 커스텀 인스펙터 |
| `Game.Tests.EditMode` | `Tests/EditMode/` | **Logic만** | EditMode 테스트 |

의존 방향은 한쪽이다. `Logic ← Runtime ← Editor`, 그리고 `Tests → Logic`.

`Scripts/` 루트의 `Game.Runtime`이 하위 폴더를 모두 포함하되,
`Logic/`과 `Editor/`는 자체 asmdef가 있어 그 하위 트리를 가져간다.
따라서 기존 폴더 구조를 바꾸지 않고 폴더 2개만 추가하면 됐다.

### 테스트가 Runtime을 참조하지 않게 한 이유

일부러 `Logic`만 참조하게 했다. Runtime을 참조할 수 있게 하면
결국 MonoBehaviour를 테스트하려 들게 되고, 그러면 씬 셋업이 필요해져
테스트가 느려지고 깨지기 쉬워진다. **참조를 막아서 로직이 Logic으로 밀려나게 했다.**

## 결과

- 스모크 테스트로 `Tests.EditMode → Logic` 참조를 검증했다. **1건 실행, 1건 통과.**
- 검증 후 스모크 파일은 삭제했다. 현재 네 어셈블리 모두 `.cs`가 0개이므로
  DLL은 생성되지 않는다. 이는 정상이다.

### 파급효과 — `internal`이 어셈블리 경계를 넘지 못한다

검증 중 실제로 컴파일 에러(CS0122)로 확인했다.
**`Game.Logic`에서 테스트하려는 타입은 `public`이어야 한다.**
`internal`로 두려면 `InternalsVisibleTo`를 별도로 걸어야 한다.

지금은 `public`으로 간다. 1인 프로젝트에서 `InternalsVisibleTo`는
얻는 것 대비 설정 비용이 크다.

## 한계 / 남은 문제

- **Logic과 Runtime의 경계는 결국 판단이다.** 어셈블리가 강제하는 건
  "Logic이 Runtime을 못 본다"는 것뿐이고, 로직을 Runtime에 써버리는 건 막지 못한다.
- `Game.Logic`에 `noEngineReferences`를 걸지 않았다. 즉 `Vector2Int`, `Mathf` 같은
  UnityEngine 타입을 쓸 수 있다. 완전한 순수 C#은 아니다.
  그리드 좌표와 수학 연산에 이들을 쓰는 편이 실용적이라고 판단했다.
  대신 **MonoBehaviour·씬 의존이 없다**는 경계는 유지된다.
- PlayMode 테스트 어셈블리는 만들지 않았다. 필요해지면 그때 추가한다.
- 어셈블리 이름 접두사 `Game`은 임시다. 게임 제목이 정해지면 바꾼다.
  asmdef 4개와 네임스페이스를 고치면 되므로 **코드가 적은 지금이 가장 싸다.**
