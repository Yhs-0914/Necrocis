# 최종보스 스프라이트 애니메이션 (2026-10-05)

1페이즈에서 기둥을 부수면 기존처럼 부서진 이미지가 남는다. 네 기둥을 모두 부수어 2페이즈로 전환할 때 `HideBrokenRemnant()`로 잔해 렌더러와 신경 연결선을 숨긴다. 기둥 파괴 기록과 열린 바닥 통행은 유지된다.

3페이즈는 기존 뇌와 촉수 디자인을 참고하여 내장 `image_gen` 도구로 생성한 4열 × 6행 스프라이트 시트를 사용한다. 초록 배경을 스킬의 `remove_chroma_key.py`로 제거하고, `Tools/BuildFinalBossAnimations.py`로 24장의 투명 PNG를 분리했다. 첫 생성에서 발견한 공격 궤적의 인접 셀 침범은 두 번째 이미지 편집으로 보정했다.

최종 시트: `Assets/_Project/Art/Generated/FinalBoss/CerebrumAnimations_v1.png`

게임에서 사용하는 프레임: `Assets/_Project/Resources/FinalBoss/PhaseThree/Animations/<Pose>/<Pose>_00.png` ~ `_03.png`

| 동작 | 재생 시점 | 재생 방식 |
| --- | --- | --- |
| Idle | 정지, 공격 후 회복, 분리 연출 | 5fps 반복 |
| Move | 실제 추적 이동 또는 돌진 이동 | 9fps 반복 |
| Melee | 붙잡기/던지기, 촉수 찌르기/부채꼴/회전 공격 | 실제 공격 구간 길이에 맞춘 1회 재생 |
| Cast | 패턴 준비 및 원거리 패턴 시전 | 7fps 반복 |
| Reflection | 실제 반사 방어 활성 구간 | 7fps 반복 |
| Death | 3페이즈에서 보스 사망 | 1.6초 1회, 마지막 프레임 유지 |

프레임은 512×512, Point 필터, PPU 128, 비압축 RGBA, mipmap 없음, 공통 pivot (0.5, 0.08)이다. 모든 프레임에 동일한 확대 비율을 적용하고 발끝 기준 바닥을 맞추어, 사망 시 몸이 바닥으로 내려앉는다. 애니메이터는 보스 이미지 오브젝트에 붙어 있어 전투용 루트를 정리한 후에도 사망 모습이 남는다. 분리 연출에서 이미지 교체 시점에 재생을 시작하고, 근접 공격은 준비/시전 루프가 덮어쓰지 않게 보호한다.

생성 원본과 최종 프롬프트:

- `Exports/FinalBossConcepts/2026-10-05-animations/CerebrumAnimationSource_v2.png`
- `Exports/FinalBossConcepts/2026-10-05-animations/prompt.md`
- `Exports/FinalBossConcepts/2026-10-05-animations/refinement-prompt.md`
- `Exports/FinalBossConcepts/2026-10-05-animations/CerebrumAnimationsPreview.png`

Unity 검증 메뉴:

- `Tools > Necrocis > Final Boss > Validate and Render Sprite Animations`: 프레임 가져오기, 재생 전환, 공격/회복, 반복, 사망 고정, 기존 피격 박스 검사 및 24개 미리보기 렌더링.
- `Tools > Necrocis > Final Boss > Run Sprite Animation Smoke Test`: 별도 테스트 저장소를 사용하여 1→2페이즈 잔해 제거, 40% 체력에서 3페이즈 분리, 추적 이동, 사망 후 마지막 스프라이트 보존을 확인.

검증 결과와 Unity 렌더링은 `Exports/FinalBossConcepts/2026-10-05-animations/Unity/`와 `PlayMode/`에 저장한다.
