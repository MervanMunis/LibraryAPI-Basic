using System;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryAPI.Models.DTOs.Request
{
    public class NationalityRequest
    {
        public string Name { get; set; } = string.Empty;

        public string NationalityCode { get; set; } = string.Empty;
    }
}

