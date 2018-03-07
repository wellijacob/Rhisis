using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Rhisis.Database.Interfaces;

namespace Rhisis.Database.Structures
{
    [Table("friends")]
    public class Friend : IDatabaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int CharacterId { get; set; }

        public int FriendId { get; set; }
    }
}
