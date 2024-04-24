using System;
using System.Collections.Generic;
using System.Web;

namespace AeroCIOTWeb.Model
{
    public class Config_CIOT
    {
        public string Cnpj { get; set; }
        public string Token { get; set; }
        public string Versao { get; set; }
        public string Ponto_Emissao { get; set; }
        public string GuId { get; set; }
        public int Id_Fopag_CIOT { get; set; }
    }
}