/*
EVERTON       MARCOS.R        17/11/2020 14:12    20201117    INCLUIR NOVOS CAMPOS
KUIPERS       DEBORAH         28/08/2023 10:11    20230828    Alterada a versão 4.2.8.0 para 4.2.11.0, Implementado envio de dados de pagadoria e cartao, para pagamento do motorista 
*/

using NFeLibrary;
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

namespace AeroCIOTWeb
{
    public partial class AeroCiotRolManifesto : System.Web.UI.Page
    {
        string CNPJ = "01014373000265";
        string Token = "012345678901234567891234";
        string Ponto_Emissao = "Matriz";
        string Nome = "";//"CN=AEROSOFT CARGAS AEREAS LTDA:01014373000184, OU=Autenticado por AR Certifique Online, OU=RFB e-CNPJ A1, OU=Secretaria da Receita Federal do Brasil - RFB, L=Sao Paulo, S=SP, O=ICP-Brasil, C=BR";
        string usuario = "";
        SqlConnection conn = null;
        bool atualiza = true;
        bool apenasTerceiro = false;
        //20190705
        //20230828
        string Versao = "4.2.11.0";//"4.2.8.0";  //ou 4.2.11.0
        bool FinalizarAnterior = false;
        string Romaneio = "";
        string GRP_Romaneio = "";
        string Manifesto = "";


        //20220713 
        private bool jaExibiuMensagem = false;

        protected void Page_Load(object sender, EventArgs e)
        {
            body1.Attributes.Remove("onload");

           

            if (!sessaoAtiva())
            {
                Response.Write("Sessão vencida ou usuário sem acesso!");
                Response.End();
            }


            //20201113
          
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


            if (!IsPostBack  && Request.QueryString.Get("GrpRol") != "" && Request.QueryString.Get("GrpRol") !=null)
            {
                GRP_Romaneio = Request.QueryString.Get("GrpRol");
                
                Romaneio = Request.QueryString.Get("NumROl");

                

                string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;

                SqlCommand cmd = new SqlCommand("dbo.getMotoristaByRol_ext", new SqlConnection(strConexao));

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.CommandTimeout = 999999;

                SqlParameter Pgrupo_rol = cmd.Parameters.Add("@grupo_rol", SqlDbType.Int);

                Pgrupo_rol.Value = Convert.ToInt32(GRP_Romaneio);

                SqlParameter Pnumero_rol = cmd.Parameters.Add("@numero_rol", SqlDbType.Int);

                Pnumero_rol.Value = Convert.ToInt32(Romaneio);

               
                if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();

                SqlDataAdapter da = new SqlDataAdapter();

                DataTable dtRol  = new DataTable();

                da.SelectCommand = cmd;
                da.Fill(dtRol);

                if (dtRol.Rows.Count > 0)
                {
                    btnGerarCIOTTerceiro.Enabled = true; 
                    txtNomeTerceiro.Text =dtRol.Rows[0]["NOME"].ToString();
                     txtCPFTerceiro.Text=dtRol.Rows[0]["CPF"].ToString() ;
                     txtNrRol.Text = Romaneio;
                     txtGrpRol.Text =GRP_Romaneio;


                     txtIRRetido.Text = dtRol.Rows[0]["VL_IRRF"].ToString();
                     txtContribuicaoINSS.Text = dtRol.Rows[0]["VL_INSS"].ToString();
                     txtINSSTransp.Text = dtRol.Rows[0]["VL_SEST_SENAT"].ToString();

                     txtPlaca.Text = dtRol.Rows[0]["PLACA"].ToString();
                     txtNomeProprietario.Text = dtRol.Rows[0]["NOME_PROPRIETARIO"].ToString();
                     txtCPFProprietario.Text = dtRol.Rows[0]["CPF_PROPRIETARIO"].ToString();
                     txtCodigoBanco.Text = dtRol.Rows[0]["COD_BANCO"].ToString();
                     txtNomeBanco.Text = dtRol.Rows[0]["DS_BANCO"].ToString();
                     txtAgencia.Text = dtRol.Rows[0]["AGENCIA"].ToString();
                     txtContaCorrente.Text = dtRol.Rows[0]["CONTA"].ToString();
                     //20201117
                     txtDataPagamento.Text = dtRol.Rows[0]["DATA_PAGAMENTO"].ToString();
                     txtRendimentoBruto.Text = dtRol.Rows[0]["CUSTO"].ToString();
                     txtDtpagamentoSaldo.Text = dtRol.Rows[0]["DATA_PAGAMENTO"].ToString();
                     txtInicioOperacao.Text = dtRol.Rows[0]["EMISSAO_STR"].ToString();
                     txtTerminoOperacao.Text = dtRol.Rows[0]["DT_PREV_CHEGADA_STR"].ToString();

                }
                else
                {
                    Response.Write("Rol/Manifesto não encontrado.");
                    Response.End();
                }


                dtRol.Dispose();


            }


            if (!IsPostBack && Request.QueryString.Get("AWB") != "" && Request.QueryString.Get("AWB") != null)
            {


                GRP_Romaneio = Request.QueryString.Get("TRANSP");

                Romaneio = Request.QueryString.Get("AWB");



                string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;

                SqlCommand cmd = new SqlCommand("dbo.getMotoristaByAWB", new SqlConnection(strConexao));

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.CommandTimeout = 999999;

                SqlParameter Pgrupo_rol = cmd.Parameters.Add("@transp", SqlDbType.VarChar);

                Pgrupo_rol.Value = GRP_Romaneio;

                SqlParameter Pnumero_rol = cmd.Parameters.Add("@AWB", SqlDbType.VarChar);


                Pnumero_rol.Value = Romaneio;


                if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();

                SqlDataAdapter da = new SqlDataAdapter();

                DataTable dtRol = new DataTable();

                da.SelectCommand = cmd;
                da.Fill(dtRol);

                if (dtRol.Rows.Count > 0)
                {
                    btnGerarCIOTTerceiro.Enabled = true;
                    txtNomeTerceiro.Text = dtRol.Rows[0]["NOME"].ToString();
                    txtCPFTerceiro.Text = dtRol.Rows[0]["CPF"].ToString();
                    txtNrRol.Text = Romaneio;
                    txtGrpRol.Text = GRP_Romaneio;
                    txtIRRetido.Text = dtRol.Rows[0]["VL_IRRF"].ToString();
                    txtContribuicaoINSS.Text = dtRol.Rows[0]["VL_INSS"].ToString();
                    txtINSSTransp.Text = dtRol.Rows[0]["VL_SEST_SENAT"].ToString();
                    txtPlaca.Text = dtRol.Rows[0]["PLACA_VEICULO"].ToString();
                    txtNomeProprietario.Text = dtRol.Rows[0]["NOME_PROPRIETARIO"].ToString();
                    txtCPFProprietario.Text = dtRol.Rows[0]["CPF_PROPRIETARIO"].ToString();
                    txtCodigoBanco.Text = dtRol.Rows[0]["COD_BANCO"].ToString();
                    txtNomeBanco.Text = dtRol.Rows[0]["DS_BANCO"].ToString();
                    txtAgencia.Text = dtRol.Rows[0]["AGENCIA"].ToString();
                    txtContaCorrente.Text = dtRol.Rows[0]["CONTA"].ToString();
                    //20201117
                    txtDataPagamento.Text = dtRol.Rows[0]["DATA_PAGAMENTO"].ToString();
                    txtDtpagamentoSaldo.Text = dtRol.Rows[0]["DATA_PAGAMENTO"].ToString();
                    txtRendimentoBruto.Text = dtRol.Rows[0]["TOTAL_AWB"].ToString();
                    txtInicioOperacao.Text = dtRol.Rows[0]["DATA_AWB_STR"].ToString();
                    txtTerminoOperacao.Text = dtRol.Rows[0]["DATA_PREVISAO_CHEGADA_STR"].ToString();


                }
                else
                {
                    Response.Write("Rol/Manifesto não encontrado.");
                    Response.End();
                }


                dtRol.Dispose();


            }

            if (!IsPostBack && Request.QueryString.Get("nrciot") != "" && Request.QueryString.Get("nrciot") != null)
            {

                form1.Visible = false;
                btnDownloadDOT(Request.QueryString.Get("nrciot"));


            }


        }


