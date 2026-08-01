namespace QuizStudyAS.Constants
{
    public static class RequestJoinStatus
    {
        public const string Pending = "PENDING";
        public const string Approved = "APPROVED";
        public const string Denied = "DENIED";
    }

    public static class ClassroomUserStatus
    {
        public const string Studying = "STUDYING";
        public const string Kicked = "KICKED";
        public const string Left = "LEFT";
    }

    public static class ClassroomMaterialStatus
    {
        public const string Available = "AVAILABLE";
        public const string Deleted = "DELETE";
    }

    public static class ClassroomRoleStatus
    {
        public const string Owner = "OWNER";
        public const string Joined = "JOINED";
    }
}