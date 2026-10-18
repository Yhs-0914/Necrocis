namespace Necrocis
{
    // Invoked synchronously in the existing Defeated callback order, before EnemyDeadState grants XP.
    public interface IEnemyDefeatApproval
    {
        void ApproveDefeat(EnemyController enemy);
    }

    public partial class EnemyController
    {
        private IEnemyDefeatApproval defeatApproval;

        public void BindDefeatApproval(IEnemyDefeatApproval approval)
        {
            if (defeatApproval != null) Defeated -= defeatApproval.ApproveDefeat;
            defeatApproval = approval;
            if (defeatApproval != null) Defeated += defeatApproval.ApproveDefeat;
        }
    }
}
