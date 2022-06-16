# [2022-1 KHU SWCD] GPU-based interactive cloth simulation  
------------------------
### 개요
> 옷, 유체 등 비 강체 물체를 더 사실적으로 렌더링하고 빠르게 렌더링하는 것은 게임처럼 실시간으로 계산이 되어야하는 분야에서 중요하게 다루어진다. cloth simulation을 통해 더 정확하고 안정적인 시뮬레이션 구현을 목표로 한다.  

------------------------
### 수행과정
- Model : Mass-Spring Model
- Force : 

![image](https://user-images.githubusercontent.com/49023736/174028044-fda86409-ad5a-4820-a6b5-b2de242fd008.png)
#
![image](https://user-images.githubusercontent.com/49023736/174028127-beffa819-0099-4e91-a683-54462332ffff.png)
#
![image](https://user-images.githubusercontent.com/49023736/174027566-8ed46264-6fb7-4136-b5dd-8e33535d414e.png)
#
![image](https://user-images.githubusercontent.com/49023736/174027599-7c596f51-002a-4b6a-8ae3-461b421da1ec.png)


- Time Integration : Euler Method
장점 : 구현이 간단하다
단점 : 업데이트 속도가 느리다
- Time Integration : Verlet Mehod
장점 : 업데이트 속도가 빠르다
단점 : 천이 과도하게 늘어나는 super-elastic effect가 심해, inverse dynamics effect와 같은 추가적인 작업이 필요하다

- Collision : Sphere
천의 정점과 구의 정점의 거리를 비교하거나, ray-cast 방법을 이용하여 구현할 수 있다. box에 비해 그 난이도가 쉬운 편이다.

- Collision : Box
box를 구현하기 위해 [1]에서 소개하는 공식들을 사용한다.

1. vertex-triangle
삼각형내의 무게중심좌표(barycentric coordinates)를 알려준다.

![image](https://user-images.githubusercontent.com/49023736/174027692-3beb62d3-00a8-4191-a86e-a88bbd094d45.png)

2. edge-edge
edge-edge의 교차지점을 알려준다.

![image](https://user-images.githubusercontent.com/49023736/174027771-692d86d6-95c5-46b7-8bc5-8567583a2afd.png)

- GPGPU : Compute Shader를 이용하여 구현한다.

------------------------
### 수행 결과
![녹화_2022_06_16_17_29_26_238](https://user-images.githubusercontent.com/49023736/174027942-b30fbf91-03f6-4324-96a9-3d8f7b1a9a8f.gif)

- 120 해상도(121x121)를 가지고 충돌을 하는 동안에도 안정적으로 100FPS(약 10m/s)를 유지하는 천을 구현했다. gpgpu를 통해 속도를 보장하며 높은 해상도는 더 자연스러운 천의 모습을 보여줄 수 있다.


------------------------
### 참고자료

[1] Robert Bridson. (2002). Robust Treatment of Collisions, Contact and Friction for Cloth Animation. SIGGRAPH, 594–603.
[2] Provot, Xavier. "Deformation constraints in a mass-spring model to describe rigid cloth behaviour." Graphics interface. Canadian Information Processing Society, 1995
[3] R Bigliani. (n.d.). Chapter 9 Collision Detection in Cloth Modeling. Retrieved from https://www.mae.ncsu.edu/eischen/wp-content/uploads/sites/17/2016/08/AKPeters-Chap8.pdf
[4] J Zheng. (2021). AN EXPERIMENTAL FAST APPROACH OF SELF-COLLISION  HANDLING IN CLOTH SIMULATION USING GPU.
[5] Cloth simulation based on simplified mass-spring model
