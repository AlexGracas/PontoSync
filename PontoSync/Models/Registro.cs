using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PontoSync.Models
{
    [Table(name: "MARCACAO", Schema = "FORPON_RELG")]
    public class Registro
    {
        [Key]
        [Column(name:"ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Chave no relógio do registro. Poderá se repetir entre diferentes relógios.
        /// </summary>
        [Column(name: "ID_MARCACAO_RELOGIO")]
        public String idMarcacaoRelogio {get;set;}
        [Column(name: "MATRICULA")]
        public String Matricula { get; set; }

        [Column(name: "MARCACAO")]
        public DateTime Marcacao { get; set; }

        [Column(name: "ID_RELOGIO")]
        public int IdRelogio { get; set; }

        [ForeignKey("IdRelogio")]
        [JsonIgnore]
        public Relogio Relogio { get; set; }
        
        [NotMapped]
        public bool Migrado
        {
            get { return MigradoFrequencia == "T"; }
            set { MigradoFrequencia = value ? "T" : "F"; }
        }

        [Column(name: "MIGRADO_FREQUENCIA")]
        public string MigradoFrequencia { get; set; } = "F";

    }
}