        protected void GerarCIOT()
        {
            bool geraIndividual = true;
            bool GerouOT = false;
            string strXMLOT = "";
            //        string GUID = "";
            string strRetorno = ""; //, strErro = "", strXMLEnvio = "", strXMLConsulta = "";
            bool deuErro = false;
            //20230828
            Versao = "4.2.11.0";                
            //Versao = "4.2.8.0";


            //if (lk.Text.Trim() != "")
        //{
        //    showError("Motorista " + row.Cells[4].Text + ": registro com CIOT já emitido!");
        //    goto Fim;
        //}

            //        strRetorno = GeraOT(CNPJ, Token, row, Versao);
        //        //                if (strRetorno == "Falha ao assinar")
        //        if (strRetorno.IndexOf("<OT>") == -1)   //não gerou xml
        //        {
        //            GerouOT = false;
        //            showError(strRetorno);
        //            goto Fim;
        //        }
        //        else
        //        {
        //            strXMLOT = strXMLOT + strRetorno;
        //            if (geraIndividual)
        //            {
        //                Envia_Consulta_OT(strXMLOT, Versao, ref deuErro);
        //                strXMLOT = "";
        //            }
        //            GerouOT = true;
        //        }
        //    }

            //if (!GerouOT)
        //    showWarning("Sem informações para geração de CIOT(s).");
        //else if (!geraIndividual)
        //{
        //    Envia_Consulta_OT(strXMLOT, Versao, ref deuErro);
        //    showInfo("CIOT(s) gerado(s).");


            //}
        //else
        //{
        //    if (deuErro)
        //        showInfo("CIOT(s) com erros. Verifique as mensagens de erro.");
        //    else
        //        showInfo("CIOT(s) gerado(s).");


            //}

        Fim:

            return;

        }



        protected void Envia_Consulta_OT(string strXMLOT, string Versao, ref bool deuErro)
        {
            string strXMLEnvio, strErro, strXMLConsulta, strRetorno;
            string GUID = "";
            //20190705

            if (Versao == "4.2.8.0")
            {
                strXMLOT = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
    "<loteOT_envio versao=\"4.2.8.0\" token=\"" + @Token + "\" xmlns=\"http://www.nddigital.com.br/nddcargo\" xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.nddigital.com.br/nddcargo file:///D:/Trabalho/SolucoesNDD/nddCargo/Integracao/Schemas%204.2.8.0/CIOT/nddcargo_loteOT_envio_4280.xsd\">" +
    "<operacoes>" + strXMLOT + "</operacoes></loteOT_envio>";
            }
            else
            {
                strXMLOT = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
    "<loteOT_envio versao=\"4.2.11.0\" token=\"" + @Token + "\" xmlns=\"http://www.nddigital.com.br/nddcargo\" xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.nddigital.com.br/nddcargo file:///D:/Trabalho/SolucoesNDD/nddCargo/Integracao/Schemas%204.2.11.0/CIOT/nddcargo_loteOT_envio_42110.xsd\">" +
    "<operacoes>" + strXMLOT + "</operacoes></loteOT_envio>";
            }

            strXMLEnvio = GeraEnvio(CNPJ, Token, ref GUID, "Enviar", Versao);

            var NDD_Homologa = new AeroCIOTWeb.NDD_Homologa.ExchangeMessage();
            var NDD_Producao = new AeroCIOTWeb.NDD_Producao.ExchangeMessage();

            string retorno_OT = "";

            if (atualiza)
            {
                if (ConfigurationManager.AppSettings["ambiente"] == "producao")
                    retorno_OT = NDD_Producao.Send(strXMLEnvio, strXMLOT);
                else
                    retorno_OT = NDD_Homologa.Send(strXMLEnvio, strXMLOT);
            }

            //salvar histórico de envio do xml
            DateTime dtEnvio = DateTime.Now;
            SalvarXML("10", strXMLEnvio, dtEnvio);   //Envio de OT
            SalvarXML("11", strXMLOT, dtEnvio);     //Lotes de OT
            SalvarXML("20", retorno_OT, dtEnvio);   //Retorno do envio de OT


            strErro = FindValueCIOT(retorno_OT.ToString(), "Body", "observacao");
            if (strErro != "")
            {

                string strNumero = FindValueCIOT2(strXMLOT.ToString(), "ide", "numero");

                SalvaErro(strNumero, strErro);

                deuErro = true;

                //                showError(strErro.Replace("'","\""));
                goto Fim;
            }

            if (atualiza)
            {

                strXMLConsulta = GeraEnvio(CNPJ, Token, ref GUID, "EnviarConsultar", Versao);

                //Se não houver retorno, espera 5 segundos x 3
                int contador = 0;
                string retorno_Consultar_OT = "";
                string strOT = "", strCIOT, strCodVerificador, strID;
                string sql = "";
                int intRetorno;

                while (contador <= 2)
                {

                    if (ConfigurationManager.AppSettings["ambiente"] == "producao")
                        retorno_Consultar_OT = NDD_Producao.Send(strXMLConsulta, "");
                    else
                        retorno_Consultar_OT = NDD_Homologa.Send(strXMLConsulta, "");


                    strOT = FindValueCIOT(retorno_Consultar_OT.ToString(), "CrossTalk_Header", "OT");
                    if (strOT == "")
                    {
                        System.Threading.Thread.Sleep(5000);
                        contador++;

                    }
                    else
                        break;
                }

                //salvar histórico de envio do xml
                SalvarXML("30", strXMLConsulta, DateTime.Now);           //Consuta de Envio de OT
                SalvarXML("40", retorno_Consultar_OT, DateTime.Now);    //Retorno da consulta de envio de OT


                if (strOT == "")
                {

                    //20220713
                    jaExibiuMensagem = true;

                    showWarning("Retorno do Envio - Tente mais tarde.");
                    goto Fim;
                }
                else
                {
                    strRetorno = FindValueCIOT(retorno_Consultar_OT, "CrossTalk_Header", "ResponseCode");
                    if (strRetorno == "200")
                    {
                        XmlDocument oXML = new XmlDocument();
                        oXML.LoadXml(retorno_Consultar_OT);
                        XmlNode root = oXML.DocumentElement;

                        XmlNamespaceManager nsmgr = new XmlNamespaceManager(oXML.NameTable);
                        nsmgr.AddNamespace("ndd", "http://www.nddigital.com.br/nddcargo");
                        XmlNodeList oNoLista = root.SelectNodes("//ndd:operacoes/ndd:OT", nsmgr); ;

                        string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;
                        SqlConnection conn = new SqlConnection(strConexao);

                        foreach (XmlNode oNo in oNoLista)
                        {
                            strID = oNo.SelectSingleNode("ndd:infOT/ndd:ide/ndd:numero", nsmgr).InnerText;
                            strCIOT = oNo.SelectSingleNode("ndd:infOT/ndd:autorizacao/ndd:ciot/ndd:numero", nsmgr).InnerText;
                            strCodVerificador = oNo.SelectSingleNode("ndd:infOT/ndd:autorizacao/ndd:ciot/ndd:ciotCodVerificador", nsmgr).InnerText;

                            if (strCIOT.Trim() != "")
                            {

                                //                                Stored_IAE_Motorista_CIOT("I", "", strCIOT, strCodVerificador, DateTime.Today, DateTime.Today.AddDays(20), Convert.ToInt32(strID), usuario, "", DateTime.Today, false);

                                sql = "SELECT count(*) FROM MOTORISTA_CIOT WHERE ID_FOPAG_CIOT_ITEM = " + strID;

                                SqlCommand cmd = new SqlCommand(sql, conn);
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandTimeout = 999999;

                                if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();

                                intRetorno = Convert.ToInt32(cmd.ExecuteScalar());

                                if (intRetorno == 0) //não encontrou
                                {
                                    Stored_IAE_Motorista_CIOT("I", "", strCIOT, strCodVerificador, DateTime.Today, DateTime.Today.AddDays(26), Convert.ToInt32(strID), usuario, "", DateTime.Now, false);
                                }
                                else
                                {
                                    Stored_IAE_Motorista_CIOT("A", "", strCIOT, strCodVerificador, DateTime.Today, DateTime.Today.AddDays(26), Convert.ToInt32(strID), usuario, "", DateTime.Now, false);
                                }

                            }

                        }

                    }

                    else
                    {

                        string strNumero = FindValueCIOT2(retorno_Consultar_OT.ToString(), "ide", "numero");

                        SalvaErro(strNumero, strOT);
                        deuErro = true;

                        //                        strOT = strOT.Replace("\n", "");
                        //                        showError(strOT);
                        goto Fim;
                    }
                }

                //                AtualizaGrid();
                //                showInfo("CIOT(s) gerado(s).");
            }

        Fim:

            return;

        }



