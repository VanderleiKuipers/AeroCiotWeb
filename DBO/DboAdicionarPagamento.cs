/*
 KUIPERS                04/04/2024 20240404             Fechando todas as conexões apos a execução da proc 
 */
using AeroCIOTWeb.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;

namespace AeroCIOTWeb.DBO
{
    public class DboAdicionarPagamento
    {
        public string getXMLCIOT_Alterar_Valor_Aplicado_OT(Config_CIOT cfgCiot, Valores_CIOT vlr_Ciot, string session, ref string retMsg)
        {
            string xmlRetorno = "";
            string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(strConexao);

            SqlCommand cmd_OT = new SqlCommand("dbo.CIOT_Alterar_OT_Envio", conn);
            cmd_OT.CommandType = CommandType.StoredProcedure;
            cmd_OT.CommandTimeout = 999999;


            SqlParameter pId_Fopag_Ciot = cmd_OT.Parameters.Add("@ID_FOPAG_CIOT_ITEM", SqlDbType.Int);
            SqlParameter pSession = cmd_OT.Parameters.Add("@session", SqlDbType.VarChar, 20);
            SqlParameter pCnpj = cmd_OT.Parameters.Add("@cnpj", SqlDbType.VarChar, 14);
            SqlParameter pToken = cmd_OT.Parameters.Add("@token", SqlDbType.VarChar, 24);
            SqlParameter pGuid = cmd_OT.Parameters.Add("@guid", SqlDbType.VarChar, 40);
            SqlParameter pVersao = cmd_OT.Parameters.Add("@Versao", SqlDbType.VarChar, 8);
            SqlParameter pData_Pagto = cmd_OT.Parameters.Add("@Data_Pagto", SqlDbType.DateTime);
            SqlParameter pFrete_Valor = cmd_OT.Parameters.Add("@Frete_Valor", SqlDbType.Decimal);
            SqlParameter pPropaganda_Valor = cmd_OT.Parameters.Add("@Propaganda_Valor", SqlDbType.Decimal);
            SqlParameter pIRRF = cmd_OT.Parameters.Add("@IRRF", SqlDbType.Decimal);
            SqlParameter pINSS = cmd_OT.Parameters.Add("@INSS", SqlDbType.Decimal);


            /*
            SqlParameter pSEST_SENAT		=	cmd_OT.Parameters.Add("@SEST_SENAT", SqlDbType.Decimal); 
            SqlParameter pSaldo         	=	cmd_OT.Parameters.Add("@Saldo", SqlDbType.Decimal);
            */

            SqlParameter pRet_Xml = new SqlParameter();
            pRet_Xml.ParameterName = "@ret_xml";
            pRet_Xml.SqlDbType = System.Data.SqlDbType.VarChar;
            pRet_Xml.Size = -1;
            pRet_Xml.Direction = ParameterDirection.ReturnValue;
            cmd_OT.Parameters.Add(pRet_Xml).Direction = ParameterDirection.Output;

            pId_Fopag_Ciot.Value = cfgCiot.Id_Fopag_CIOT;
            pSession.Value = session;
            pToken.Value = cfgCiot.Token;
            pCnpj.Value = cfgCiot.Cnpj;
            pGuid.Value = cfgCiot.GuId;
            pVersao.Value = cfgCiot.Versao;

            pData_Pagto.Value = vlr_Ciot.Data_Pagto;
            pFrete_Valor.Value = Convert.ToDecimal(vlr_Ciot.Rend_Bruto);
            pPropaganda_Valor.Value = Convert.ToDecimal(vlr_Ciot.Propaganda);
            pIRRF.Value = Convert.ToDecimal(vlr_Ciot.IR_Retido);
            pINSS.Value = Convert.ToDecimal(vlr_Ciot.INSS_Transp);

            //20230830
            SqlParameter pretMsg = cmd_OT.Parameters.Add("@ret_msg", SqlDbType.VarChar, 255);
            pretMsg.Direction = ParameterDirection.Output;

            if (cmd_OT.Connection.State == ConnectionState.Closed)
                cmd_OT.Connection.Open();

            //2023083001 SqlDataReader dr_OT = cmd_OT.ExecuteReader();

            cmd_OT.ExecuteNonQuery();

            String strXml_OT;

            strXml_OT = cmd_OT.Parameters["@ret_xml"].Value.ToString();
            xmlRetorno = strXml_OT;

            //2023083001
            if (!pretMsg.Value.Equals(DBNull.Value) && !pretMsg.Value.ToString().Equals(""))
            {
                retMsg = pretMsg.Value.ToString();
            }
            //20240404
            if (cmd_OT.Connection.State == ConnectionState.Open)
                cmd_OT.Connection.Close();

            return xmlRetorno;
        }

        public void Grava_Valor_Aplicado_OT(Parcela_Adicional_OT parcela_Adicional_Ot)
        {
            string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;
            SqlConnection conn = new SqlConnection(strConexao);

            SqlCommand cmd_OT = new SqlCommand("dbo.Stored_Grava_Parcela_Adicional_OT", conn);
            cmd_OT.CommandType = CommandType.StoredProcedure;
            cmd_OT.CommandTimeout = 999999;

            SqlParameter pId_Ciot = cmd_OT.Parameters.Add("@ID_CIOT", SqlDbType.Int);
            SqlParameter pCiot = cmd_OT.Parameters.Add("@CIOT", SqlDbType.VarChar, 12);
            SqlParameter pCnpj = cmd_OT.Parameters.Add("@CNPJ", SqlDbType.VarChar, 14);
            SqlParameter pGuid = cmd_OT.Parameters.Add("@GUID", SqlDbType.VarChar, 40);
            SqlParameter pDt_Previsao = cmd_OT.Parameters.Add("@DATA_PREVISAO", SqlDbType.DateTime);
            SqlParameter pVlr_Aplicado = cmd_OT.Parameters.Add("@VALOR_APLICADO", SqlDbType.Decimal);
            
            pId_Ciot.Value = parcela_Adicional_Ot.Id_Ciot;
            pCiot.Value = parcela_Adicional_Ot.Ciot;
            pCnpj.Value = parcela_Adicional_Ot.Cnpj;
            pGuid.Value = parcela_Adicional_Ot.Guid;
            pDt_Previsao.Value = parcela_Adicional_Ot.Data_Previsao;
            pVlr_Aplicado.Value = parcela_Adicional_Ot.Valor_Aplicado;

            if (cmd_OT.Connection.State == ConnectionState.Closed)
                cmd_OT.Connection.Open();

            try
            {
                var retorno = cmd_OT.ExecuteNonQuery();
                //20240404
                if (cmd_OT.Connection.State == ConnectionState.Open)
                    cmd_OT.Connection.Close();    
            }
            catch (Exception e)
            {
                //20240404
                if (cmd_OT.Connection.State == ConnectionState.Open)
                    cmd_OT.Connection.Close();    

                throw e;
            }
        }
    }
}