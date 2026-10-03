namespace CFIP.Contracts
{
    public enum ManagementCommandRequestStatus
    {
        Rejected = 0,
        Queued = 1,
        AlreadyPending = 2,
        AlreadyConfirmed = 3,
        WriteFailed = 4
    }

    public static class ManagementCommandRequestStatusPolicy
    {
        public static bool IsAccepted(this ManagementCommandRequestStatus status)
        {
            return status == ManagementCommandRequestStatus.Queued ||
                   status == ManagementCommandRequestStatus.AlreadyPending ||
                   status == ManagementCommandRequestStatus.AlreadyConfirmed;
        }

        public static bool IsBrokerConfirmed(this ManagementCommandRequestStatus status)
        {
            return status == ManagementCommandRequestStatus.AlreadyConfirmed;
        }
    }
}
