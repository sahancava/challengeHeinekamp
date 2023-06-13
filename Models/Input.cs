using System.ComponentModel.DataAnnotations;

namespace challengeHeinekamp.Models
{
    public class InputModel {
        [Key]
        public int Id { get; set; }
        [MaxLength(200)]
        public string? Name { get; set; }
        [MaxLength(200)]
        public string? OriginalFileName { get; set; }
        public System.DateTime? UploadDateTime { get; set; }
        public int? DownloadCount { get; set; }
        [MaxLength(200)]
        public string? UploadedBy { get; set; }
        [MaxLength(10)]
        public string? Icon { get; set; }
        [MaxLength(10)]
        public string? Extension { get; set; }
        [MaxLength(36)]
        public Guid Guid { get; set; }
        public System.DateTime? latestDownloadDateTime { get; set; }
    }
}