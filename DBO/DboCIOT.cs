using AeroCIOTWeb.dsCIOTTableAdapters;
using AeroCIOTWeb.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Xml;

namespace AeroCIOTWeb.DBO
{
    public class DboCIOT
    {
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

        public string GeraEnvio(string CNPJ, string Token, ref string GUID, string Acao, string Versao)
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

            strXml_OT = strXml_OT.Replace("\r\n", "");


            dr_OT.Close();
            conn.Close();

            return strXml_OT;
        }

        /// <summary>
        /// Salva xmls enviados e retorno
        /// </summary>
        /// <param name="Tipo"></param> 
        /// <param name="strXML"></param>
        /// <param name="dtEnvio"></param>
        public void SalvarXML(string Tipo, string strXML, DateTime dtEnvio)
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

        public Motorista_Ciot Get_Motorista_Ciot(string Id_Ciot, string Cpf)
        {
            string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(strConexao);

            string sql = "select Id ,Cpf_Motorista ,Data_Geracao ,Ciot ,Inicio_Viagem,Final_Viagem ,Id_Fopag_Ciot_Item	" +
                         ",Dt_Inclusao ,Usua_Inclusao ,Cv ,Tp_Finalizado ,Usuario_Finalizacao ,Dt_Finalizacao ,Placa " +
                         ",Cpf_Proprietario ,Ciot_Protocolo_Encerramento ,Cnpj_Destino ,Msg_Erro " +
                         " from MOTORISTA_CIOT where Id_Fopag_Ciot_Item = @ID and Cpf_Motorista = @Cpf ";

            SqlCommand cmd = new SqlCommand(sql, conn);

            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 999999;

            cmd.Parameters.Add("@ID", SqlDbType.Int);
            cmd.Parameters["@ID"].Value = Convert.ToInt32(Id_Ciot);

            cmd.Parameters.Add("@Cpf", SqlDbType.VarChar,14);
            cmd.Parameters["@Cpf"].Value = Cpf;

            if (cmd.Connection.State == ConnectionState.Closed)
                cmd.Connection.Open();

            Motorista_Ciot motorista_Ciot = new Motorista_Ciot();

            using (SqlDataReader sdr = cmd.ExecuteReader())
            {
                while (sdr.Read())
                {
                    motorista_Ciot.Id = Convert.ToInt32(sdr["Id"]);
                    motorista_Ciot.Cpf_Motorista = sdr["Cpf_Motorista"].ToString();
                    motorista_Ciot.Data_Geracao = Convert.ToDateTime(sdr["Data_Geracao"]);
                    motorista_Ciot.Ciot = sdr["Ciot"].ToString();

                    //20230323 ORNELLAS 
                    if (!DBNull.Value.Equals(sdr["Inicio_Viagem"]))
                    {
                        motorista_Ciot.Inicio_Viagem = Convert.ToDateTime(sdr["Inicio_Viagem"]);
                    }
                    //20230323 ORNELLAS 
                    if (!DBNull.Value.Equals(sdr["Final_Viagem"]))
                    {
                        motorista_Ciot.Inicio_Viagem = Convert.ToDateTime(sdr["Final_Viagem"]);
                    }

                    motorista_Ciot.Id_Fopag_Ciot_Item = Convert.ToInt32(sdr["Id_Fopag_Ciot_Item"]);
                    motorista_Ciot.Dt_Inclusao = Convert.ToDateTime(sdr["Dt_Inclusao"]);
                    motorista_Ciot.Cv = sdr["Cv"].ToString();
                    motorista_Ciot.Tp_Finalizado = Convert.ToInt32(sdr["Tp_Finalizado"]);
                    motorista_Ciot.Usuario_Finalizacao = sdr["Usuario_Finalizacao"].ToString();
                    if (sdr["Dt_Finalizacao"].ToString() != string.Empty && sdr["Dt_Finalizacao"].ToString() != DateTime.MinValue.ToString())
                    {
                        motorista_Ciot.Dt_Finalizacao = Convert.ToDateTime(sdr["Dt_Finalizacao"]);
                    }
                    motorista_Ciot.Placa = sdr["Placa"].ToString();
                    motorista_Ciot.Cpf_Proprietario = sdr["Cpf_Proprietario"].ToString();
                    motorista_Ciot.Ciot_Protocolo_Encerramento = sdr["Ciot_Protocolo_Encerramento"].ToString();
                    motorista_Ciot.Cnpj_Destino = sdr["Cnpj_Destino"].ToString();
                    motorista_Ciot.Msg_Erro = sdr["Msg_Erro"].ToString();
                }
                sdr.Close();
                sdr.Dispose();
            }

            conn.Close();
            conn.Dispose();

            return motorista_Ciot;
        }

