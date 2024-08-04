﻿using System.Text.Json.Serialization;

namespace LibraryAPI.Models.DTOs.Request
{
    public class LoanReturnRequest
    {
        public string MemberId { get; set; } = string.Empty;

        public DateTime? ReturnDate { get; set; }

        public DateTime DueDate { get; set; }
    }
}
