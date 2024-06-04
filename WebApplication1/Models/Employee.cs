namespace WebApplication1.Controllers
{
    public class Employee
    {
        public int EmployeeID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public string JobTitle { get; set; }
        public int? CreatedByUserId { get; set; }
        public string? CreatedByUsername { get; set; }

    }

}