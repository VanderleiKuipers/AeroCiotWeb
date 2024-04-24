/*
 RESPONSAVEL        SOLICITANTE     DATA                INDICE      COMENTARIOS
 ===============    =============   ==================  =========   ==============================================================================================================
 KUIPERS                                                            Criação...
 ORNELLAS                           30/08/2023 09:44    2023083001  Identificando usuário para aplicar controle de permissão...
 KUIPERS                            19/09/1024 12:30                Habilitado botão de recalculo         
 */
using AeroCIOTWeb.DBO;
using AeroCIOTWeb.dsCIOTTableAdapters;
using AeroCIOTWeb.Model;
using NFeLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

namespace AeroCIOTWeb
{
    public partial class AdicionarPagamento : System.Web.UI.Page
    {

        protected string msg = "Alterar Pagamento";
        string CNPJ = "";
        string Token = "";
        string ID_CIOT = "";
        string Cpf_Motorista = "";
        string Nome = "";
        bool atualiza = true;
        string Versao = "";
        bool jaRegistrado = false;
        private bool jaExibiuMensagem = false;
        string usuario = "";
        string session = "";

        protected void Page_Load(object sender, EventArgs e)
        {

            body1.Attributes.Remove("onload");

            ID_CIOT = Request.QueryString.Get("ciot");
            Cpf_Motorista = Request.QueryString.Get("cpf");

            DboCIOT dboCiot = new DboCIOT();
            var motoristaCiot = dboCiot.Get_Motorista_Ciot(ID_CIOT, Cpf_Motorista);

            session = Request.QueryString.Get("session");
            
            if (!dboCiot.sessaoAtiva(session, getDtSessao(), ref usuario,"Alterar valor CIOT"))
            {
                Response.Write("Sessão vencida ou usuário sem acesso!");
                Response.End();
            }

            if (motoristaCiot.Tp_Finalizado == 1)
            {
                Response.Write("O CIOT " + motoristaCiot.Ciot + ", encontra-se finalizado!");
                Response.End();
            }

            var configCiot = dboCiot.Get_Config_CIOT();
            CNPJ = configCiot.Cnpj;
            Token = configCiot.Token;
            Versao = configCiot.Versao;

            if (!IsPostBack) {
                msg = "Alterar valor do CIOT " + motoristaCiot.Ciot;
                preencherTela(motoristaCiot);
            }
                       
        }

        protected void btnAdicionarParcela_Click(object sender, EventArgs e)
        {
            DateTime dtaPrevisao;
            double vlrAplicado;
             
            try
            {
                dtaPrevisao = Convert.ToDateTime(txtdataPrevisao.Text);
            }
            catch(Exception )
            {
                showError("Informe a data de previsão de pagamento válida!");                
                return;
            }            
            
            try
            {
                vlrAplicado = Convert.ToDouble(txtValorAplicado.Text);
                var r = (1 / vlrAplicado);
            }
            catch(Exception)
            {
                showError("Informe o novo valor do frete!");
                return; 
            }

            var vlrCiot = new Valores_CIOT() { Rend_Bruto = Convert.ToDouble(txtVlrBruto.Text)};
            
            double result = 0;

            if(vlrCiot.Rend_Bruto != vlrAplicado)
            {
                if(vlrAplicado > vlrCiot.Rend_Bruto)
                {
                    result = (vlrAplicado - vlrCiot.Rend_Bruto);
                    showError("Novo valor é " + string.Format("{0:0.00}", result) + " maior quer o valor atual!");
                }
                else
                if(vlrAplicado < vlrCiot.Rend_Bruto)
                {
                    result = (vlrCiot.Rend_Bruto - vlrAplicado);
                    showError("Novo valor é " + string.Format("{0:0.00}", result) + " menor quer o valor atual!");                    
                }

            }

            DboCIOT dboCiot = new DboCIOT();

            var motoristaCiot = dboCiot.Get_Motorista_Ciot(ID_CIOT, Cpf_Motorista);       

            AdicionarParcelaOT(CNPJ, Token, motoristaCiot.Id_Fopag_Ciot_Item);
                                 
        }

