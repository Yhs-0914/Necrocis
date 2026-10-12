using System;
using System.Collections.Generic;
using System.Linq;
using Necrocis;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NecrocisEditor
{
    [CustomEditor(typeof(MonsterBalanceCatalog))]
    public sealed class MonsterBalanceCatalogEditor : Editor
    {
        private const string StylePath = "Assets/_Project/Editor/MonsterBalance/MonsterBalanceInspector.uss";
        private readonly List<SerializedObject> boundObjects = new List<SerializedObject>();
        private VisualElement root;
        private VisualElement sourcePanel;
        private VisualElement resultPanel;
        private HelpBox validation;
        private DropdownField monsterField;
        private DropdownField statSetField;
        private GameDifficulty previewDifficulty;
        private int monsterIndex;
        private int previewStage;
        private string previewStatSetId = "Default";
        private float patternCoefficient = 1.5f;
        private float rearmSeconds = 3f;
        private MonsterTier boundTier;
        private bool rebuilding;

        private MonsterBalanceCatalog Catalog => (MonsterBalanceCatalog)target;
        private MonsterDefinition Selected => Catalog.monsters != null && monsterIndex >= 0
            && monsterIndex < Catalog.monsters.Count ? Catalog.monsters[monsterIndex] : null;

        public override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement { name = "monster-balance-root" };
            root.AddToClassList("monster-balance");
            StyleSheet sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StylePath);
            if (sheet != null) root.styleSheets.Add(sheet);
            Rebuild();
            return root;
        }

        private void Rebuild()
        {
            if (root == null || rebuilding) return;
            rebuilding = true;
            root.Unbind();
            root.Clear();
            DisposeSources();
            var heading = new Label("몬스터 밸런스 · 바이옴 엘리트");
            heading.AddToClassList("balance-title");
            root.Add(heading);
            root.Add(new HelpBox("몬스터 원본 수정은 다음 생성부터 반영됩니다. 진행도 선택은 미리보기 전용이며, 실전은 바이옴 입장 시 확정됩니다. 바이옴 엘리트는 아래 맵 배치 목록에 등록된 종류만 전용 지점에서 등장합니다. 처치 수 조건은 사용하지 않습니다.", HelpBoxMessageType.Info));

            List<string> names = Catalog.monsters == null ? new List<string>()
                : Catalog.monsters.Select((m, i) => m != null ? m.displayName + " [" + m.tier + "]" : "(빈 참조 " + i + ")").ToList();
            if (names.Count == 0) names.Add("(몬스터 없음)");
            monsterIndex = Mathf.Clamp(monsterIndex, 0, names.Count - 1);
            monsterField = new DropdownField("몬스터", names, monsterIndex) { name = "monster-selector" };
            monsterField.RegisterValueChangedCallback(_ => { monsterIndex = monsterField.index; Rebuild(); });
            root.Add(monsterField);

            var difficulty = new EnumField("미리보기 난이도", previewDifficulty) { name = "difficulty-selector" };
            difficulty.RegisterValueChangedCallback(e => { previewDifficulty = (GameDifficulty)e.newValue; Rebuild(); });
            root.Add(difficulty);

            List<string> sets = Selected?.statSets?.Where(s => s != null).Select(s => s.id).ToList() ?? new List<string>();
            if (sets.Count == 0) sets.Add("(StatSet 없음)");
            int setIndex = Mathf.Max(0, sets.IndexOf(previewStatSetId));
            previewStatSetId = sets[setIndex];
            statSetField = new DropdownField("StatSet / 페이즈", sets, setIndex) { name = "statset-selector" };
            statSetField.RegisterValueChangedCallback(e => { previewStatSetId = e.newValue; RefreshPreview(); });
            root.Add(statSetField);

            var stage = new SliderInt("미리보기 진행도", 0, MonsterProgressionProfile.MaximumStage)
            { value = previewStage, showInputField = true, name = "stage-selector" };
            stage.RegisterValueChangedCallback(e => { previewStage = e.newValue; RefreshPreview(); });
            root.Add(stage);

            var coefficient = new FloatField("테스트 패턴 피해 계수") { value = patternCoefficient, name = "coefficient-preview" };
            coefficient.tooltip = "미리보기 전용 입력이며 패턴 에셋이나 런을 수정하지 않습니다.";
            coefficient.RegisterValueChangedCallback(e => { patternCoefficient = e.newValue; RefreshPreview(); });
            root.Add(coefficient);
            var rearm = new FloatField("테스트 재사용 대기 (초)") { value = rearmSeconds, name = "rearm-preview" };
            rearm.tooltip = "공격 예고/회복 시간은 포함하지 않습니다. 이 입력은 저장되지 않습니다.";
            rearm.RegisterValueChangedCallback(e => { rearmSeconds = e.newValue; RefreshPreview(); });
            root.Add(rearm);

            validation = new HelpBox(string.Empty, HelpBoxMessageType.Error) { name = "balance-validation" };
            root.Add(validation);
            resultPanel = new VisualElement { name = "balance-results" };
            resultPanel.AddToClassList("balance-results");
            root.Add(resultPanel);
            sourcePanel = new VisualElement { name = "balance-source-editors" };
            sourcePanel.SetEnabled(!EditorApplication.isPlayingOrWillChangePlaymode);
            root.Add(sourcePanel);
            boundTier = Selected != null ? Selected.tier : MonsterTier.Elite;

            AddSource(Selected, "1 · 개체 기본값 원본", true,
                ("monsterId", "고정 ID"), ("displayName", "표시 이름"), ("tier", "등급"),
                ("pattern", "공격 패턴 원본"), ("statSets", "기본 스탯 / 페이즈"), ("reward", "처치 보상"), ("contact", "접촉 피해"), ("patternDamage", "패턴 피해 계수 (공격력 × 계수)"), ("designNote", "설계 메모"));
            if (Selected != null && Selected.pattern is GasSacPatternSettings gas)
            {
                AddSource(gas, "1-A · 가스낭 기본 팽창 폭발", true,
                    ("triggerDistance", "공격 시작 거리"), ("burstRadius", "폭발 반경"), ("windupSeconds", "팽창 예고 (초)"),
                    ("burstVisualSeconds", "단발 폭발 표시 (초)"), ("recoverySeconds", "쭈그러진 회복 (초)"),
                    ("rearmSeconds", "회복 후 재사용 대기 (초)"), ("spawnGraceSeconds", "생성 직후 여유 (초)"),
                    ("presentation", "표현 원본"));
                AddSource(gas, "1-B · 압축 가스탄 / 거리 선택", true,
                    ("orbEnabled", "추가 가스탄 사용"), ("orbExitDistance", "근거리 복귀 거리"),
                    ("orbEnterDistance", "원거리 진입 거리"), ("boundaryPriority", "최초 경계 구간 우선순위"),
                    ("orbMaxDistance", "가스탄 최대 시작 거리"), ("orbWindupSeconds", "고정 조준 예고 (초)"),
                    ("orbSpeed", "탄속 (m/초)"), ("orbLifetimeSeconds", "탄 수명 (초)"), ("orbHitRadius", "탄 판정 반경"),
                    ("orbRecoverySeconds", "B 회복 (초)"), ("orbRearmSeconds", "B 회복 후 대기 (초)"));
                AddSource(gas.presentation, "1-V · 가스낭 표현", false,
                    ("directionalPresentation", "A/B·사망 방향별 표현 원본"),
                    ("idle", "기본 모습"), ("inflationFrames", "팽창 프레임"), ("deflated", "회복 모습"),
                    ("deathFrames", "사망 전용 프레임 (가스 배출 → 붕괴)"), ("deathFrameSeconds", "사망 프레임당 시간 (초)"),
                    ("gasPuff", "가스 구름"), ("filledCircle", "그림자·가스 디스크"), ("warningColor", "위험 테두리 색"), ("dangerFillColor", "위험 범위 배경색 / 투명도"),
                    ("burstColor", "폭발 색"), ("gasColor", "가스 색"),
                    ("groundShadowSize", "바닥 그림자 크기"), ("groundShadowColor", "바닥 그림자 색"));
                if (gas.presentation != null && gas.presentation.directionalPresentation != null)
                    sourcePanel.Add(new Button(() => Selection.activeObject = gas.presentation.directionalPresentation)
                    { text = "가스낭 방향 → 동작 → 프레임 편집" });
            }
            if (Selected != null && Selected.pattern is HardenedResiduePatternSettings residue)
            {
                AddSource(residue, "1-A · 굳은 잔여체 / 부스러기 턱", true,
                    ("triggerDistance", "공격 시작 거리"), ("placementDistance", "앞쪽 배치 거리"), ("footprint", "착지·차단 기준 길이 / 폭 (m)"),
                    ("windupSeconds", "조각 들기·균열 예고 (초)"), ("dropSeconds", "내려놓기 (초)"), ("recoverySeconds", "본체 회복 (초)"),
                    ("rearmSeconds", "회복 후 재사용 대기 (초)"), ("spawnGraceSeconds", "생성 직후 여유 (초)"),
                    ("rubbleLifetime", "부스러기 유지 (초)"), ("hitsToBreak", "파괴에 필요한 유효 타격 수"), ("presentation", "표현 원본"));
                AddSource(residue.presentation, "1-V · 굳은 잔여체 표현", false,
                    ("directionalPresentation", "방향별 동작·조각 출발점 원본"), ("airborneChip", "분리 후 비행용 조각"),
                    ("idleFrames", "대기 프레임"), ("moveFrames", "이동 프레임"), ("liftFrames", "조각 들기 프레임"),
                    ("release", "내려놓기"), ("recovery", "회복"), ("hit", "피격"), ("rubble", "좌우 착지 잔해"), ("verticalRubble", "상하 착지 잔해 (선택안)"),
                    ("verticalRubbleGroundDepthFraction", "세로 잔해 바닥 길이 비율 (표현 전용)"),
                    ("deathFrames", "전용 사망 프레임"), ("deathFrameSeconds", "사망 프레임당 시간 (초)"),
                    ("idleFrameSeconds", "대기 프레임당 시간 (초)"), ("moveFrameSeconds", "이동 프레임당 시간 (초)"),
                    ("crackColor", "착지 균열·테두리 색"), ("dangerFillColor", "위험 범위 배경색 / 투명도"),
                    ("groundDisc", "그림자 디스크"), ("groundShadowSize", "그림자 크기"), ("groundShadowColor", "그림자 색"));
                if (residue.presentation != null && residue.presentation.directionalPresentation != null)
                    sourcePanel.Add(new Button(() => Selection.activeObject = residue.presentation.directionalPresentation)
                    { text = "굳은 잔여체 방향 → 동작 → 조각 출발점 편집" });
            }
            if (Selected != null && Selected.pattern is InflammationEmberPatternSettings ember)
            {
                AddSource(ember, "1-A · 염증 불씨 / 피격 반격", true,
                    ("counterMaxDistance", "반격 최대 거리"), ("approachStopDistance", "접근 정지 거리"),
                    ("windupSeconds", "핵 점등·가시 준비 (초)"), ("recoverySeconds", "발사 후 회복 (초)"),
                    ("rearmSeconds", "회복 후 재사용 대기 (초)"), ("thornSpeed", "가시탄 속도 (m/초)"),
                    ("thornLifetimeSeconds", "가시탄 수명 (초)"), ("thornLength", "가시탄 전체 길이"),
                    ("thornRadius", "가시탄 반두께 / 판정 반경"), ("presentation", "표현 원본"));
                AddSource(ember.presentation, "1-V · 염증 불씨 표현", false,
                    ("directionalPresentation", "방향별 동작·사망·발사 기준점 원본"),
                    ("idleFrames", "대기 프레임"), ("moveFrames", "이동 프레임"), ("preparationFrames", "핵 점등·가시 준비 프레임"),
                    ("release", "발사"), ("recovery", "회복"), ("hit", "피격"), ("deathFrames", "전용 사망 프레임"),
                    ("thornFrames", "가시탄 프레임"), ("idleFrameSeconds", "대기 프레임당 시간 (초)"),
                    ("moveFrameSeconds", "이동 프레임당 시간 (초)"), ("releaseFrameSeconds", "회복 안의 발사 포즈 시간 (초)"),
                    ("deathFrameSeconds", "사망 프레임당 시간 (초)"), ("thornFrameSeconds", "가시탄 프레임당 시간 (초)"),
                    ("groundDisc", "본체 그림자 디스크"), ("groundShadowSize", "본체 그림자 크기"), ("groundShadowColor", "본체 그림자 색"));
                if (ember.presentation != null && ember.presentation.directionalPresentation != null)
                {
                    sourcePanel.Add(new Button(() => Selection.activeObject = ember.presentation.directionalPresentation)
                    { text = "염증 불씨 방향 → 동작 → 발사 기준점 편집" });
                    sourcePanel.Add(new Label("발사 기준점은 Visual 로컬 좌표를 지면 XZ로 적용합니다. 비행 높이는 가시탄 반두께를 따르며, 변경은 다음 생성부터 반영됩니다."));
                }
            }
            if (Selected != null && Selected.pattern is HangoverRemnantPatternSettings hangover)
            {
                AddSource(hangover, "1-A · 숙취 잔재 / 분리·후퇴·지연 폭발", true,
                    ("triggerDistance", "공격 시작 거리"), ("spawnGraceSeconds", "생성 직후 여유 (초)"),
                    ("windupSeconds", "고정 위치 준비·예고 (초)"), ("retreatDistance", "후퇴 거리 (m)"),
                    ("retreatSpeed", "후퇴 속도 (m/초, 기본 이동 배율 중복 없음)"),
                    ("burstDelaySeconds", "잔여물 생성 후 폭발 지연 (초)"), ("burstRadius", "빨간 범위·피해 공통 반경 (m)"),
                    ("recoverySeconds", "후퇴 후 회복 (초)"), ("rearmSeconds", "회복·잔여물 정리 후 재사용 대기 (초)"), ("presentation", "표현 원본"));
                AddSource(hangover.presentation, "1-V · 숙취 잔재 / 정면 고정 표현", false,
                    ("idleFrames", "정면 대기"), ("moveFrames", "정면 이동"), ("preparationFrames", "분리 준비"),
                    ("release", "분리"), ("retreatFrames", "정면 유지 후퇴"), ("recovery", "회복"), ("hit", "피격"),
                    ("deathFrames", "정면 전용 사망 6장"), ("remnantFrames", "지면 잔여물 2장"), ("burstFrames", "터짐 4장"),
                    ("idleFrameSeconds", "대기 프레임 시간"), ("moveFrameSeconds", "이동 프레임 시간"),
                    ("releaseFrameSeconds", "후퇴 안의 분리 포즈 시간"), ("retreatFrameSeconds", "후퇴 프레임 시간"),
                    ("deathFrameSeconds", "사망 프레임 시간"), ("remnantFrameSeconds", "잔여물 맥동 시간"), ("burstFrameSeconds", "터짐 프레임 시간"),
                    ("dangerFillColor", "빨간 위험 범위 색·투명도"), ("groundDisc", "그림자 디스크"),
                    ("groundShadowSize", "그림자 크기"), ("groundShadowColor", "그림자 색"));
                sourcePanel.Add(new Label("본체와 사망은 정면 고정입니다. 실제 이동·후퇴 방향과 분리 위치는 패턴이 소유합니다. 설정 변경은 다음 생성부터 반영됩니다."));
            }
            if (Selected != null && Selected.pattern is DustClumpPatternSettings dust)
            {
                AddSource(dust, "1-A · 분진 뭉치 / 고정 핵·이동 구름", true,
                    ("triggerDistance", "발동 거리"), ("spawnGraceSeconds", "생성 직후 여유 (초)"),
                    ("windupSeconds", "응집·빨간 원 예고 (초)"), ("launchOffset", "핵에서 구름 출발 거리 (m)"),
                    ("cloudRadius", "빨간 원·피해 공통 반경 (m)"), ("cloudSpeed", "구름 속도 (m/초)"),
                    ("cloudLifetimeSeconds", "구름 이동 수명 (초)"), ("recoverySeconds", "구름 정리 후 회복 (초)"),
                    ("rearmSeconds", "회복 후 재사용 대기 (초)"), ("presentation", "표현 원본"));
                AddSource(dust.presentation, "1-V · 분진 뭉치 / 방향 공통·상태별 사망", false,
                    ("idleFrames", "대기"), ("moveFrames", "이동"), ("preparationFrames", "응집 시작"),
                    ("release", "분리"), ("coreFrames", "핵 노출"), ("recovery", "재형성"), ("hit", "덩어리 피격"),
                    ("deathFrames", "덩어리 사망 6장"), ("coreDeathFrames", "핵 사망 6장"),
                    ("cloudFrames", "구름 이동 4장"), ("dissolveFrames", "구름 소멸 2장"),
                    ("idleFrameSeconds", "대기 프레임 시간"), ("moveFrameSeconds", "이동 프레임 시간"),
                    ("coreFrameSeconds", "핵 맥동 시간"), ("cloudFrameSeconds", "구름 프레임 시간"),
                    ("dissolveFrameSeconds", "소멸 프레임 시간"), ("deathFrameSeconds", "사망 프레임 시간"),
                    ("cloudOpacity", "구름 불투명도"), ("dangerFillColor", "빨간 전체 범위 색"),
                    ("playerOverlapOpacity", "플레이어와 겹칠 때 불투명도 (표현만)"),
                    ("coreColliderSize", "노출 핵 충돌 크기"), ("coreColliderCenter", "노출 핵 충돌 중심"),
                    ("groundDisc", "그림자 디스크"), ("groundShadowSize", "덩어리 그림자"), ("coreShadowSize", "핵 그림자"),
                    ("groundShadowColor", "그림자 색"));
                sourcePanel.Add(new Label("응집부터 구름 정리까지 핵의 위치를 고정합니다. 핵은 계속 피격 가능하고 구름은 한 번만 피해를 판정합니다. 변경은 다음 생성부터 반영됩니다."));
            }
            if (Selected != null && Selected.pattern is PollenInvaderPatternSettings pollen)
            {
                AddSource(pollen, "1-A · 꽃가루 침입자 / 기본 3갈래 발사", true,
                    ("triggerDistance", "발동 거리"), ("approachStopDistance", "접근 정지 거리"),
                    ("spawnGraceSeconds", "생성 직후 여유 (초)"), ("windupSeconds", "껍질 벌림 예고 (초)"),
                    ("spreadHalfAngle", "중앙에서 좌우 탄 각도 (도)"), ("pelletSpeed", "탄속 (m/초)"),
                    ("pelletLifetimeSeconds", "탄 수명 (초)"), ("pelletRadius", "탄 크기·판정·비행 높이 공통 반경"),
                    ("recoverySeconds", "A 단발 회복 (초)"), ("rearmSeconds", "A 회복 후 재사용 대기 (초)"), ("presentation", "표현 원본"));
                AddSource(pollen, "1-B · 꽃가루 / 회전 예고·두 번째 탄막", true,
                    ("followup.enabled", "B 추가 탄막 사용 (다음 생성부터)"),
                    ("followup.angleOffset", "회전 각도 크기 · 좌우 대칭 (도)"), ("followup.rotationSeconds", "두 발사 사이 전체 회전 예고 (초)"),
                    ("followup.recoverySeconds", "B 연속 공격 최종 회복 (초)"), ("followup.rearmSeconds", "B 회복 후 재사용 대기 (초)"));
                AddSource(pollen.presentation, "1-V · 꽃가루 / 본체·사망·공용 탄", false,
                    ("directionalPresentation", "방향별 본체·사망·개방부 기준점"),
                    ("idleFrames", "대기"), ("moveFrames", "이동"), ("preparationFrames", "껍질 벌림"),
                    ("release", "발사"), ("recovery", "닫힘"), ("hit", "피격"), ("deathFrames", "사망 6장"), ("pelletFrames", "공용 꽃가루탄 2장"),
                    ("rotationFrames", "B 회전 예고 3장 · 방향 원본 키"),
                    ("idleFrameSeconds", "대기 프레임 시간"), ("moveFrameSeconds", "이동 프레임 시간"), ("releaseFrameSeconds", "발사 프레임 시간"),
                    ("deathFrameSeconds", "사망 프레임 시간"), ("pelletFrameSeconds", "탄 프레임 시간"),
                    ("groundDisc", "본체 그림자"), ("groundShadowSize", "본체 그림자 크기"), ("groundShadowColor", "본체 그림자 색"));
                if (pollen.presentation != null && pollen.presentation.directionalPresentation != null)
                    sourcePanel.Add(new Button(() => Selection.activeObject = pollen.presentation.directionalPresentation) { text = "꽃가루 방향·동작·개방부 편집" });
                sourcePanel.Add(new Label("B는 첫 3발 → 회전 예고 → 각도를 바꾼 3발입니다. 두 발사는 같은 사이클의 피해 기회 1회를 공유합니다. 두 번째 피해 계수는 기본값의 pollen-followup 원본을 편집합니다. A/B 중 선택한 회복·대기 한 벌만 적용합니다."));
            }
            if (Selected != null && Selected.pattern is HelicoSpiralPatternSettings helico)
            {
                AddSource(helico, "1-A · 헬리코 나선충 / 방향 고정 직선 돌진", true,
                    ("triggerDistance", "발동 거리"), ("approachStopDistance", "접근 정지 거리"), ("spawnGraceSeconds", "생성 직후 여유 (초)"),
                    ("windupSeconds", "몸 비틀기 예고 (초)"), ("dashDistance", "몸 중심 이동 거리 (m)"), ("dashSeconds", "전체 거리 돌진 시간 (초)"),
                    ("bodyWidth", "몸통 폭 · 예고/피해/접촉 공통"), ("bodyHalfLength", "몸통 중심선 반길이 · 편모 제외"),
                    ("recoverySeconds", "회복 (초)"), ("rearmSeconds", "회복 후 대기 (초)"), ("presentation", "표현 원본"));
                AddSource(helico.presentation, "1-V · 나선충 / 8방향 본체·사망", false,
                    ("directionalPresentation", "방향별 프레임·머리 기준점"), ("idleFrames", "대기"), ("moveFrames", "이동"),
                    ("preparationFrames", "비틀기"), ("dashFrames", "돌진"), ("recoveryFrames", "회복"), ("hit", "피격"), ("deathFrames", "사망 6장"),
                    ("idleFrameSeconds", "대기 프레임 시간"), ("moveFrameSeconds", "이동 프레임 시간"), ("deathFrameSeconds", "사망 프레임 시간"), ("dangerColor", "빨간 경로 채움"), ("dangerBorderColor", "위 바닥 대비 경계선"));
                if (helico.presentation != null && helico.presentation.directionalPresentation != null)
                    sourcePanel.Add(new Button(() => Selection.activeObject = helico.presentation.directionalPresentation) { text = "나선충 8방향·동작·머리 기준점 편집" });
                sourcePanel.Add(new Label("돌진 속도는 거리/시간입니다. 빨간 경로는 양 끝 반원을 포함한 몸통의 전체 이동 영역이며, 편모는 판정에서 제외합니다. 피해 계수는 기본값 helico-dash에서 조절합니다. 돌진 후 기본 회복으로 이어지며 꼬리 휩쓸기는 제외되었습니다."));
            }
            if (Selected != null && Selected.pattern is OilFilmPatternSettings oil)
            {
                AddSource(oil, "1-A · 기름막 / 전방 전체 광역 공격", true,
                    ("triggerDistance", "발동 거리"), ("approachStopDistance", "접근 정지 거리"), ("spawnGraceSeconds", "생성 직후 여유 (초)"),
                    ("windupSeconds", "압축·전체 범위 예고 (초)"), ("attackRange", "부채꼴 사거리 · 예고/그림/피해 공통"), ("arcDegrees", "부채꼴 전체 각도"),
                    ("releaseSeconds", "펼침 (초)"), ("impactNormalizedTime", "펼침 중 단발 타격 시점 (0~1)"),
                    ("recoverySeconds", "회복 (초)"), ("rearmSeconds", "회복 후 대기 (초)"), ("coreRadius", "핵 접촉 반경 · 막 제외"), ("presentation", "표현 원본"));
                AddSource(oil.presentation, "1-V · 기름막 / 4방향·동작·사망", false,
                    ("directionalPresentation", "방향별 원본"), ("idleFrames", "대기"), ("moveFrames", "이동"), ("preparationFrames", "압축4장"),
                    ("releaseFrames", "펼침4장"), ("recoveryFrames", "회복4장"), ("hitFrames", "피격2장"), ("deathFrames", "사망6장"),
                    ("idleFrameSeconds", "대기 프레임 시간"), ("moveFrameSeconds", "이동 프레임 시간"), ("hitFrameSeconds", "피격 프레임 시간"),
                    ("deathFrameSeconds", "사망 프레임 시간"), ("dangerColor", "빨간 전체 범위 채움"), ("dangerBorderColor", "범위 경계선"));
                sourcePanel.Add(new Label("전방 부채꼴 전체를 예고하고 펼침 순간 한 번 타격합니다. 안쪽 안전 거리·테두리 전용 판정은 없습니다. 핵 접촉 반경과 플레이어가 때리는 몸체는 별개입니다. 피해 계수는 기본값 oil-film-spread에서 편집합니다."));
            }
            DifficultyBalanceProfile profile = GetProfile();
            string tierProperty = boundTier == MonsterTier.Normal ? "enemies" : boundTier == MonsterTier.Boss ? "bosses" : "elites";
            AddSource(profile, "2 · " + previewDifficulty + " / " + boundTier + " 배율 원본", false,
                (tierProperty, "선택 등급 배율 (한 번만 적용)"));
            AddSource(Catalog.progression, "3 · 몬스터 진행도 원본", false, ("stages", "단계별 배율 (누적 곱 아님)"));

            if (Catalog.biomeEliteSpawns != null)
                foreach (BiomeEliteSpawnConfig spawn in Catalog.biomeEliteSpawns)
                    if (spawn != null) AddSource(spawn, "4 · " + spawn.name + " / 바이옴 엘리트 맵 배치", false,
                        ("enabled", "배치 사용"), ("monsters", "구현된 바이옴 엘리트 목록"),
                        ("minimumCount", "맵 전체 최소 수"), ("maximumCount", "맵 전체 최대 수"),
                        ("minimumPerType", "종류별 최소 수"), ("minimumSpacing", "개체 간격 (칸)"),
                        ("entranceExclusionRadius", "입구 제외 반경 (칸)"), ("portalExclusionRadius", "포털 제외 반경 (칸)"), ("bossExclusionPadding", "보스방 여유 (칸)"),
                        ("clearanceCells", "보행 여유 (칸)"), ("activationDistance", "활성화 거리"),
                        ("releaseDistance", "해제 거리"), ("leashDistance", "복귀 범위"));

            var refs = new Foldout { text = "카탈로그 참조 관리", value = false };
            foreach (string name in new[] { "difficultyCatalog", "progression", "monsters", "biomeEliteSpawns" })
                refs.Add(new PropertyField(serializedObject.FindProperty(name)));
            refs.Bind(serializedObject);
            refs.TrackSerializedObjectValue(serializedObject, _ => root.schedule.Execute(Rebuild));
            sourcePanel.Add(refs);
            rebuilding = false;
            RefreshPreview();
        }

        private DifficultyBalanceProfile GetProfile()
        {
            if (Catalog.difficultyCatalog == null) return null;
            return Catalog.difficultyCatalog.Get(previewDifficulty);
        }

        private void AddSource(UnityEngine.Object source, string title, bool expanded, params (string field, string label)[] fields)
        {
            if (source == null) return;
            var foldout = new Foldout { text = title, value = expanded };
            foldout.AddToClassList("balance-source");
            var path = new Label(AssetDatabase.GetAssetPath(source));
            path.AddToClassList("balance-path");
            foldout.Add(path);
            foldout.Add(new Button(() => EditorGUIUtility.PingObject(source)) { text = "원본 에셋 위치" });
            var data = new SerializedObject(source);
            boundObjects.Add(data);
            foreach (var entry in fields)
            {
                SerializedProperty property = data.FindProperty(entry.field);
                if (property != null) foldout.Add(new PropertyField(property, entry.label));
            }
            foldout.Bind(data);
            foldout.TrackSerializedObjectValue(data, _ =>
            {
                if (Selected != null && Selected.tier != boundTier) root.schedule.Execute(Rebuild);
                else RefreshPreview();
            });
            sourcePanel.Add(foldout);
        }

        private void RefreshPreview()
        {
            if (rebuilding || resultPanel == null) return;
            RefreshDefinitionChoices();
            resultPanel.Clear();
            List<string> errors = Catalog.GetValidationErrors();
            if (errors.Count > 0) { ShowError(string.Join("\n", errors)); return; }
            MonsterDefinition definition = Selected;
            string id = statSetField.value;
            if (!MonsterBalanceResolver.TryResolve(definition, id, GetProfile(), Catalog.progression,
                    previewStage, out MonsterBalanceSnapshot snapshot, out string error))
            { ShowError(error); return; }
            if (float.IsNaN(patternCoefficient) || float.IsInfinity(patternCoefficient) || patternCoefficient < 0f
                || float.IsNaN(rearmSeconds) || float.IsInfinity(rearmSeconds) || rearmSeconds < 0f)
            { ShowError("테스트 계수와 대기시간은 유한한 0 이상 값이어야 합니다."); return; }
            float damage, cooldown;
            try { damage = snapshot.GetPatternDamage(patternCoefficient); cooldown = snapshot.GetRearmCooldown(rearmSeconds); }
            catch (ArgumentOutOfRangeException) { ShowError("테스트 계산 결과가 범위를 초과했습니다."); return; }
            validation.AddToClassList("balance-hidden");
            definition.TryGetStatSet(id, out MonsterStatSet basis);
            Catalog.progression.TryGetStage(previewStage, out MonsterProgressionStage progression);
            EnemyDifficultyBalance difficulty = MonsterBalanceResolver.GetTierBalance(GetProfile(), definition.tier);
            Result("최종값 · 읽기 전용", "아래 결과는 원본 에셋에 역저장되지 않습니다.");
            Result("최대 체력", $"{basis.maxHealth:0.###} × {difficulty.maxHealth:0.###} × {progression.maxHealth:0.###} = {snapshot.MaxHealth:0.###}");
            Result("공격력", $"{basis.attackPower:0.###} × {difficulty.outgoingDamage:0.###} × {progression.attackPower:0.###} = {snapshot.AttackPower:0.###}");
            Result("처치 경험치", $"Round({definition.reward.baseExperience} × {difficulty.experienceReward:0.###} × {progression.experience:0.###}) = {snapshot.Experience}");
            Result("이동속도", $"{basis.moveSpeed:0.###} × {difficulty.moveSpeed:0.###} = {snapshot.MoveSpeed:0.###} (진행도 미적용)");
            Result("테스트 패턴 피해", $"{snapshot.AttackPower:0.###} × {patternCoefficient:0.###} = {damage:0.###}");
            Result("접촉 피해", definition.contact.enabled ? $"{snapshot.AttackPower:0.###} × {definition.contact.damageCoefficient:0.###} = {snapshot.ContactDamage:0.###}" : "사용 안 함 = 0");
            Result("재사용 대기", $"{rearmSeconds:0.###} × {snapshot.CooldownMultiplier:0.###} = {cooldown:0.###}초 (예고/회복/진행도 미적용)");
            Result("게임 체력 단위", $"HP {snapshot.HealthUnits:0} / 테스트 패턴 1회 피해 {CharacterStats.ToHealthDeltaUnits(damage):0} / 접촉 {CharacterStats.ToHealthDeltaUnits(snapshot.ContactDamage):0}");
            Result("체력 반올림", "기존 게임 규칙: HP·피해는 정수, 양수 피해 최소 1. 플레이어 피격 효과/무적 전 기준이며 경험치 반올림과 별개입니다.");
            foreach (MonsterPatternDamage pattern in definition.patternDamage)
            {
                float patternDamage = snapshot.GetPatternDamage(pattern.coefficient);
                Result("패턴 · " + pattern.id, $"{snapshot.AttackPower:0.###} × {pattern.coefficient:0.###} = {patternDamage:0.###} (1회 HP 피해 {CharacterStats.ToHealthDeltaUnits(patternDamage):0})");
            }
        }

        private void Result(string label, string value)
        {
            var row = new VisualElement();
            row.AddToClassList("balance-result-row");
            var key = new Label(label);
            key.AddToClassList("balance-result-key");
            var text = new Label(value);
            text.AddToClassList("balance-result-value");
            row.Add(key);
            row.Add(text);
            resultPanel.Add(row);
        }

        private void RefreshDefinitionChoices()
        {
            if (Selected == null) return;
            List<string> names = Catalog.monsters.Select((m, i) => m != null
                ? m.displayName + " [" + m.tier + "]" : "(빈 참조 " + i + ")").ToList();
            if (!monsterField.choices.SequenceEqual(names))
            {
                monsterField.choices = names;
                monsterField.SetValueWithoutNotify(names[monsterIndex]);
            }
            List<string> sets = Selected.statSets?.Where(s => s != null).Select(s => s.id).ToList() ?? new List<string>();
            if (sets.Count == 0) sets.Add("(StatSet 없음)");
            if (!statSetField.choices.SequenceEqual(sets))
            {
                int index = Mathf.Max(0, sets.IndexOf(previewStatSetId));
                statSetField.choices = sets;
                previewStatSetId = sets[index];
                statSetField.SetValueWithoutNotify(previewStatSetId);
            }
        }

        private void ShowError(string error)
        {
            validation.text = error;
            validation.RemoveFromClassList("balance-hidden");
        }

        private void DisposeSources()
        {
            foreach (SerializedObject data in boundObjects) data.Dispose();
            boundObjects.Clear();
        }

        private void OnDisable()
        {
            root?.Unbind();
            root = null;
            resultPanel = null;
            DisposeSources();
        }
    }
}