        protected string GeraEnvio(string CNPJ, string Token, ref string GUID, string Acao, string Versao)
        {
            if (GUID == "")
                GUID = Gera_GUID(CNPJ, Token);

            string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(strConexao);

            //Webservice NDD Cargo para gerar OT
            SqlCommand cmd_OT = new SqlCommand("dbo.CIOT_OT", conn);
            cmd_OT.CommandType = CommandType.StoredProcedure;
            cmd_OT.CommandTimeout = 999999;

            SqlParameter pACAO_OT = cmd_OT.Parameters.Add("@ACAO", SqlDbType.Text);
            pACAO_OT.Value = Acao;

            SqlParameter pCNPJ_OT = cmd_OT.Parameters.Add("@CNPJ", SqlDbType.Text);
            pCNPJ_OT.Value = CNPJ;

            SqlParameter pToken_OT = cmd_OT.Parameters.Add("@Token", SqlDbType.Text);
            pToken_OT.Value = Token;

            SqlParameter pGUID_OT = cmd_OT.Parameters.Add("@GUID", SqlDbType.Text);
            pGUID_OT.Value = GUID;

            //20190705
            SqlParameter pVersao = cmd_OT.Parameters.Add("@Versao", SqlDbType.Text);
            pVersao.Value = Versao;

            SqlParameter PXML_OT = new SqlParameter();
            PXML_OT.ParameterName = "@XML";
            PXML_OT.SqlDbType = System.Data.SqlDbType.VarChar;
            PXML_OT.Size = -1;
            PXML_OT.Direction = ParameterDirection.ReturnValue;
            cmd_OT.Parameters.Add(PXML_OT).Direction = ParameterDirection.Output;

            if (cmd_OT.Connection.State == ConnectionState.Closed) cmd_OT.Connection.Open();

            SqlDataReader dr_OT = cmd_OT.ExecuteReader();

            String strXml_OT;

            strXml_OT = cmd_OT.Parameters["@XML"].Value.ToString();

            dr_OT.Close();
            conn.Close();

            return strXml_OT;
        }


        protected string GeraOT(string CNPJ, string Token, int IDFOPAGCIOTITEM, string Versao, string CpfMotorista)
        {
            string GUID = "";

            int ID_FOPAG_CIOT_ITEM = IDFOPAGCIOTITEM;


            string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(strConexao);

            if (FinalizarAnterior == true)
            {
                //Antes de gerar um CIOT, encerra o anterior
                string strRetorno = FinalizaCIOTAnterior(IDFOPAGCIOTITEM, Versao, CpfMotorista);

                if (strRetorno != "")
                    return strRetorno;
            }

            GUID = Gera_GUID(CNPJ, Token);

            SqlCommand cmd_OT_RAW = new SqlCommand("dbo.CIOT_Gerar_OT_RAW", conn);
            cmd_OT_RAW.CommandType = CommandType.StoredProcedure;
            cmd_OT_RAW.CommandTimeout = 999999;

            SqlParameter pCNPJ_OT_RAW = cmd_OT_RAW.Parameters.Add("@CNPJ", SqlDbType.Text);
            pCNPJ_OT_RAW.Value = CNPJ;

            SqlParameter pToken_OT_RAW = cmd_OT_RAW.Parameters.Add("@Token", SqlDbType.Text);
            pToken_OT_RAW.Value = Token;

            SqlParameter pGUID_OT_RAW = cmd_OT_RAW.Parameters.Add("@GUID", SqlDbType.Text);
            pGUID_OT_RAW.Value = GUID;

            SqlParameter pPonto_Emissao_OT = cmd_OT_RAW.Parameters.Add("@Ponto_Emissao", SqlDbType.Text);
            pPonto_Emissao_OT.Value = Ponto_Emissao;

            SqlParameter pID_FOPAG_CIOT_ITEM_OT = cmd_OT_RAW.Parameters.Add("@ID_FOPAG_CIOT_ITEM", SqlDbType.Int);
            pID_FOPAG_CIOT_ITEM_OT.Value = ID_FOPAG_CIOT_ITEM;

            SqlParameter PXML_OT_RAW = new SqlParameter();
            PXML_OT_RAW.ParameterName = "@XML";
            PXML_OT_RAW.SqlDbType = System.Data.SqlDbType.VarChar;
            PXML_OT_RAW.Size = -1;
            PXML_OT_RAW.Direction = ParameterDirection.ReturnValue;
            cmd_OT_RAW.Parameters.Add(PXML_OT_RAW).Direction = ParameterDirection.Output;

            //20190705
            SqlParameter pVersao = cmd_OT_RAW.Parameters.Add("@Versao", SqlDbType.Text);
            pVersao.Value = Versao;

            if (cmd_OT_RAW.Connection.State == ConnectionState.Closed) cmd_OT_RAW.Connection.Open();

            SqlDataReader dr_OT_RAW = cmd_OT_RAW.ExecuteReader();

            String strXml_OT_RAW;

            strXml_OT_RAW = cmd_OT_RAW.Parameters["@XML"].Value.ToString();

            dr_OT_RAW.Close();
            cmd_OT_RAW.Dispose();
            conn.Close();

            //20201222
            if (strXml_OT_RAW.IndexOf("Erro:") > 0)
            {
                showError(strXml_OT_RAW);
                return strXml_OT_RAW;
            }

            //Assina xml


            //20230721 Permitir efetuar assinatura digital através da API...

            string ativarApi = Request.QueryString.Get("api");

            if (ativarApi == null || ativarApi.Equals("")) {

                ativarApi = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["ATIVAR_MICRO_SERVICO_API"]);
            }
              
            /*20231127 FALTANDO TERMINAR A IMPLEMENTAÇÃO ...ORNELLAS 
            //20230720
            if (ativarApi != null && ativarApi.Equals("1"))
            {

                return getXMLCIOTAssinadoAPI(ID_FOPAG_CIOT_ITEM, strXml_OT_RAW);


            }
            else
            {
            */
                //20220510 
                string serverAssinaturaDigital = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["SERVER_ASSINATURA_DIGITAL"]);

                strXml_OT_RAW = getRemoteObjectExt(serverAssinaturaDigital).getXMLAssinado(strXml_OT_RAW, "infOT", Nome, false);


