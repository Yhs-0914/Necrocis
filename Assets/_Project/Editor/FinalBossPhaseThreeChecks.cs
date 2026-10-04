using System;
using System.Reflection;
using Necrocis;
using UnityEditor;
using UnityEngine;

namespace NecrocisEditor
{
    /// <summary>Non-mutating geometry and imported-art checks; does not enter Play Mode or touch saves.</summary>
    public static class FinalBossPhaseThreeChecks
    {
        [MenuItem("Tools/Necrocis/Final Boss/Validate Phase Three Assets and Geometry")]
        public static void Validate()
        {
            string[] names = { "MobileCerebrum", "NeuralLance", "NeuralRing", "CellBomb", "HomingNeuralCell" };
            foreach (string name in names)
            {
                string path = "Assets/_Project/Resources/FinalBoss/PhaseThree/" + name + ".png";
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                Require(sprite != null && sprite.bounds.size.x > 0f, "Missing imported sprite: " + name);
                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                Require(importer != null && importer.textureType == TextureImporterType.Sprite
                    && importer.alphaIsTransparency && !importer.mipmapEnabled, "Incorrect sprite settings: " + name);
            }
            Rect safe = new Rect(10f, 10f, 20f, 20f);
            Vector2 extents = new Vector2(.7f, .5f);
            Require(FinalBossPhaseThreeController.CanTraverseSafeArea(safe, new Vector3(20f, 0f, 20f),
                new Vector3(21f, 0f, 20f), extents), "Interior move rejected");
            Require(!FinalBossPhaseThreeController.CanTraverseSafeArea(safe, new Vector3(29f, 0f, 20f),
                new Vector3(30f, 0f, 20f), extents), "Footprint crossed closing wall");
            Require(FinalBossPhaseThreeController.CanTraverseSafeArea(safe, new Vector3(32f, 0f, 20f),
                new Vector3(31f, 0f, 20f), extents), "Overtaken player cannot escape inward");
            Require(!FinalBossPhaseThreeController.CanTraverseSafeArea(safe, new Vector3(32f, 0f, 20f),
                new Vector3(33f, 0f, 20f), extents), "Overtaken player can move outward");
            MethodInfo lane = typeof(FinalBossPhaseThreeController).GetMethod("InLane", BindingFlags.NonPublic | BindingFlags.Static);
            Require((bool)lane.Invoke(null, new object[] { new Vector3(4f, 9f, .9f), Vector3.zero, Vector3.right, 9f, 1.2f }), "Lane ignored planar hit");
            Require(!(bool)lane.Invoke(null, new object[] { new Vector3(4f, 0f, 2f), Vector3.zero, Vector3.right, 9f, 1.2f }), "Lane hit outside warning");
            Require(!(bool)lane.Invoke(null, new object[] { Vector3.left, Vector3.zero, Vector3.right, 9f, 1.2f }), "Lane hit behind boss");
            MethodInfo segment = typeof(FinalBossPhaseThreeController).GetMethod("SegmentDistance", BindingFlags.NonPublic | BindingFlags.Static);
            float distance = (float)segment.Invoke(null, new object[] { new Vector3(5f, 4f, 1f), Vector3.zero, Vector3.right * 10f });
            Require(Mathf.Abs(distance - 1f) < .0001f, "Dash swept collision missed crossing");
            MethodInfo pursuit = typeof(FinalBossPhaseThreeController).GetMethod("CalculatePursuitStep", BindingFlags.NonPublic | BindingFlags.Static);
            Vector3 step = (Vector3)pursuit.Invoke(null, new object[] { Vector3.zero, Vector3.right * 4f, 3.1f, 3.2f, 1f });
            Require(Mathf.Abs(step.x - .8f) < .0001f, "Pursuit overshot stopping distance");
            ValidateMobileHitboxes();
            Debug.Log("[FinalBoss] PASS: 5 phase-three sprites, 9 geometry checks and mobile-hitbox creation/reuse/recreation. Play-mode choreography still needs visual QA.");
        }

