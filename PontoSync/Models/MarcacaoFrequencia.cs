using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PontoSync.Models
{
    [Table(name: "FRQ_MARCACAO", Schema = "SRH2")]
    public class MarcacaoFrequencia
    {
        public MarcacaoFrequencia() { }

        public MarcacaoFrequencia(int id, Registro registro, String Matricula)
        {
            DataMarcacao = registro.Marcacao.Date;
            HoraMarcacao = registro.Marcacao.ToString("HH:mm");
            MatServidor = Matricula;
            Dispositivo = registro.Relogio.Nome;
            TipoOrigem = "REG";
            Cracha = Matricula;
            Usuario = Matricula;
            StatusMarcacao = "DEF";
            Id = id;
        }
        [Column(name: "SQ_MARCACAO")]
        public int Id { get; set; }

        [Column(name: "MAT_SERVIDOR")]
        [Display(Name ="Matrícula")]
        public String MatServidor { get; set; }

        [Column(name: "DT", TypeName = "Date")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        [DataType(DataType.Date)]
        [Display(Name = "Data")]
        public DateTime DataMarcacao{get;set;}

        [Column(name: "NR_CRACHA")]
        public string Cracha { get; set; }

        [Column(name: "MARCACAO")]
        [Display(Name = "Hora")]
        public String HoraMarcacao { get; set; }


        [Column(name: "NR_DISPOSITIVO")]
        [Display(Name = "Nome Dispositivo")]
        public String Dispositivo { get; set; }

        /// <summary>
        /// Status da Marcação (Deferido-DEF, Solicitada-SOL, Indeferido-IND)
        /// </summary>
        [Column(name: "ST_MARCACAO")]
        [Display(Name = "Status")]
        public String StatusMarcacao { get; set; }

        /// <summary>
        /// Origem (Cadastro Manual pela SEREF-MAN, Importada-IMP, Registrado-REG, Digitada - DIG, Abono - ABO)
        /// </summary>
        [Column(name: "TP_ORIGEM")]
        [Display(Name = "Tipo Origem",Description = "Cadastro Manual pela SEREF-MAN, Importada-IMP, Registrado-REG, Digitada - DIG, Abono - ABO")]
        public String TipoOrigem { get; set; }

        [Display(Name = "Código Importação")]
        [Column(name: "SQ_IMPORTACAO")]
        public int? Sq_Importacao { get; set; }

        [Column(name: "USUARIO")]
        [Display(Name = "Usuário")]
        public String Usuario { get; set; }
    }
}
