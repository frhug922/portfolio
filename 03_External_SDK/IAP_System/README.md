### 📁 IAP_System

Unity IAP 및 원스토어 SDK를 기반으로 인앱 결제 시스템을 구성한 모듈입니다.

---

## 💡 주요 기능

- Unity IAP 및 OneStore SDK 기반 인앱 결제 처리
    
- 구매 요청/응답 처리 및 실패/성공 이벤트 핸들링
    
- 결제 서버 검증 및 재시도 로직 포함
    
- 구매 복원 및 가격 정보 조회
    

---

## 🛠 사용 기술

- Unity IAP
    
- 원스토어 SDK (PurchaseClientImpl)
    
- Unity Coroutine
    
- JSON 파싱 및 로컬 저장 처리
    

---

## 📂 구성 파일

|파일명|설명|
|---|---|
|`IAPManager.cs`|Unity IAP 및 OneStore SDK를 모두 지원하는 결제 통합 매니저. 상품 등록, 결제 요청, 결과 처리, 서버 검증 로직 포함.|

---

## 📌 특이사항

- 플랫폼에 따라 Android OneStore 또는 Unity IAP를 자동으로 분기 처리
    
- 결제 완료 후 서버 검증이 실패할 경우, 일정 횟수 재시도 후 사용자에게 안내
    
- 원스토어 연동 시 별도 PurchaseData 객체를 통해 상품 정보 저장 및 확인 수행