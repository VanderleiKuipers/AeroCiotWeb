/*

*/

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using AeroCIOTWeb.dsCIOTTableAdapters;
using AeroCIOTWeb.DBO;
using AeroCIOTWeb.Model;
using System.Linq;
using NFeLibrary;

namespace AeroCIOTWeb
{
    public partial class AlterarCiotRol : System.Web.UI.Page
    {
        protected string msg = "Alterar CIOT";
        string CNPJ = "";
        string Token = "";        
        string Nome = "";
        string usuario = "";
        string Versao = "";  //ou 4.2.11.0
        string Ponto_Emissao = "";
        private string ID_CIOT = "";
        private string Cpf_Motorista = "";
        bool atualiza = true;
        CIOT dadosCiot;
        bool jaRegistrado = false;
        
        private bool jaExibiuMensagem = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!sessaoAtiva())
            {
                Response.Write("Sessão vencida ou usuário sem acesso!");
                Response.End();
            }

            ID_CIOT = Request.QueryString.Get("ciot");
            Cpf_Motorista = Request.QueryString.Get("cpf");

            DboCIOT dboCiot = new DboCIOT();

            var motoristaCiot = dboCiot.Get_Motorista_Ciot(ID_CIOT, Cpf_Motorista);

            var configCiot = dboCiot.Get_Config_CIOT();
            CNPJ = configCiot.Cnpj;
            Token = configCiot.Token;
            Versao = configCiot.Versao;
            Ponto_Emissao = configCiot.Ponto_Emissao; 

          
            int? idCTe = null;
            string cac = null;
            bool? isND = null;
            bool? tpComplemento = null;

            DataTable dtConfig = new getConfigCTETableAdapter().getData(
              Convert.ToInt32(1),
              null,
              ref idCTe,
              ref cac,
              ref isND,
              ref tpComplemento);

            if (dtConfig == null || dtConfig.Rows.Count == 0)
            {

                Response.Write("Configuração Certificado digital não encontrada!");
                Response.End();
            }
            else
            {
                Nome = Convert.ToString(dtConfig.Rows[0]["ds_CertificadoDigital"]);
            }

