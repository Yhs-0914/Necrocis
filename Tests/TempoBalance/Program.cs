using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
class Program
{
    static int failures, checks;
    static Type Rules => typeof(Program).Assembly.GetType("Necrocis.TempoBalanceRules");
    static object Call(string name, params object[] args) => Rules.GetMethod(name).Invoke(null,args);
    static void Check(bool ok,string label){checks++;if(!ok){failures++;Console.WriteLine("FAIL "+label);}}
    static void Main(string[] args)
    {
        string root=args[0];
        foreach(string rel in new[]{"Assets/_Project/Resources/Balance/LevelProgressionConfig.asset","Assets/_Project/Resources/Balance/Difficulty/HardLevelProgressionConfig.asset"})
        {
            string text=File.ReadAllText(Path.Combine(root,rel));
            foreach(var pair in new[]{("maxLevel",30),("jobSelectionLevel",5),("skill1UnlockLevel",5),("skill2UnlockLevel",10)})
                Check(Regex.IsMatch(text,@"(?m)^  "+pair.Item1+": "+pair.Item2+@"
?$"),rel+" "+pair.Item1);
        }
        Check(Rules!=null,"combat tempo behavior is implemented");
        if(Rules!=null)
        {
            Check((float)Call("LevelMultiplier",1,.03f,30)==1f,"level one has no scaling");
            Check(Math.Abs((float)Call("LevelMultiplier",15,.03f,30)-1.42f)<.0001,"linear health scaling at 15");
            Check((float)Call("LevelMultiplier",99,.03f,30)==(float)Call("LevelMultiplier",30,.03f,30),"level scaling is capped");
            Check((float)Call("LevelMultiplier",-1,-.1f,30)==1f,"invalid negative values cannot weaken enemies");
            Check((float)Call("FieldHitDamage",false,false)==2f,"normal hit one heart");
            Check((float)Call("FieldHitDamage",true,false)==3f,"normal elite hit one and a half hearts");
            Check((float)Call("FieldHitDamage",true,true)==4f,"hard elite hit two hearts");
            Check(!(bool)Call("RollDrop",0f,0f),"zero chance never drops");
            Check((bool)Call("RollDrop",.999f,1f),"guaranteed drop");
            Check((bool)Call("RollDrop",.249f,.25f)&&!(bool)Call("RollDrop",.25f,.25f),"elite probability boundary");
            Check(!(bool)Call("RollDrop",float.NaN,.25f),"invalid random value cannot drop");
            Check((int)Call("PendingItemReward",1,0,true,15)==1,"starter reward on a new run");
            Check((int)Call("PendingItemReward",14,1,true,15)==0,"no premature midgame reward");
            Check((int)Call("PendingItemReward",15,1,true,15)==2,"midgame reward at 15");
            Check((int)Call("PendingItemReward",30,3,true,15)==0,"claimed rewards never repeat at max level");
            Check((int)Call("PendingItemReward",15,0,true,15)==1,"starter reward precedes midgame after a level jump");
            Check((int)Call("PendingItemReward",1,0,false,15)==0,"starter reward can be disabled");
        }
        Type stateType=typeof(Program).Assembly.GetType("Necrocis.ItemRewardState");
        Check(stateType!=null,"persistent reward state exists");
        if(stateType!=null)
        {
            dynamic state=stateType.GetMethod("ForNewRun").Invoke(null,null);
            Check((bool)state.BeginOffer(1,new[]{"double_core","homing_cell","triple_core"}),"three-choice offer starts");
            Check(!(bool)state.BeginOffer(1,new[]{"reroll"}),"pending offer cannot reroll");
            Check(!(bool)state.Claim("not_offered"),"unoffered item cannot claim reward");
            dynamic saved=state.Copy();
            Check((int)saved.pendingReward==1 && saved.pendingItemIds.Count==3,"pending choices captured for continue");
            Check((bool)state.Claim("homing_cell"),"offered selection claims reward");
            Check((int)state.claimedMask==1 && (int)state.pendingReward==0,"claim is recorded exactly once");
            Check(!(bool)state.Claim("homing_cell"),"second claim fails");
            Check((int)saved.claimedMask==0 && saved.pendingItemIds.Count==3,"save copy is independent");
            dynamic legacy=Activator.CreateInstance(stateType);legacy.MigrateLegacy(30,15);
            Check((int)legacy.claimedMask==3,"old saves do not gain retroactive rewards");
            dynamic oldMidgame=Activator.CreateInstance(stateType);oldMidgame.MigrateLegacy(14,15);
            Check((int)oldMidgame.claimedMask==1,"old level14 save can still earn future level15 reward");
            dynamic duplicate=stateType.GetMethod("ForNewRun").Invoke(null,null);
            Check(!(bool)duplicate.BeginOffer(1,new[]{"same","same","same"}),"duplicate cards rejected");
        }
        Console.WriteLine($"{checks-failures}/{checks} checks passed");Environment.ExitCode=failures==0?0:1;
    }
}
