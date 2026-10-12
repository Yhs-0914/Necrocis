using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Necrocis;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace NecrocisEditor
{
    public static class EliteIntegrationAudit
    {
        private sealed class InspectorHost : EditorWindow { }
        [Serializable] private sealed class Row
        {
            public string id, name, definition, pattern, biome;
            public float hp, attack, speed, contact;
            public int xp, deathFrames;
        }
        [Serializable] private sealed class SpawnRow
        {
            public string biome, path;
            public string[] ids;
            public int min, max, each, clearance;
            public float spacing, activation, release;
        }
        [Serializable] private sealed class Report
        {
            public int pass, fail;
            public List<string> checks = new List<string>();
            public List<Row> monsters = new List<Row>();
            public List<SpawnRow> spawns = new List<SpawnRow>();
            public string normal, hard, progression;
        }
        public static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Saved Edit Mode required");
            var report = new Report();
            void Check(bool ok, string message) { if (!ok) throw new InvalidOperationException(message); report.pass++; report.checks.Add("PASS " + message); }
            try
            {
                var catalog = Resources.Load<MonsterBalanceCatalog>(MonsterBalanceRuntime.CatalogResourcePath);
                Check(catalog != null && catalog.GetValidationErrors().Count == 0, "Runtime catalog and all source references valid");
                report.normal = AssetDatabase.GetAssetPath(catalog.difficultyCatalog.normal); report.hard = AssetDatabase.GetAssetPath(catalog.difficultyCatalog.hard); report.progression = AssetDatabase.GetAssetPath(catalog.progression);
                var dependencies = new HashSet<string>(AssetDatabase.GetDependencies(AssetDatabase.GetAssetPath(catalog), true));
                var buildScenes = new HashSet<string>(EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path));
                foreach (string name in new[] { "Intestine", "Liver", "Lung", "Stomach" })
                {
                    var biome = AssetDatabase.LoadAssetAtPath<BiomeConfig>("Assets/_Project/Data/BiomeConfigs/" + name + "BiomeConfig.asset");
                    var config = biome.biomeEliteSpawnConfig;
                    Check(buildScenes.Contains("Assets/_Project/Scenes/" + name + ".unity") && config != null && config.enabled
                        && config.monsters.Count == 2 && catalog.biomeEliteSpawns.Contains(config), name + ": enabled game scene / biome config / catalog all reference the same two-species source");
                    Check(config.minimumCount >= 2 && config.maximumCount >= config.minimumCount && config.minimumPerType == 1 && config.GetValidationError() == null, name + ": sparse dedicated placement with at least one of each species");
                    report.spawns.Add(new SpawnRow { biome=name,path=AssetDatabase.GetAssetPath(config),ids=config.monsters.Select(r=>r.monsterDefinition.monsterId).ToArray(),min=config.minimumCount,max=config.maximumCount,each=config.minimumPerType,clearance=config.clearanceCells,spacing=config.minimumSpacing,activation=config.activationDistance,release=config.releaseDistance });
                    using var data = new SerializedObject(config);
                    for (int i=0;i<config.monsters.Count;i++)
                    {
                        var definition=config.monsters[i].monsterDefinition;
                        var ui=new EnemySpawnRuleBalanceDrawer().CreatePropertyGUI(data.FindProperty("monsters").GetArrayElementAtIndex(i));
                        var paths=ui.Query<PropertyField>().ToList().Select(p=>p.bindingPath.Split('.').Last()).ToArray();
                        Check(new[]{"maxHealth","moveSpeed","attackDamage","expReward","contactDamage","killTriggerCount","killTriggerEnemyName","attackRange","attackCooldown","projectileSpeed","deathSprites"}.All(p=>!paths.Contains(p)),definition.monsterId+": unused base/FSM/kill-trigger duplicates hidden in placement inspector");
                        Check(paths.Contains("monsterDefinition")&&paths.Contains("chaseRadius")&&paths.Contains("colliderSize"),definition.monsterId+": actual source, chase and receiving-body controls retained");
                        Check(dependencies.Contains(AssetDatabase.GetAssetPath(definition))&&dependencies.Contains(AssetDatabase.GetAssetPath(definition.pattern)),definition.monsterId+": runtime Resources catalog includes definition and pattern");
                        var set=definition.statSets.Single(s=>s.id=="Default");
                        Object presentation=(Object)definition.pattern.GetType().GetField("presentation").GetValue(definition.pattern);
                        var death=(Sprite[])presentation.GetType().GetField("deathFrames").GetValue(presentation);
                        Check(death.Length==6&&death.All(s=>s!=null),definition.monsterId+": six connected death frames");
                        report.monsters.Add(new Row{id=definition.monsterId,name=definition.displayName,biome=name,definition=AssetDatabase.GetAssetPath(definition),pattern=AssetDatabase.GetAssetPath(definition.pattern),hp=set.maxHealth,attack=set.attackPower,speed=set.moveSpeed,xp=definition.reward.baseExperience,contact=definition.contact.damageCoefficient,deathFrames=death.Length});
                    }
                }
                var editor=Editor.CreateEditor(catalog);
                var host=ScriptableObject.CreateInstance<InspectorHost>();
                try
                {
                    host.titleContent=new GUIContent("Inspector integration check"); host.ShowUtility();
                    var ui=editor.CreateInspectorGUI();
                    host.rootVisualElement.Add(ui);
                    foreach(var definition in catalog.monsters)
                    {
                        ui.Q<DropdownField>("monster-selector").index=catalog.monsters.IndexOf(definition);
                        var paths=ui.Query<PropertyField>().ToList().Select(p=>p.bindingPath).ToArray();
                        string tier=definition.tier==MonsterTier.Boss?"bosses":definition.tier==MonsterTier.Normal?"enemies":"elites";
                        Check(new[]{"statSets","reward","contact","patternDamage",tier,"stages"}.All(paths.Contains),definition.monsterId+": dedicated base/reward/contact/pattern coefficients + selected tier difficulty + progression sources exposed");
                        foreach(var mode in new[]{GameDifficulty.Normal,GameDifficulty.Hard})
                        {
                            ui.Q<EnumField>("difficulty-selector").value=mode;
                            foreach(int stage in new[]{0,1,2,3,4})
                            {
                                ui.Q<SliderInt>("stage-selector").value=stage;
                                var errors=ui.Q<HelpBox>("balance-validation");
                                Check(errors.ClassListContains("balance-hidden")&&ui.Q("balance-results").childCount>=9,definition.monsterId+" "+mode+"/stage"+stage+": calculated Inspector preview valid");
                            }
                        }
                        if(definition.pattern!=null)
                        {
                            paths=ui.Query<PropertyField>().ToList().Select(p=>p.bindingPath).ToArray();
                            Check(paths.Contains("windupSeconds")&&paths.Contains("rearmSeconds")&&!paths.Contains("tailSweepEnabled"),definition.monsterId+": pattern timing controls present, retired tail absent");
                        }
                    }
                }
                finally {host.rootVisualElement.Clear();Object.DestroyImmediate(editor);host.Close();}
                Check(!((HelicoSpiralPatternSettings)report.monsters.Where(r=>r.id=="elite.s-01").Select(r=>AssetDatabase.LoadAssetAtPath<MonsterDefinition>(r.definition)).Single().pattern).tailSweepEnabled,"S-01 retired tail remains off");
            }
            catch(Exception e){report.fail++;report.checks.Add("FAIL "+e);throw;}
            finally
            {
                Directory.CreateDirectory("Logs");File.WriteAllText("Logs/Elite-ALL01-audit.json",JsonUtility.ToJson(report,true));File.WriteAllLines("Logs/Elite-ALL01-audit-results.txt",report.checks);
            }
        }
    }
}
