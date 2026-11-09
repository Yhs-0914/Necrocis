using System;
using Necrocis;
class Program
{
    static int checks,failures;
    static void Check(bool b,string name){checks++;if(!b){failures++;Console.WriteLine("FAIL "+name);}}
    static void Main()
    {
        int statChoices=0,evolutions=0; LevelUpManager.OnLevelUp=()=>statChoices++;
        LevelUpManager.OnEvolution+=_=>evolutions++;
        LevelUpManager.ResetProgress();
        LevelUpManager.AddResolvedEnemyKillExp(1000);
        Check(LevelUpManager.GetCurrentLevel()==2 && statChoices==1,"large XP opens only one choice at a time");
        LevelUpManager.AddResolvedEnemyKillExp(100);
        Check(LevelUpManager.GetCurrentLevel()==2 && statChoices==1,"additional kills cannot overwrite an open choice");
        for(int i=0;i<10 && LevelUpManager.GetCurrentLevel()<5;i++)LevelUpManager.ProcessNextPendingLevelUp();
        Check(LevelUpManager.GetCurrentLevel()==5 && LevelUpManager.GetCurrentJob()==JobType.None,"level jump stops at first evolution");
        Check(!LevelUpManager.IsSkillUnlocked(1),"no class skill before evolution choice");
        LevelUpManager.SetJob(JobType.Mage);
        Check(LevelUpManager.IsSkillUnlocked(1) && !LevelUpManager.IsSkillUnlocked(2),"first evolution unlocks first skill");
        for(int i=0;i<40 && LevelUpManager.HasPendingLevelUp();i++)LevelUpManager.ProcessNextPendingLevelUp();
        int evolutionsBeforeSecondBoundary=evolutions;
        LevelUpManager.RestoreProgress(new PlayerProgressSaveData {level=9,job=JobType.Mage,itemRewards=ItemRewardState.ForNewRun()});
        LevelUpManager.AddResolvedEnemyKillExp(LevelUpManager.GetExpRequired());
        for(int i=0;i<40 && LevelUpManager.GetCurrentLevel()<10;i++)LevelUpManager.ProcessNextPendingLevelUp();
        Check(LevelUpManager.GetCurrentLevel()==10 && LevelUpManager.IsSkillUnlocked(2),"second evolution at level10");
        Check(evolutions==evolutionsBeforeSecondBoundary+1,"exactly two evolution notifications");
        LevelUpManager.RestoreProgress(new PlayerProgressSaveData {level=14,job=JobType.Mage,itemRewards=ItemRewardState.ForNewRun()});
        int before=statChoices;
        LevelUpManager.AddResolvedEnemyKillExp(LevelUpManager.GetExpRequired());
        var saved=new PlayerProgressSaveData();LevelUpManager.CaptureProgress(saved);
        LevelUpManager.RestoreProgress(saved);
        Check(LevelUpManager.HasPendingLevelUp(),"save while choosing level15 reward preserves pending stat choice");
        var resume=typeof(LevelUpManager).GetMethod("ResumePendingChoices");
        Check(resume!=null,"restored choice has a replay entry point");
        if(resume!=null){resume.Invoke(null,null);Check(statChoices==before+2,"continue reopens unresolved stat choice");}
        LevelUpManager.RecordSelection(LevelUpStatChoice.HealthUp);
        var committed=new PlayerProgressSaveData();LevelUpManager.CaptureProgress(committed);
        Check(!committed.growthChoicePending,"committed stat is acknowledged before delayed reveal/autosave");
        int levelDuringReveal=LevelUpManager.GetCurrentLevel();
        LevelUpManager.AddResolvedEnemyKillExp(10000);
        Check(LevelUpManager.GetCurrentLevel()==levelDuringReveal,"XP waits for prior choice presentation to finish");
        Check(evolutions==evolutionsBeforeSecondBoundary+1,"restoring saved progression does not heal or replay evolution");
        int jobPrompts=0;LevelUpManager.OnJobSelect=()=>jobPrompts++;
        LevelUpManager.RestoreProgress(new PlayerProgressSaveData {level=5});
        if(resume!=null)resume.Invoke(null,null);
        Check(jobPrompts==1,"legacy jobless level5 save reopens first evolution");
        LevelUpManager.RestoreProgress(new PlayerProgressSaveData {level=29,job=JobType.Mage,itemRewards=ItemRewardState.ForNewRun()});
        LevelUpManager.AddResolvedEnemyKillExp(100000);LevelUpManager.ProcessNextPendingLevelUp();LevelUpManager.AddResolvedEnemyKillExp(100000);
        Check(LevelUpManager.GetCurrentLevel()==30,"maxlevel stays30 after excess XP");
        Console.WriteLine($"{checks-failures}/{checks} progression checks passed");Environment.ExitCode=failures==0?0:1;
    }
}
