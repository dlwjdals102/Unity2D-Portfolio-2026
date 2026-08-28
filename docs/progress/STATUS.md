---
updated: 2026-08-28
phase: 2
---

# 현재 상태

> **덮어쓰는 문서다.** 과거 기록을 쌓지 않는다. 이력은 git이 갖고 있다.
> 작업을 시작할 때 읽고, 마칠 때 갱신한다.

## 한 줄 요약

**Phase 1 (세팅) 완료.** Unity 프로젝트 생성·컴파일 통과. Phase 2 수직 슬라이스 착수 전.

## 지금 하는 일

Phase 2 첫 기능 착수 대기. 순서는 아래를 따른다.

1. 플레이어 이동 → 2. 공격 → 3. 적 1종 → 4. 피격·사망 → 5. 씬 흐름

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
| 입력 | Input System 1.18.0 |

### 설치된 패키지

2D 템플릿에서 이 프로젝트에 필요한 것만 남겼다.
버전은 에디터 권장값을 따랐다 (템플릿 기본값은 6.1 기준이라 6.3에서 컴파일 실패).

`2d.sprite` `2d.tilemap` `ide.visualstudio` `inputsystem` `test-framework` `ugui`

제외: URP, 2D animation, psdimporter, spriteshape, aseprite, timeline,
visualscripting, collab-proxy, multiplayer.center

## 열려 있는 질문

결론이 나면 여기서 지우고 `확정된 것` 또는 `docs/decisions/`로 옮긴다.

- **어셈블리 정의(asmdef) 구조** — EditMode 테스트로 게임 코드를 검증하려면
  게임 코드가 asmdef 안에 있어야 한다. `Assets/Tests/EditMode`는 폴더만 있고
  아직 비어 있다. **첫 테스트를 쓰기 전에 결정해야 한다.**
- **URP 도입 여부** — Phase 5에서 2D 라이트가 필요하면 그때 판단.
  스프라이트 기반이라 나중 전환 비용이 낮아 지금 결정하지 않았다.
- 아이템 20종이 데이터 추가만으로 되는지 → **7주차에 점검.** 안 되면 12종으로 축소.
- GitHub 저장소 공개 여부 (제출 시점에 공개 필요)
