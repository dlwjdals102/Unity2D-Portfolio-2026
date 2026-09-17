---
date: 2026-09-17
status: accepted
superseded_by:
related: [asmdef-structure.md, room-layout-linear-walk.md, ../specs/room-layout.md]
---

# Logic 어셈블리를 주제별 하위 폴더로 나누고 네임스페이스도 따라간다

## 문제

`Scripts/Logic/` 에는 하위 폴더 없이 파일이 쌓였다. 방 배치 생성 규칙을 넣고 나니 **15 개**가 되어 한눈에 보기 어려웠다.

| 주제 | 파일 |
|---|---|
| 스탯 | `Stat`, `StatModifier`, `ModifierType`, `StatType` |
| 가방 | `BagGrid`, `IGridItem`, `PlacedItem`, `SynergyCondition` |
| 방 배치 | `RoomCell`, `RoomLayout`, `RoomLayoutGenerator`, `RoomType`, `DoorSide` |
| 일반 계산 | `Hysteresis`, `CircleSpread` |

Runtime 은 이미 기능별 폴더(`Player`, `Enemy`, `Combat` 등)였고 `Logic/` 만 한 층이었다.
[asmdef 결정](asmdef-structure.md) 때는 `Logic/` 이 비어 있어 안의 모양을 정하지 않았다.

## 검토한 대안

### 안 A: 한 층에 두고 이름 앞말로 묶는다
- 장점: 옮길 것이 없다. 방 파일을 넣을 때 이렇게 했다(`Room` 으로 시작).
- 단점: 파일이 늘수록 목록이 길어진다. `Hysteresis` 처럼 앞말이 없는 파일은 묶이지 않는다.

### 안 B: 주제별 하위 폴더, 네임스페이스는 `JM2D.Logic` 그대로
- 장점: 고칠 줄이 적다.
- 단점: '네임스페이스는 스크립트 폴더를 따른다' 는 규칙에 예외가 생긴다.

### 안 C: 주제별 하위 폴더, 네임스페이스도 폴더를 따른다
- 장점: 규칙 그대로다. Test Runner 목록이 주제별로 묶인다.
- 단점: `Logic` 15 개의 `namespace` 줄과 쓰는 쪽 17 개 파일의 `using` 을 고친다.

### 일반 계산 둘의 자리

| 안 | 판단 |
|---|---|
| **`Common/`** | 두 파일은 이름과 코드에 '적' 이라는 말이 없다. 다만 아무거나 넣는 서랍이 되기 쉽다 |
| `Enemies/` | 지금 쓰는 곳이 적뿐이라 찾기 쉽다. 방 배치나 보스가 같은 계산을 쓰는 날 폴더 이름이 틀린 말이 된다 |

## 선택과 이유

**안 C. `Stats/`, `Bag/`, `Rooms/`, `Common/` 으로 나누고 네임스페이스를 `JM2D.Logic.Stats` 처럼 바꿨다.
테스트도 같은 모양으로 옮기고 `JM2D.Tests.Stats` 처럼 바꿨다.**

Unity 의 프로젝트 정리 가이드는 종류별이든 기능별이든 한 방식을 일관되게 쓰라고 권한다. Runtime 이 기능별이라 `Logic/` 도 맞췄다.
안 B 는 규칙에 예외를 만들고, 고칠 줄이 적다는 이득은 한 번뿐이다.

`Common/` 이 서랍이 되지 않도록 **게임 용어가 없는 계산만 둔다**는 조건을 규칙에 함께 적었다.

**Runtime 은 건드리지 않았다.**

- `Core/` 의 `EnemyCounter` 는 공용이 아니라 클리어 판정이다. 4-C 에서 방 클리어로 바뀔 가능성이 커 그때 자리를 정한다. 지금 옮기면 두 번 옮긴다.
- `Data/` 만 종류별(ScriptableObject 모음)이다. 에셋 폴더 `ScriptableObjects/` 와 짝이라 그대로 둔다.
- 폴더 이름의 단수와 복수가 섞여 있다(`Enemy`, `Player` 와 `Items`, `Weapons`). 바꾸면 네임스페이스를 쓰는 곳이 모두 바뀌고 이번 목적과 관계가 없다.

## 결과

- 파일 22 개를 `.meta` 와 함께 옮겼다. git 에 있던 14 개는 `git mv` 로 옮겨 이름 바꾸기로 남았다.
- 네임스페이스를 바꾼 파일은 22 개(Logic 15, 테스트 7), `using` 만 바꾼 Runtime 파일은 10 개다.
  `using` 은 주석을 뺀 코드에서 실제로 쓰는 타입을 보고 넣어, 쓰지 않는 `using` 이 생기지 않았다.
  `ItemData` 와 `ItemInventory` 는 스탯과 가방을 둘 다 써 두 줄이 됐다.
- **직렬화된 데이터는 깨지지 않았다.** 에셋, 프리팹, 씬에 `JM2D.Logic` 을 이름으로 저장한 곳이 없었고, `[SerializeReference]` 를 쓰지 않으며,
  `ItemData` 의 열거형은 번호로 저장된다. Logic 에는 컴포넌트가 없어 스크립트 참조(GUID)도 걸리지 않는다.
- EditMode 테스트 111 개가 통과했고, 게임 한 판과 `Missing Script` 를 확인했다.

## 한계 / 남은 문제

- **`Common/` 은 조건으로만 지킨다.** 컴파일러가 막지 않는다.
- **주제 폴더 사이를 옮길 때마다 `using` 이 바뀐다.** 한 주제가 다른 주제의 타입을 쓰기 시작하면 폴더 경계가 맞는지 다시 본다.
  지금 Logic 안에서 주제를 넘는 참조는 없다.
- **asmdef 결정은 `Logic` 에서 `Vector2Int` 와 `Mathf` 를 허용하지만, 지금까지 `UnityEngine` 을 쓴 파일이 없다.**
  `RoomCell` 주석에 이것을 규칙처럼 적었다가 사실에 맞게 고쳤다. 허용과 관례가 다르다는 것을 기억한다.
- Runtime 폴더 이름의 단수와 복수가 여전히 섞여 있다.
