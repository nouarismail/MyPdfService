using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyPdfService.Models
{
    public class FileModel
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public byte[] Content { get; set; }
        public virtual string? PdfFileName { get; set; }
    }
}