        /// <summary>
        /// Método para Geração do XML, envio e gravação na base de dados 
        /// </summary>
        /// <param name="Cnpj">CNPJ da Contratante</param>
        /// <param name="Token">O token será usado para determinar quais os níveis de permissionamento podem ser aceitos. 
        ///                     Por exemplo: determinar um valor limite de operações de transporte por token.</param>
        /// <param name="Cv">Código verificador do CIOT</param>
        /// <param name="Data_Previsao">Data e hora para o pagamento da parcela</param>
        /// <param name="ValorAplicado">Valor aplicado da parcela, não levando em consideração os descontos.</param>
        private void AdicionarParcelaOT(string Cnpj, string Token, int Id_Fopag_CIOT )
        {   
            string GUID = "";
            string strErro = "";

            DboCIOT dboCiot = new DboCIOT();

            string strXMLAdParcela = dboCiot.GeraEnvio(CNPJ, Token, ref GUID, "Alterar", Versao);

            GUID = dboCiot.Gera_GUID(CNPJ, Token);
            DboAdicionarPagamento addPagamento = new DboAdicionarPagamento();

            //20230830
            string retMsg = null;

            Config_CIOT cfgCiot = new Config_CIOT();            
            cfgCiot.Cnpj = Cnpj;            
            cfgCiot.GuId = GUID;
            cfgCiot.Ponto_Emissao = "Matriz";
            cfgCiot.Token = Token;
            cfgCiot.Versao = Versao;
            cfgCiot.Id_Fopag_CIOT = Id_Fopag_CIOT;

            Valores_CIOT vlr_Ciot = new Valores_CIOT();

            vlr_Ciot.Rend_Bruto = Convert.ToDouble(txtValorAplicado.Text); 	
            vlr_Ciot.Rend_Bruto  = Convert.ToDouble(txtVlrBruto.Text); 	 	
	        vlr_Ciot.Data_Pagto = Convert.ToDateTime(txtdataPrevisao.Text);
            vlr_Ciot.Data_Pagto_Saldo = txtDtaPgtoSaldo.Text != "" ? Convert.ToDateTime(txtDtaPgtoSaldo.Text) : vlr_Ciot.Data_Pagto; 	 	
            vlr_Ciot.IR_Retido = Convert.ToDouble(txtIRr.Text); 	 			
            vlr_Ciot.Contribuicao_Inss = Convert.ToDouble(txtVlrInss.Text); 	 		
            vlr_Ciot.INSS_Transp = Convert.ToDouble(txtVlrInssTransp.Text); 	 	
            vlr_Ciot.Valor_ISS = Convert.ToDouble(txtVlrIss.Text); 	 		
            vlr_Ciot.Valor_Liquido = Convert.ToDouble(txtVlrLiquido.Text); 	 	
            vlr_Ciot.Outros_Desc = Convert.ToDouble(txtVlrOutroDesc.Text); 	 	
            vlr_Ciot.Propaganda = Convert.ToDouble(txtVlrPropaganda.Text); 	 	
            vlr_Ciot.Saldo = Convert.ToDouble(txtVlrSaldo.Text); 	 		

            var session = Request.QueryString.Get("session");

            var strXml = addPagamento.getXMLCIOT_Alterar_Valor_Aplicado_OT(cfgCiot, vlr_Ciot, session, ref retMsg);

            //2023083001
            if (retMsg != null && !retMsg.Equals("")) {

                showError(retMsg);
                goto Fim;

            }

            if (strXml == "")
            {
                showError("ID_CIOT: " + ID_CIOT.ToString() + " inválido!");
                goto Fim;
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

            string ativarApi = Request.QueryString.Get("api");

            if (ativarApi == null || ativarApi.Equals(""))
            {
                ativarApi = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["ATIVAR_MICRO_SERVICO_API"]);
            }


            //20230720
            if (ativarApi != null && ativarApi.Equals("1"))
            {
               strXml = dboCiot.getXMLCIOTAssinadoAPI(cfgCiot.Id_Fopag_CIOT, strXml, session);
            }
            else
            {
                //20220906
                string serverAssinaturaDigital = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["SERVER_ASSINATURA_DIGITAL"]);

                strXml = getRemoteObjectExt(serverAssinaturaDigital).getXMLAssinado(strXml, "infOT", Nome /*20200715 false*/, false /*20210308*/ );

            }


            ndOT[0].InnerXml = strXml;

            strXml = xmlDoc.InnerXml;

            if (strXml.Substring(0, 3) == "NOK")
            {
                showError(strXml);
                goto Fim;
            }

            var NDD_Homologa = new AeroCIOTWeb.NDD_Homologa.ExchangeMessage();
            var NDD_Producao = new AeroCIOTWeb.NDD_Producao.ExchangeMessage();

            string retorno_OT = "";

            if (atualiza)
            {
                if (ConfigurationManager.AppSettings["ambiente"] == "producao")
                    retorno_OT = NDD_Producao.Send(strXMLAdParcela, strXml);
                else
                    retorno_OT = NDD_Homologa.Send(strXMLAdParcela, strXml);
            }

            //salvar histórico de envio do xml
            DateTime dtEnvio = DateTime.Now;
            dboCiot.SalvarXML("98", strXMLAdParcela, dtEnvio);   //Envio da alteração de parcela
            dboCiot.SalvarXML("99", strXml, dtEnvio);           //CIOT para para envio
            dboCiot.SalvarXML("00", retorno_OT, dtEnvio);       //Retorno do envio da 

