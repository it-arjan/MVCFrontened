using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MVCFrontend.Models
{
    public class PostbackData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string MessageId { get; set; }
        public string UserName { get; set; }
        public DateTime Started { get; set; }

        public string Duration { get; set; }
        public string Content { get; set; }
    }
}