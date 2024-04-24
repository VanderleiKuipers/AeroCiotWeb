/*
 KUIPERS    DEBORAH                  19/03/2024 08:40    20240316    CORREÇÕES NO ENVIO DO CANCELAMENTO DE CIOT
 */
using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Data.SqlClient;
using AeroCIOTWeb.dsCIOTTableAdapters;
using System.Xml;
using System.Text;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using NFeLibrary;
using System.Net;
using AeroCIOTWeb.DBO;
using AeroCIOTWeb.Model;

namespace AeroCIOTWeb
{
    public partial class CancelarCIOT : System.Web.UI.Page
    {
        protected string msg = "Cancelar CIOT";
        string Versao = "";  //ou 4.2.11.0
        private string ID_CIOT = "";
        private string Cpf_Motorista = "";        
        string CNPJ = "";
        string Token = "";
        string Nome = "";
        string usuario = "";
        bool atualiza = true;
        private bool jaExibiuMensagem = false;
        string session = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            DboCIOT dboCiot = new DboCIOT();

            ID_CIOT = Request.QueryString.Get("ciot");
            Cpf_Motorista = Request.QueryString.Get("cpf");

            session = Request.QueryString.Get("session");
            
            if (!dboCiot.sessaoAtiva(session, getDtSessao(), ref usuario, "Cancelar CIOT"))
            {
                Response.Write("Sessão vencida ou usuário sem acesso!");
                Response.End();
            }

            var motoristaCiot = dboCiot.Get_Motorista_Ciot(ID_CIOT, Cpf_Motorista);            

            var configCiot = dboCiot.Get_Config_CIOT();
            CNPJ = configCiot.Cnpj;
            Token = configCiot.Token;
            Versao = "4.2.11.0"; //configCiot.Versao;

            if (!IsPostBack)
            {
                msg = "Cancelar CIOT " + motoristaCiot.Ciot;                
            }
        }

        /// <summary>
        /// Método para validar sé o CIOT esta vinculado a algum MDF-e ou ROL
        /// </summary>
        /// <param name="ID_CIOT">ID do CIOT da tabla AEROSOFT.dbo.MOTORISTA_CIOT </param>
        /// <returns>True our False</returns>
        private bool permiteCancelarCIOT(int ID_CIOT)
        {

            bool ret = true;

            string retMsg = null;

            new QueriesTableAdapter().Stored_Permite_Cancelar_CIOT(ID_CIOT, ref retMsg);

            if (retMsg != null && !retMsg.Equals(""))
            {

                ret = false;

                showError(retMsg);
            }

            return ret;
        }

        protected void btnCandelar_Click(object sender, EventArgs e)
        {
            CancelarCIOTEnv(Convert.ToInt32(ID_CIOT), Versao, txtMotivoCancelamento.Text.ToString());            
        }
        
