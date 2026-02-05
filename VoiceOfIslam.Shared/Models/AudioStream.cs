using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VoiceOfIslam.Shared.Models
{
    public class AudioStream
    {
        [Key]
        public Guid Id { get; set; } = Guid.CreateVersion7();

        [Required]
        [MaxLength(200)] 
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public string BlobUrl { get; set; } = string.Empty; // URL to Azure Blob Storage

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ScheduledAt { get; set; } // Specific for Monday Live streams

        public bool IsLive { get; set; } = false;

        [MaxLength(100)]
        public string Speaker { get; set; } = "Unknown";

        public TimeSpan Duration { get; set; }
    }
}
