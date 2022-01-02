namespace Venbacodes.Samples.ABAC.Domain
{
    public static class Permissions
    {
        public enum Access
        {
            None = 0,
            View
        }

        public enum Scope
        {
            None = 0,
            All,
            Specific,
            Self
        }

        public enum Module
        {
            None = 0,            
            Category,
            Division
        }
    }
}
