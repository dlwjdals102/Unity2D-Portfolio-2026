# Unity / C# 코딩 규칙

## 금지

- `Update()` 안에서 `GetComponent`, `Find`, `Camera.main` 호출
  → `Awake`에서 캐싱
- `GameObject.Find` / `FindObjectOfType` 상시 사용
  → `[SerializeField]` 인스펙터 참조 우선, 초기화 1회만 예외 허용
- **`FindObjectOfType` / `FindObjectsOfType` 자체가 Unity 6에서 폐기됨**
  → 꼭 써야 하면 `FindFirstObjectByType` / `FindAnyObjectByType`을 쓴다.
- `public` 필드로 인스펙터 노출 → `[SerializeField] private` 사용
- 매 프레임 `Instantiate` / `Destroy` → 오브젝트 풀링
- 문자열 기반 API: `SendMessage`, `Invoke("이름")`, `Animator.Play("이름")`
  → 애니메이터는 `Animator.StringToHash`로 캐싱
- **Unity 오브젝트에 `?.` `??` `?? =` 사용**
  → Unity의 fake null과 충돌해 파괴된 오브젝트를 살아있다고 판단한다.
    반드시 `if (obj == null)`로 비교한다. *(면접 단골 질문)*
- `Update` 경로에서 LINQ 및 람다 캡처 → GC Alloc 유발
- 릴리스 경로에 `Debug.Log` 잔류

## 준수

- 코루틴은 시작한 곳이 정리 책임을 진다. `OnDisable`에서 중단 처리.
- 이벤트 구독(`+=`)은 반드시 해제(`-=`)와 짝을 이룬다.
- UI는 매 프레임 폴링하지 않고 **이벤트로 갱신**한다.
- 씬 간 데이터 전달에 static 남용 금지.

## 네이밍

| 대상 | 규칙 | 예 |
|---|---|---|
| 클래스 / 메서드 | PascalCase | `EnemyController`, `TakeDamage` |
| private 필드 | `_camelCase` | `_currentHp` |
| public 프로퍼티 | PascalCase | `CurrentHp` |
| 상수 | PascalCase | `MaxHealth` |
| 인터페이스 | `I` + PascalCase | `IDamageable` |
| bool 필드·프로퍼티 | `Is` / `Has` / `Can` + PascalCase | `IsDashing`, `HasBufferedDash` |
| ScriptableObject 에셋 | `타입_이름` | `EnemyData_Slime` |

`bool`에서 접두사를 빼면 명사처럼 보여 숫자나 객체를 담은 것으로 읽힌다.
접두사를 붙이되 **뒷부분을 문장으로 읽어 보고 말이 되는지 확인한다.**
`IsDashCooldown`은 "이것이 대시 쿨다운인가"로 읽혀 어색하다.
`IsDashOnCooldown`("대시가 쿨다운 중인가")처럼 고친다.

**설정값 필드와 이름이 겹치지 않게 한다.** `float _dashCooldown`이 있는데
`bool DashCooldown`을 두면 밑줄과 대소문자만 다른 두 이름이 생긴다.

부정이 겹치면 읽기 어려워진다. `!IsDashOnCooldown`이 여러 번 나오면
`IsDashReady`처럼 의미를 뒤집는 편이 나을 때가 있다.

## 클래스 안의 선언 순서

**항상 같은 순서로 쓴다.** 어디를 봐야 할지 고민할 일이 없어진다.

1. 인스펙터에 노출되는 값 (`[SerializeField]`): 관련된 것끼리 `[Header]`로 묶는다
2. 다른 오브젝트·컴포넌트 참조
3. 내부 상태 (런타임에만 바뀌는 값)
4. 프로퍼티
5. 유니티 생명주기 메서드. 순서는 `Awake` → `OnEnable` → `Start` → `Update` → `FixedUpdate` → `OnDisable` → `OnDestroy`
6. 나머지 메서드

**`[SerializeField]` 가 붙었다고 다 1번이 아니다.** ScriptableObject 데이터
참조는 값의 자리를 대신하므로 1번이고, 씬 오브젝트 참조는 2번이다.

**3번과 4번을 섞지 않는다.** 값(상태)과 계산(프로퍼티)이 뒤섞이면
무엇이 진짜 데이터인지 흐려진다.

인스펙터 값이 대여섯 개를 넘어가면 `[System.Serializable]` 클래스로 묶는 것을 고려한다.
Phase 3부터는 ScriptableObject로 빼는 쪽이 우선이다.

## 어셈블리 (asmdef)

근거와 버린 대안은 `docs/decisions/asmdef-structure.md` 에 있다.

- 의존 방향은 한쪽이다. `Logic ← Runtime ← Editor`, `Tests → Logic`.
  **거꾸로 참조하고 싶어지면 코드가 잘못된 자리에 있다는 신호다.**
- 테스트할 로직은 `Logic/`에 둔다. MonoBehaviour·씬에 의존하면 Logic이 아니다.
- **`internal`은 어셈블리 경계를 넘지 못한다.** 테스트 대상 타입은 `public`으로 쓴다.
- 테스트 어셈블리는 `Runtime`을 참조하지 않는다. 일부러 막아둔 것이다.
  참조를 열면 MonoBehaviour를 테스트하려 들게 되고 테스트가 느려지고 깨진다.
