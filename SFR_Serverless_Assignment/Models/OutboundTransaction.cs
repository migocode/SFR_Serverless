using System;

namespace SFR_Serverless_Assignment.Models
{
    public class OutboundTransaction
    {
        public DateTime Date { get; set; } = DateTime.UtcNow;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string CreditorIban { get; set; } = string.Empty;
        public string DebitorIban { get; set; } = string.Empty;
    }
}