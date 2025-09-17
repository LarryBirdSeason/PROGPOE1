using System.ComponentModel.DataAnnotations;

namespace PROGPOE1.Models
{
    public class Claim
    {
        public int Id { get; set; }
        public string User { get; set; } = string.Empty;
        public string Month { get; set; } = string.Empty; // e.g., "2025-01"
        [Range(1, double.MaxValue, ErrorMessage = "Hours must be positive.")]
        public double Hours { get; set; }
        public double Amount { get; set; }
        public string Status { get; set; } = "Submitted";
        public List<string> Documents { get; set; } = new List<string>();
        public string Notes { get; set; } = string.Empty;
    }
}