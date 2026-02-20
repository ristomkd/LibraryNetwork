namespace LibraryNetwork.Models
{
    public class Enums
    {
        public enum LoanStatus
        {
            Reserved = 0,
            Active = 1,
            Returned = 2,
            Overdue = 3,
            Cancelled = 4
        }

        public enum SemesterType
        {
            Winter = 0,
            Summer = 1
            // или H1/H2 ако сакаш
        }
    }
}
