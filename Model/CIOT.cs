using System;
using System.Collections.Generic;
using System.Web;

namespace AeroCIOTWeb.Model
{
    public class CIOT
    {
        public int Id_Ciot { get; set; }
        public string NomeTerceiro { get; set; }
        public string CpfTerceiro { get; set; }
        public DateTime DataPagamento { get; set; }
        public double RendimentoBruto { get; set; }
        public double IrRetido { get; set; }
        public double ContribuicaoInss { get; set; }
        public double InssTransp { get; set; }
        public int CodigoBanco { get; set; }
        public string NomeBanco { get; set; }
        public int Agencia { get; set; }
        public string ContaCorrente { get; set; }
        public double Saldo { get; set; }
        public DateTime DtPagamentosaldo { get; set; }
        public DateTime InicioOperacao { get; set; }
        public DateTime TerminoOperacao { get; set; }
        public string Usuario { get; set; }
        public string Placa { get; set; }
        public string CpfProprietario { get; set; }
        public string CnpjDestinatario { get; set; }
    }
}