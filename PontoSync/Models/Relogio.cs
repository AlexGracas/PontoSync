using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PontoSync.Models
{

    [Table(name: "RELOGIO", Schema = "FORPON_RELG")]
    public class Relogio
    {
        [Key]
        [Column(name:"ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column(name: "URL")]
        [DataType(DataType.Url)]
        public Uri URL { get; set; }

        [Column(name: "NOME")]
        [DataType(DataType.Text)]
        public String Nome { get; set; }

        [Column(name: "DESCRICAO")]
        public String Descricao { get; set; }

        [DataType(DataType.DateTime)]
        [Column(name: "ULTIMALEITURA")]
        public DateTime? UltimaLeitura { get; set; }

        [DataType(DataType.DateTime)]
        [Column(name: "ULTIMOSUCESSO")]
        public DateTime? UltimoSucesso { get; set; }

        [DataType(DataType.DateTime)]
        [Column(name: "ULTIMAFALHA")]
        public DateTime? UltimaFalha { get; set; }

        public ICollection<Registro> Registros { get; set; }

        [Column(name: "USUARIO")]
        public String Usuario { get; set; }
        
        [DataType(DataType.Password)]
        [Column(name: "SENHA")]
        public String Senha { get; set; }
    }
}
