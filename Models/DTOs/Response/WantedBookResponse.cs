﻿namespace LibraryAPI.Models.DTOs.Response
{
    public class WantedBookResponse
    {
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Languages { get; set; }

        public List<string>? SubCategories { get; set; }
    }
}
