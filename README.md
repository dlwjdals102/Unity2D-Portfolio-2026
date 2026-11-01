# JM2D

아이템을 가방 어디에 놓느냐로 빌드가 갈리는 2D 탑다운 액션 로그라이트.
게임 클라이언트 개발자 지원용 포트폴리오입니다.
Unity 6.3 LTS (6000.3.9f1) · C# · 1인 · 약 3~4개월 (2026.08 ~)

> **(GIF 자리)** 가방에 아이템을 끼우고 스탯이 바뀌는 장면

**[브라우저에서 플레이](https://play.unity.com/ko/games/a6484628-c957-42ee-aaa5-a6d5591c3d69/unity2d-portfolio-2026)** · **[플레이 영상](#)** · **[상세 포트폴리오 (노션)](#)**

## 기술 스택

| 분류 | 쓴 것 |
|---|---|
| 게임플레이 | 상태 머신(FSM), 상속과 다형성, 인터페이스 기반 피격 판정 |
| 입력과 UI | Input System 액션 맵, 대시 선입력 버퍼, 이벤트 기반 UI 갱신 |
| 최적화 | 투사체 오브젝트 풀링 |
| 데이터 | ScriptableObject 데이터 주도, 스탯과 모디파이어 중첩, JSON 세이브 직렬화 |
| 알고리즘 | 절차적 방 배치, BFS 흐름장 길찾기, 격자 배치 유효성 검사 |
| 구조와 검증 | 어셈블리 분리, EditMode 유닛테스트 |

셋은 상세 포트폴리오에서 설계 판단과 함께 자세히 다룹니다.

- **가방 격자 배치와 시너지**: 목표를 바꾸면 가방의 절반 이상을 다시 놓게 됩니다
- **적 공통 부모와 보스**: 보스를 올릴 때 부모 클래스가 한 줄 바뀌고 적 넷은 그대로였습니다
- **절차적 방 배치와 BFS 흐름장 길찾기**: 적이 몇 마리로 늘어도 길찾기 계산은 한 번입니다

## 구조

**제가 쓴 코드는 [`Assets/_Project`](Assets/_Project) 안에만 있습니다.**
받아 온 것은 [`Assets/ThirdParty`](Assets/ThirdParty) 에 모았고 출처는 맨 아래 표에 전부 적었습니다.

**어셈블리를 넷으로 나누고 의존 방향을 한쪽으로 고정했습니다.**

```
JM2D.Logic  <--  JM2D.Runtime  <--  JM2D.Editor     화살표는 '참조한다' 를 뜻합니다
    ^
    |
JM2D.Tests.EditMode                                 Logic 만 참조합니다
```

`JM2D.Logic` 은 MonoBehaviour 를 쓰지 않는 계산만 담고 **테스트 대상**입니다.
테스트 어셈블리가 `Runtime` 을 참조하는 것은 **일부러 막았습니다.**
[어셈블리를 가른 이유와 버린 대안](docs/decisions/asmdef-structure.md)

**어느 폴더를 보면 되는지.**

| 무엇 | 어디 |
|---|---|
| 순수 로직과 그 테스트 | [`Logic/`](Assets/_Project/Scripts/Logic) 과 [`Tests/EditMode/`](Assets/Tests/EditMode). 주제 폴더가 짝을 이룹니다 |
| 플레이어 입력과 이동, 대시 | [`Scripts/Player/`](Assets/_Project/Scripts/Player) |
| 적 상태 머신과 보스 | [`Scripts/Enemy/`](Assets/_Project/Scripts/Enemy) |
| 피격 판정과 체력 | [`Scripts/Combat/`](Assets/_Project/Scripts/Combat) |
| 방 생성과 타일 칠하기 | [`Scripts/Rooms/`](Assets/_Project/Scripts/Rooms) |
| 데이터 에셋 클래스 | [`Scripts/Data/`](Assets/_Project/Scripts/Data) |

## 설계 기록

판단할 때마다 고른 안과 버린 안을 남겼습니다. 바꾼 판단은 옛 문서를 지우지 않고 이어 붙였습니다.
[설계 결정](docs/decisions) · [기능 명세](docs/specs) · [기획서](docs/design/GDD.md)

<details>
<summary><b>조작</b></summary>

| 키 | 하는 일 |
|---|---|
| `WASD` / 방향키 / 패드 왼쪽 스틱 | 이동 |
| `Space` / `Shift` | 대시 |
| 마우스 이동 | 조준 |
| 마우스 왼쪽 | 공격 |
| `Tab` | 가방 열고 닫기 |
| `R` | 손에 든 아이템 돌리기 |
| `1` `2` `3` | 보상 고르기 |
| `E` | 마을에서 상호작용 |
| `Esc` | 일시정지 |

</details>

<details>
<summary><b>사용 리소스</b></summary>

직접 만들지 않은 것은 전부 여기 적습니다. 이 표가 유일한 출처 기록입니다.

| 종류 | 이름 | 출처 | 라이선스 |
|---|---|---|---|
| 폰트 | Pretendard v1.3.9 (길형진) | [github.com/orioncactus/pretendard](https://github.com/orioncactus/pretendard) | SIL Open Font License 1.1 |
| 스프라이트 | TOP-DOWN Adventure Pack v1.0 (o_lobster) | [o-lobster.itch.io/adventure-pack](https://o-lobster.itch.io/adventure-pack) | CC BY 4.0 |
| 스프라이트 | Ninja Adventure Asset Pack (pixel-boy) 중 예고 표식 1점 | [pixel-boy.itch.io/ninja-adventure-asset-pack](https://pixel-boy.itch.io/ninja-adventure-asset-pack) | CC0 1.0 |
| 효과음 | Impact Sounds (Kenney) 중 `footstep_carpet_000.ogg` 1점 | [kenney.nl/assets/impact-sounds](https://kenney.nl/assets/impact-sounds) | CC0 1.0 |
| 효과음 | RPG Audio (Kenney) 중 `drawKnife1.ogg` 1점 | [kenney.nl/assets/rpg-audio](https://kenney.nl/assets/rpg-audio) | CC0 1.0 |
| 음악 | Chiptune Adventure Music Pack (PPEAK / Preston Peak) 중 `secret base` 1곡 | [ppeak.itch.io/chip-free](https://ppeak.itch.io/chip-free) | CC BY 4.0 |
| 스프라이트 | RPG Icon Pack - Dark Fantasy Equipment (Clockwork Raven) 중 아이템 아이콘 15점 | [clockworkraven.itch.io/rpg-icon-pack-100-dark-fantasy-equipment](https://clockworkraven.itch.io/rpg-icon-pack-100-dark-fantasy-equipment) | 자체 라이선스 (아래) |

**아이콘 팩만 저장소에 없습니다.** 그 팩의 라이선스가 에셋 재배포를 금지합니다.
아이콘이 비면 색 사각형으로 그려서 **클론해도 게임은 돕니다.**
그림을 넣으려면 위 링크에서 받아 `Assets/ThirdParty/Sprites/16x16.png` 로 두면 됩니다.
`.meta` 를 저장소에 남겨 두어 슬라이스와 연결이 그대로 복원됩니다.

</details>