        public Config_CIOT Get_Config_CIOT()
        {
            string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(strConexao);

            string sql = "select cnpj ,token ,versao ,ponto_emissao from config_ciot";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 999999;

            if (cmd.Connection.State == ConnectionState.Closed)
                cmd.Connection.Open();

            Config_CIOT configCiot = new Config_CIOT();

            using (SqlDataReader sdr = cmd.ExecuteReader())
            {
                while (sdr.Read())
                {
                    configCiot.Cnpj = sdr["cnpj"].ToString();
                    configCiot.Token = sdr["token"].ToString();
                    configCiot.Versao = sdr["versao"].ToString();
                    configCiot.Ponto_Emissao = sdr["ponto_emissao"].ToString();                    
                }
                sdr.Close();
                sdr.Dispose();
            }

            conn.Close();
            conn.Dispose();

            return configCiot;
        }

        public CIOT get_Dados_Ciot(string Id_Ciot, string Cpf)
        {
            string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(strConexao);

            string sql = " select fciot.nome as NomeTerceiro, fciot.cpf as CpfTerceiro, fciot.data_pagto as DataPagamento, "+
                         " fciot.rend_bruto as RendimentoBruto, fciot.ir_retido as IrRetido, fciot.contribuicao_inss as ContribuicaoInss, "+
                         " fciot.inss_transp as InssTransp, fciot.banco as CodigoBanco, fciot.nome_do_banco as NomeBanco, "+
                         " fciot.agencia,  fciot.conta_corrente + fciot.digito as ContaCorrente, "+
                         " isnull(fciot.saldo,0) as saldo, isnull(fciot.data_pagto_saldo,'') as DtPagamentoSaldo, " +
                         " mciot.inicio_viagem as InicioOperacao, mciot.final_viagem as TerminoOperacao, mciot.data_geracao, "+
                         " mciot.dt_inclusao,  mciot.usua_inclusao as Usuario, "+
                         " isnull(mciot.placa,'') as placa, isnull(mciot.cpf_proprietario,'') as CpfProprietario, "+
                         " isnull(mciot.cnpj_destino,'') as CnpjDestinatario "+
                         " from fopag_ciot_item fciot with(nolock) inner join " +
                         " motorista_ciot mciot with(nolock) on mciot.cpf_motorista = fciot.cpf and " +
                         " mciot.id_fopag_ciot_item = fciot.id  " +
                         " where mciot.id_fopag_ciot_item = @id_ciot and mciot.cpf_motorista = @Cpf "; 

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 999999;

            cmd.Parameters.Add("@id_ciot", SqlDbType.Int);
            cmd.Parameters["@id_ciot"].Value = Convert.ToInt32(Id_Ciot);

            cmd.Parameters.Add("@Cpf", SqlDbType.VarChar,14);
            cmd.Parameters["@Cpf"].Value = Cpf;

            if (cmd.Connection.State == ConnectionState.Closed)
                cmd.Connection.Open();

            CIOT dadosCIOT = new CIOT();

            using (SqlDataReader sdr = cmd.ExecuteReader())
            {
                while (sdr.Read())
                {
                    dadosCIOT.NomeTerceiro = sdr["NomeTerceiro"].ToString();
                    dadosCIOT.CpfTerceiro = sdr["CpfTerceiro"].ToString();
                    dadosCIOT.DataPagamento = Convert.ToDateTime(sdr["DataPagamento"]);
                    dadosCIOT.RendimentoBruto = Convert.ToDouble(sdr["RendimentoBruto"]);
                    dadosCIOT.IrRetido  = Convert.ToDouble(sdr["IrRetido"]);
                    dadosCIOT.ContribuicaoInss =  Convert.ToDouble(sdr["ContribuicaoInss"]);
                    dadosCIOT.InssTransp = Convert.ToDouble(sdr["InssTransp"]);
                    dadosCIOT.CodigoBanco = Convert.ToInt32(sdr["CodigoBanco"]);
                    dadosCIOT.NomeBanco = sdr["NomeBanco"].ToString();
                    dadosCIOT.Agencia= Convert.ToInt32(sdr["Agencia"]);
                    dadosCIOT.ContaCorrente = sdr["ContaCorrente"].ToString();
                    dadosCIOT.Saldo = Convert.ToDouble(sdr["Saldo"]);
                    dadosCIOT.DtPagamentosaldo = Convert.ToDateTime(sdr["DtPagamentoSaldo"]);
                    dadosCIOT.InicioOperacao = Convert.ToDateTime(sdr["InicioOperacao"]);
                    dadosCIOT.TerminoOperacao = Convert.ToDateTime(sdr["TerminoOperacao"]);
                    dadosCIOT.Usuario= sdr["Usuario"].ToString();
                    dadosCIOT.Placa            = sdr["Placa"].ToString();
                    dadosCIOT.CpfProprietario  = sdr["CpfProprietario"].ToString();
                    dadosCIOT.CnpjDestinatario = sdr["CnpjDestinatario"].ToString();
                }
            }

            conn.Close();
            conn.Dispose();

            return dadosCIOT;
        }

