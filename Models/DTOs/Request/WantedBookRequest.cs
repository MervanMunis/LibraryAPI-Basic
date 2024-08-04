﻿namespace LibraryAPI.Models.DTOs.Request
{
    public class WantedBookRequest
    {
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public short LanguageId { get; set; }

        public short CategoryId { get; set; }
    }
}
