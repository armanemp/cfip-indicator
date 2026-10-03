using System;
using cAlgo.API;

namespace CFIP.cBot.Execution
{
    internal sealed class CbotRuntimeAudioCoordinator
    {
        public void PlayStarted(
            Robot robot,
            bool liveArmed)
        {
            Play(
                robot,
                liveArmed
                    ? SoundType.PositiveNotification
                    : SoundType.Confirmation,
                liveArmed
                    ? "STARTED_LIVE_ARMED"
                    : "STARTED_DISARMED");
        }

        public void PlayLiveDisarmed(
            Robot robot)
        {
            Play(
                robot,
                SoundType.NegativeNotification,
                "LIVE_EXECUTION_DISARMED");
        }

        public void PlayStopped(
            Robot robot)
        {
            Play(
                robot,
                SoundType.NegativeNotification,
                "STOPPED");
        }

        public void PlayBlocked(
            Robot robot,
            string reason)
        {
            Play(
                robot,
                SoundType.NegativeNotification,
                "BLOCKED|" + (reason ?? "UNKNOWN"));
        }

        public void PlayExecutionConfirmed(
            Robot robot)
        {
            Play(
                robot,
                SoundType.PositiveNotification,
                "EXECUTION_CONFIRMED");
        }

        public void PlayExecutionRejected(
            Robot robot)
        {
            Play(
                robot,
                SoundType.NegativeNotification,
                "EXECUTION_REJECTED");
        }

        private void Play(
            Robot robot,
            SoundType sound,
            string eventKey)
        {
            if (robot == null)
                return;

            try
            {
                robot.Notifications.PlaySound(sound);
                robot.Print(
                    "CFIP cBot AUDIO | event={0} | cue={1}",
                    eventKey,
                    sound);
            }
            catch (Exception ex)
            {
                robot.Print(
                    "CFIP cBot AUDIO FAILED | event={0} | cue={1} | error={2}",
                    eventKey,
                    sound,
                    ex.Message);
            }
        }
    }
}
