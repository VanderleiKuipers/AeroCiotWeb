using System;
using System.Collections.Generic;
using System.Web;

namespace AeroCIOTWeb.Model
{
    public class Parcela_Adicional_OT
    {
        public int Id_Ciot { get; set; }
        public string Ciot { get; set; }
        public string Cnpj { get; set; }
        public string Guid { get; set; }
        public DateTime Data_Previsao { get; set; }
        public double Valor_Aplicado { get; set; }
    }
}