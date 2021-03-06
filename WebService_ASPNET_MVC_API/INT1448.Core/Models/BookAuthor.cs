﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace INT1448.Core.Models
{
    [Table("BookAuthors")]
    public class BookAuthor
    {
        [Key]
        [Column(Order = 1)]
        [Required]
        public int BookID { set; get; }

        [Key]
        [Column(Order = 2)]
        [Required]
        public int AuthorID { set; get; }

        [ForeignKey("BookID")]
        public virtual Book Book { get; set; }

        [ForeignKey("AuthorID")]
        public virtual Author Author { get; set;}
    }
}