        public List<Bancos> get_LstBancos()
        {
            string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(strConexao);

            string sql = " select Cod_banco, Ds_Banco as Nome_Banco, " +
                         " right('000' + cast(Cod_banco as varchar(3)),3) + ' - ' +  Ds_Banco as Ds_Banco "+
                         " from Bancos order by Ds_Banco";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 999999;

            if (cmd.Connection.State == ConnectionState.Closed)
                cmd.Connection.Open();

            List<Bancos> lstBancos = new List<Bancos>();

            using (SqlDataReader sdr = cmd.ExecuteReader())
            {
                while (sdr.Read())
                {
                    Bancos dadosBaco = new Bancos();
                    dadosBaco.Cod_Banco = Convert.ToInt32(sdr["Cod_banco"]);
                    dadosBaco.Ds_Banco = sdr["Ds_Banco"].ToString();
                    dadosBaco.Nome_Banco = sdr["Nome_Banco"].ToString();
                    lstBancos.Add(dadosBaco);
                }
            }

            conn.Close();
            conn.Dispose();

            return lstBancos;
        }

        public bool sessaoAtiva(string session, DataTable dtSession, ref string usuario, string nomeMenu = "")
        {
            DataTable dt = dtSession;

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
            PSession.Value = session;

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

            if (PenabledMenu.Value.ToString() == "N")
                return false;

            Pdesc_nivel2.Value = nomeMenu;

            if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();

          
            cmd.Dispose();
            conn.Dispose();
            
            return true; 

        }

        public string GetXmlCiot(string ID_FOPAG_CIOT_ITEM)
        {
            string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(strConexao);

            string sql = " select xml_ciot from ciot_xml where id_fopag_ciot_item = " + ID_FOPAG_CIOT_ITEM;

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 999999;

            if (cmd.Connection.State == ConnectionState.Closed)
                cmd.Connection.Open();

            string xmlCiot = "";

            using (SqlDataReader sdr = cmd.ExecuteReader())
            {
                sdr.Read();
                xmlCiot = sdr["xml_ciot"].ToString();
            }

            cmd.Clone();
            cmd.Dispose();

            return xmlCiot;
        }

        public string GetCIOT_Gerar_OT(string CNPJ, string Token, string Id_Fopag_Ciot_Item, string Versao, string id_sessao)
        {
            string GUID = "";

            string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(strConexao);

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
            pPonto_Emissao_OT.Value = "Matriz";

            SqlParameter pID_FOPAG_CIOT_ITEM_OT = cmd_OT_RAW.Parameters.Add("@ID_FOPAG_CIOT_ITEM", SqlDbType.Int);
            pID_FOPAG_CIOT_ITEM_OT.Value = Id_Fopag_Ciot_Item;

            SqlParameter PXML_OT_RAW = new SqlParameter();
            PXML_OT_RAW.ParameterName = "@XML";
            PXML_OT_RAW.SqlDbType = System.Data.SqlDbType.VarChar;
            PXML_OT_RAW.Size = -1;
            PXML_OT_RAW.Direction = ParameterDirection.ReturnValue;
            cmd_OT_RAW.Parameters.Add(PXML_OT_RAW).Direction = ParameterDirection.Output;

            SqlParameter pVersao = cmd_OT_RAW.Parameters.Add("@Versao", SqlDbType.Text);
            pVersao.Value = Versao;


            SqlParameter pSessao = cmd_OT_RAW.Parameters.Add("@session", SqlDbType.Text);
            pSessao.Value = id_sessao;

            if (cmd_OT_RAW.Connection.State == ConnectionState.Closed) cmd_OT_RAW.Connection.Open();

            SqlDataReader dr_OT_RAW = cmd_OT_RAW.ExecuteReader();

            string strXml_OT_RAW;

            if (DBNull.Value.Equals(cmd_OT_RAW.Parameters["@XML"].Value))
            {
                strXml_OT_RAW = "Não foi possível gerar XML!";
            }
            else
            {
                strXml_OT_RAW = cmd_OT_RAW.Parameters["@XML"].Value.ToString();
            }

            dr_OT_RAW.Close();
            cmd_OT_RAW.Dispose();
            conn.Close();

            return strXml_OT_RAW;
        }