            if (!IsPostBack)
            {
                msg = "Alterar CIOT - " + motoristaCiot.Ciot;
                PreencheTelaCiot();           
            }            

        }

        protected void PreencheTelaCiot()
        {
            DateTime dtMiniDb;
            dtMiniDb = Convert.ToDateTime("01/01/1900");

            DboCIOT dboCiot = new DboCIOT();

            dadosCiot = dboCiot.get_Dados_Ciot(ID_CIOT, Cpf_Motorista);

            //txtNomeTerceiro.Text = dadosCiot.NomeTerceiro;
            //txtCPFTerceiro.Text = dadosCiot.CpfTerceiro;
            //txtIRRetido.Text = dadosCiot.IrRetido.ToString("n2");
            //txtContribuicaoINSS.Text = dadosCiot.ContribuicaoInss.ToString("n2");
            //txtINSSTransp.Text = dadosCiot.InssTransp.ToString("n2");
            //txtSaldo.Text = dadosCiot.Saldo.ToString("n2");
            //txtPlaca.Text = dadosCiot.Placa;
            txtAgencia.Text = dadosCiot.Agencia.ToString();
            txtContaCorrente.Text = dadosCiot.ContaCorrente.ToString();
            //txtDataPagamento.Text = dadosCiot.DataPagamento > dtMiniDb ? dadosCiot.DataPagamento.ToString("dd/MM/yyyy") : "";
            //txtDtpagamentoSaldo.Text = dadosCiot.DtPagamentosaldo > dtMiniDb ? dadosCiot.DtPagamentosaldo.ToString("dd/MM/yyyy") : "";
            //txtRendimentoBruto.Text = dadosCiot.RendimentoBruto.ToString("n2");
            //txtInicioOperacao.Text = dadosCiot.InicioOperacao > dtMiniDb ? dadosCiot.InicioOperacao.ToString("dd/MM/yyyy") : "";
            //txtTerminoOperacao.Text = dadosCiot.TerminoOperacao > dtMiniDb ? dadosCiot.TerminoOperacao.ToString("dd/MM/yyyy") : "";

            btnGravarCIOT.Enabled = true;

            DdLstBancos.DataSource = dboCiot.get_LstBancos();
            DdLstBancos.DataBind();

            DdLstBancos.SelectedValue = dadosCiot.CodigoBanco.ToString();
        }
      
        private void showWarning(string msg)
        {
            string str = msg.Replace('"', ' ').Replace("'", "").Replace('\n', ' ').Replace('\t', ' ').Replace("\n", "").Replace("\t", "").Replace("\r", "").Trim();

           
            body1.Attributes.Add("onload", "alert('" + msg + "')");
        }

        private void showError(string msg)
        {
            string str = msg.Replace('"', ' ').Replace("'", "").Replace('\n', ' ').Replace('\t', ' ').Replace("\n", "").Replace("\t", "").Replace("\r", "").Trim();

            //        body1.Attributes.Add("onload", "showError('" + msg + "')");
            body1.Attributes.Add("onload", "alert('" + msg + "')");
        }

        private void showInfo(string msg)
        {
            string str = msg.Replace('"', ' ').Replace("'", "").Replace('\n', ' ').Replace('\t', ' ').Replace("\n", "").Replace("\t", "").Replace("\r", "").Trim();

            //        body1.Attributes.Add("onload", "showHint('" + msg + "')");
            body1.Attributes.Add("onload", "alert('" + msg + "')");
        }

        protected void GerarAlteracaoCIOT()
        {
            string GUID = "";
            string strErro = "";

            DboCIOT dboCiot = new DboCIOT();

            GUID = dboCiot.Gera_GUID(CNPJ, Token);

            string strXMLAlterarCiot = dboCiot.GeraEnvio(CNPJ, Token, ref GUID, "Alterar", Versao);

            #region Gera XML e transmite para WSDL nddCargo

            //
            dadosCiot = dboCiot.get_Dados_Ciot(ID_CIOT, Cpf_Motorista);
            dadosCiot.CodigoBanco = Convert.ToInt32(DdLstBancos.SelectedValue.ToString());

            var dadosBanco = dboCiot.get_LstBancos().ToList();

            dadosCiot.NomeBanco = dadosBanco.FirstOrDefault(b => b.Cod_Banco == dadosCiot.CodigoBanco).Nome_Banco;

            dadosCiot.Agencia = Convert.ToInt32(txtAgencia.Text);
            dadosCiot.ContaCorrente = txtContaCorrente.Text;
            //
            DboAlterarCIOT alterarCiot = new DboAlterarCIOT();
            var configCiot = dboCiot.Get_Config_CIOT();

            var strXml = alterarCiot.Get_Xml_CIOT_Alterar_OT(configCiot, GUID, Convert.ToInt32(ID_CIOT), dadosCiot,txtMotivoAlteracao.Text);
                                       
            if (strXml == "")
            {
                showError("ID_CIOT: " + ID_CIOT.ToString() + " inválido!");
                return; 
            }

            int? idCTe = null;
            string cac = null;
            bool? isND = null;
            bool? tpComplemento = null;

            DataTable dtConfig = new getConfigCTETableAdapter().getData(
              Convert.ToInt32(1),
              null,
              ref idCTe,
              ref cac,
              ref isND,
              ref tpComplemento);

            if (dtConfig == null || dtConfig.Rows.Count == 0)
            {
                Response.Write("Configuração Certificado digital não encontrada!");
                Response.End();
            }
            else
            {
                Nome = Convert.ToString(dtConfig.Rows[0]["ds_CertificadoDigital"]);
            }

            XmlDocument xmlDoc = new XmlDocument();            
            xmlDoc.LoadXml(strXml);
            XmlNodeList ndOT = xmlDoc.GetElementsByTagName("alterarOT_envio");

            strXml = ndOT[0].InnerXml;

            strXml = getRemoteObjectExt("tcp://192.168.1.20:8087/NFeAPPrs").getXMLAssinado(strXml, "infOT", Nome, false);

            ndOT[0].InnerXml = strXml;

            strXml = xmlDoc.InnerXml;

            if (strXml.Substring(0, 3) == "NOK")
            {
                showError(strXml);
                return;
            }

            var NDD_Homologa = new AeroCIOTWeb.NDD_Homologa.ExchangeMessage();
            var NDD_Producao = new AeroCIOTWeb.NDD_Producao.ExchangeMessage();

            string retorno_OT = "";

            if (atualiza)
            {
                if (ConfigurationManager.AppSettings["ambiente"] == "producao")
                    retorno_OT = NDD_Producao.Send(strXMLAlterarCiot, strXml);
                else
                    retorno_OT = NDD_Homologa.Send(strXMLAlterarCiot, strXml);
            }

            //salvar histórico de envio do xml
            DateTime dtEnvio = DateTime.Now;
            dboCiot.SalvarXML("98", strXMLAlterarCiot, dtEnvio);   //Envio da alteração de parcela
            dboCiot.SalvarXML("99", strXml, dtEnvio);           //CIOT para para envio
            dboCiot.SalvarXML("00", retorno_OT, dtEnvio);       //Retorno do envio da 

            strErro = dboCiot.FindValueCIOT(retorno_OT.ToString(), "Body", "observacao");
            if (strErro != "")
            {
                showError(strErro);
                return;
            }

            string strXMLConsulta = dboCiot.GeraEnvio(CNPJ, Token, ref GUID, "Alterar", Versao);

            //Se não houver retorno, espera 5 segundos x 3
            int contador = 0;
            string retorno_Consultar = "";
            string strOT = "", strRetorno, strCIOT, strCodVerificador;
            string strprotocoloAlter, strDataAlteracao;

            if (atualiza)
            {
                while (contador <= 2)
                {
                    if (ConfigurationManager.AppSettings["ambiente"] == "producao")
                        retorno_Consultar = NDD_Producao.Send(strXMLConsulta, "");
                    else
                        retorno_Consultar = NDD_Homologa.Send(strXMLConsulta, "");

                    strOT = dboCiot.FindValueCIOT(retorno_Consultar.ToString(), "CrossTalk_Header", "retornoConsulta");
                    if (strOT == "")
                    {
                        System.Threading.Thread.Sleep(5000);
                        contador++;
                    }
                    else
                        break;
                }

                //salvar histórico de envio do xml
                dboCiot.SalvarXML("01",strXMLConsulta, DateTime.Now); //Consulta da Alteração
                dboCiot.SalvarXML("02", retorno_Consultar, DateTime.Now); //Retorno da consulta da Alteração

                if (strOT == "")
                {
                      jaExibiuMensagem = true;

                     showWarning("Retorno da alteração - Tente mais tarde.");
                     return;
                }
                else
                {
                    var motoristaCiot = dboCiot.Get_Motorista_Ciot(ID_CIOT, Cpf_Motorista);

                    Parcela_Adicional_OT parcela_Adicional_Ot = new Parcela_Adicional_OT();

                    strRetorno = dboCiot.FindValueCIOT(retorno_Consultar, "CrossTalk_Header", "ResponseCode");

                    if (strRetorno == "200")
                    {
                        XmlDocument oXML = new XmlDocument();
                        oXML.LoadXml(retorno_Consultar);
                        XmlNode root = oXML.DocumentElement;

                        XmlNamespaceManager nsmgr = new XmlNamespaceManager(oXML.NameTable);
                        nsmgr.AddNamespace("ndd", "http://www.nddigital.com.br/nddcargo");

                        XmlNodeList oNoLista = root.SelectNodes("CrossTalk_Body/retornoConsulta");

                        foreach (XmlNode oNo in oNoLista)
                        {
                            strCIOT = oNo.SelectSingleNode("ndd:cacelamento/ndd:autorizacao/ndd:ciot/ndd:numero", nsmgr).InnerText;
                            strCodVerificador = oNo.SelectSingleNode("ndd:cacelamento/ndd:autorizacao/ndd:ciot/ndd:ciotCodVerificador", nsmgr).InnerText;
                            strDataAlteracao = oNo.SelectSingleNode("ndd:cacelamento/ndd:dataHora", nsmgr).InnerText;

                            //salvar as informações
                            if (strCIOT.Trim() != "")
                            {
                                alterarCiot.GravaAlteracaoCiot(dadosCiot);
                            }
                        }
                    }                    
                }

                showInfo("CIOT Alterado.");
            }     

            #endregion

            //

        }


        public RemoteObject getRemoteObjectExt(string serverCTe)
        {
            /* Ornellas 01/11/2010
                * ESTOU USANDO REMOTING POIS O IIS RETORNAVA O SEGUINTE ERRO: 
                * Erro ao assinar digitalmente o arquivo XML: Erro: Problema ao acessar o certificado digitalm_safeCertContext é um manipulador inválido.
                */

            //select channel to communicate with server
            TcpChannel chan = new TcpChannel();

            foreach (TcpChannel creg in ChannelServices.RegisteredChannels)
            {
                if (creg.ChannelName.Equals(chan.ChannelName))
                {
                    jaRegistrado = true;
                    break;
                }
            }

            if (!jaRegistrado) ChannelServices.RegisterChannel(chan);


            RemoteObject remObject = (RemoteObject)Activator.GetObject(
                typeof(RemoteObject),
                //"tcp://192.168.1.80:8087/NFeAPPrs"
                serverCTe
                );

            return remObject;
        }


        protected void btnGravarCIOT_Click(object sender, EventArgs e)
        {
            //20220713
            jaExibiuMensagem = false;

            string retorno = "";

            if (txtMotivoAlteracao.Text == "")
                retorno = "Informe o motivo da alteração!";

            //if (!IsDate(txtTerminoOperacao.Text))
            //    retorno = "Data de Término da Operação inválida!";

            if (retorno != "")
            {
                showError(retorno);
                return;
            }

            GerarAlteracaoCIOT();

        }
  
        public static bool IsDate(Object obj)
        {
            string strDate = obj.ToString();
            try
            {
                DateTime dt = DateTime.Parse(strDate);
                if (dt != DateTime.MinValue && dt != DateTime.MaxValue)
                    return true;
                return false;
            }
            catch
            {
                return false;
            }
        }
  
        public DataTable getDtSessao()
        {
            DataTable dtSessao = null;

            try
            {
                if (dtSessao == null)
                    dtSessao = new getSessionTableAdapter().GetData(IDSessao());

                Session["dtSessao"] = dtSessao;
            }
            catch
            {
                Response.Redirect("http://www.aerosoftcargas.com.br");
                Response.End();
            }

            return dtSessao;
        }

        public string IDSessao()
        {

            string session = Request.QueryString.Get("session");
            //armazena ID de sessao ....
            Session["IDSession"] = session;

            return session;
        }

        public bool sessaoAtiva()
        {
            DataTable dt = getDtSessao();

            if (dt == null) return false;
            if (dt.Rows.Count == 0) return false;

            usuario = dt.Rows[0]["username"].ToString();

            //Checa o acesso para o CIOT
            string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(strConexao);

            SqlCommand cmd = new SqlCommand("dbo.getAcessoNivelMenuExt", new SqlConnection(strConexao));
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 999999;

            SqlParameter PSession = cmd.Parameters.Add("@session", SqlDbType.VarChar, 20);
            PSession.Value = Request.QueryString["session"].ToString();

            SqlParameter Pdesc_nivel1 = cmd.Parameters.Add("@desc_nivel1", SqlDbType.VarChar, 20);
            Pdesc_nivel1.Value = "CIOT";

            SqlParameter Pdesc_nivel2 = cmd.Parameters.Add("@desc_nivel2", SqlDbType.VarChar, 255);
            Pdesc_nivel2.Value = "";

            SqlParameter Pdesc_nivel3 = cmd.Parameters.Add("@desc_nivel3", SqlDbType.VarChar, 255);
            Pdesc_nivel3.Value = "";

            SqlParameter Pdesc_nivel4 = cmd.Parameters.Add("@desc_nivel4", SqlDbType.VarChar, 255);
            Pdesc_nivel4.Value = "";

            SqlParameter PenabledMenu = new SqlParameter();
            PenabledMenu.ParameterName = "@enabledMenu";
            PenabledMenu.SqlDbType = System.Data.SqlDbType.Char;
            PenabledMenu.Size = -1;
            PenabledMenu.Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(PenabledMenu).Direction = ParameterDirection.Output;

            if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();

            int intRetorno = cmd.ExecuteNonQuery();

            //20190610
            //        cmd.Dispose();
            //        conn.Dispose();

            if (PenabledMenu.Value.ToString() == "N")
                return false;

            //20190610 Verifica a permissão apenas de terceiro

            Pdesc_nivel2.Value = "Alterar CIOT";

            if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();

            intRetorno = cmd.ExecuteNonQuery();

            cmd.Dispose();
            conn.Dispose();

            return PenabledMenu.Value.ToString() == "S" ? true : false; 

        }

    }
}