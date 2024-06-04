using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace WebApplication1.Models
{
    public class Document
    {
        public int DocumentId { get; set; }
        public string DocumentName { get; set; }
        public string FileName { get; set; }
        public string DocumentBase64 { get; set; }
        public int UploadedBy { get; set; }
        public string? UploadedByUsername { get; set; } 

        public DateTime CreatedAt { get; set; }
       public IFormFileCollection Files { get; set; }

    }
}
