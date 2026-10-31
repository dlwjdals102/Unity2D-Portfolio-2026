# JM2D

아이템을 가방 어디에 놓느냐로 빌드가 갈리는 2D 탑다운 액션 로그라이트.
게임 클라이언트 개발자 지원용 포트폴리오입니다.
Unity 6.3 LTS (6000.3.9f1) · C# · 1인 · 약 3~4개월 (2026.08 ~)

<!-- 계단 4에서 끼운다: 가방에 아이템을 끼우고 스탯이 바뀌는 GIF -->
> **(GIF 자리)** 가방에 아이템을 끼우고 스탯이 바뀌는 장면

**[상세 포트폴리오 (노션)](#)** · **[플레이 영상](#)** · **[실행 빌드 (Windows)](#)**

## 기술 스택

| 분류 | 쓴 것 |
|---|---|
| 게임플레이 | 상태 머신(FSM), 상속과 다형성, 인터페이스 기반 피격 판정, 이벤트 기반 UI 갱신 |
| 최적화 | 투사체 오브젝트 풀링 |
| 데이터 | ScriptableObject 데이터 주도, JSON 세이브 직렬화 |
| 알고리즘 | 절차적 방 배치, BFS 흐름장 길찾기, 격자 배치 유효성 검사 |
| 구조와 검증 | 어셈블리 분리, EditMode 유닛테스트 |

셋은 상세 포트폴리오에서 설계 판단과 함께 자세히 다룹니다.

- **가방 격자 배치와 시너지**: 5×5 격자의 어디에 어떻게 놓느냐로 빌드가 갈립니다
- **적 공통 부모와 보스**: 일반 적과 보스가 한 부모를 씁니다. 보스를 올릴 때 부모 클래스가 한 줄 바뀌었습니다
- **절차적 방 배치와 BFS 흐름장 길찾기**: 적이 몇 마리든 길찾기 계산이 한 번입니다

## 구조

어셈블리를 나누고 **의존 방향을 한쪽으로 고정했습니다.** `JM2D.Runtime`(씬 컴포넌트)이
`JM2D.Logic`(MonoBehaviour 에 의존하지 않는 순수 로직)을 참조하고, `JM2D.Tests.EditMode` 는
`Logic` 만 참조합니다. **`Runtime` 참조는 일부러 막았습니다.**

[어셈블리를 가른 이유와 버린 대안](docs/decisions/asmdef-structure.md)

## 설계 기록

판단할 때마다 고른 안과 버린 안을 남겼습니다.
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

**아이콘 팩만 저장소에서 빼 두었습니다.** 그 팩의 라이선스가 게임과 영상에 쓰는 것은 허용하면서
**에셋을 함께 배포해 받는 쪽이 그 그림에 접근하게 하는 것은 막습니다.** 공개 저장소에 그림을
올리는 것이 거기 걸린다고 보아 `.gitignore` 에 넣었습니다.

**클론해도 게임은 돕니다.** 아이콘이 비면 색 사각형으로 그리도록 처음부터 만들어 두었습니다.
그림을 넣고 싶으면 위 링크에서 받아 `Assets/ThirdParty/Sprites/16x16.png` 로 두면 됩니다.
`.meta` 를 저장소에 남겨 두어 슬라이스와 연결이 그대로 복원됩니다.

</details>
