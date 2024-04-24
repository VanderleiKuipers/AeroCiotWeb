using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AeroCIOTWeb.Model
{
    public class Valores_CIOT
    {
        public int id_Fopag_Ciot_Item { get; set; }
        public DateTime Data_Pagto {get; set;}
        public double Rend_Bruto {get; set;}
        public double Contribuicao_Inss {get; set;}
        public double IR_Retido {get; set;}
        public double Valor_ISS {get; set;}
        public double Outros_Desc {get; set;}
        public double INSS_Transp {get; set;}
        public double Valor_Liquido {get; set;}
        public double Propaganda {get; set;}
        public double Total {get; set;}
        public double Saldo {get; set;}
        public double Vl_Pedagio { get; set; }
        public DateTime? Data_Pagto_Saldo { get; set; }
        public string Ret_Msg { get; set; }
    }
}