        /// <summary>
        /// Método para gerar, enviar e gravar informações do cancelamento do CIOT
        /// </summary>
        /// <param name="ID">ID da tabela AEROSOFT.dbo.MOTORISTA_CIOT</param>
        /// <param name="Versao">Versão da integração que está sendo realizada.</param>
        /// <param name="motivoCancelamento">Motivo pelo qual está sendo solicitado o cancelamento da Operação de Transporte.</param>
        private void CancelarCIOTEnv(int ID, string Versao, string motivoCancelamento)
        {
            if (!permiteCancelarCIOT(ID)) return;

            string GUID = "";
            string strErro = "";

            DboCIOT dboCiot = new DboCIOT();

            GUID = dboCiot.Gera_GUID(CNPJ, Token);

            var configCiot = dboCiot.Get_Config_CIOT();
            
            string  strXmHeader = dboCiot.GeraEnvio(CNPJ, Token, ref GUID, "Cancelar", Versao);
                        
           #region Gera XML e transmite para WSDL nddCargo

            var strXMLCancelar = dboCiot.Get_Xml_CIOT_Cancelar_OT(configCiot, txtMotivoCancelamento.Text, GUID, ID);

            strXMLCancelar = dboCiot.getXMLCancelaCIOTAssinadoAPI(ID, strXMLCancelar, session);

            //string serverAssinaturaDigital = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["SERVER_ASSINATURA_DIGITAL"]);            
            //strXml = getRemoteObjectExt(serverAssinaturaDigital).getXMLAssinado(strXml, "cancelarOT_envio", Nome, false);

            if (strXmHeader.Substring(0, 3) == "NOK")
            {
                showError(strXmHeader);
                goto Fim;
            }

            var NDD_Homologa = new AeroCIOTWeb.NDD_Homologa.ExchangeMessage();
            var NDD_Producao = new AeroCIOTWeb.NDD_Producao.ExchangeMessage();

            string retorno_OT = "";

            if (atualiza)
            {
                if (ConfigurationManager.AppSettings["ambiente"] == "producao")
                    retorno_OT = NDD_Producao.Send(strXmHeader, strXMLCancelar);
                else
                    retorno_OT = NDD_Homologa.Send(strXmHeader, strXMLCancelar);
            }

            //salvar histórico de envio do xml
            DateTime dtEnvio = DateTime.Now;
            dboCiot.SalvarXML("93", strXMLCancelar, dtEnvio);   //Envio do Cancelamento
            dboCiot.SalvarXML("94", strXmHeader, dtEnvio);           //CIOT para cancelar
            dboCiot.SalvarXML("95", retorno_OT, dtEnvio);       //Retorno do envio do Cancelamento

            strErro = dboCiot.FindValueCIOT(retorno_OT.ToString(), "Body", "observacao");
            if (strErro != "")
            {
                showError(strErro);
                goto Fim;
            }

            string strXMLConsulta = dboCiot.GeraEnvio(CNPJ, Token, ref GUID, "ConsultaCancelamento", Versao);

            //Se não houver retorno, espera 5 segundos x 3
            int contador = 0;
            string retorno_Consultar = "";
            string strOT = "", strRetorno, strCIOT, strCodVerificador;            
            string strprotocoloCanc, strDataCancelamento;
            XmlDocument oXML = new XmlDocument();
            

            if (atualiza)
            {
                while (contador <= 2)
                {
                    if (ConfigurationManager.AppSettings["ambiente"] == "producao")
                        retorno_Consultar = NDD_Producao.Send(strXMLConsulta, "");
                    else
                        retorno_Consultar = NDD_Homologa.Send(strXMLConsulta, "");

                    strOT = dboCiot.FindValueCIOT(retorno_Consultar.ToString(), "CrossTalk_Header", "retornoConsultaCancelamento");
                    if (strOT == "")
                    {
                        System.Threading.Thread.Sleep(5000);
                        contador++;
                    }
                    else
                        break;
                }

                if (retorno_Consultar.Length > 0)
                {                    
                    oXML.LoadXml(retorno_Consultar);                    
                }

            #endregion

                //salvar histórico de envio do xml
                dboCiot.SalvarXML("96", strXMLConsulta, DateTime.Now); //Consulta do Cancelamento
                dboCiot.SalvarXML("97", retorno_Consultar, DateTime.Now); //Retorno da consulta do Cancelamento

                #region Grava retorno na base de dados

                if (strOT == "")
                {
                    jaExibiuMensagem = true;
                    showWarning("Retorno do Cancelamento - Tente mais tarde.");

                    goto Fim;
                }
                else
                {
                    strRetorno = dboCiot.FindValueCIOT(retorno_Consultar, "CrossTalk_Header", "ResponseCode");
                    if (strRetorno == "200")
                    {

                        XmlNamespaceManager nsmgr = new XmlNamespaceManager(oXML.NameTable);
                        nsmgr.AddNamespace("ndd", "http://www.nddigital.com.br/nddcargo");
                        
                        XmlNode root = oXML.DocumentElement;
                        XmlNodeList oNoLista = root.SelectNodes("CrossTalk_Body/retornoConsultaCancelamento");
                        XmlNodeList retorno = oXML.GetElementsByTagName("retornoConsultaCancelamento");

                        string msg = retorno[0]["mensagens"]["mensagem"]["observacao"].InnerText;                        
                        strCIOT = retorno[0]["cancelamento"]["autorizacao"]["ciot"]["numero"].InnerText;
                        strCodVerificador = retorno[0]["cancelamento"]["autorizacao"]["ciot"]["ciotCodVerificador"].InnerText;
                        strDataCancelamento = retorno[0]["cancelamento"]["dataHora"].InnerText;
                        strprotocoloCanc = retorno[0]["cancelamento"]["protocoloCanc"].InnerText;
                        
                        //salvar as informações
                        if (strCIOT.Trim() != "")
                        {
                            Motorista_Ciot_Cancelado mto_Ciot_Canc = new Motorista_Ciot_Cancelado();

                            mto_Ciot_Canc.Id_Ciot = Convert.ToInt32(ID_CIOT);
                            mto_Ciot_Canc.Cpf_Motorista = Cpf_Motorista;
                            mto_Ciot_Canc.Ciot = strCIOT; 
                            mto_Ciot_Canc.Ciot_Protocolo_Cancelamento = strprotocoloCanc;
                            mto_Ciot_Canc.Dt_Cancelamento = Convert.ToDateTime(strDataCancelamento);
                            mto_Ciot_Canc.Tp_Cancelamento = 0;
                            mto_Ciot_Canc.Usuario_Cancelamento = usuario;
                            mto_Ciot_Canc.Motivo_Cancelamento = motivoCancelamento;
                            mto_Ciot_Canc.Retorno_Ndd = msg;

                            dboCiot.Grava_CancelamentoCIOT(mto_Ciot_Canc);

                        }
                    }
                    else
                    {
                        //20220211 Identificando CIOT cancelado na NDD para cancelada o mesmo em nosso sistema...
                        //if ((strOT.Contains("Operação de Transporte já esta cancelada") && strOT.Contains("já está cancelada")) || strOT.Contains("está encerrada"))1
                        {
                            XmlNodeList retorno = oXML.GetElementsByTagName("retornoConsultaCancelamento");
                            string msg = retorno[0]["mensagens"]["mensagem"]["observacao"].InnerText;         
                          //  strCIOT = retorno[0]["cancelamento"]["autorizacao"]["ciot"]["numero"].InnerText;
                          //  strprotocoloCanc = retorno[0]["cancelamento"]["protocoloCanc"].InnerText;
                          //  strDataCancelamento = retorno[0]["cancelamento"]["dataHora"].InnerText;

                            Motorista_Ciot_Cancelado mto_Ciot_Canc = new Motorista_Ciot_Cancelado();

                            mto_Ciot_Canc.Id_Ciot = Convert.ToInt32(ID_CIOT); 
                            mto_Ciot_Canc.Cpf_Motorista = Cpf_Motorista;
                            mto_Ciot_Canc.Ciot = "";
                            mto_Ciot_Canc.Ciot_Protocolo_Cancelamento = "";
                            //mto_Ciot_Canc.Dt_Cancelamento = Convert.ToDateTime(strDataCancelamento);
                            mto_Ciot_Canc.Tp_Cancelamento = 0;
                            mto_Ciot_Canc.Usuario_Cancelamento = usuario;
                            mto_Ciot_Canc.Motivo_Cancelamento = motivoCancelamento;
                            mto_Ciot_Canc.Retorno_Ndd = msg;

                            dboCiot.Grava_CancelamentoCIOT(mto_Ciot_Canc);

                        }
                        showError(strOT);
                        goto Fim;
                    }
                }
                #endregion
                
                showInfo("CIOT(s) cancelados(s).");
            }

        Fim:
            return;
        }

