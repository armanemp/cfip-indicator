namespace cAlgo
{
    internal static class ExecutionControlPresentationRule
    {
        public static bool IsInteractive => false;

        public static string ComposeStatusText(
            string caption,
            bool enabled)
        {
            string normalizedCaption =
                string.IsNullOrWhiteSpace(caption)
                    ? "EXECUTION"
                    : caption.Trim();

            return
                normalizedCaption +
                "  •  " +
                (enabled ? "ON" : "OFF");
        }
    }
}
