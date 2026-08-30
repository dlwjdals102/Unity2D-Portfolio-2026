<!-- 이 파일은 매 세션 자동으로 로드된다. CLAUDE.md 와 같은 우선순위다.
     원래 CLAUDE.md 6번 이었으나 분량 때문에 분리했다. -->

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
| ScriptableObject 에셋 | `타입_이름` | `EnemyData_Slime` |
