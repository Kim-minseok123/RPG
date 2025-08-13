# [Unity 3D] Online RPG Portfolio 
## 목차
  - [소개](#소개) 
  - [개발 동기](#개발-동기)
  - [개발 환경](#개발-환경)
  - [사용 기술](#사용-기술)
  - [개발 과정](#개발-과정)
  - [플레이영상](#플레이영상)
  - [게임 다운로드](#게임-다운로드)
## 소개
<div align="center">

<img alt="Title" src="https://github.com/user-attachments/assets/65954443-6ff9-4f11-a493-ebea7f529c03" width="49%" height="230"/>
<img alt="login" src="https://github.com/user-attachments/assets/104c5596-7f0b-4051-b30d-1f14c1c1b794" width="49%" height="230"/>
<img alt="Lobby" src="https://github.com/user-attachments/assets/cda836c1-c89d-44cf-9233-c845b8120d73" width="49%" height="230"/>
<img alt="Main" src="https://github.com/user-attachments/assets/06c2ddd1-4a2c-407a-81ea-b2941675089e" width="49%" height="230"/>
<img alt="Fight" src="https://github.com/user-attachments/assets/2cd067a9-accb-4985-90ec-0601ef7bf5fa" width="49%" height="230"/>
<img alt="Quest" src="https://github.com/user-attachments/assets/1b5a1420-4f6c-4661-bb69-17fe56f42b58" width="49%" height="230"/>
<img alt="Shop" src="https://github.com/user-attachments/assets/b090c9f6-0a00-4515-b7db-5e9b8c1fd29e" width="49%" height="230"/>
<img alt="Boss" src="https://github.com/user-attachments/assets/bc28ff0a-5913-4747-9f7d-04c8f9a89bcd" width="49%" height="230"/>
  < 게임 플레이 사진 >

</div>

+ Unity 3D Online RPG 입니다.

+ 게임 개발자로서 역량을 쌓기 위해 처음으로 제작한 온라인 RPG 포트폴리오입니다.

+ 현재 클라이언트는 유로 에셋으로 인해 소스코드만 공개되어 있습니다.

+ 개발 기간: 2024.04.09 ~ 2024.08.21 ( 약 4개월 )

+ 개발 인원 : 1인

+ 형상 관리: Git SourceTree

<br>

## 개발 동기
게임 클라이언트 프로그래머가 되기 위해 꾸준히 공부하던 중, 온라인 게임이 주류를 이루는 게임 시장의 특성을 깨달았다. 

이에 따라 클라이언트와 서버 간의 협업이 필수적이며, 이를 위해서는 두 영역에 대한 상호 지식이 필요하다는 결론에 도달했다. 

이 프로젝트는 RPG를 포함한 온라인 게임의 필수 요소들을 서버와 연동해 개발하면서 클라이언트와 서버에 대한 깊은 이해를 얻기 위해 기획되었다. 
이를 통해 클라이언트와 서버 모두에 대한 이해를 바탕으로 게임 개발자로서의 성장을 이루고자 한다.

<br>

## 개발 환경
+ Unity 2022.3.38f1 LTS

+ C# Console

+ ASP .net Core 웹 애플리케이션

+ MS-SQL, Entity Framework Core

<br>

## 사용 기술
### 🔗 네트워크
- **TCP 서버 구축**을 통해 클라이언트와 서버 간의 안정적인 통신 환경 구현
- **Zone 기반 패킷 최적화**  
  - 클라이언트의 위치와 `VisionSize`에 기반하여 해당 Zone의 데이터만 송수신  
  - 불필요한 패킷 송수신을 차단하여 **네트워크 부하 최소화**
- **멀티스레드 기반 서버 구조**
  - 전투, 로그인 등 주요 기능을 스레드 단위로 분리
  - 각 스레드에 **작업 큐(Work Queue)** 를 두어 Lock 없이 순차적으로 처리  
  - **Deadlock 방지**, 처리 효율 향상

### 🧱 소프트웨어 아키텍처 & 디자인 패턴
- **싱글톤 패턴**을 적극 활용하여 주요 시스템을 매니저 단위로 구성  
  *(UI, Resource, Inventory, Network, Object, Quest, Data, Pool, Sound 등)*
- **상태 머신(Finite State Machine)**  
  - 플레이어 및 몬스터의 **행동 및 상태 전이**를 체계적으로 관리

### 💾 데이터 관리
- **Local DB 활용**  
  - 플레이어, 몬스터, 퀘스트 등의 데이터를 로컬에 저장 및 관리
- **JSON 기반의 외부 데이터 관리**  
  - 아이템, 퀘스트 등 게임 데이터를 JSON으로 분리 관리  
  - 유지보수 용이, 확장성 우수

### 🚀 퍼포먼스 최적화
- **오브젝트 풀링 및 캐싱 기법 적용**
  - 자주 생성/삭제되는 오브젝트에 대한 **재사용 시스템 구현**
  - **메모리 사용 최적화**, **CPU 부담 감소**

## 개발 과정

+ 블로그 개발 과정
https://toward-mainobject.tistory.com/category/Unity/%EC%98%A8%EB%9D%BC%EC%9D%B8%20RPG

  + [[Unity 3D] Title과 Login 구현 (UI 작업, 서버 연동)](https://toward-mainobject.tistory.com/83)
  + [[Unity 3D] 캐릭터 선택창 및 캐릭터 생성](https://toward-mainobject.tistory.com/84)  
  + [[Unity 3D] Navigation을 이용한 캐릭터 이동](https://toward-mainobject.tistory.com/85?category=1179741)  
  + [[Unity 3D] 캐릭터 이동 동기화](https://toward-mainobject.tistory.com/86?category=1179741)  
  + [[Unity 3D] 캐릭터 공격 애니메이션 동기화](https://toward-mainobject.tistory.com/87)
  + [[Unity 3D] 몬스터 움직임 동기화 with Dedicated Server](https://toward-mainobject.tistory.com/88) 
  + [[Unity 3D] 오브젝트 전투 및 몬스터의 플레이어 추격](https://toward-mainobject.tistory.com/89?category=1179741)  
  + [[Unity 3D] 각종 UI 제작! Stat창, Inven창, Equip창](https://toward-mainobject.tistory.com/90) 
  + [[Unity 3D] 아이템 드랍 및 획득](https://toward-mainobject.tistory.com/91?category=1179741)  
  + [[Unity 3D] 획득한 아이템 착용하기](https://toward-mainobject.tistory.com/92)   
  + [[Unity 3D] 스탯 포인트 사용하기](https://toward-mainobject.tistory.com/93?category=1179741)  
  + [[Unity 3D] UI 이미지 드래그 그리고 퀵슬롯 등록하기](https://toward-mainobject.tistory.com/94)  
  + [[Unity 3D] 아이템 인벤창에서 옮기기](https://toward-mainobject.tistory.com/95)  
  + [[Unity 3D] 상점 Npc - 아이템 구매, 판매하기](https://toward-mainobject.tistory.com/96)  
  + [[Unity 3D] 클라이언트, 서버 최적화 ](https://toward-mainobject.tistory.com/97) 
  + [[Unity 3D] 보스 원정대 꾸리기 + 보스 컷신](https://toward-mainobject.tistory.com/98)  
  + [[Unity 3D] 보스 드래곤의 패턴 만들기](https://toward-mainobject.tistory.com/99)  
  + [[Unity 3D] 각종 사운드 추가](https://toward-mainobject.tistory.com/100)  
  + [[Unity 3D] 퀘스트 시스템 구현하기](https://toward-mainobject.tistory.com/101)  
  + [[Unity 3D] 퀘스트 DB에 저장하기](https://toward-mainobject.tistory.com/102)
  + [[Unity 3D] 유저간 채팅 시스템 구현하기](https://toward-mainobject.tistory.com/103)
  + [[Unity 3D] 3D RPG 게임 테스트 플레이 영상](https://toward-mainobject.tistory.com/104)

<br>

## 플레이영상
https://www.youtube.com/watch?v=Ayq_w8Nptmc

## 게임 다운로드
현재는 서버를 배포하지 않은 상태이기 때문에 로컬 서버에서만 플레이 가능합니다.

서버 :

Server.sln을 실행 후 패키지 관리자 콘솔에 AccountServer와 Server 프로젝트를 선택후 
update-database -Context AppDbContext 를 입력해주시고 Server와 AccountServer를 실행하면 됩니다.

클라이언트 :

https://drive.google.com/file/d/1is8LPT8gwKZkPly40ZcZdfnmIQCEkRH0/view?usp=sharing
링크에서 다운 받은 후 압축을 풀고 RPG 폴더 안(Assets와 같은 위치)에 Builds 폴더를 넣으면 됩니다.(이 후 서버를 열어야 정상작동합니다.) Buils안 RPG1이나 RPG2를 실행하시면됩니다.


