using System.Collections.Generic;
using System.Linq;
using Necrocis;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace NecrocisEditor
{
    [CustomEditor(typeof(EnemyDirectionalPresentation))]
    public sealed class EnemyDirectionalPresentationEditor : Editor
    {
        private VisualElement root, framePanel;
        private HelpBox validation;
        private DropdownField direction, motion;
        private readonly Dictionary<Image, int> previews = new Dictionary<Image, int>();
        private EnemyDirectionalPresentation Data => (EnemyDirectionalPresentation)target;
        private static readonly string[] DirectionFields = { "front", "back", "source", "frontDiagonal", "backDiagonal" };
        private static readonly string[] OriginFields = { "frontOrigin", "backOrigin", "sideOrigin", "frontDiagonalOrigin", "backDiagonalOrigin" };
        private static readonly string[] MotionLabels = { "대기", "공격 준비", "회복", "피격", "사망", "이동", "발사 · 내려놓기" };

        public override VisualElement CreateInspectorGUI()
        {
            root = new VisualElement { name = "directional-presentation-editor" };
            root.AddToClassList("monster-balance");
            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/_Project/Editor/MonsterBalance/MonsterBalanceInspector.uss");
            if (style != null) root.styleSheets.Add(style);
            var title = new Label("방향별 몬스터 표현"); title.AddToClassList("balance-title"); root.Add(title);
            root.Add(new HelpBox("방향 → 동작 순서로 편집합니다. 측면은 기존 표현 원본을 찾는 키이며 여기서 복제 편집하지 않습니다. 변경은 새 생성/명시적 재연결 때 반영됩니다. 굳은 잔여체의 시각 기준점은 분리된 조각의 아래쪽 피벗입니다. 가스탄 지면 비행 높이는 별도 판정 반경을 따릅니다.", HelpBoxMessageType.Info));
            root.Add(new PropertyField(serializedObject.FindProperty("mode"), "방향 수"));
            root.Add(new PropertyField(serializedObject.FindProperty("authoredFacingLeft"), "측면·사선 원본은 왼쪽 방향"));
            root.Add(new PropertyField(serializedObject.FindProperty("boundaryHysteresisDegrees"), "방향 경계 유지 각도"));
            direction = new DropdownField("방향", new List<string> { "정면 · 화면 아래", "후면 · 화면 위", "측면 · 원본 키", "앞 사선 · 8방향용", "뒤 사선 · 8방향용" }, 0);
            motion = new DropdownField("동작", MotionLabels.ToList(), 0);
            direction.RegisterValueChangedCallback(_ => RebuildFrames()); motion.RegisterValueChangedCallback(_ => RebuildFrames());
            root.Add(direction); root.Add(motion);
            validation = new HelpBox("", HelpBoxMessageType.Info); root.Add(validation);
            framePanel = new VisualElement(); root.Add(framePanel);
            if (AssetDatabase.GetAssetPath(Data) == GasSacDirectionalImport.AssetPath)
            {
                var preview = new Button(() => DirectionalPresentationRunner.Preview()) { text = "C-04 가스낭 Game 미리보기 (Play Mode)" };
                preview.SetEnabled(!EditorApplication.isPlayingOrWillChangePlaymode);
                root.Add(preview);
                root.schedule.Execute(() => preview.SetEnabled(!EditorApplication.isPlayingOrWillChangePlaymode)).Every(250);
            }
            root.Bind(serializedObject);
            root.TrackSerializedObjectValue(serializedObject, _ => RefreshPreview());
            RebuildFrames();
            return root;
        }

        private void RebuildFrames()
        {
            framePanel.Unbind(); framePanel.Clear(); previews.Clear(); serializedObject.Update();
            framePanel.Add(new PropertyField(serializedObject.FindProperty(OriginFields[direction.index]), "시각적 생성 기준점 (Visual 로컬)"));
            if (direction.index >= 3 && Data.mode == EnemyDirectionMode.Four)
                framePanel.Add(new HelpBox("현재는 4방향입니다. 8방향으로 전환하려면 모든 사선 프레임을 채워야 합니다.", HelpBoxMessageType.Info));
            SerializedProperty array = serializedObject.FindProperty("frames");
            for (int i = 0; i < array.arraySize; i++)
            {
                SerializedProperty row = array.GetArrayElementAtIndex(i);
                if (row.FindPropertyRelative("motion").enumValueIndex != motion.index) continue;
                var box = new VisualElement(); box.AddToClassList("direction-frame");
                box.Add(new Label(row.FindPropertyRelative("label").stringValue));
                var source = new PropertyField(row.FindPropertyRelative("source"), "측면 원본 키"); source.SetEnabled(false); box.Add(source);
                if (direction.index != 2) box.Add(new PropertyField(row.FindPropertyRelative(DirectionFields[direction.index]), "이 방향 프레임"));
                var image = new Image { scaleMode = ScaleMode.ScaleToFit }; image.AddToClassList("direction-preview");
                box.Add(image); previews.Add(image, i); framePanel.Add(box);
            }
            if (previews.Count == 0) framePanel.Add(new HelpBox("이 종에는 이 동작의 별도 프레임이 없습니다.", HelpBoxMessageType.Info));
            framePanel.Bind(serializedObject); RefreshPreview();
        }

        private void RefreshPreview()
        {
            string error = Data.GetValidationError();
            validation.text = error ?? "필수 방향·사망 프레임과 원본 키 검증 통과";
            validation.messageType = error == null ? HelpBoxMessageType.Info : HelpBoxMessageType.Error;
            SerializedProperty array = serializedObject.FindProperty("frames");
            foreach (var entry in previews)
                if (entry.Value < array.arraySize)
                    entry.Key.sprite = array.GetArrayElementAtIndex(entry.Value).FindPropertyRelative(DirectionFields[direction.index]).objectReferenceValue as Sprite;
        }
    }
}