        public RemoteObject getRemoteObjectExt(string serverCTe)
        {
            /* Ornellas 01/11/2010
                * ESTOU USANDO REMOTING POIS O IIS RETORNAVA O SEGUINTE ERRO: 
                * Erro ao assinar digitalmente o arquivo XML: Erro: Problema ao acessar o certificado digitalm_safeCertContext é um manipulador inválido.
                */

            //select channel to communicate with server
            TcpChannel chan = new TcpChannel();
            bool jaRegistrado = false;
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
        
        private void showError(string msg)
        {
            string str = msg.Replace('"', ' ').Replace("'", "").Replace('\n', ' ').Replace('\t', ' ').Replace("\n", "").Replace("\t", "").Replace("\r", "").Trim();
            body1.Attributes.Add("onload", "alert('" + str + "')");
        }

        private void showInfo(string msg)
        {
            string str = msg.Replace('"', ' ').Replace("'", "").Replace('\n', ' ').Replace('\t', ' ').Replace("\n", "").Replace("\t", "").Replace("\r", "").Trim();
            body1.Attributes.Add("onload", "alert('" + str + "')");
        }

        private void showWarning(string msg)
        {
            string str = msg.Replace('"', ' ').Replace("'", "").Replace('\n', ' ').Replace('\t', ' ').Replace("\n", "").Replace("\t", "").Replace("\r", "").Trim();
            body1.Attributes.Add("onload", "alert('" + msg + "')");
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
                Response.Redirect("https://www.aerosoftcargas.com.br/login");
                Response.End();
            }

            return dtSessao;
        }

        public string IDSessao()
        {
            session = Request.QueryString.Get("session");
            //armazena ID de sessao ....
            Session["IDSession"] = session;

            return session;
        }
        
    }
}