﻿namespace LibraryAPI.Models.DTOs.Response
{
    public class SubCategoryResponse
    {
        public short SubCategoryId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? SubCategoryStatus { get; set; }
    }
}
