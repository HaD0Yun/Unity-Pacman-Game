# Unity Pacman Game

> **이 프로젝트는 30분 만에 오로지 AI가 모든 것을 만들었습니다.**  
> 코드 작성, 아키텍처 설계, 버그 수정까지 전 과정을 AI(Claude)가 수행했습니다.

![Unity](https://img.shields.io/badge/Unity-6000.3.0f1-black?logo=unity)
![License](https://img.shields.io/badge/License-MIT-green)
![AI Generated](https://img.shields.io/badge/AI%20Generated-100%25-blue)

## 🎮 소개

클래식 팩맨을 Unity 6 (URP)로 재현한 프로젝트입니다. 
외부 에셋 없이 **모든 스프라이트를 코드로 절차적 생성**하며, 
원작의 고스트 AI 패턴을 충실히 구현했습니다.

### ✨ 주요 특징

- **100% AI 제작**: 30분 내에 AI가 설계부터 구현까지 전 과정 수행
- **외부 에셋 제로**: 모든 그래픽을 `ProceduralSpriteGenerator`로 코드 생성
- **클래식 미로**: 원작과 동일한 28x31 타일 구조
- **4가지 고스트 AI**: 각각 고유한 추격 패턴 구현
  - 🔴 **Blinky (Shadow)**: 팩맨 직접 추격
  - 🩷 **Pinky (Speedy)**: 팩맨 4칸 앞 매복
  - 🩵 **Inky (Bashful)**: Blinky와 협동 측면 공격
  - 🟠 **Clyde (Pokey)**: 8칸 이내 접근 시 후퇴
- **New Input System**: 최신 Unity 입력 시스템 지원

---

## 🕹️ 조작법

| 키 | 동작 |
|:---:|:---:|
| `W` `A` `S` `D` / `↑` `←` `↓` `→` | 이동 |
| `R` | 게임 재시작 |

---

## 🏗️ 프로젝트 구조

```
Assets/
├── Scripts/
│   ├── Core/           # GameManager, GameConstants, GameBootstrapper
│   ├── Maze/           # MazeData, MazeGenerator (Tilemap)
│   ├── Player/         # PacmanController
│   ├── Ghosts/         # GhostBase, Blinky, Pinky, Inky, Clyde
│   ├── Collectibles/   # Dot, PowerPellet, DotManager
│   ├── UI/             # UIManager
│   └── Utilities/      # ProceduralSpriteGenerator
├── ScriptableObjects/
│   ├── GameConfig/     # 게임 설정 데이터
│   └── MazeData/       # 미로 레이아웃 데이터
└── Scenes/
    └── SampleScene.unity
```

---

## 🎯 구현된 기능

### 게임플레이
- [x] 그리드 기반 이동 + 입력 버퍼링
- [x] 터널 랩어라운드 (좌우 이동)
- [x] 도트 & 파워펠릿 수집
- [x] 점수 시스템 (10점/도트, 50점/파워펠릿)
- [x] 생명 시스템 (3개 시작)
- [x] 레벨 진행

### 고스트 AI
- [x] Scatter 모드 (코너 순찰)
- [x] Chase 모드 (팩맨 추격)
- [x] Frightened 모드 (파워펠릿 효과)
- [x] Eaten 모드 (눈만 남아 귀환)
- [x] 고스트 하우스 출입

### 기술적 특징
- [x] 절차적 스프라이트 생성
- [x] Tilemap 기반 미로 렌더링
- [x] 컴포넌트 기반 충돌 감지 (태그 미사용)
- [x] ScriptableObject 기반 설정
- [x] 이벤트 드리븐 아키텍처

---

## 🛠️ 기술 스택

- **Unity** 6000.3.0f1
- **Render Pipeline**: URP (Universal Render Pipeline)
- **Input System**: New Input System
- **UI**: TextMesh Pro

---

## 🚀 실행 방법

1. Unity Hub에서 Unity 6000.3.x 설치
2. 프로젝트 열기
3. `Assets/Scenes/SampleScene.unity` 실행
4. Play 버튼 클릭

---

## 🤖 AI 개발 과정

이 프로젝트는 **Claude (Anthropic)**가 다음 과정을 통해 30분 만에 완성했습니다:

1. **아키텍처 설계**: 폴더 구조, 클래스 설계
2. **코드 작성**: 17개 C# 스크립트 생성
3. **미로 데이터**: 클래식 팩맨 레이아웃 구현
4. **스프라이트 생성**: 절차적 그래픽 시스템
5. **버그 수정**: 태그 오류, 입력 시스템, 초기화 순서 문제 해결
6. **테스트 & 디버깅**: Unity MCP를 통한 실시간 테스트

### 사용된 AI 도구
- **Claude**: 코드 생성 및 아키텍처 설계
- **Unity MCP**: Unity Editor 실시간 제어 및 테스트

---

## 📄 라이선스

MIT License

---

## 🙏 크레딧

- **Original Pacman**: Namco (1980)
- **AI Development**: Claude (Anthropic)
- **Unity MCP Integration**: AI-Game-Developer

---

<p align="center">
  <b>🎮 Made with AI in 30 minutes 🤖</b>
</p>
