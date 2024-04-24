using System;
using System.Collections.Generic;
using System.Web;

namespace AeroCIOTWeb.Model
{
    public class Motorista_Ciot_Cancelado
    {
        public int Id_Ciot { get; set; }
        public string Cpf_Motorista { get; set; }
        public string Ciot { get; set; }
        public string Ciot_Protocolo_Cancelamento { get; set; }
        public DateTime? Dt_Cancelamento { get; set; }
        public int Tp_Cancelamento { get; set; }
        public string Usuario_Cancelamento { get; set; }
        public string Motivo_Cancelamento { get; set; }
        public string Retorno_Ndd { get; set; }
    }
}