        public string getXMLCIOTAssinadoAPI(int idFopagCiotItem, string strXml_OT_RAW, string session)
        {

            string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(strConexao);

            SqlCommand cmd = new SqlCommand("getXMLCIOTAssinadoAPI", conn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.CommandTimeout = 999999;

            SqlParameter PSession =
                cmd.Parameters.Add("@session", SqlDbType.VarChar, 20);
            PSession.Value = session;

            SqlParameter Pid_fopag_ciot_item =
                cmd.Parameters.Add("@id_fopag_ciot_item", SqlDbType.Int);
            Pid_fopag_ciot_item.Value = idFopagCiotItem;

            SqlParameter Pxml =
                cmd.Parameters.Add("@xml", SqlDbType.VarChar, -1);
            Pxml.Value = strXml_OT_RAW;

            SqlParameter Pret_msg =
                cmd.Parameters.Add("@ret_msg", SqlDbType.VarChar, 255);

            Pret_msg.Direction = ParameterDirection.Output;


            SqlParameter PxmlAssinado =
               cmd.Parameters.Add("@ret_xml_assinado", SqlDbType.VarChar, -1);

            PxmlAssinado.Direction = ParameterDirection.Output;


            if (conn.State == ConnectionState.Closed) conn.Open();

            cmd.ExecuteNonQuery();

            if (Pret_msg.Value.ToString() != "")
            {
                return Pret_msg.Value.ToString();                
            }

            string xmlAssinado = PxmlAssinado.Value.ToString();

            if (conn.State == ConnectionState.Open) 
                conn.Close();

            return xmlAssinado;

        }

        public string getXMLCancelaCIOTAssinadoAPI(int idFopagCiotItem, string strXml_OT_RAW, string session)
        {

            string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(strConexao);

            SqlCommand cmd = new SqlCommand("getXMLCancelaCIOTAssinadoAPI", conn);

            cmd.CommandType = CommandType.StoredProcedure;

            cmd.CommandTimeout = 999999;

            SqlParameter PSession =
                cmd.Parameters.Add("@session", SqlDbType.VarChar, 20);
            PSession.Value = session;

            SqlParameter Pid_fopag_ciot_item =
                cmd.Parameters.Add("@id_fopag_ciot_item", SqlDbType.Int);
            Pid_fopag_ciot_item.Value = idFopagCiotItem;

            SqlParameter Pxml =
                cmd.Parameters.Add("@xml", SqlDbType.VarChar, -1);
            Pxml.Value = strXml_OT_RAW;

            SqlParameter Pret_msg =
                cmd.Parameters.Add("@ret_msg", SqlDbType.VarChar, 255);

            Pret_msg.Direction = ParameterDirection.Output;


            SqlParameter PxmlAssinado =
               cmd.Parameters.Add("@ret_xml_assinado", SqlDbType.VarChar, -1);

            PxmlAssinado.Direction = ParameterDirection.Output;


            if (conn.State == ConnectionState.Closed) conn.Open();

            cmd.ExecuteNonQuery();

            if (Pret_msg.Value.ToString() != "")
            {
                return Pret_msg.Value.ToString();
            }

            string xmlAssinado = PxmlAssinado.Value.ToString();

            if (conn.State == ConnectionState.Open)
                conn.Close();

            return xmlAssinado;

        }

        public DataSet getDadosMotoristaCIOT(Motorista_Ciot_Dados motoristaCitodados)
        {
            string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;
            SqlCommand cmd = new SqlCommand("dbo.getDadosMotoristaCIOT", new SqlConnection(strConexao));

            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 999999;

            SqlParameter PCPF_MOTORISTA = cmd.Parameters.Add("@CPF_MOTORISTA", SqlDbType.Text);
            PCPF_MOTORISTA.Value = motoristaCitodados.Cpf;

            SqlParameter PNOME_MOTORISTA = cmd.Parameters.Add("@NOME_MOTORISTA", SqlDbType.Text);
            PNOME_MOTORISTA.Value = motoristaCitodados.NomeMotorista;

            SqlParameter PID_IMPORTACAO = cmd.Parameters.Add("@ID_IMPORTACAO", SqlDbType.Int);
            PID_IMPORTACAO.Value = motoristaCitodados.IdImportacao;

            SqlParameter PSEPARA_INSS = cmd.Parameters.Add("@SEPARA_INSS", SqlDbType.Bit);
            PSEPARA_INSS.Value = 1;

            SqlParameter Pnumero_ciot = cmd.Parameters.Add("@NUMERO_CIOT", SqlDbType.VarChar, 12);
            Pnumero_ciot.Value = motoristaCitodados.Ciot;

            SqlParameter Ptp_motorista = cmd.Parameters.Add("@TP_MOTORISTA", SqlDbType.VarChar, 1);
            Ptp_motorista.Value = motoristaCitodados.TpMotorista;

            SqlParameter Ptp_situacao = cmd.Parameters.Add("@TP_SITUACAO", SqlDbType.VarChar, 1);
            Ptp_situacao.Value = motoristaCitodados.Tp_Situacao;

            SqlParameter Ptp_Importacao = cmd.Parameters.Add("@TP_IMPORTACAO", SqlDbType.VarChar, 3);
            Ptp_Importacao.Value = motoristaCitodados.Tp_Importacao;

            if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();

            SqlDataAdapter da = new SqlDataAdapter();
            DataSet dt = new DataSet();

            da.SelectCommand = cmd;
            da.Fill(dt);

            if (cmd.Connection.State == ConnectionState.Open) 
                cmd.Connection.Close();

            return dt;
        }

        public string Get_Xml_CIOT_Cancelar_OT(Config_CIOT configCiot, string motivo_Cancelamento, string GUID, int Id_Fopag_Ciot_Item)
        {
            string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(strConexao);

            SqlCommand cmd_OT_RAW = new SqlCommand("dbo.CIOT_Cancelar_OT", conn);

            cmd_OT_RAW.CommandType = CommandType.StoredProcedure;
            cmd_OT_RAW.CommandTimeout = 999999;

            SqlParameter pCNPJ_OT_RAW = cmd_OT_RAW.Parameters.Add("@CNPJ", SqlDbType.VarChar,14);
            pCNPJ_OT_RAW.Value = configCiot.Cnpj;

            SqlParameter pToken_OT_RAW = cmd_OT_RAW.Parameters.Add("@Token", SqlDbType.VarChar,24);
            pToken_OT_RAW.Value = configCiot.Token;

            SqlParameter pGUID_OT_RAW = cmd_OT_RAW.Parameters.Add("@GUID", SqlDbType.VarChar,40);
            pGUID_OT_RAW.Value = GUID;

            SqlParameter pMotivo_Cancelamento = cmd_OT_RAW.Parameters.Add("@Motivo_Cancelamento", SqlDbType.VarChar, 255);
            pMotivo_Cancelamento.Value = motivo_Cancelamento;

            SqlParameter pID_FOPAG_CIOT_ITEM_OT = cmd_OT_RAW.Parameters.Add("@ID_FOPAG_CIOT_ITEM", SqlDbType.Int);
            pID_FOPAG_CIOT_ITEM_OT.Value = Id_Fopag_Ciot_Item;

            SqlParameter PXML_OT_RAW = new SqlParameter();
            PXML_OT_RAW.ParameterName = "@XML";
            PXML_OT_RAW.SqlDbType = System.Data.SqlDbType.VarChar;
            PXML_OT_RAW.Size = -1;
            PXML_OT_RAW.Direction = ParameterDirection.ReturnValue;
            cmd_OT_RAW.Parameters.Add(PXML_OT_RAW).Direction = ParameterDirection.Output;

            SqlParameter pVersao = cmd_OT_RAW.Parameters.Add("@Versao", SqlDbType.Text);
            pVersao.Value = "4.2.11.0";//configCiot.Versao;

            if (cmd_OT_RAW.Connection.State == ConnectionState.Closed) cmd_OT_RAW.Connection.Open();

            SqlDataReader dr_OT_RAW = cmd_OT_RAW.ExecuteReader();

            string strXml_OT_RAW;

            if (DBNull.Value.Equals(cmd_OT_RAW.Parameters["@XML"].Value))
            {
                strXml_OT_RAW = "Não foi possível gerar XML!";
            }
            else
            {
                strXml_OT_RAW = cmd_OT_RAW.Parameters["@XML"].Value.ToString();
            }

            dr_OT_RAW.Close();
            cmd_OT_RAW.Dispose();
            conn.Close();

            return strXml_OT_RAW;
        }

        public void Grava_CancelamentoCIOT(Motorista_Ciot_Cancelado mto_Ciot_Canc)
        {
            string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(strConexao);

            SqlCommand cmd_OT = new SqlCommand("dbo.Stored_Grava_Motorista_Ciot_Cancelado", conn);
            cmd_OT.CommandType = CommandType.StoredProcedure;
            cmd_OT.CommandTimeout = 999999;

            SqlParameter pId_Ciot = cmd_OT.Parameters.Add("@Id_Ciot", SqlDbType.Int);
            SqlParameter pCpf_Motorista = cmd_OT.Parameters.Add("@Cpf_Motorista", SqlDbType.VarChar, 14);
            SqlParameter pCiot = cmd_OT.Parameters.Add("@Ciot", SqlDbType.VarChar, 12);
            SqlParameter pCiot_Protocolo_Cancelamento = cmd_OT.Parameters.Add("@Ciot_Protocolo_Cancelamento", SqlDbType.VarChar, 15);
            SqlParameter pDt_Cancelamento = cmd_OT.Parameters.Add("@Dt_Cancelamento", SqlDbType.DateTime);
            SqlParameter pTp_Cancelamento = cmd_OT.Parameters.Add("@Tp_Cancelamento", SqlDbType.Bit);
            SqlParameter pUsuario_Cancelamento = cmd_OT.Parameters.Add("@Usuario_Cancelamento", SqlDbType.VarChar, 20);
            SqlParameter pMotivo_Cancelamento = cmd_OT.Parameters.Add("@Motivo_Cancelamento", SqlDbType.VarChar, 255);
            SqlParameter pRetorno_Ndd = cmd_OT.Parameters.Add("@Retorno_Ndd", SqlDbType.VarChar, 255);


            pId_Ciot.Value = mto_Ciot_Canc.Id_Ciot;
            pCpf_Motorista.Value = mto_Ciot_Canc.Cpf_Motorista;
            pCiot.Value = mto_Ciot_Canc.Ciot;
            pCiot_Protocolo_Cancelamento.Value = mto_Ciot_Canc.Ciot_Protocolo_Cancelamento;
            if (mto_Ciot_Canc.Dt_Cancelamento != null)
                pDt_Cancelamento.Value = mto_Ciot_Canc.Dt_Cancelamento;
            pTp_Cancelamento.Value = mto_Ciot_Canc.Tp_Cancelamento;
            pUsuario_Cancelamento.Value = mto_Ciot_Canc.Usuario_Cancelamento;
            pMotivo_Cancelamento.Value = mto_Ciot_Canc.Motivo_Cancelamento;
            pRetorno_Ndd.Value = mto_Ciot_Canc.Retorno_Ndd;

            if (cmd_OT.Connection.State == ConnectionState.Closed)
                cmd_OT.Connection.Open();
            try
            {
                var retorno = cmd_OT.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void Atualiza_Valores_Ciot(int id_fopag_ciot_item, Valores_CIOT valores_Recalc_Ciot)
        {
            string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(strConexao);

            SqlCommand cmd_OT = new SqlCommand("dbo.Stored_Atualiza_Valores_Ciot", conn);

            cmd_OT.CommandType = CommandType.StoredProcedure;
            cmd_OT.CommandTimeout = 999999;

            SqlParameter pId_Fopag_Ciot_Item = cmd_OT.Parameters.Add("@id_fopag_ciot_item", SqlDbType.Int);
            pId_Fopag_Ciot_Item.Value = id_fopag_ciot_item;

            SqlParameter pRend_Bruto = cmd_OT.Parameters.Add("@rend_bruto", SqlDbType.Money, -1);
            pRend_Bruto.Direction = ParameterDirection.Input;

            SqlParameter pContribuicao_Inss =
                cmd_OT.Parameters.Add("@contribuicao_inss", SqlDbType.Money, -1);
            pContribuicao_Inss.Direction = ParameterDirection.Input;

            SqlParameter pIR_Retido =
                cmd_OT.Parameters.Add("@IR_Retido", SqlDbType.Money, -1);
            pIR_Retido.Direction = ParameterDirection.Input;

            SqlParameter pValor_ISS =
                cmd_OT.Parameters.Add("@Valor_ISS", SqlDbType.Money, -1);
            pValor_ISS.Direction = ParameterDirection.Input;

            SqlParameter pOutros_Desc =
                cmd_OT.Parameters.Add("@Outros_Desc", SqlDbType.Money, -1);
            pOutros_Desc.Direction = ParameterDirection.Input;

            SqlParameter pInss_Transp =
                cmd_OT.Parameters.Add("@INSS_Transp", SqlDbType.Money, -1);
            pInss_Transp.Direction = ParameterDirection.Input;

            SqlParameter pValor_Liquido =
                cmd_OT.Parameters.Add("@Valor_Liquido", SqlDbType.Money, -1);
            pValor_Liquido.Direction = ParameterDirection.Input;

            SqlParameter pPropaganda =
                cmd_OT.Parameters.Add("@Propaganda", SqlDbType.Decimal, -1);
            pPropaganda.Direction = ParameterDirection.Input;

            SqlParameter pTotal =
                cmd_OT.Parameters.Add("@Total", SqlDbType.Decimal, -1);
            pTotal.Direction = ParameterDirection.Output;

            SqlParameter pSaldo =
               cmd_OT.Parameters.Add("@saldo", SqlDbType.Decimal, -1);
            pSaldo.Direction = ParameterDirection.Output;

            pRend_Bruto.Value = valores_Recalc_Ciot.Rend_Bruto;
            pContribuicao_Inss.Value = valores_Recalc_Ciot.Contribuicao_Inss;
            pIR_Retido.Value = valores_Recalc_Ciot.IR_Retido;
            pValor_ISS.Value = valores_Recalc_Ciot.Valor_ISS;
            pOutros_Desc.Value = valores_Recalc_Ciot.Outros_Desc;
            pInss_Transp.Value = valores_Recalc_Ciot.INSS_Transp;
            pPropaganda.Value = valores_Recalc_Ciot.Propaganda;
            pTotal.Value = valores_Recalc_Ciot.Total;
            pValor_Liquido.Value = valores_Recalc_Ciot.Valor_Liquido;

            if (conn.State == ConnectionState.Closed) conn.Open();

            cmd_OT.ExecuteNonQuery();

            conn.Close();
            conn.Dispose();
        }

        public Valores_CIOT get_ValoresCiot(int id_Ciot_Item)
        {
            string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(strConexao);

            string sql = "select [data_pagto] " +
                          " ,isnull([rend_bruto],0) rend_bruto" +
                          " ,isnull([contribuicao_inss],0) contribuicao_inss " +
                          "	,isnull([IR_Retido],0) IR_Retido " +
                          " ,isnull([Valor_ISS],0) Valor_ISS " +
                          " ,isnull([Outros_Desc],0) Outros_Desc " +
                          "	,isnull([INSS_Transp],0) INSS_Transp " +
                          " ,isnull([Valor_Liquido],0) Valor_Liquido " +
                          " ,isnull([Propaganda],0) Propaganda" +
                          "	,isnull([Total],0) Total" +
                          " ,isnull([saldo],0) saldo " +
                          " ,[data_pagto_saldo] " +
                          "from [dbo].[FOPAG_CIOT_item] " +
                          "where ID = @id_ciot_item ";

            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = 999999;

            cmd.Parameters.Add("@id_ciot_item", SqlDbType.Int);
            cmd.Parameters["@id_ciot_item"].Value = Convert.ToInt32(id_Ciot_Item);

            if (cmd.Connection.State == ConnectionState.Closed)
                cmd.Connection.Open();

            Valores_CIOT vlrsCiot = new Valores_CIOT();

            using (SqlDataReader sdr = cmd.ExecuteReader())
            {
                while (sdr.Read())
                {
                    vlrsCiot.Data_Pagto = Convert.ToDateTime(sdr["data_pagto"]);
                    vlrsCiot.Contribuicao_Inss = Convert.ToDouble(sdr["Contribuicao_Inss"]);

                    if (sdr["data_pagto_saldo"].ToString() != "")
                        vlrsCiot.Data_Pagto_Saldo = Convert.ToDateTime(sdr["data_pagto_saldo"]);

                    vlrsCiot.INSS_Transp = Convert.ToDouble(sdr["INSS_Transp"]);
                    vlrsCiot.IR_Retido = Convert.ToDouble(sdr["IR_Retido"]);
                    vlrsCiot.Outros_Desc = Convert.ToDouble(sdr["Outros_Desc"]);
                    vlrsCiot.Propaganda = Convert.ToDouble(sdr["Propaganda"]);
                    vlrsCiot.Rend_Bruto = Convert.ToDouble(sdr["Rend_Bruto"]);
                    vlrsCiot.Saldo = Convert.ToDouble(sdr["Saldo"]);
                    vlrsCiot.Total = Convert.ToDouble(sdr["Total"]);
                    vlrsCiot.Valor_ISS = Convert.ToDouble(sdr["Valor_ISS"]);
                    vlrsCiot.Valor_Liquido = Convert.ToDouble(sdr["Valor_Liquido"]);
                }
            }

            return vlrsCiot;

        }

        public Valores_CIOT getCalcValoresCiot(string cpfMotorista, Valores_CIOT vlrsCiot)
        {
            string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(strConexao);

            SqlCommand cmd_OT = new SqlCommand("dbo.getCalculaVlrCIOT", conn);
            cmd_OT.CommandType = CommandType.StoredProcedure;
            cmd_OT.CommandTimeout = 999999;

            SqlParameter pCpfMotorista = cmd_OT.Parameters.Add("@cpf", SqlDbType.VarChar);
            pCpfMotorista.Value = cpfMotorista;

            SqlParameter pRend_Bruto = cmd_OT.Parameters.Add("@rend_bruto", SqlDbType.Money);

            SqlParameter pContribuicao_Inss =
                cmd_OT.Parameters.Add("@contribuicao_inss", SqlDbType.Money, -1);
            pContribuicao_Inss.Direction = ParameterDirection.InputOutput;

            SqlParameter pIR_Retido =
                cmd_OT.Parameters.Add("@IR_Retido", SqlDbType.Money, -1);
            pIR_Retido.Direction = ParameterDirection.InputOutput;

            SqlParameter pValor_ISS =
                cmd_OT.Parameters.Add("@Valor_ISS", SqlDbType.Money, -1);
            pValor_ISS.Direction = ParameterDirection.InputOutput;

            SqlParameter pOutros_Desc =
                cmd_OT.Parameters.Add("@Outros_Desc", SqlDbType.Money, -1);
            pOutros_Desc.Direction = ParameterDirection.InputOutput;

            SqlParameter pInss_Transp =
                cmd_OT.Parameters.Add("@INSS_Transp", SqlDbType.Money, -1);
            pInss_Transp.Direction = ParameterDirection.InputOutput;

            SqlParameter pValor_Liquido =
                cmd_OT.Parameters.Add("@Valor_Liquido", SqlDbType.Money, -1);
            pValor_Liquido.Direction = ParameterDirection.Output;

            SqlParameter pPropaganda =
                cmd_OT.Parameters.Add("@Propaganda", SqlDbType.Money, -1);
            pPropaganda.Direction = ParameterDirection.InputOutput;

            SqlParameter pTotal =
                cmd_OT.Parameters.Add("@Total", SqlDbType.Money, -1);
            pTotal.Direction = ParameterDirection.InputOutput;

            SqlParameter pVl_Pedagio =
                cmd_OT.Parameters.Add("@vl_pedagio", SqlDbType.Money, -1);
            pVl_Pedagio.Direction = ParameterDirection.InputOutput;

            SqlParameter pSaldo =
               cmd_OT.Parameters.Add("@saldo", SqlDbType.Money, -1);
            pSaldo.Direction = ParameterDirection.InputOutput;

            pRend_Bruto.Value = vlrsCiot.Rend_Bruto;
            pContribuicao_Inss.Value = vlrsCiot.Contribuicao_Inss;
            pIR_Retido.Value = vlrsCiot.IR_Retido;
            pValor_ISS.Value = vlrsCiot.Valor_ISS;
            pOutros_Desc.Value = vlrsCiot.Outros_Desc;
            pInss_Transp.Value = vlrsCiot.INSS_Transp;
            pPropaganda.Value = vlrsCiot.Propaganda;

            if (conn.State == ConnectionState.Closed) conn.Open();

            cmd_OT.ExecuteNonQuery();

            Valores_CIOT vlrsCalcCiot = new Valores_CIOT();

            vlrsCalcCiot.Contribuicao_Inss = Convert.ToDouble(pContribuicao_Inss.Value);
            vlrsCalcCiot.INSS_Transp = Convert.ToDouble(pInss_Transp.Value);
            vlrsCalcCiot.IR_Retido = Convert.ToDouble(pIR_Retido.Value);
            vlrsCalcCiot.Outros_Desc = Convert.ToDouble(pOutros_Desc.Value);
            vlrsCalcCiot.Propaganda = Convert.ToDouble(pPropaganda.Value);
            vlrsCalcCiot.Rend_Bruto = vlrsCiot.Rend_Bruto;
            vlrsCalcCiot.Saldo = Convert.ToDouble(pSaldo.Value);
            vlrsCalcCiot.Vl_Pedagio = Convert.ToDouble(pVl_Pedagio.Value);
            vlrsCalcCiot.Total = Convert.ToDouble(pTotal.Value);

            return vlrsCalcCiot;

        }



    }
}