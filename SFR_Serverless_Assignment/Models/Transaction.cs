using System;

namespace SFR_Serverless_Assignment.Models
{
    public class Transaction
    {
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string CreditorIban { get; set; }
        public string DebitorIban { get; set; }
    }
}