using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PontoSync.Models
{
    [Table("SERVIDOR",Schema ="SRH2")]
    public class Servidor
    {
        [Column("MAT_SERVIDOR")]
        [Key]
        public String Matricula { get; set; }

        [Column("NOM")]
        public String Nome { get; set; }

        [Column("E_MAIL")]
        public String Email { get; set; }
    }
}
