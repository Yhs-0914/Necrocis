using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Necrocis;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NecrocisEditor
{
    // Regression checks for retiring the November 14-15 item additions.
    public static class RetiredItemChecks
    {
        private const BindingFlags Members = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly string[] Retired = { "dilute_mucus",
            "binding_mucus",
            "luminous_tissue",
            "focusing_organ",
            "split_fragment",
            "radial_tissue",
            "acid_leak",
            "acid_cyst",
            "elastic_vessel",
            "layered_vessel",
            "dilute_mucosa",
            "concentrated_mucosa",
            "cooling_nerve",
            "frost_ganglion",
            "ruptured_capillary",
            "spurting_vessel",
            "clot_membrane",
            "aggregated_platelets",
            "regeneration_fragment",
            "regenerative_marrow",
            "blood_larva",
            "pulsing_blood_drone",
            "shield_vesicle",
            "layered_guardian",
            "spiral_fiber",
            "compressed_alveolus",
            "suture_stinger",
            "orbit_vesicle",
            "spinal_hook",
            "distance_lens",
            "impact_marrow",
            "clasping_tendon",
            "burst_valve",
            "opening_cartilage",
            "reflex_ganglion",
            "resonant_bone",
            "mana_reservoir",
            "rehearsal_tissue",
            "differentiation_cell",
            "residual_muscle",
            "medulla_valve",
            "paired_nerve",
            "retaliation_cortex",
            "long_synapse",
            "ciliated_sole",
            "friction_mucus",
            "afterimage_fiber",
            "elastic_joint",
            "pressure_pouch",
            "landing_pad",
            "corner_nerve",
            "twin_step",
            "steady_step",
            "escape_vein",
            "infectious_mucus",
            "frozen_fissure",
            "bloodtrail_receptor",
            "clot_disruptor",
            "compound_antigen",
            "acid_seal",
            "nerve_wick",
            "slow_coagulum",
            "serum_leech",
            "cold_bloom",
            "rupture_platelets",
            "surplus_serum",
            "buffer_fat",
            "scar_tissue",
            "leukocyte_barrier",
            "shock_absorber",
            "pulse_stitch",
            "fibrin_lock",
            "serous_rind",
            "recovery_capsule",
            "target_lymphocyte",
            "predator_larva",
            "bilobed_parasite",
            "marrow_nest",
            "retrieval_tentacle",
            "guardian_spore",
            "feeder_sac",
            "patrolling_rib",
            "pulse_companion",
            "relay_colony",
            "scent_receptor",
            "organ_imprint",
            "mutation_lens",
            "marrow_memory",
            "symbiotic_receptor",
            "boss_marrow_seal",
            "lymph_reservoir",
            "regional_buffer",
            "trophy_receptor",
            "redundancy_filter" };
        private static readonly string[] Original = { "double_core",
            "triple_core",
            "homing_cell",
            "hypertrophy_cell",
            "reflux_organ",
            "piercing_mucus",
            "laryngeal_nerve",
            "beam_organ",
            "split_tissue",
            "explosive_blood_cell",
            "acidic_rupture",
            "cell_proliferation",
            "pulse_bullet",
            "vascular_reflection",
            "toxic_mucosa",
            "freezing_nerve",
            "hemorrhage_organ",
            "overheated_organ",
            "mutant_eye",
            "organ_tentacle",
            "rampage_bloodflow",
            "muscle_spasm",
            "unstable_core",
            "bio_resonance",
            "blood_pressure_burst",
            "void_cell",
            "forbidden_growth",
            "overclock_nerve",
            "blood_contract",
            "hyperplasia_heart",
            "decay_organ",
            "rupture_muscle",
            "imperfect_regeneration",
            "severance_reflex",
            "bio_gamble",
            "exoskeleton",
            "platelet_membrane",
            "recovery_factor",
            "reflective_skin",
            "bio_barrier",
            "split_regeneration",
            "infected_host",
            "spore_colony",
            "blood_drone",
            "guardian_organ",
            "tentacle_colony",
            "electric_neural_network",
            "infection_transference",
            "macrophage",
            "gluttonous_organ",
            "heart_sniper",
            "bloodflow_acceleration",
            "focused_nerve",
            "execution_instinct",
            "berserk_cell",
            "unstable_cell",
            "grotesque_growth",
            "mutation_rampage",
            "parasitic_bomb",
            "frenzy_hormone",
            "dense_muscle",
            "hardened_muscle",
            "vital_cell",
            "vital_organ",
            "swift_cell",
            "elastic_fiber",
            "elongated_fiber",
            "active_fiber" };
        private static bool passed, started, oldOptionsEnabled;
        private static EnterPlayModeOptions oldOptions;
        private static double deadline;
        private static int assertions;
        public static void Run()
        {
            if (!Application.isBatchMode || EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Use an isolated Unity batch project.");
            passed = started = false; assertions = 0;
            oldOptionsEnabled = EditorSettings.enterPlayModeOptionsEnabled; oldOptions = EditorSettings.enterPlayModeOptions;
            EditorSettings.enterPlayModeOptionsEnabled = true; EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload;
            SaveService.UseStorageRootForTests(Path.GetFullPath("tmp/retired-items-" + Guid.NewGuid().ToString("N")));
            GameplayFocusOwner.TestFocusOverride = true;
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            deadline = EditorApplication.timeSinceStartup + 120;
            EditorApplication.playModeStateChanged += OnState; EditorApplication.update += Tick; EditorApplication.isPlaying = true;
        }
        private static void Tick()
        {
            if (!started && EditorApplication.isPlaying && !EditorApplication.isCompiling && Time.frameCount > 2)
            { started = true; Execute(); return; }
            if (EditorApplication.timeSinceStartup < deadline) return;
            Debug.LogError("RetiredItemChecks timed out."); Finish();
        }
        private static void Execute()
        {
            try
            {
                SessionRuntimeBindings.Register();
                Require(SaveService.TryBeginNewGame(GameDifficulty.Normal,out var error),"isolated new run: " + error);
                InputManager.Instance.Focus.SetNativeFocused(true); InputManager.ResetSessionClock();
                var root = new GameObject("RetiredItemCheckPlayer"); PlayerCompositionRoot.EnsureGameplay(root);
                var player = root.GetComponent<PlayerController>(); player.enabled = false;
                root.GetComponent<PlayerAttack>().enabled = false; root.GetComponent<PlayerClassSkillController>().enabled = false;
                var stats = player.Stats.RuntimeStats;
                stats.SetBaseStat(CharacterStatType.MaxHealth,100); stats.SetBaseStat(CharacterStatType.AttackPower,10);
                stats.SetBaseStat(CharacterStatType.AttackSpeed,1); stats.SetBaseStat(CharacterStatType.MoveSpeed,5); stats.SetCurrentHealth(100);
                var inventory = root.GetComponent<PlayerItemManager>(); inventory.PopulateBasicProjectileTemplateItems();
                Require(ItemDefinitions.All.Count == 68 && inventory.ItemEntries.Count == 68,"68 original entries remain");
                Require(ItemDefinitions.All.Select(item=>item.ItemId).ToHashSet(StringComparer.OrdinalIgnoreCase).SetEquals(Original),"exact original catalog IDs preserved");
                foreach(string id in Retired)
                {
                    Require(!ItemDefinitions.TryGet(id,out _) && !inventory.TryGetItemEntry(id,out _),id+" absent from catalog and F8 list");
                    Require(!inventory.TryAcquireItem(id,out var reason) && reason == PlayerItemAcquireFailureReason.NotFoundInCatalog,id+" cannot be acquired");
                }
                foreach(string id in Original) Require(inventory.TryAcquireItem(id,out _),"original can be acquired: "+id);
                Require(inventory.ItemCount == 68,"original items retain unlimited inventory");
                inventory.ClearAllItems();
                var serialized = Get<List<PlayerItemManager.PlayerItemEntry>>(inventory,"itemEntries");
                foreach(string id in Retired) serialized.Add(new PlayerItemManager.PlayerItemEntry(id,id,"stale serialized item",PlayerItemCategory.AttackStyle));
                Set(inventory,"startingItemIds",Retired.ToList()); inventory.RebuildCatalog();
                Require(inventory.ItemEntries.Count == 68 && Get<List<string>>(inventory,"startingItemIds").Count == 0,"stale scene and prefab entries are purged");
                var mixedSave = Retired.Select(id=>new SavedItemStateData{itemId=id}).ToList();
                mixedSave.AddRange(new[]{new SavedItemStateData{itemId="dense_muscle"},new SavedItemStateData{itemId="swift_cell"},new SavedItemStateData{itemId="swift_cell"}});
                inventory.RestoreSavedItems(mixedSave);
                Require(inventory.ItemCount == 3 && inventory.CaptureSavedItems().All(item=>!Retired.Contains(item.itemId)),"mixed old save restores only retained items");
                Near(stats.AttackPower,11,"existing attack stat still applies"); Near(stats.AttackSpeed,1.3f,"swift remains +15 percent per acquisition");
                var full = inventory.ItemEntries.Select(item=>item.ItemId).ToArray();
                for(int seed=0;seed<100;seed++)
                {
                    var offers=CampaignRewardRules.RefreshOffers(Retired.Take(3),full,new[]{"swift_cell"},null,seed);
                    Require(offers.Count==3 && offers.Distinct().Count()==3 && offers.All(id=>Original.Contains(id)),"saved retired reward candidates refreshed seed "+seed);
                }
                var panel=root.GetComponent<PlayerItemTestPanel>() ?? root.AddComponent<PlayerItemTestPanel>();
                Set(panel,"searchQuery","신속");
                Require((bool)Invoke(panel,"MatchesSearch","신속 세포","swift_cell") && !(bool)Invoke(panel,"MatchesSearch","응축 근육","dense_muscle"),"F8 Korean name search remains functional");
                inventory.ClearAllItems();
                foreach(bool elite in new[]{false,true})
                {
                    string kind=elite?"elite":"normal";
                    var rule=new EnemySpawnRuleConfig{name="ImmediateExperienceCheck-"+kind,maxHealth=10,moveSpeed=0,colliderSize=Vector3.one,enableContactDamage=false,expReward=elite?19:7,isElite=elite};
                    var enemy=EnemyController.Acquire(null,rule.name,EnemyController.GetPoolArchetypeId(rule));
                    enemy.ConfigureWithContext(null,rule,new Vector3(20,0,20),new Vector3(20,0,20),new EnemySpawnContext(null,new FlatWorld(),EnemyEncounterRole.Field));
                    enemy.SetAiSuppressed(true);
                    Require(enemy.IsElite==elite,"experience fixture tier: "+kind);
                    int xp=0; Action<int> received=amount=>xp+=amount; LevelUpManager.OnExpGained+=received;
                    try
                    {
                        int expected=Mathf.RoundToInt(rule.expReward*DifficultyBalanceService.GetEnemyBalance(false).experienceReward);
                        enemy.TakeDamage(10000,player.GetHealthEffectSource("retired-check"));
                        Require(xp==expected && xp>0,kind+" grants full experience immediately in death call, without approaching pickup");
                        int once=xp; enemy.GrantExp(); Require(xp==once,kind+" experience cannot be granted twice");
                        Require(GameObject.Find("ExperiencePickup")==null,kind+" death does not spawn experience orbs");
                    }
                    finally { LevelUpManager.OnExpGained-=received; }
                }
                Debug.Log("RETIRED_ITEMS_PASS assertions="+assertions); passed=true;
            }
            catch(Exception error){Debug.LogException(error);}
            finally {EditorApplication.isPlaying=false;}
        }
        private static T Get<T>(object target,string name)=>(T)target.GetType().GetField(name,Members).GetValue(target);
        private static void Set(object target,string name,object value)=>target.GetType().GetField(name,Members).SetValue(target,value);
        private static object Invoke(object target,string name,params object[] args)=>target.GetType().GetMethod(name,Members).Invoke(target,args);
        private static void Near(float actual,float expected,string label)=>Require(Mathf.Abs(actual-expected)<.005f,label+": "+actual+" != "+expected);
        private static void Require(bool value,string label){assertions++;if(!value)throw new InvalidOperationException(label);}
        private static void OnState(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredEditMode)Finish();}
        private static void Finish()
        {
            EditorApplication.update-=Tick; EditorApplication.playModeStateChanged-=OnState;
            EditorSettings.enterPlayModeOptionsEnabled=oldOptionsEnabled; EditorSettings.enterPlayModeOptions=oldOptions;
            GameplayFocusOwner.TestFocusOverride=null; SaveService.ResetStaticStateForTests(); EditorApplication.Exit(passed?0:1);
        }
        private sealed class FlatWorld : IEnemyWorldQueries
        {
            public bool HasTerrain => false;
            public bool IsValidCell(Vector2Int cell) => true;
            public bool IsWalkableCell(Vector2Int cell) => true;
            public Vector2Int WorldToCell(Vector3 p) => new Vector2Int(Mathf.FloorToInt(p.x), Mathf.FloorToInt(p.z));
            public Vector3 CellToWorld(Vector2Int c) => new Vector3(c.x, 0, c.y);
            public float GetGroundHeight(Vector3 p) => 0;
            public float GetGroundHeight(Vector2Int c) => 0;
            public bool CanEnemyMove(Vector3 from, Vector3 to, Vector2 half) => true;
            public bool IsPlayerInsideLockedEncounter(Vector3 p) => false;
        }

    }
}