        [MenuItem("Tools/Necrocis/Final Boss/Validate Phase Three Hitboxes")]
        public static void ValidateMobileHitboxes()
        {
            // Inactive, temporary objects keep scene gameplay and player saves untouched.
            GameObject fixture = new GameObject("PhaseThreeHitboxRegression");
            fixture.SetActive(false);
            try
            {
                FinalBossPhaseThreeController controller = fixture.AddComponent<FinalBossPhaseThreeController>();
                GameObject targetObject = new GameObject("DamageTarget");
                targetObject.transform.SetParent(fixture.transform, false);
                targetObject.layer = 8;
                EnemyController target = targetObject.AddComponent<EnemyController>();
                BoxCollider core = targetObject.AddComponent<BoxCollider>();
                core.isTrigger = true;
                GameObject visualObject = new GameObject("Artwork", typeof(SpriteRenderer));
                visualObject.transform.SetParent(fixture.transform, false);
                SpriteRenderer visual = visualObject.GetComponent<SpriteRenderer>();
                visual.sprite = Resources.Load<Sprite>("FinalBoss/PhaseThree/MobileCerebrum");
                Require(visual.sprite != null, "Missing mobile artwork for hitbox checks");
                visual.transform.SetPositionAndRotation(new Vector3(3f, .35f, 7f), Quaternion.Euler(45f, 0f, 7f));
                visual.transform.localScale = new Vector3(1.3f, .9f, 1f);
                const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
                typeof(FinalBossPhaseThreeController).GetField("damageTarget", flags).SetValue(controller, target);
                typeof(FinalBossPhaseThreeController).GetField("mobileSprite", flags).SetValue(controller, visual.sprite);
                typeof(FinalBossPhaseThreeController).GetField("bossVisual", flags).SetValue(controller, visual.transform);
                typeof(FinalBossPhaseThreeController).GetField("bossRenderer", flags).SetValue(controller, visual);
                MethodInfo configure = typeof(FinalBossPhaseThreeController).GetMethod("ConfigureMobileHitboxes", flags);
                configure.Invoke(controller, null);
                AssertHitboxes(target, core);
                AssertArtworkCoverage(target, visual);
                visual.flipX = true;
                visual.transform.localPosition += Vector3.up * .12f;
                typeof(FinalBossPhaseThreeController).GetMethod("UpdateArtworkHitboxes", flags).Invoke(controller, null);
                AssertArtworkCoverage(target, visual);
                configure.Invoke(controller, null);
                AssertHitboxes(target, core);
                UnityEngine.Object.DestroyImmediate(target.transform.Find("PhaseIII_CrownHitbox").GetComponent<BoxCollider>());
                configure.Invoke(controller, null);
                AssertHitboxes(target, core);
                Debug.Log("[FinalBoss] PASS: eight mobile hitboxes created/reused/recreated; brain, stem and four limbs covered; transparent corners excluded; flip/bob/lean/scale followed.");
            }
            finally { UnityEngine.Object.DestroyImmediate(fixture); }
        }

        private static void AssertHitboxes(EnemyController target, BoxCollider core)
        {
            BoxCollider[] boxes = target.GetComponentsInChildren<BoxCollider>(true);
            Require(boxes.Length == 8, "Mobile hitboxes missing or duplicated");
            Require(core.center == new Vector3(0f, 1f, 0f)
                && core.size == new Vector3(4f, 2f, 3.25f), "Incorrect ground-anchor hitbox");
            BoxCollider brain = target.transform.Find("PhaseIII_LowerBrainHitbox").GetComponent<BoxCollider>();
            BoxCollider crown = target.transform.Find("PhaseIII_CrownHitbox").GetComponent<BoxCollider>();
            Require(brain != null && crown != null && brain.size.x > crown.size.x,
                "Missing brain/crown silhouette regions");
            foreach (BoxCollider box in boxes)
                Require(box.enabled && box.isTrigger && box.gameObject.layer == target.gameObject.layer
                    && box.GetComponentInParent<EnemyController>(true) == target, "Hitbox lost its shared damage target");
        }

        private static void AssertArtworkCoverage(EnemyController target, SpriteRenderer visual)
        {
            Vector2[] hits =
            {
                new Vector2(.19f, .55f), new Vector2(.79f, .62f), new Vector2(.5f, .85f),
                new Vector2(.51f, .35f), new Vector2(.1f, .45f), new Vector2(.92f, .28f),
                new Vector2(.32f, .14f), new Vector2(.69f, .12f)
            };
            foreach (Vector2 uv in hits) Require(ArtworkPointCovered(target, visual, uv), "Visible artwork missed at " + uv);
            Require(!ArtworkPointCovered(target, visual, new Vector2(.02f, .9f)), "Empty upper-left corner became hittable");
            Require(!ArtworkPointCovered(target, visual, new Vector2(.98f, .85f)), "Empty upper-right corner became hittable");
        }

        private static bool ArtworkPointCovered(EnemyController target, SpriteRenderer visual, Vector2 uv)
        {
            Bounds bounds = visual.sprite.bounds;
            float x = Mathf.Lerp(bounds.min.x, bounds.max.x, uv.x);
            if (visual.flipX) x = -x;
            Vector3 world = visual.transform.TransformPoint(new Vector3(x, Mathf.Lerp(bounds.min.y, bounds.max.y, uv.y), 0f));
            foreach (BoxCollider box in target.GetComponentsInChildren<BoxCollider>(true))
            {
                if (box.transform == target.transform) continue;
                Vector3 delta = box.transform.InverseTransformPoint(world) - box.center;
                Vector3 half = box.size * .5f + Vector3.one * .001f;
                if (Mathf.Abs(delta.x) <= half.x && Mathf.Abs(delta.y) <= half.y && Mathf.Abs(delta.z) <= half.z) return true;
            }
            return false;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("[FinalBoss III] " + message);
        }
    }
}
