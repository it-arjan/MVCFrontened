using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MyData.Models
{
    public class RequestLogEntry
    {
        public RequestLogEntry(RequestLogEntry toClone)
        {
            User = toClone.User;
            Timestamp = toClone.Timestamp;
        }
        public RequestLogEntry()
        {
            Timestamp = DateTime.Now;
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string User { get; set; }
        public string Ip { get; set; }
        public string AspSessionId { get; set; }
        public string Path { get; set; }
        public string Method { get; set; }
        public string ContentType { get; set; }
        public int    RecentContributions { get; set; }

        [DisplayFormat(DataFormatString = "{0:ddd MMMM dd, H:mm}")]
        public DateTime Timestamp { get; set; }

    }
}