                return strXml_OT_RAW;
            //}
        }
        private void SalvarXML(string Tipo, string strXML, DateTime dtEnvio)
        {
            strXML = strXML.Replace("'", "''");
            string sql = "INSERT NDD_CIOT_XML (TIPO,XML_NDD,DH_INCLUSAO) VALUES ('" + Tipo + "','" + @strXML + "',CONVERT(DATETIME,'" + dtEnvio.ToString("dd/MM/yyyy HH:mm:ss.fff") + "',103))";

            string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(strConexao);

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 999999;

            if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();

            cmd.ExecuteNonQuery();
            conn.Close();
        }

        //20200602 Permitir informar servidor ...
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


        public RemoteObject getRemoteObject()
        {
            
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
                // "tcp://192.168.1.80:8087/NFeAPPrs"
                Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["SERVER_CTe"])
                );

            return remObject;
        }

        protected void SalvaErro(string numero, string erro)
        {
            erro.Replace("'", "\"").Replace("\n", " ");

            string sql = "SELECT count(*) FROM MOTORISTA_CIOT WHERE ID_FOPAG_CIOT_ITEM = " + numero;

            SqlCommand cmd = new SqlCommand(sql, getConn());
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 999999;

            if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();

            int intRetorno = Convert.ToInt32(cmd.ExecuteScalar());

            if (intRetorno == 0) //não encontrou
            {
                Stored_IAE_Motorista_CIOT("I", "", "", "", null, null, Convert.ToInt32(numero), usuario, "", DateTime.Now, false, erro);
            }
            else
            {
                Stored_IAE_Motorista_CIOT("A", "", "", "", null, null, Convert.ToInt32(numero), usuario, "", DateTime.Now, false, erro);
            }

        }

        private void Stored_IAE_Motorista_CIOT(string Tipo, string CPF_Motorista, string CIOT, string CV, DateTime? Inicio_Viagem, DateTime? Final_Viagem, int ID_FOPAG_CIOT_ITEM, string Usuario, string CPF_Proprietario, DateTime Data_Geracao, bool Tp_Finalizado, string erro = null)
        {
            string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(strConexao);

            SqlCommand cmd = new SqlCommand("dbo.STORED_IAE_MOTORISTA_CIOT", new SqlConnection(strConexao));
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 999999;

            SqlParameter PTipo = cmd.Parameters.Add("@Tipo", SqlDbType.Char, 1);
            PTipo.Value = Tipo;

            SqlParameter PCPF_MOTORISTA = cmd.Parameters.Add("@CPF_MOTORISTA", SqlDbType.VarChar, 11);
            PCPF_MOTORISTA.Value = CPF_Motorista;

            SqlParameter PCIOT = cmd.Parameters.Add("@CIOT", SqlDbType.VarChar, 12);
            PCIOT.Value = CIOT;

            SqlParameter PCV = cmd.Parameters.Add("@CV", SqlDbType.VarChar, 4);
            PCV.Value = CV;

            SqlParameter PINICIO_VIAGEM = cmd.Parameters.Add("@INICIO_VIAGEM", SqlDbType.DateTime);
            PINICIO_VIAGEM.Value = Inicio_Viagem;

            SqlParameter PFINAL_VIAGEM = cmd.Parameters.Add("@FINAL_VIAGEM", SqlDbType.DateTime);
            PFINAL_VIAGEM.Value = Final_Viagem;

            SqlParameter PID_FOPAG_CIOT_ITEM = cmd.Parameters.Add("@ID_FOPAG_CIOT_ITEM", SqlDbType.Int);
            PID_FOPAG_CIOT_ITEM.Value = ID_FOPAG_CIOT_ITEM;

            SqlParameter PUsuario = cmd.Parameters.Add("@Usuario", SqlDbType.VarChar, 20);
            PUsuario.Value = Usuario;

            SqlParameter PCPF_PROPRIETARIO = cmd.Parameters.Add("@CPF_PROPRIETARIO", SqlDbType.VarChar, 11);
            PCPF_PROPRIETARIO.Value = CPF_Proprietario;

            SqlParameter PDATA_GERACAO = cmd.Parameters.Add("@DATA_GERACAO", SqlDbType.DateTime);
            PDATA_GERACAO.Value = Data_Geracao;

            SqlParameter PTP_FINALIZADO = cmd.Parameters.Add("@TP_FINALIZADO", SqlDbType.Bit);
            PTP_FINALIZADO.Value = Tp_Finalizado ? 1 : 0;

            SqlParameter PERRO = cmd.Parameters.Add("@MSG_ERRO", SqlDbType.VarChar, 1000);
            PERRO.Value = erro;

            if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();

            if (atualiza)
            {
                int intRetorno = cmd.ExecuteNonQuery();
            }

            cmd.Dispose();
            conn.Dispose();
        }



        private SqlConnection getConn()
        {
            if (conn == null)
                conn = new SqlConnection(ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString);
            return conn;
        }


        private DataTable retDataTableText(string sql)
        {
            SqlCommand cmd = new SqlCommand(sql, getConn());
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 999999;

            if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();
            cmd.ExecuteNonQuery();

            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            da.Fill(dt);

            cmd.Dispose();
            da.Dispose();
            //        conn.Dispose();
            getConn().Close();

            return dt;
        }


        public string Gera_GUID(string CNPJ, string Token)
        {
            string GUID = "";

            string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(strConexao);

            SqlCommand cmd = new SqlCommand("dbo.CIOT_Gerar_GUID", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 999999;

            SqlParameter pCNPJ = cmd.Parameters.Add("@CNPJ", SqlDbType.Text);
            pCNPJ.Value = CNPJ;

            SqlParameter pToken = cmd.Parameters.Add("@Token", SqlDbType.Text);
            pToken.Value = Token;

            SqlParameter PXML = new SqlParameter();
            PXML.ParameterName = "@XML";
            PXML.SqlDbType = System.Data.SqlDbType.VarChar;
            PXML.Size = -1;
            PXML.Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(PXML).Direction = ParameterDirection.Output;

            if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();

            SqlDataReader dr = cmd.ExecuteReader();

            String strXml;

            strXml = cmd.Parameters["@XML"].Value.ToString();

            dr.Close();
            cmd.Dispose();
            conn.Close();

            var NDD_Homologa = new AeroCIOTWeb.NDD_Homologa.ExchangeMessage();
            var NDD_Producao = new AeroCIOTWeb.NDD_Producao.ExchangeMessage();

            string retorno;
            if (ConfigurationManager.AppSettings["ambiente"] == "producao")
                retorno = NDD_Producao.Send(strXml, "");
            else
                retorno = NDD_Homologa.Send(strXml, "");

            GUID = FindValueCIOT(retorno.ToString(), "CrossTalk_Header", "GUID"); ;

            return GUID;
        }


        public string FindValueCIOT2(string XMLFile, string TAGValue, string TAGValue2)
        {
            string _returnValueCIOTVerificador = string.Empty;

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(XMLFile);

            XmlNodeList _nodeListTAG1 = doc.GetElementsByTagName(TAGValue);

           


            foreach (XmlNode nodeTAG1 in _nodeListTAG1)
            {
                for (int i = 0; i < nodeTAG1.ChildNodes.Count; i++)
                    if (nodeTAG1.ChildNodes[i].Name == TAGValue2)
                    {
                        _returnValueCIOTVerificador = nodeTAG1.ChildNodes[i].InnerText;
                        break;
                    }
            }
          


            return _returnValueCIOTVerificador;
        }

        public string FindValueCIOT(string XMLFile, string TAGValue, string TAGValue2)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(XMLFile);

            XmlNodeList _nodeListTAG1 = doc.GetElementsByTagName(TAGValue);
            XmlNodeList _nodeListTAG2 = doc.GetElementsByTagName(TAGValue2);

            string _returnValueCIOTVerificador = string.Empty;

            foreach (XmlNode nodeTAG1 in _nodeListTAG1)
            {
                foreach (XmlNode nodeTAG2 in _nodeListTAG2)
                {
                    _returnValueCIOTVerificador = nodeTAG2.InnerText;
                }
            }

            return _returnValueCIOTVerificador;
        }

        private void showWarning(string msg)
        {
            string str = msg.Replace('"', ' ').Replace("'", "").Replace('\n', ' ').Replace('\t', ' ').Replace("\n", "").Replace("\t", "").Replace("\r", "").Trim();

           
            body1.Attributes.Add("onload", "alert('" + msg + "')");
        }



        protected string FinalizaCIOTAnterior(int IDFOPAGCIOTITEM, string Versao, string CpfMotorista)
        {
            int ID_FOPAG_CIOT_ITEM = IDFOPAGCIOTITEM;
            string Motorista_CPF = CpfMotorista;
            string strRetorno = "";

            string sql = "SELECT TOP 1 * FROM MOTORISTA_CIOT WHERE CPF_MOTORISTA= '" + Motorista_CPF + "' AND CIOT<>'' AND ISNULL(TP_FINALIZADO,0)=0 ORDER BY ID DESC";
            DataTable dtCIOT = retDataTableText(sql);

            if (dtCIOT.Rows.Count > 0)
            {
                if (dtCIOT.Rows[0]["CIOT"].ToString().Substring(0, 2) == "08")
                {
                    if (dtCIOT.Rows[0]["DT_FINALIZACAO"].ToString() == "")
                    {
                        FinalizaCIOT(Convert.ToInt32(dtCIOT.Rows[0]["ID_FOPAG_CIOT_ITEM"]), Versao);
                    }
                    else
                        strRetorno = "CIOT já finalizado.";
                }
                else
                    strRetorno = "CIOT anterior não é da NDD.";
            }
            else
                strRetorno = "CIOT não encontrado.";

            dtCIOT.Dispose();

            return strRetorno;
        }

        protected void FinalizaCIOT(int ID_FOPAG_CIOT_ITEM, string Versao)
        {
            string GUID = "";
            string strErro = "";

            GUID = Gera_GUID(CNPJ, Token);

            string strXMLEncerrar = GeraEnvio(CNPJ, Token, ref GUID, "Encerrar", Versao);

            string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(strConexao);

            SqlCommand cmd = new SqlCommand("dbo.CIOT_Encerrar_OT", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 999999;

            SqlParameter pCNPJ = cmd.Parameters.Add("@CNPJ", SqlDbType.Text);
            pCNPJ.Value = CNPJ;

            SqlParameter pToken = cmd.Parameters.Add("@Token", SqlDbType.Text);
            pToken.Value = Token;

            SqlParameter pGUID_OT_RAW = cmd.Parameters.Add("@GUID", SqlDbType.Text);
            pGUID_OT_RAW.Value = GUID;

            SqlParameter pID_FOPAG_CIOT_ITEM_OT = cmd.Parameters.Add("@ID_FOPAG_CIOT_ITEM", SqlDbType.Int);
            pID_FOPAG_CIOT_ITEM_OT.Value = ID_FOPAG_CIOT_ITEM;

            SqlParameter PXML = new SqlParameter();
            PXML.ParameterName = "@XML";
            PXML.SqlDbType = System.Data.SqlDbType.VarChar;
            PXML.Size = -1;
            PXML.Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(PXML).Direction = ParameterDirection.Output;

            //20190705
            SqlParameter pVersao = cmd.Parameters.Add("@Versao", SqlDbType.Text);
            pVersao.Value = Versao;

            if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();

            SqlDataReader dr = cmd.ExecuteReader();

            String strXml;

            strXml = cmd.Parameters["@XML"].Value.ToString();

            dr.Close();
            cmd.Dispose();
            conn.Close();

            if (strXml == "")
            {
                showError("ID_FOPAG_CIOT_ITEM: " + ID_FOPAG_CIOT_ITEM.ToString() + " inválido!");
                goto Fim;
            }

            //strXml = AssinaXML(strXml, "encerrarOT_envio");

            //20220510 
            string serverAssinaturaDigital = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["SERVER_ASSINATURA_DIGITAL"]);

            strXml = getRemoteObjectExt(serverAssinaturaDigital).getXMLAssinado(strXml, "encerrarOT_envio", Nome, false);

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
                    retorno_OT = NDD_Producao.Send(strXMLEncerrar, strXml);
                else
                    retorno_OT = NDD_Homologa.Send(strXMLEncerrar, strXml);
            }

            //salvar histórico de envio do xml
            DateTime dtEnvio = DateTime.Now;
            SalvarXML("50", strXMLEncerrar, dtEnvio);   //Envio do Encerramento
            SalvarXML("51", strXml, dtEnvio);           //CIOT para encerrar
            SalvarXML("60", retorno_OT, dtEnvio);       //Retorno do envio do Encerramento

            strErro = FindValueCIOT(retorno_OT.ToString(), "Body", "observacao");
            if (strErro != "")
            {
                showError(strErro);
                goto Fim;
            }

            string strXMLConsulta = GeraEnvio(CNPJ, Token, ref GUID, "EncerrarConsultar", Versao);

            //Se não houver retorno, espera 5 segundos x 3
            int contador = 0;
            string retorno_Consultar = "";
            string strOT = "", strRetorno, strCIOT, strCodVerificador, strID;
            string sql = "";
            int intRetorno;
            string strprotocoloEnce, strDataEncerramento;

            if (atualiza)
            {
                while (contador <= 2)
                {
                    if (ConfigurationManager.AppSettings["ambiente"] == "producao")
                        retorno_Consultar = NDD_Producao.Send(strXMLConsulta, "");
                    else
                        retorno_Consultar = NDD_Homologa.Send(strXMLConsulta, "");

                    strOT = FindValueCIOT(retorno_Consultar.ToString(), "CrossTalk_Header", "retornoConsultaEncerramento");
                    if (strOT == "")
                    {
                        System.Threading.Thread.Sleep(5000);
                        contador++;
                    }
                    else
                        break;
                }

                //salvar histórico de envio do xml
                SalvarXML("70", strXMLConsulta, DateTime.Now); //Consulta do Encerramento
                SalvarXML("80", retorno_Consultar, DateTime.Now); //Retorno da consulta do Encerramento

                if (strOT == "")
                {
                    //20220713
                    jaExibiuMensagem = true;

                    showWarning("Retorno do Encerramento - Tente mais tarde.");
                    goto Fim;
                }
                else
                {
                    strRetorno = FindValueCIOT(retorno_Consultar, "CrossTalk_Header", "ResponseCode");
                    if (strRetorno == "200")
                    {
                        XmlDocument oXML = new XmlDocument();
                        oXML.LoadXml(retorno_Consultar);
                        XmlNode root = oXML.DocumentElement;

                        XmlNamespaceManager nsmgr = new XmlNamespaceManager(oXML.NameTable);
                        nsmgr.AddNamespace("ndd", "http://www.nddigital.com.br/nddcargo");

                        XmlNodeList oNoLista = root.SelectNodes("CrossTalk_Body/retornoConsultaEncerramento");

                        foreach (XmlNode oNo in oNoLista)
                        {
                            strCIOT = oNo.SelectSingleNode("ndd:encerramento/ndd:autorizacao/ndd:ciot/ndd:numero", nsmgr).InnerText;
                            strCodVerificador = oNo.SelectSingleNode("ndd:encerramento/ndd:autorizacao/ndd:ciot/ndd:ciotCodVerificador", nsmgr).InnerText;
                            strDataEncerramento = oNo.SelectSingleNode("ndd:encerramento/ndd:dataHora", nsmgr).InnerText;
                            strprotocoloEnce = oNo.SelectSingleNode("ndd:encerramento/ndd:protocoloEnce", nsmgr).InnerText;

                            //salvar as informações
                            if (strCIOT.Trim() != "")
                            {
                                //                        sql = "UPDATE MOTORISTA_CIOT SET CIOT_PROTOCOLO_ENCERRAMENTO = '" + strprotocoloEnce + "', CIOT_DATA_ENCERRAMENTO=CONVERT(DATETIME,'" + strDataEncerramento + "',120) WHERE CIOT_NDD = '" + strCIOT + "'";
                                sql = "UPDATE MOTORISTA_CIOT SET CIOT_PROTOCOLO_ENCERRAMENTO = '" + strprotocoloEnce + "', DT_FINALIZACAO=CONVERT(DATETIME,'" + strDataEncerramento + "',120),TP_FINALIZADO=1 WHERE CIOT = '" + strCIOT + "'";

                                cmd = new SqlCommand(sql, conn);
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandTimeout = 999999;

                                if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();

                                intRetorno = cmd.ExecuteNonQuery();

                            }
                        }
                    }
                    else
                    {
                        showError(strOT);
                        goto Fim;
                    }
                }

                //AtualizaGrid();
                showInfo("CIOT(s) finalizado(s).");
            }

        Fim:
            return;
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

        protected void GerarCIOTTerceiro()
        {
            string sql = "";
            string retMsg = "";
            //20230828
            Versao = "4.2.11.0";                
            //Versao = "4.2.8.0";
          
            
            //Verifica se o motorista tem CIOT não finalizado
            sql = "select * from motorista_ciot where cpf_motorista='" + txtCPFTerceiro.Text + "' and tp_finalizado=0 and isnull(ciot,'')<>''";
            DataTable dtMotoristaCIOT = retDataTableText(sql);
            if (dtMotoristaCIOT.Rows.Count != 0)
            {
                retMsg = "Motorista " + txtNomeTerceiro.Text + " com CIOT " + dtMotoristaCIOT.Rows[0]["CIOT"].ToString() + " não finalizado!";
                showError(retMsg);
                return;
            }



            if ((IsCpf(txtCNPJDestinatario.Text) == false) && (IsCnpj(txtCNPJDestinatario.Text) == false))
            {
                retMsg = " Campo 'CNPJ do destinatário' inválido !";
                showError(retMsg);
                return;
            }
            

            //Verifica se o motorista é terceiro
            //20190712                    sql = "select * from motorista where cpf='" + campos[14] + "' and tp_motorista='T'";
            sql = "select * from motorista where cpf='" + txtCPFTerceiro.Text + "' and tp_motorista='T'";
            DataTable dtMotorista = retDataTableText(sql);
            if (dtMotorista.Rows.Count == 0)
            {
                retMsg = "Motorista " + txtCPFTerceiro.Text + " não é terceiro!";
                showError(retMsg);
                return;
            }
            //Verifica se o motorista tem CIOT não finalizado
            sql = "select * from motorista_ciot where cpf_motorista='" + txtCPFTerceiro.Text + "' and tp_finalizado=0 and isnull(ciot,'')<>''";
            DataTable dtMotoristaCIOT2 = retDataTableText(sql);
            if (dtMotoristaCIOT.Rows.Count != 0)
            {
                retMsg = "Motorista " + txtCPFTerceiro.Text + " com CIOT " + dtMotoristaCIOT2.Rows[0]["CIOT"].ToString() + " não finalizado!";
                showError(retMsg);
                return;
            }
            
           
            if (!txtDataPagamento.Text.Equals(""))
            {
                try
                {

                    Convert.ToDateTime(txtDataPagamento.Text);
                }
                catch
                {
                    retMsg = "Campo 'Data pagamento' inválido ";
                    showError(retMsg);
                    return;

                }
            }


            if ( txtRendimentoBruto.Text.Trim() != "")
            {
                try
                {

                    Convert.ToDecimal(txtRendimentoBruto.Text);
                }
                catch
                {
                    retMsg = "Campo 'Rend. Bruto ' inválido ";
                    showError(retMsg);
                    return;

                }
            }

            if ( txtIRRetido.Text.Trim() != "")
            {
                try
                {

                    Convert.ToDecimal(txtIRRetido.Text);
                }
                catch
                {
                    retMsg = "Campo 'IR Retido' inválido ";
                    showError(retMsg);
                    return;

                }
            }
            if ( txtContribuicaoINSS.Text.Trim() != "")
            {
                try
                {

                    Convert.ToDecimal(txtContribuicaoINSS.Text);
                }
                catch
                {
                    retMsg = "Campo 'Contribuição INSS	' inválido ";
                    showError(retMsg);
                    return;

                }
            }
            if (txtINSSTransp.Text.Trim() != "")
            {
                try
                {

                    Convert.ToDecimal(txtINSSTransp.Text);
                }
                catch
                {
                    retMsg = "Campo 'INSS Transp' inválido ";
                    showError(retMsg);
                    return;

                }
            }
           

          
            //20190704 Consistir valores numéricos campos[5] a campos[8]
            if (txtRendimentoBruto.Text.Trim() == "")txtRendimentoBruto.Text = "0";
            if (txtIRRetido.Text.Trim() == "") txtIRRetido.Text = "0";
            if (txtContribuicaoINSS.Text.Trim() == "") txtContribuicaoINSS.Text= "0";
            if (txtINSSTransp.Text.Trim() == "") txtINSSTransp.Text = "0";

            
            //20190723 Consistir campos novos
            if (txtSaldo.Text.Trim() == "") txtSaldo.Text = "0";
            
            /*
            if ((campos[20] == null) || (campos[20].Trim() == ""))
                campos[20] = "null";
            else
                campos[20] = "'" + campos[20] + "'";
            */
          

            //Inlcuindo os 2 primeiros campos (empresa e filial com 1 e 1 )
            sql = "insert into FOPAG_CIOT_ITEM (" +
                "ID_IMPORTACAO, empresa, filial, nome, cpf, data_pagto, rend_bruto, IR_Retido, contribuicao_inss, INSS_Transp, Banco, Nome_do_banco, Agencia, Conta_corrente, digito,saldo, data_pagto_saldo) " +
                "values (0,'" + "1" + "','" + "1" + "','" + txtNomeTerceiro.Text + "','" + txtCPFTerceiro.Text + "',CONVERT(datetime,'" + txtDataPagamento.Text + "',103)," +
                txtRendimentoBruto.Text.ToString().Replace(",", ".") + "," + txtIRRetido.Text.ToString().Replace(",", ".") + "," + txtContribuicaoINSS.Text.ToString().Replace(",", ".") + "," +
                txtINSSTransp.Text.ToString().Replace(",", ".") + ",'" + txtCodigoBanco.Text + "','" + txtNomeBanco.Text + "','" + txtAgencia.Text + "','" + txtContaCorrente.Text + "','" +""+"','"+
                txtSaldo.Text.ToString().Replace(",", ".") + "',";


            if (txtDtpagamentoSaldo.Text.Trim() == "")
                 sql = sql + "CONVERT(datetime,null,103)" + ")"; 
            else
                sql = sql + "CONVERT(datetime,'" + txtDtpagamentoSaldo.Text + "',103)" + ")";

            SqlCommand cmd = new SqlCommand(sql, getConn());
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 999999;

            if (getConn().State == ConnectionState.Closed) getConn().Open();

            int retorno = cmd.ExecuteNonQuery();

            if (retorno < 1)
            {
                retMsg = "Problema na inclusão do FOPAG_CIOT_ITEM: " + txtNomeTerceiro.Text + " !";
                showError(retMsg);
                return;
            }

            //recupera o ID do FOPAG_CIOT_ITEM
            sql = "SELECT MAX(ID) FROM FOPAG_CIOT_ITEM";

            cmd = new SqlCommand(sql, getConn());
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 999999;

            if (getConn().State == ConnectionState.Closed) getConn().Open();

            int id_fopag_ciot_item = (int)cmd.ExecuteScalar();

            if (id_fopag_ciot_item < 1)
            {
                retMsg = "Problema na recuperação do ID FOPAG_CIOT_ITEM.";
                showError(retMsg);
                return;
            }


            sql = "insert into MOTORISTA_CIOT (" +
                "CPF_MOTORISTA, CIOT, INICIO_VIAGEM,FINAL_VIAGEM, ID_FOPAG_CIOT_ITEM, DATA_GERACAO, DT_INCLUSAO, USUA_INCLUSAO, PLACA, CPF_PROPRIETARIO, CNPJ_DESTINO) " +
                "values ('" + txtCPFTerceiro.Text + "','',convert(datetime,'" + txtInicioOperacao.Text + "',103),convert(datetime,'" + txtTerminoOperacao.Text + "',103)," +
                id_fopag_ciot_item.ToString() + ",getdate(),getdate(),'" + usuario + "','" + txtPlaca.Text + "','" + txtCPFProprietario.Text + "','" +
                txtCNPJDestinatario.Text.ToString().Replace(".", "").Replace("-", "").Replace("/", "") + "')";

            cmd = new SqlCommand(sql, getConn());
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 999999;

            if (getConn().State == ConnectionState.Closed) getConn().Open();

            retorno = cmd.ExecuteNonQuery();

            if (retorno < 1)
            {
                retMsg = "Problema na inclusão do MOTORISTA_CIOT: " + txtNomeTerceiro.Text + " !";
                showError(retMsg);
                return;
            }


            //gera a tabela do item incluído
            string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;

            cmd = new SqlCommand("dbo.getDadosMotoristaCIOT", new SqlConnection(strConexao));

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.CommandTimeout = 999999;

            SqlParameter PCPF_MOTORISTA = cmd.Parameters.Add("@CPF_MOTORISTA", SqlDbType.Text);

            PCPF_MOTORISTA.Value = "";

            SqlParameter PNOME_MOTORISTA = cmd.Parameters.Add("@NOME_MOTORISTA", SqlDbType.Text);

            PNOME_MOTORISTA.Value = "";

            SqlParameter PID_IMPORTACAO = cmd.Parameters.Add("@ID_IMPORTACAO", SqlDbType.Int);
            PID_IMPORTACAO.Value = -1;

            SqlParameter PSEPARA_INSS = cmd.Parameters.Add("@SEPARA_INSS", SqlDbType.Bit);
            PSEPARA_INSS.Value = 1;

            SqlParameter PID_FOPAG_CIOT_ITEM = cmd.Parameters.Add("@ID_FOPAG_CIOT_ITEM", SqlDbType.Int);
            PID_FOPAG_CIOT_ITEM.Value = id_fopag_ciot_item;

            if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();

            SqlDataAdapter da = new SqlDataAdapter();

            DataSet dt = new DataSet();

            da.SelectCommand = cmd;
            da.Fill(dt);


           

            string strRetorno;
            bool deuErro = false;

            strRetorno = GeraOT(CNPJ, Token, id_fopag_ciot_item, Versao, "");
            if (strRetorno.IndexOf("<OT>") == -1)   //não gerou xml
            {
                showError(strRetorno);
            }
            else
            {
                Envia_Consulta_OT(strRetorno, Versao, ref deuErro);

                //Atualiza dataset 
                dt = new DataSet();

                da.SelectCommand = cmd;
                da.Fill(dt);

                if (deuErro)
                    showError("Erro: " + dt.Tables[0].Rows[0]["MSG_ERRO"].ToString().Replace("\n", ""));
                else
                {
                    showInfo("CIOT " + dt.Tables[0].Rows[0]["CIOT"].ToString() + " gerado.");
                    btnDownloadDOT(dt.Tables[0].Rows[0]["CIOT"].ToString());
                }
            }
        }

        protected void txtCNPJDestinatario_TextChanged(object sender, EventArgs e)
        {
            string retorno = "";

            string sql = "select nome from cliente " +
            "where codigo_cnpj_cpf='" + txtCNPJDestinatario.Text + "'";

            DataTable dt = retDataTableText(sql);

            if (dt.Rows.Count == 0)
                retorno = "Destinatário " + txtCNPJDestinatario.Text + " não encontrado!";
            else
                txtNomeDestinatario.Text = dt.Rows[0]["Nome"].ToString();

            dt.Dispose();

            if (retorno != "")
            {
                showError(retorno);
                return;
            }
        }

        protected void btnGerarCIOTTerceiro_Click(object sender, EventArgs e)
        {
            //20220713
            jaExibiuMensagem = false;

            string retorno = "";
            //Refaz as validações
            //txtCPFTerceiro_TextChanged(sender, e);
            //txtCPFProprietario_TextChanged(sender, e);
            //txtPlaca_TextChanged(sender, e);
            txtCNPJDestinatario_TextChanged(sender, e);

            if (!IsDate(txtDataPagamento.Text))
                retorno = "Data de Pagamento inválida!";

            if (!IsDate(txtInicioOperacao.Text))
                retorno = "Data de Iniício da Operação inválida!";

            if (!IsDate(txtTerminoOperacao.Text))
                retorno = "Data de Término da Operação inválida!";

            if (retorno != "")
            {
                showError(retorno);
                return;
            }

                         

            GerarCIOTTerceiro();


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
  
        private static bool IsCpf(string cpf)
        {
            int[] multiplicador1 = new int[9] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[10] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            cpf = cpf.Trim().Replace(".", "").Replace("-", "");
            if (cpf.Length != 11)
                return false;

            for (int j = 0; j < 10; j++)
                if (j.ToString().PadLeft(11, char.Parse(j.ToString())) == cpf)
                    return false;

            string tempCpf = cpf.Substring(0, 9);
            int soma = 0;

            for (int i = 0; i < 9; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

            int resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            string digito = resto.ToString();
            tempCpf = tempCpf + digito;
            soma = 0;
            for (int i = 0; i < 10; i++)
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = digito + resto.ToString();

            return cpf.EndsWith(digito);
        }

        private static bool IsCnpj(string cnpj)
        {
            int[] multiplicador1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            cnpj = cnpj.Trim().Replace(".", "").Replace("-", "").Replace("/", "");
            if (cnpj.Length != 14)
                return false;

            string tempCnpj = cnpj.Substring(0, 12);
            int soma = 0;

            for (int i = 0; i < 12; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];

            int resto = (soma % 11);
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            string digito = resto.ToString();
            tempCnpj = tempCnpj + digito;
            soma = 0;
            for (int i = 0; i < 13; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];

            resto = (soma % 11);
            if (resto < 2)
                resto = 0;
            else
                resto = 11 - resto;

            digito = digito + resto.ToString();

            return cnpj.EndsWith(digito);
        }

        protected void btnDownloadDOT(string numciot)
        {
            bool GerouDOT = false;
            string strXMLOT = "";
            string strRetorno = ""; //, strErro = "", strXMLEnvio = "", strXMLConsulta = "";
            bool deuErro = false;
            //20230828
            Versao = "4.2.11.0";                
            //Versao = "4.2.8.0";
           
            strXMLOT = "";

                    

                   
            //if (lk.Text.Trim() == "")
            //{
            //    showError("Motorista " + row.Cells[4].Text + ": registro sem CIOT!");
            //    goto Fim;
            //}

            strRetorno = DownloadDOT(CNPJ, Token, numciot, Versao);
            //                if (strRetorno == "Falha ao assinar")
            if (strRetorno.IndexOf("Signature") == -1)   //não gerou xml
            {
                GerouDOT = false;
                showError(strRetorno);
                goto Fim;
            }
            else
            {
                strXMLOT = strXMLOT + strRetorno;
                Envia_Download_DOT(strXMLOT, Versao, ref deuErro);
                //strXMLOT = "";
                //GerouDOT = true;
                //goto Fim;
                if (deuErro)
                        return;
                //Visualiza um DOT de cada vez.
            }
                
            
            if (!GerouDOT && strXMLOT == "")
                showWarning("Sem CIOT selecionado.");

            else
            {
                if (deuErro)
                    showInfo("DOT(s) com erros.");
                //else
                //  showInfo("DOT(s) gerado(s).");

                //            AtualizaGrid();
            }


        Fim:

            return;



        }

        protected string DownloadDOT(string CNPJ, string Token, string  numciot, string Versao)
        {
            string GUID = "";

            //string CIOT = row.Cells[1].Text;


            string CIOT = numciot;

            string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(strConexao);

            GUID = Gera_GUID(CNPJ, Token);

            SqlCommand cmd_DOT = new SqlCommand("dbo.CIOT_Download_DOT", conn);
            cmd_DOT.CommandType = CommandType.StoredProcedure;
            cmd_DOT.CommandTimeout = 999999;

            SqlParameter pCNPJ_OT_RAW = cmd_DOT.Parameters.Add("@CNPJ", SqlDbType.Text);
            pCNPJ_OT_RAW.Value = CNPJ;

            SqlParameter pToken_OT_RAW = cmd_DOT.Parameters.Add("@Token", SqlDbType.Text);
            pToken_OT_RAW.Value = Token;

            SqlParameter pGUID_OT_RAW = cmd_DOT.Parameters.Add("@GUID", SqlDbType.Text);
            pGUID_OT_RAW.Value = GUID;

            SqlParameter pCIOT = cmd_DOT.Parameters.Add("@CIOT", SqlDbType.Text);
            pCIOT.Value = @CIOT;

            SqlParameter PXML_OT_RAW = new SqlParameter();
            PXML_OT_RAW.ParameterName = "@XML";
            PXML_OT_RAW.SqlDbType = System.Data.SqlDbType.VarChar;
            PXML_OT_RAW.Size = -1;
            PXML_OT_RAW.Direction = ParameterDirection.ReturnValue;
            cmd_DOT.Parameters.Add(PXML_OT_RAW).Direction = ParameterDirection.Output;

            //20190705
            SqlParameter pVersao = cmd_DOT.Parameters.Add("@Versao", SqlDbType.Text);
            pVersao.Value = Versao;

            if (cmd_DOT.Connection.State == ConnectionState.Closed) cmd_DOT.Connection.Open();

            SqlDataReader dr_OT_RAW = cmd_DOT.ExecuteReader();

            String strXml_DOT;

            strXml_DOT = cmd_DOT.Parameters["@XML"].Value.ToString();

            dr_OT_RAW.Close();
            cmd_DOT.Dispose();
            conn.Close();

            //Assina xml

            //20220510 
            string serverAssinaturaDigital = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["SERVER_ASSINATURA_DIGITAL"]);

            strXml_DOT = getRemoteObjectExt(serverAssinaturaDigital).getXMLAssinado(strXml_DOT, "downloadOperacao_envio", Nome, false);

            return strXml_DOT;
        }

        protected void Envia_Download_DOT(string strXMLOT, string Versao, ref bool deuErro)
        {
            string strXMLEnvio, strErro, strXMLConsulta, strRetorno;
            string GUID = "";
            //20190705
            /*
                    if (Versao == "4.2.8.0")
                    {
                        strXMLOT = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
            "<loteOT_envio versao=\"4.2.8.0\" token=\"" + @Token + "\" xmlns=\"http://www.nddigital.com.br/nddcargo\" xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.nddigital.com.br/nddcargo file:///D:/Trabalho/SolucoesNDD/nddCargo/Integracao/Schemas%204.2.8.0/CIOT/nddcargo_loteOT_envio_4280.xsd\">" +
            "<operacoes>" + strXMLOT + "</operacoes></loteOT_envio>";
                    }
                    else
                    {
                        strXMLOT = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
            "<loteOT_envio versao=\"4.2.11.0\" token=\"" + @Token + "\" xmlns=\"http://www.nddigital.com.br/nddcargo\" xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.nddigital.com.br/nddcargo file:///D:/Trabalho/SolucoesNDD/nddCargo/Integracao/Schemas%204.2.11.0/CIOT/nddcargo_loteOT_envio_42110.xsd\">" +
            "<operacoes>" + strXMLOT + "</operacoes></loteOT_envio>";
                    }
            */
            strXMLEnvio = GeraEnvio(CNPJ, Token, ref GUID, "DownloadDOT", Versao);

            var NDD_Homologa = new AeroCIOTWeb.NDD_Homologa.ExchangeMessage();
            var NDD_Producao = new AeroCIOTWeb.NDD_Producao.ExchangeMessage();

            string retorno_OT = "";

            if (atualiza)
            {
                if (ConfigurationManager.AppSettings["ambiente"] == "producao")
                    retorno_OT = NDD_Producao.Send(strXMLEnvio, strXMLOT);
                else
                    retorno_OT = NDD_Homologa.Send(strXMLEnvio, strXMLOT);
            }

            //salvar histórico de envio do xml
            DateTime dtEnvio = DateTime.Now;
            SalvarXML("85", strXMLEnvio, dtEnvio);   //Envio de OT
            SalvarXML("86", strXMLOT, dtEnvio);     //Lotes de OT
            SalvarXML("87", retorno_OT, dtEnvio);   //Retorno do envio de OT


            strErro = FindValueCIOT(retorno_OT.ToString(), "Body", "observacao");
            if (strErro != "")
            {

                string strNumero = FindValueCIOT2(strXMLOT.ToString(), "ide", "numero");

                SalvaErro(strNumero, strErro);

                deuErro = true;

                //                showError(strErro.Replace("'","\""));
                goto Fim;
            }

            if (atualiza)
            {

                strXMLConsulta = GeraEnvio(CNPJ, Token, ref GUID, "DownloadDOTConsultar", Versao);

                //Se não houver retorno, espera 5 segundos x 3
                int contador = 0;
                string retorno_Consultar_OT = "";
                string strOT = "", strCIOT, strCodVerificador, strID;
                string sql = "";
                int intRetorno;

                while (contador <= 2)
                {

                    if (ConfigurationManager.AppSettings["ambiente"] == "producao")
                        retorno_Consultar_OT = NDD_Producao.Send(strXMLConsulta, "");
                    else
                        retorno_Consultar_OT = NDD_Homologa.Send(strXMLConsulta, "");


                    strOT = FindValueCIOT(retorno_Consultar_OT.ToString(), "CrossTalk_Body", "retornoDownloadOperacao");
                    if (strOT == "")
                    {
                        System.Threading.Thread.Sleep(5000);
                        contador++;

                    }
                    else
                        break;
                }

                //salvar histórico de envio do xml
                SalvarXML("88", strXMLConsulta, DateTime.Now);           //Consuta de Envio de OT
                SalvarXML("89", retorno_Consultar_OT, DateTime.Now);    //Retorno da consulta de envio de OT


                if (strOT == "")
                {

                    //20220713
                    jaExibiuMensagem = true;

                    showWarning("Retorno do Envio - Tente mais tarde.");
                    goto Fim;
                }
                else
                {
                    strRetorno = FindValueCIOT(retorno_Consultar_OT, "CrossTalk_Header", "ResponseCode");
                    if (strRetorno == "200")
                    {
                        XmlDocument oXML = new XmlDocument();
                        oXML.LoadXml(retorno_Consultar_OT);
                        XmlNode root = oXML.DocumentElement;

                        XmlNamespaceManager nsmgr = new XmlNamespaceManager(oXML.NameTable);
                        nsmgr.AddNamespace("ndd", "http://www.nddigital.com.br/nddcargo");

                        XmlNodeList oNoCIOT = root.SelectNodes("//ndd:ciot", nsmgr);

                        strCIOT = oNoCIOT[0].InnerText;


                        XmlNodeList oNoLista = root.SelectNodes("//ndd:retDownloadOperacao", nsmgr);

                        strID = oNoLista[0].SelectSingleNode("ndd:pdf", nsmgr).InnerText;


                        Session.Add("dot", strID);

                        //body1.Attributes.Add("onload", "window.open('DOT.aspx')");

                        body1.Attributes.Add("onload", "window.location.href='DOT.aspx'");

                        
                        //Response.Redirect("DOT.aspx");
                        return;

                        /*

                                            byte[] base64EncodedBytes = System.Convert.FromBase64String(strID);


                                                                Response.ContentType = "application/pdf";

                                                                Response.AddHeader("content-length", base64EncodedBytes.Length.ToString());

                                                                Response.Write("<script>window.open('Pesquisa.aspx','_blank');</script>");

                                                                Response.BinaryWrite(base64EncodedBytes);
                        */

                        /*

                                            string strArquivo = @"\\192.168.1.18\j$\integracao\DOT_"+strCIOT+".pdf";
  
                                            if (File.Exists(strArquivo) )
                                                File.Delete(strArquivo);
                    
                                            File.WriteAllBytes(strArquivo, base64EncodedBytes);
                        */

                    }

                    else
                    {

                        /* atualizar
                        string strNumero = FindValueCIOT2(retorno_Consultar_OT.ToString(), "ide", "numero");
                        SalvaErro(strNumero, strOT);
                        */

                        deuErro = true;

                        goto Fim;
                    }
                }

            }

        Fim:

            return;

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

            Pdesc_nivel2.Value = "Cadastra Apenas Terceiro";

            if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();

            intRetorno = cmd.ExecuteNonQuery();

            cmd.Dispose();
            conn.Dispose();

            apenasTerceiro = PenabledMenu.Value.ToString() == "S" ? true : false;
            return true;

        }

    

    }
}