            strErro = dboCiot.FindValueCIOT(retorno_OT.ToString(), "Body", "observacao");
            if (strErro != "")
            {
                showError(strErro);
                goto Fim;
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
                    goto Fim;
                }
                else
                {
                                        
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
                                Motorista_Ciot motoristaCiot = dboCiot.Get_Motorista_Ciot(ID_CIOT, Cpf_Motorista);
                                dboCiot.Atualiza_Valores_Ciot(motoristaCiot.Id_Fopag_Ciot_Item, vlr_Ciot);                              

                            }
                        }
                    }                    
                }

                showInfo("CIOT(s) Alterados(s).");

            }

        Fim:
            return;            
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

        private void preencherTela(Motorista_Ciot motoristaCiot)
        {
            DboCIOT dboCiot = new DboCIOT();
            var dados_Ciot = dboCiot.get_ValoresCiot(motoristaCiot.Id_Fopag_Ciot_Item);

            txtdataPrevisao.Text = dados_Ciot.Data_Pagto.ToShortDateString();
            txtValorAplicado.Text = string.Format("{0:0.00}", dados_Ciot.Rend_Bruto);
            txtVlrBruto.Text = string.Format("{0:0.00}", dados_Ciot.Rend_Bruto);
            txtDtaPgtoSaldo.Text = dados_Ciot.Data_Pagto_Saldo.ToString();
            txtIRr.Text = string.Format("{0:0.00}", dados_Ciot.IR_Retido);
            txtVlrInss.Text = string.Format("{0:0.00}", dados_Ciot.Contribuicao_Inss);
            txtVlrInssTransp.Text = string.Format("{0:0.00}", dados_Ciot.INSS_Transp);
            txtVlrIss.Text = string.Format("{0:0.00}", dados_Ciot.Valor_ISS);
            txtVlrLiquido.Text = string.Format("{0:0.00}", dados_Ciot.Valor_Liquido);
            txtVlrOutroDesc.Text = string.Format("{0:0.00}", dados_Ciot.Outros_Desc);
            txtVlrPropaganda.Text = string.Format("{0:0.00}", dados_Ciot.Propaganda);
            txtVlrSaldo.Text = string.Format("{0:0.00}", dados_Ciot.Saldo);

        }

        protected void btnCalcular_Click(object sender, EventArgs e)
        {

            DboCIOT dboCiot = new DboCIOT();

            var vlrsCiot = new Valores_CIOT();

            vlrsCiot.Rend_Bruto         =   Convert.ToDouble(txtValorAplicado.Text);
            if (txtDtaPgtoSaldo.Text != "")
                vlrsCiot.Data_Pagto_Saldo	 = 	Convert.ToDateTime(txtDtaPgtoSaldo.Text); 	
            vlrsCiot.IR_Retido          =   Convert.ToDouble(txtIRr.Text); 			
            vlrsCiot.Contribuicao_Inss  =   Convert.ToDouble(txtVlrInss.Text); 		
            vlrsCiot.INSS_Transp        =   Convert.ToDouble(txtVlrInssTransp.Text); 	
            vlrsCiot.Valor_ISS          =   Convert.ToDouble(txtVlrIss.Text); 			
            vlrsCiot.Valor_Liquido      =   Convert.ToDouble(txtVlrLiquido.Text); 		
            vlrsCiot.Outros_Desc        =   Convert.ToDouble(txtVlrOutroDesc.Text); 	
            vlrsCiot.Propaganda =  Convert.ToDouble(txtVlrPropaganda.Text);
            vlrsCiot.Saldo = Convert.ToDouble(txtVlrSaldo.Text); 		

            var vlrsCalc_Ciot = dboCiot.getCalcValoresCiot(Cpf_Motorista, vlrsCiot);

            if (vlrsCalc_Ciot.Ret_Msg != null)
             {
                 showError(vlrsCalc_Ciot.Ret_Msg);
                 return;
             }

            //txtdataPrevisao.Text = dados_Ciot.Data_Pagto.ToShortDateString();
            txtValorAplicado.Text = string.Format("{0:0.00}", vlrsCalc_Ciot.Rend_Bruto);
            txtVlrBruto.Text = string.Format("{0:0.00}", vlrsCalc_Ciot.Rend_Bruto);
            txtDtaPgtoSaldo.Text = vlrsCalc_Ciot.Data_Pagto_Saldo.ToString();
            txtIRr.Text = string.Format("{0:0.00}", vlrsCalc_Ciot.IR_Retido);
            txtVlrInss.Text = string.Format("{0:0.00}", vlrsCalc_Ciot.Contribuicao_Inss);
            txtVlrInssTransp.Text = string.Format("{0:0.00}", vlrsCalc_Ciot.INSS_Transp);
            txtVlrIss.Text = string.Format("{0:0.00}", vlrsCalc_Ciot.Valor_ISS);
            txtVlrLiquido.Text = string.Format("{0:0.00}", vlrsCalc_Ciot.Valor_Liquido);
            txtVlrOutroDesc.Text = string.Format("{0:0.00}", vlrsCalc_Ciot.Outros_Desc);
            txtVlrPropaganda.Text = string.Format("{0:0.00}", vlrsCalc_Ciot.Propaganda);
            txtVlrSaldo.Text = string.Format("{0:0.00}", vlrsCalc_Ciot.Saldo);
        }

    }
}