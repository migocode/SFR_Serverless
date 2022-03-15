using System.Collections.Generic;

namespace SFR_Serverless_Assignment.Models
{
    public class Report
    {
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public Customer Customer { get; set; } = new Customer();
        public List<OutboundTransaction> Transactions { get; set; } = new List<OutboundTransaction>();
    }
}
