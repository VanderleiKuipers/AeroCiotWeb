using AeroCIOTWeb.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;

namespace AeroCIOTWeb.DBO
{
    public class DboAlterarCIOT
    {
        public String Get_Xml_CIOT_Alterar_OT(Config_CIOT configCiot, string guId, int id_Ciot, CIOT dadosCiot, string motivoAlteracao)
        {
            string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(strConexao);

            SqlCommand cmd = new SqlCommand("dbo.getXML_CIOT_Alterar_OT", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 999999;

            SqlParameter pCNPJ = cmd.Parameters.Add("@CNPJ", SqlDbType.VarChar,14);
            pCNPJ.Value = configCiot.Cnpj;

            SqlParameter pToken = cmd.Parameters.Add("@Token", SqlDbType.VarChar,24);
            pToken.Value = configCiot.Token;

            SqlParameter pGUID_OT_RAW = cmd.Parameters.Add("@GUID", SqlDbType.VarChar,40);
            pGUID_OT_RAW.Value = guId;

            SqlParameter pID_CIOT = cmd.Parameters.Add("@ID_CIOT", SqlDbType.Int);
            pID_CIOT.Value = id_Ciot;

            SqlParameter pPonto_Emissao = cmd.Parameters.Add("@Ponto_Emissao", SqlDbType.VarChar,50);
            pPonto_Emissao.Value = configCiot.Ponto_Emissao;

            SqlParameter pNomeInstituicao = cmd.Parameters.Add("@NomeInstituicao", SqlDbType.VarChar, 50);
            pNomeInstituicao.Value = dadosCiot.NomeBanco;

            SqlParameter pNumeroInstituicao = cmd.Parameters.Add("@NumeroInstituicao", SqlDbType.VarChar, 05);
            pNumeroInstituicao.Value = dadosCiot.CodigoBanco.ToString();

            SqlParameter pNumeroAgencia = cmd.Parameters.Add("@NumeroAgencia", SqlDbType.VarChar, 10);
            pNumeroAgencia.Value = dadosCiot.Agencia.ToString();

            SqlParameter pConta = cmd.Parameters.Add("@NumeroConta", SqlDbType.VarChar, 10);
            pConta.Value = dadosCiot.ContaCorrente.ToString();

            SqlParameter pMotivo = cmd.Parameters.Add("@Motivo", SqlDbType.VarChar, 255);
            pMotivo.Value = motivoAlteracao;

            SqlParameter PXML = new SqlParameter();
            PXML.ParameterName = "@XML";
            PXML.SqlDbType = System.Data.SqlDbType.VarChar;
            PXML.Size = -1;
            PXML.Direction = ParameterDirection.ReturnValue;
            cmd.Parameters.Add(PXML).Direction = ParameterDirection.Output;

            SqlParameter pVersao = cmd.Parameters.Add("@Versao", SqlDbType.Text);
            pVersao.Value = configCiot.Versao;

            if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();

            SqlDataReader dr = cmd.ExecuteReader();

            String strXml;

            strXml = cmd.Parameters["@XML"].Value.ToString();

            dr.Close();
            cmd.Dispose();
            conn.Close();

            return strXml;
        }

        public void GravaAlteracaoCiot(CIOT Dados_Ciot)
        {
            string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(strConexao);

            SqlCommand cmd = new SqlCommand("dbo.Stored_Grava_Alteracao_Ciot", conn);
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 999999;

            SqlParameter pId_Ciot = cmd.Parameters.Add("@Id_Ciot", SqlDbType.Int);
            pId_Ciot.Value = Dados_Ciot.Id_Ciot;

            SqlParameter pCpf = cmd.Parameters.Add("@Cpf_Motorista", SqlDbType.VarChar,11);
            pCpf.Value = Dados_Ciot.CpfTerceiro;

            SqlParameter pNomeBanco = cmd.Parameters.Add("@NomeInstituicao", SqlDbType.VarChar,50);
            pNomeBanco.Value = Dados_Ciot.NomeBanco;

            SqlParameter pCodBanco = cmd.Parameters.Add("@NumeroInstituicao", SqlDbType.VarChar,5);
            pCodBanco.Value = Dados_Ciot.CodigoBanco.ToString();

            SqlParameter pNAgencia = cmd.Parameters.Add("@NumeroAgencia", SqlDbType.VarChar,10);
            pNAgencia.Value = Dados_Ciot.Agencia.ToString();

            SqlParameter pConta = cmd.Parameters.Add("@NumeroConta", SqlDbType.VarChar,18);
            pConta.Value = Dados_Ciot.Agencia.ToString();

        }

    }
}