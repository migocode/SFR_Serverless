using System.Collections.Generic;

namespace SFR_Serverless_Assignment.Models
{
    public class Report
    {
        public int Month { get; set; }
        public string MonthName { get; set; }
        public string Iban { get; set; }
        public Customer Customer { get; set; }
        public IEnumerable<Transaction> Transactions { get; set; }
    }
}
