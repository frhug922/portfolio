# 🎮 Unity 게임 개발 포트폴리오

이 포트폴리오는 실제 Unity 기반 게임 프로젝트에 참여하며 개발한 **주요 시스템의 구조**, **핵심 소스 코드**, 그리고 **기획 의도 및 기술적 접근 방식**을 정리한 문서입니다.

---

## 📌 포함된 시스템 요약

|시스템명|설명|주요 기술|
|---|---|---|
|**IAP 시스템**|구글 및 원스토어 SDK를 활용한 인앱 결제 처리 구현|Unity IAP, OneStore SDK|
|**광고 시스템**|IronSource SDK를 활용한 보상형 및 배너 광고 기능|IronSource SDK, 유저 보상 처리, 서버 연동|
|**상점 시스템**|서버 연동 및 UI 기반의 상점 탭/상품 관리 시스템|Unity UI, 서버 통신, 상품 정보 파싱|
|**클랜전 시스템**|TCP 소켓 기반의 실시간 클랜전, 참가/매칭/보상 처리|TCP Socket, 서버 연동, 데이터 패킷 구성|

---

## 📂 폴더 구성 설명:

각 시스템 폴더 안에 상세 설명 문서(README.md)와 소스코드가 있습니다.

```scss
📁 포트폴리오 
├── 📄 README.md (현재 문서) 
├── 📁 01_CS_System (Client-Server 시스템)
│   ├── 📄 README.md (클랜전, 소켓 등 요약)
│   ├── 📄 TCPSocketManager.cs
│   ├── 📄 WebHttp.cs
│   └── 📁 ClanWar_System
│       ├── 📄 README.md (클랜전 시스템 설명)
│       └── 📄 ClanWarUI.cs 외 여러 구현 코드
│
├── 📁 02_UI_System (UI 시스템)
│   ├── 📄 README.md (UI 구성 요약 및 처리 방식)
│   ├── 📁 Shop_System
│   │   ├── 📄 README.md (상점 구현 설명)
│   │   └── 📄 ShopUIController.cs 외
│   └── 📁 Pass_System
│       ├── 📄 README.md (패스 UI 처리 설명)
│       └── 📄 PassUI.cs 외
│
└── 📁 03_External_SDK (외부 SDK 연동)
    ├── 📄 README.md (광고, 결제 SDK 통합 요약)
    ├── 📁 Ads_System
    │   ├── 📄 README.md (IronSource 광고 흐름 설명)
    │   └── 📄 AdsManager.cs
    └── 📁 IAP_System
        ├── 📄 README.md (Unity IAP / 원스토어 처리 설명)
        └── 📄 IAPManager.cs
```

---

## 🔧 사용 기술 정리

- **언어**: C#
    
- **엔진 및 프레임워크**: Unity 2021+
    
- **주요 라이브러리/SDK**: Unity IAP, OneStore SDK, IronSource SDK
    
- **서버 연동 방식**: HTTP + TCP Socket (Coroutine 기반 처리)
    
- **기획/설계 방식**: 시스템 분리, OOP 기반 구조화, 실시간 처리 중심 설계