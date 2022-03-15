using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFR_Serverless_Assignment.Models
{
    public class InboundTransaction
    {
        public string Date { get; set; } = DateTime.UtcNow.ToString("o");
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string CreditorIban { get; set; } = string.Empty;
        public string DebitorIban { get; set; } = string.Empty;
    }
}
