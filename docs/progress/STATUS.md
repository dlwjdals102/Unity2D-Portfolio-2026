---
updated: 2026-08-28
phase: 2
---

# 현재 상태

> **덮어쓰는 문서다.** 과거 기록을 쌓지 않는다. 이력은 git이 갖고 있다.
> 작업을 시작할 때 읽고, 마칠 때 갱신한다.

## 한 줄 요약

**Phase 2 진행 중.** 이동·대시 완료. 공격 구현 중 (조준까지 완료).

## 지금 하는 일

[플레이어 공격](../specs/player-attack.md) 구현. 계단으로 쪼개서 진행 중이다.

- [x] 명세 작성
- [x] 계단 1 — 마우스 조준 방향 읽기
- [ ] 계단 2 — 클릭하면 투사체가 나간다
- [ ] 계단 3 — 연사 간격
- [ ] 계단 4 — 벽 충돌과 수명
- [ ] 계단 5 — 대시 중 발사 금지

Phase 2 남은 순서: 적 1종 → 피격·사망 → 씬 흐름.
**각 기능은 착수 전 `docs/specs/`에 명세를 만든다.**

## 막힌 것 (Blockers)

없음.

## 확정된 것

바꾸려면 근거가 필요하다. 가볍게 뒤집지 않는다.

| 항목 | 결정 |
|---|---|
| 엔진 | Unity 6.3 LTS (6000.3.9f1) — **고정** |
| 장르 | 2D 탑다운 액션 로그라이트 |
| 조작 | 수동 조준 + 대시 회피 |
| 정체성 | 가방 그리드 배치로 빌드가 갈린다 |
| 아트 | 도형 프로토타입 기본, 에셋은 Phase 5 |
| 협업 | 페어 모드 (CLAUDE.md 1번) |
| 렌더 파이프라인 | **Built-in** (URP 미도입) |
| 입력 | Input System 1.18.0 — **전용** (구 Input Manager 비활성) |
| 입력 사용법 | `.inputactions` 에셋 + 생성 C# 래퍼 |
| 게임 제목 | **JM2D** |
| 어셈블리 | JM2D.Logic / .Runtime / .Editor / .Tests.EditMode |

### 설치된 패키지

2D 템플릿에서 이 프로젝트에 필요한 것만 남겼다.
버전은 에디터 권장값을 따랐다 (템플릿 기본값은 6.1 기준이라 6.3에서 컴파일 실패).

`2d.sprite` `2d.tilemap` `ide.visualstudio` `inputsystem` `test-framework` `ugui`

제외: URP, 2D animation, psdimporter, spriteshape, aseprite, timeline,
visualscripting, collab-proxy, multiplayer.center

## 열려 있는 질문

결론이 나면 여기서 지우고 `확정된 것` 또는 `docs/decisions/`로 옮긴다.

- **URP 도입 여부** — Phase 5에서 2D 라이트가 필요하면 그때 판단.
  스프라이트 기반이라 나중 전환 비용이 낮아 지금 결정하지 않았다.
- 아이템 20종이 데이터 추가만으로 되는지 → **7주차에 점검.** 안 되면 12종으로 축소.
- GitHub 저장소 공개 여부 (제출 시점에 공개 필요)
