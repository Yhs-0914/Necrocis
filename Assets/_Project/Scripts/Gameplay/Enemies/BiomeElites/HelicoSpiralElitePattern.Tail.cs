namespace Necrocis
{
    public sealed partial class HelicoSpiralElitePattern
    {
        // S-01-B was removed by design. Keep read-only diagnostics for archived editor fixtures.
        public HelicoTailSweepGeometry TailGeometry => null;
        public bool TailActionVisible => false;
        public float TailProgress => 0;
        public bool TailCompleted => false;
        public int TailHitAttempts => 0;
        public bool TailEnabled => false;
        public float TailWindupDuration => 0;
        public float TailSweepDuration => 0;

        private void DisableLegacyTailRendering()
        {
            var legacy = GetComponent<HelicoTailSweepVisual>();
            if (legacy == null) return;
            legacy.Hide();
            legacy.enabled = false;
        }
    }
}
