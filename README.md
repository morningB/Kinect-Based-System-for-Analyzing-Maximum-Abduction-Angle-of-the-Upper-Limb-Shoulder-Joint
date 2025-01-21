## 프로젝트 개요

- 실제 임상에서 사용되는 **접촉식 각도기**를 대체하는 **비접촉식** 시스템을 개발하여 환자의 팔 외전각을 측정할 수 있게 했습니다.
- 이 시스템은 정확한 측정 데이터를 제공하고 **재활 치료 과정**의 효율성을 향상시키는 것을 목표로 했습니다.
- **20개의 Skeleton point**를 활용해 인체의 움직임을 정량적으로 분석하고 실시간으로 모니터링했습니다.

  https://github.com/user-attachments/assets/39e841f5-ae0b-4983-b5ef-4d11d18dfadb
- **2D** 공간의 **좌표 데이터**를 기반으로 사용자의 자세와 움직임을 추적했습니다.
- 개발한 시스템과 실제 각도기 측정값 간의 **유의성 분석** 결과, 두 측정 방식 사이에 **유의미한 차이가 없음**을 확인했습니다.


담당 인원 : 1명 

---

## 주요 기능 및 기여한 역할

- **비접촉식 각도 측정 알고리즘 개발**: Kinect를 사용하여 환자의 팔 각도를 측정하는 알고리즘을 설계하였습니다.
      
    - 해당 자세한 코드는 [**이곳에서**](https://github.com/morningB/Kinect-Based-System-for-Analyzing-Maximum-Abduction-Angle-of-the-Upper-Limb-Shoulder-Joint/blob/master/KinectSkeleton.cs) 확인할 수 있습니다.
- **데이터 분석 및 유의성 검사**: 수집된 각도 데이터를 분석하고, 통계적 유의성을 검증하여 시스템의 신뢰도를 높였습니다.
    
  
    

---

## 코드 설명

- **각도 계산 코드**
    
    ```csharp
    static private String third(float ankleX, float ankleY, float ankleZ, float shoulderRightX, float shoulderRightY, float shoulderRightZ, float wristRightX, float wristRightY, float wristRightZ)
            {
                double[] vector1 = { ankleX - shoulderRightX, ankleY - shoulderRightY, ankleZ - shoulderRightZ };
                double[] vector2 = { wristRightX - shoulderRightX, wristRightY - shoulderRightY, wristRightZ - shoulderRightZ };
    
                double dotProduct = DotProduct(vector1, vector2);
                double magnitude1 = Magnitude(vector1);
                double magnitude2 = Magnitude(vector2);
    
                double cosTheta = dotProduct / (magnitude1 * magnitude2);
                double angleInRadians = Math.Acos(cosTheta);
                double angleInDegrees = RadiansToDegrees(angleInRadians);
    
                return angleInDegrees.ToString();
            }
    ```
    
- 데이터 처리를 용이하게 하기 위한 **CSV 파일 형태로 저장**
    - [해당 코드](https://github.com/morningB/Kinect-Based-System-for-Analyzing-Maximum-Abduction-Angle-of-the-Upper-Limb-Shoulder-Joint/blob/master/KinectSkeleton.cs)
    
    ```csharp
    static StreamWriter fileWriter = new StreamWriter("average.csv");
    static StreamWriter allFileWriter = new StreamWriter("averageeveryan.csv");
    static StreamWriter pointFileWriter = new StreamWriter("averagepoint.csv");
    .
    .
    .
    // 오른팔 왼팔 ,왼무릎,오른 무릎, 왼다리,오른다리
    fileWriter.WriteLine($"{"Average angle"}, {averageshoulderRightAngle1},{averageshoulderLeftAngle1},{averageKneeLeftAngle1},{averageKneeRightAngle1},{averageLegLeftAngle1},{averageLegRightAngle1}");
    allFileWriter.WriteLine($"{frameCount},{currentRightAngle},{currentLeftAngle},{currentKneeLeftAngle},{currentLegLeft},{currentLegRight}");
    pointFileWriter.WriteLine($"{currentslopeRight},{currentslopeLeft}");
    ```
    

---

## 트러블 슈팅

### 🥵문제 배경

- **문제**
    - 비접촉식 각도 측정 알고리즘 개발 과정에서, 팔의 외전각이 90도를 초과할 때 **잘못된 결과를 출력하는 문제**가 발생했습니다.
    - 이는 환자의 실제 팔 각도를 정확하게 측정해야 하는 시스템의 **핵심적인 신뢰성 문제**였습니다.
- **원인**
    - 두 벡터의 좌표 값으로 기울기를 계산하고 절댓값을 적용하여 내적을 통해 각도를 구하는 기존 공식이 90도 이상의 각도를 측정할 수 없다는 것을 확인했습니다.

### 😁해결 방법

- **해결**
    - 각도 계산 알고리즘을 좌표 간 벡터를 구하고 두 벡터 사이의 각도를  코사인을 활용한 공식으로 개선하여 모든 각도 범위에서 정확한 측정이 가능하도록 했습니다.

---

## 성과

- **학술 발표**
    - 2023년 대한의용생체공학회 포스터 발표
- **학술제 발표**
    - 순천향대학교 학술제 **산학 분야 우수상** 수상
        
      

---

## 사용 기술

- **C#:**  Kinect 데이터 수집 및 분석을 위한 핵심 언어로 사용
- **Kinect SDK:** Skeleton point를 더 쉽게 활용하기 위해 사용
- **Git:** 버전 관리를 위해 사용
- **Excel:** 데이터 분석을 하기 위해 사용

---
