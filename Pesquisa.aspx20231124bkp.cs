/*
YUKI            DEBORAH.G       10/06/2019 10:10    20190610    Considera a permissão apenas de terceiro.
YUKI                            04/07/2019 15:15    20190704    Consistir valores numéricos campos[5] a campos[8].
YUKI            FELIPE.C        04/07/2019 15:15    20190704    Pra acesso somente a terceiros, não habilita a finalização do CIOT.
YUKI            DEBORAH.G       12/07/2019 14:08    20190705    Utilização da versão 4.2.8.0  do webservice para gerar CIOT de ETC.
YUKI            DEBORAH.G       23/07/2019 11:47    20190723    Inclusão de adiantamento e saldo. Visualização do DOT.
EVERTON         DEBORAH.G       17/12/2019 16:01    20191217    VALIDAÇOES DE VARIOS CAMPOS...
EVERTON                         15/06/2020 18:13    20200615    PEGAR CERTIFICADO VALIDO
ORNELLAS/EVERTON                15/07/2020 12:31    20200715    Windows solicitando a escolha do certificado visualmente...
EVERTON                         15/07/2020 15:32    20200715    VALIDAR CADASTRO DE PROPRIETARIO/ MOTORISTA-->PLACA
ORNELLAS                        08/03/2021 12:10    20210308    Revogando 20200715... 
 *                                                              Para casos de GUID começando com números, ocorria o seguinte erro ao assinar informando o ID:
 *                                                              NOK - Erro: Ao assinar o documento - Malformed reference element.

ORNELLAS                        15/03/2021 17:27    20210315    Permitir filtrar pelo ID da importação gerada através do ROL...
ORNELLAS                        12/05/2021 14:25    20210512    Permitir efetuar pesquisa através do número do CIOT...
ORNELLAS                        23/07/2021 15:57    20210723    Exibindo detalhes de erro retornado pelo serviço WEB ao tentar exibir o PDF do CIOT...
ORNELLAS                        30/08/2021 10:43    20210830    Registrando responsável pela finalização do CIOT...
ORNELLAS        MARCOS.R        14/03/2022 11:10    20220314    Em 14/03/2022 10:24, Marcos Araujo escreveu:
                                                                Não “finalizar” CIOT se os rols e/ou AWBs vinculados ao respectivo CIOT estiver associado a MDF-e 
                                                                não encerrado...
 * 
ORNELLAS        JESSICA.M       19/10/2022 08:35    20221019    Mensagem "Operação de Transporte já está encerrada" adicionada para reconhecimento de encerramento de CIOT efetuado por fora do sistema...
ORNELLAS                        22/11/2022 07:30    20221122    Permitir cancelamento de CIOT através de chamada remota... 
KUIPERS         DEBORAH         08/05/2023 15:20    20230508    Adicionado parametro id_Sessao, para indentificar o usuário e redirecionar a procedure CIOT_Gerar_OT_RAW para procedure CIOT_Gerar_OT_RAW_NEW, com a versão 4.2.11.0 do ciot 
  
ORNELLAS                        21/07/2023          20230721    Permitir efetuar assinatura digital através da API...
KUIPERS         DEBORAH         28/08/2023 10:11    20230828    Alterada a versão 4.2.8.0 para 4.2.11.0, Implementado envio de dados de pagadoria e cartao, para pagamento do motorista  
ORNELLAS        MARCOS.R        28/08/2023 14:24    2023082801  Permitir finalizar CIOT não gerado através da FOPAG...   
  
ORNELLAS        MARCOS.R        10/10/2023 10:31    20231010    Permitir consultar status do CIOT antes de emitir o MDF-e...  
                                                                [ Caso de MDF-e emitido com CIOT cancelado...]
 * 
KUIPERS                         24/11/2023 10:00    20231124    Finalizar o CIOT no sistema quando o status na NDD estiver cancelado(status = 2)
 *                                                              Gravação do XML antes do envio para o webservices para consulta posteiror.                
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


public partial class AeroCIOT_Pesquisa : System.Web.UI.Page
{
    string CNPJ = "01014373000265";
    string Token = "012345678901234567891234";
    string Ponto_Emissao = "Matriz";
    //string Nome = "CN=AEROSOFT CARGAS AEREAS LTDA:01014373000184, OU=Autenticado por AR Certifique Online, OU=RFB e-CNPJ A1, OU=Secretaria da Receita Federal do Brasil - RFB, L=Sao Paulo, S=SP, O=ICP-Brasil, C=BR";
    string Nome = "";
    string usuario = "";
    SqlConnection conn = null;
    bool atualiza = true;
    bool apenasTerceiro = false;
    //20190705
    //20230823
    string Versao = "4.2.8.0";  //ou 4.2.11.0


    //20210723
    private string lastError = "";

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

        //20200615
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

        //20210308 Permitir testar assinatura digital de um XML informado...
        string idSeqXMLStr = Request.QueryString.Get("idSeqXMLTeste");
        if (idSeqXMLStr != null && !idSeqXMLStr.Equals(""))
        { 
        
            int idSeqXML = Convert.ToInt32(Request.QueryString.Get("idSeqXMLTeste"));

            Response.Write(testarAssinatura(idSeqXML));
            Response.End();
            
        }

        //20231010 Consulta de resumo do CIOT para permitir identificar se o mesmo está cancelado antes de gerar o MDF-e...
        string consultar = Request.QueryString.Get("consultar");

        if (consultar != null && !consultar.Equals("")) {

            int idFoPagItem = 0;
            
            string ciot = "";

            try
            {
                idFoPagItem = Convert.ToInt32(Request.QueryString.Get("idFoPagItem"));
                
                ciot = Request.QueryString.Get("ciot");

                if (ciot == null || ciot.Equals("") || idFoPagItem == 0)
                {

                    Response.Write("Informe o número do CIOT e o ID do item do FOPAG!");

                }
                else
                {
                    lastError = "";

                    if (consultarResumoCIOT(ciot, idFoPagItem))
                    {
                        Response.Write("OK");
                    }
                    else {

                        if (lastError != null && !lastError.Equals(""))
                        {

                            Response.Write("NOK|" + lastError);
                        }
                        else
                        {
                            Response.Write("NOK");
                        }
                    };
                }

            }
            catch (Exception ex)
            {

                Response.Write(ex.Message);
            }

            Response.End();

            return;

        }

        //20221122 Encerramento automático de CIOT...
        string encerrar = Request.QueryString.Get("encerrar");
        
        if (encerrar != null && !encerrar.Equals("")) {



            int ID_FOPAG_CIOT_ITEM = 0;

            try
            {
                ID_FOPAG_CIOT_ITEM = Convert.ToInt32(Request.QueryString.Get("idFoPagItem"));
                //20230828
                //Versao = "4.2.11.0";
                Versao = "4.2.8.0";

                FinalizaCIOT(ID_FOPAG_CIOT_ITEM, Versao);
            }
            catch (Exception ex) {

                Response.Write(ex.Message);
            }

            Response.End();

            return;
        
        }

        if (Request.QueryString.Get("terceiro") == "S")
        {

              //<div visible="true" id="Terceiro" runat="server" style="overflow: auto; height: 100%; width:100%;">

            CIOT.Visible = false;
            Terceiro.Visible = true;

        }
        else
        {
            CIOT.Visible = true;
            Terceiro.Visible = false;

            if (!IsPostBack)
            {

                //20210315
                string idImportacaoStr = Request.QueryString.Get("id");
                int ? idImportacao = null;

                dsImportacao.SelectParameters["id"].DefaultValue = "0";

                if (idImportacaoStr != null && !idImportacaoStr.Equals("")) {

                    try
                    {
                        idImportacao = Convert.ToInt32(idImportacaoStr);

                        dsImportacao.SelectParameters["id"].DefaultValue = Convert.ToString(idImportacao);
                    }
                    catch {
                        idImportacao = null;
                    }
                }

                //20190610
                ddImportacao.DataBind();

                if (apenasTerceiro)
                {
                    ddImportacao.SelectedIndex = ddImportacao.Items.Count - 1;
                    //ddImportacao.SelectedIndexChanged();
                    ddImportacao.Enabled = false;
                    //20190704
                    chkFinalizarAnterior.Enabled=false;
                    btnFinalizarCIOT.Enabled = false;
                }

                
                AtualizaGrid();
            }

            btnExcluir.OnClientClick = "return confirm('Confirma a exclusão do CIOT?')";
        }

        btnDownloadDOT.Visible = false;

    }


    protected void gvPesquisa_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName.Equals("Selecionar") )
        {
            int index = Convert.ToInt32(e.CommandArgument);

            GridView gv = (GridView)sender;

            gv.SelectedIndex = index;

            GridViewRow row = gv.Rows[index];

            return; 
       }

        if (e.CommandName.Equals("VisualizarCIOT"))
        {

            int index = Convert.ToInt32(e.CommandArgument);

            GridView gv = (GridView)sender;

            gv.SelectedIndex = index;

            GridViewRow row = gv.Rows[index];

            string strRetorno = ""; //, strErro = "", strXMLEnvio = "", strXMLConsulta = "";
            bool deuErro = false;

            //20230828
            //Versao = "4.2.11.0";
            Versao = "4.2.8.0";
            
            strRetorno = DownloadDOT(CNPJ, Token, row, Versao);
            //                if (strRetorno == "Falha ao assinar")
            if (strRetorno.IndexOf("Signature") == -1)   //não gerou xml
            {
                showError(strRetorno);
                goto Fim;
            }
            else
            {
                Envia_Download_DOT(strRetorno, Versao, ref deuErro);
                //if (deuErro)
                //    break;
            }

            if (deuErro)
            {
                //20210723
                if (lastError != null && !lastError.Equals(""))
                {
                    showInfo("DOT(s) com erros:\n" + lastError);
                }
                else
                {
                    showInfo("DOT(s) com erros.");
                }

            }
        Fim:
            return;
        }


        if (e.CommandName.Equals("cancelarCIOT"))
        {

            int index = Convert.ToInt32(e.CommandArgument);

            GridView gv = (GridView)sender;
            gv.SelectedIndex = index;
                        
            GridViewRow row = gv.Rows[index];

            LinkButton lk = (LinkButton)row.FindControl("lkCIOT");
            if (lk.Text.Trim() == "")
            {
                showError("Motorista " + row.Cells[4].Text + ": registro sem CIOT!");
                return;
            }

            string CIOT = row.Cells[31].Text;
            string Cpf = row.Cells[03].Text;
            string sessionID = Session["IDSession"].ToString();

            body1.Attributes.Add("onload", "cancelarCIOT('" + sessionID + "','" + CIOT + "','" + Cpf + "');");         

        }

        if (e.CommandName.Equals("visualizarXml"))
        {

            int index = 0; //Convert.ToInt32(e.CommandArgument);

            GridView gv = (GridView)sender;
            gv.SelectedIndex = index;
                        
            GridViewRow row = gv.Rows[index];

            LinkButton lk = (LinkButton)row.FindControl("lkViewXml");
            if (lk.Text.Trim() == "")
            {
                showError("Motorista " + row.Cells[4].Text + ": registro sem CIOT!");
                return;
            }

            string CIOT = row.Cells[31].Text;
            string Cpf = row.Cells[03].Text;
            string sessionID = Session["IDSession"].ToString();

            body1.Attributes.Add("onload", "visualizarXml('" + sessionID + "','" + CIOT + "','" + Cpf + "');");         

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

        Pdesc_nivel2.Value = "Cadastra Apenas Terceiro";

        if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();

        intRetorno = cmd.ExecuteNonQuery();

        cmd.Dispose();
        conn.Dispose();

        apenasTerceiro = PenabledMenu.Value.ToString() == "S" ? true : false;
        
        return true;

    }
    

    private void showWarning(string msg)
    {
        string str = msg.Replace('"', ' ').Replace("'", "").Replace('\n', ' ').Replace('\t', ' ').Replace("\n", "").Replace("\t", "").Replace("\r", "").Trim();

//        body1.Attributes.Add("onload", "showWarning('" + msg + "')");
        body1.Attributes.Add("onload", "alert('" + msg + "')");
    }


    protected void gvPesquisa_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvPesquisa.PageIndex = e.NewPageIndex;

        DataTable dataTable = (DataTable)ViewState["dtState"];
        gvPesquisa.DataSource = dataTable; 
        gvPesquisa.DataBind();
//        AtualizaGrid();
    }

    
    private void showError(string msg)
    {
        string str = msg.Replace('"', ' ').Replace("'", "").Replace('\n', ' ').Replace('\t', ' ').Replace("\n", "").Replace("\t", "").Replace("\r", "").Trim();

        lastError = str;

//        body1.Attributes.Add("onload", "showErrotp_motr('" + msg + "')");
        body1.Attributes.Add("onload", "alert('" + str + "')");
    }


    private void showInfo(string msg)
    {
        string str = msg.Replace('"', ' ').Replace("'", "").Replace('\n', ' ').Replace('\t', ' ').Replace("\n", "").Replace("\t", "").Replace("\r", "").Trim();

//        body1.Attributes.Add("onload", "showHint('" + msg + "')");
        body1.Attributes.Add("onload", "alert('" + str + "')");
    }

    protected void ddImportacao_SelectedIndexChanged(object sender, EventArgs e)
    {
        hdImportacao.Value = ddImportacao.SelectedValue;

        AtualizaGrid();
    }


    protected void AtualizaGrid()
    {
        string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;

        SqlCommand cmd = new SqlCommand("dbo.getDadosMotoristaCIOT", new SqlConnection(strConexao));

        cmd.CommandType = CommandType.StoredProcedure;

        cmd.CommandTimeout = 999999;

        SqlParameter PCPF_MOTORISTA = cmd.Parameters.Add("@CPF_MOTORISTA", SqlDbType.Text);

        PCPF_MOTORISTA.Value = txtCPF.Text;

        SqlParameter PNOME_MOTORISTA = cmd.Parameters.Add("@NOME_MOTORISTA", SqlDbType.Text);

        PNOME_MOTORISTA.Value = txtNomeMotorista.Text;

        SqlParameter PID_IMPORTACAO = cmd.Parameters.Add("@ID_IMPORTACAO", SqlDbType.Int);

        if (ddImportacao.SelectedValue=="")
            PID_IMPORTACAO.Value = 0;
        else
            PID_IMPORTACAO.Value = Convert.ToInt32(ddImportacao.SelectedValue);

        SqlParameter PSEPARA_INSS = cmd.Parameters.Add("@SEPARA_INSS", SqlDbType.Bit);

        PSEPARA_INSS.Value = 1;


        //20210512
        SqlParameter Pnumero_ciot = cmd.Parameters.Add("@NUMERO_CIOT", SqlDbType.VarChar,12);
        Pnumero_ciot.Value = txtCIOT.Text;


        //20230828
        SqlParameter Ptp_motorista = cmd.Parameters.Add("@TP_MOTORISTA", SqlDbType.VarChar,1);
        Ptp_motorista.Value = ddTpMotorista.SelectedValue;

        //20230828
        SqlParameter Ptp_situacao = cmd.Parameters.Add("@TP_SITUACAO", SqlDbType.VarChar, 1);
        Ptp_situacao.Value = ddSituacaoCIOT.SelectedValue;


        if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();

        SqlDataAdapter da = new SqlDataAdapter();

        DataSet dt = new DataSet();

        da.SelectCommand = cmd;
        da.Fill(dt);

        gvPesquisa.DataSource = dt;
        gvPesquisa.DataBind();

        ViewState["dtState"] = dt.Tables[0];
        ViewState["sortdr"] = "Desc";
        ViewState["sortexp"] = "";

        //gvPesquisa_Sorting(sender, e);
        //MOTORISTA_CIOT.DATA_GERACAO desc,ID_FOPAG_CIOT_ITEM desc
        //object sender;
        //GridViewSortEventArgs e = new GridViewSortEventArgs(gvPesquisa.SortExpression, gvPesquisa.SortDirection);
        //gvPesquisa_Sorting(null, e);

        gvPesquisa.Columns[0].ControlStyle.CssClass = "congela";

        gvPesquisa.SelectedIndex = -1;

    }


    protected void gvPesquisa_Sorting(object sender, GridViewSortEventArgs e)
    {
        DataTable dataTable = (DataTable)ViewState["dtState"];

        if (dataTable != null)
        {
            DataView dataView = new DataView(dataTable);

            if (e.SortExpression != ViewState["sortexp"].ToString())
            {
                ViewState["sortdr"] = "Desc";
                ViewState["sortexp"] = e.SortExpression;
            }

            if (Convert.ToString(ViewState["sortdr"]) == "Asc")
            {
                dataView.Sort = e.SortExpression + " Desc";
                ViewState["sortdr"] = "Desc";
            }
            else
            {
                dataView.Sort = e.SortExpression + " Asc";
                ViewState["sortdr"] = "Asc";
            }

            DataTable sortedDT = dataView.ToTable();
            ViewState["dtState"] = sortedDT;

            gvPesquisa.PageIndex = 0;

            gvPesquisa.DataSource = sortedDT;
            gvPesquisa.DataBind();
        }
    }
    
    protected void btnGerarCIOT_Click(object sender, EventArgs e)
    {
  
        
        bool geraIndividual = true;
        bool GerouOT = false;
        string strXMLOT = "";
//        string GUID = "";
        string strRetorno = ""; //, strErro = "", strXMLEnvio = "", strXMLConsulta = "";
        bool deuErro = false;

        //20220713 
        jaExibiuMensagem = false;
        //20230828
        //Versao = "4.2.11.0";
        Versao = "4.2.8.0";

        foreach (GridViewRow row in gvPesquisa.Rows)
        {
            CheckBox cb = (CheckBox)row.FindControl("chkItem");
            if (cb != null && cb.Checked)
            {
                LinkButton lk = (LinkButton)row.FindControl("lkCIOT");

                //if (row.Cells[1].Text.Trim() != "&nbsp;")


                /*20210308 
                if (lk.Text.Trim() != "")
                {
                    showError("Motorista " + row.Cells[4].Text + ": registro com CIOT já emitido!");
                    goto Fim;
                }*/


                if (lk.Text.Trim() == "")
                {

                    strRetorno = GeraOT(CNPJ, Token, row, Versao,IDSessao());
                    //                if (strRetorno == "Falha ao assinar")
                    if (strRetorno.IndexOf("<OT>") == -1)   //não gerou xml
                    {
                        GerouOT = false;
                        showError(strRetorno);
                        goto Fim;
                    }
                    else
                    {
                        strXMLOT = strXMLOT + strRetorno;
                        if (geraIndividual)
                        {
                            Envia_Consulta_OT(strXMLOT, Versao, ref deuErro);
                            strXMLOT = "";
                        }
                        GerouOT = true;
                    }

                }
            }
        }

        if (!GerouOT)
            showWarning("Sem informações para geração de CIOT(s).");
        else if (!geraIndividual) 
        {
            Envia_Consulta_OT(strXMLOT, Versao, ref deuErro);
            showInfo("CIOT(s) gerado(s).");

            AtualizaGrid();
        }
        else
        {
            //20220713 
            if (!jaExibiuMensagem)
            {
                if (deuErro)
                    showInfo("CIOT(s) com erros. Verifique as mensagens de erro.");
                else
                    showInfo("CIOT(s) gerado(s).");

            }
            AtualizaGrid();
        }

    Fim:
        
        return;

    }
    
    protected void Envia_Consulta_OT(string strXMLOT,string Versao, ref bool deuErro)
    {
        string strXMLEnvio, strErro, strXMLConsulta, strRetorno;
        string GUID = "";
//20190705

        if (Versao == "4.2.8.0")
        {
            strXMLOT =  "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                        "<loteOT_envio versao=\"4.2.8.0\" token=\"" + @Token + "\" xmlns=\"http://www.nddigital.com.br/nddcargo\" xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.nddigital.com.br/nddcargo file:///D:/Trabalho/SolucoesNDD/nddCargo/Integracao/Schemas%204.2.8.0/CIOT/nddcargo_loteOT_envio_4280.xsd\">" +
                        "<operacoes>" + strXMLOT + "</operacoes></loteOT_envio>";
        }
        else
        {
            strXMLOT =  "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" +
                        "<loteOT_envio versao=\"4.2.11.0\" token=\"" + @Token + "\" xmlns=\"http://www.nddigital.com.br/nddcargo\" xmlns:ds=\"http://www.w3.org/2000/09/xmldsig#\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.nddigital.com.br/nddcargo file:///D:/Trabalho/SolucoesNDD/nddCargo/Integracao/Schemas%204.2.11.0/CIOT/nddcargo_loteOT_envio_42110.xsd\">" +
                        "<operacoes>" + strXMLOT + "</operacoes></loteOT_envio>";
        }

        strXMLEnvio = GeraEnvio(CNPJ, Token, ref GUID, "Enviar", Versao);

        var NDD_Homologa = new AeroCIOTWeb.NDD_Homologa.ExchangeMessage();
        var NDD_Producao = new AeroCIOTWeb.NDD_Producao.ExchangeMessage();

        string retorno_OT="";

        DateTime dtEnvio = DateTime.Now;
        SalvarXML("09", strXMLEnvio, dtEnvio); // 20231124 Grava pré Envio de OT

        if (atualiza)
        {
            if (ConfigurationManager.AppSettings["ambiente"] == "producao")
                retorno_OT = NDD_Producao.Send(strXMLEnvio, strXMLOT);
            else
                retorno_OT = NDD_Homologa.Send(strXMLEnvio, strXMLOT);
        }

        //salvar histórico de envio do xml        
        SalvarXML("10", strXMLEnvio,dtEnvio);   //Envio de OT
        SalvarXML("11", strXMLOT, dtEnvio);     //Lotes de OT
        SalvarXML("20", retorno_OT, dtEnvio);   //Retorno do envio de OT


        strErro = FindValueCIOT(retorno_OT.ToString(), "Body", "observacao");
        if (strErro != "")
        {

            string strNumero = FindValueCIOT2(strXMLOT.ToString(), "ide", "numero");

            SalvaErro(strNumero,strErro);

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

                    SalvaErro(strNumero, strOT  );
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
    
    /// <summary>
    /// Metodo que gera o XML para criar o CIOT na NDD
    /// </summary>
    /// <param name="CNPJ">CNPJ da AeroSoft</param>
    /// <param name="Token">Token de autenticação na integração</param>
    /// <param name="row">row Item do grid a ser enviado(dados do CIOT/Motorista)</param>
    /// <param name="Versao">Versão do LayOut do XML</param>
    /// <param name="id_sessao">id_sessao para identificar o usuário do sitema</param>
    /// <returns>Retorna o XML do CIOT para envio a integrção NDD</returns>
    protected string GeraOT(string CNPJ,string Token, GridViewRow row, string Versao, string id_sessao)
    {
        string GUID = "";
        /*
        string Motorista_CPF =  row.Cells[3].Text;
        string Motorista_Nome  = row.Cells[4].Text;
        string Motorista_RNTRC = row.Cells[11].Text == "&nbsp;" ? "0" : row.Cells[11].Text ;
        string Placa  = row.Cells[12].Text;
        string Contratante_CNPJ  = row.Cells[14].Text;
        string Contratante_Nome  = row.Cells[13].Text;
        string Contratante_CEP = row.Cells[15].Text;
        string Contratante_Cidade = row.Cells[16].Text;
        string Contratante_Estado = row.Cells[17].Text;
        string Contratante_Logradouro = row.Cells[18].Text;
        string Contratante_Endereco = row.Cells[19].Text;
        string Contratante_Nr = row.Cells[20].Text;
        string Contratante_Complemento = row.Cells[21].Text;
        string Contratante_Bairro = row.Cells[22].Text;
        string Contratante_RNTC = row.Cells[23].Text;
        decimal Frete_Valor = Convert.ToDecimal(row.Cells[6].Text == "&nbsp;" ? "0" : row.Cells[6].Text);
        decimal Propaganda_Valor = Convert.ToDecimal(row.Cells[7].Text == "&nbsp;" ? "0" : row.Cells[7].Text);
        */
        int ID_FOPAG_CIOT_ITEM = Convert.ToInt32(row.Cells[31].Text);
        /*
        decimal IRRF = Convert.ToDecimal(row.Cells[31].Text == "&nbsp;" ? "0" : row.Cells[31].Text);
        decimal INSS = Convert.ToDecimal(row.Cells[32].Text == "&nbsp;" ? "0" : row.Cells[32].Text);
        decimal SEST_SENAT = Convert.ToDecimal(row.Cells[33].Text == "&nbsp;" ? "0" : row.Cells[33].Text);
        string Proprietario_CPF = row.Cells[35].Text;
        string Proprietario_Nome = row.Cells[36].Text;
        string CNPJ_Destino = row.Cells[37].Text == "&nbsp;" ? "" : row.Cells[37].Text;
        string Data_Inicio = row.Cells[28].Text == "&nbsp;" ? DateTime.Today.ToString("dd/MM/yyyy") : row.Cells[28].Text;
        string Data_Final = row.Cells[29].Text == "&nbsp;" ? DateTime.Today.AddDays(20).ToString("dd/MM/yyyy") : row.Cells[29].Text;
        */

        string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;
        SqlConnection conn = new SqlConnection(strConexao);
        
        

        if (chkFinalizarAnterior.Checked == true)
        {
            //Antes de gerar um CIOT, encerra o anterior
            string strRetorno = FinalizaCIOTAnterior(row, Versao);

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
/*
        SqlParameter pMotorista_CPF_OT = cmd_OT_RAW.Parameters.Add("@Motorista_CPF", SqlDbType.Text);
        pMotorista_CPF_OT.Value = Motorista_CPF;

        SqlParameter pMotorista_Nome_OT = cmd_OT_RAW.Parameters.Add("@Motorista_Nome", SqlDbType.Text);
        pMotorista_Nome_OT.Value = Motorista_Nome;

        SqlParameter pMotorista_RNTRC_OT = cmd_OT_RAW.Parameters.Add("@Motorista_RNTRC", SqlDbType.Text);
        pMotorista_RNTRC_OT.Value = Motorista_RNTRC;

        SqlParameter pPlaca_OT = cmd_OT_RAW.Parameters.Add("@Placa", SqlDbType.Text);
        pPlaca_OT.Value = Placa;

        SqlParameter pContratante_CNPJ_OT = cmd_OT_RAW.Parameters.Add("@Contratante_CNPJ", SqlDbType.Text);
        pContratante_CNPJ_OT.Value = Contratante_CNPJ;

        SqlParameter pContratante_Nome_OT = cmd_OT_RAW.Parameters.Add("@Contratante_Nome", SqlDbType.Text);
        pContratante_Nome_OT.Value = Contratante_Nome;

        SqlParameter pContratante_CEP_OT = cmd_OT_RAW.Parameters.Add("@Contratante_CEP", SqlDbType.Text);
        pContratante_CEP_OT.Value = Contratante_CEP;

        SqlParameter pContratante_Cidade_OT = cmd_OT_RAW.Parameters.Add("@Contratante_Cidade", SqlDbType.Text);
        pContratante_Cidade_OT.Value = Contratante_Cidade;

        SqlParameter pContratante_Estado_OT = cmd_OT_RAW.Parameters.Add("@Contratante_Estado", SqlDbType.Text);
        pContratante_Estado_OT.Value = Contratante_Estado;

        SqlParameter pContratante_Logradouro_OT = cmd_OT_RAW.Parameters.Add("@Contratante_Logradouro", SqlDbType.Text);
        pContratante_Logradouro_OT.Value = Contratante_Logradouro;

        SqlParameter pContratante_Endereco_OT = cmd_OT_RAW.Parameters.Add("@Contratante_Endereco", SqlDbType.Text);
        pContratante_Endereco_OT.Value = Contratante_Endereco;

        SqlParameter pContratante_Nr_OT = cmd_OT_RAW.Parameters.Add("@Contratante_Nr", SqlDbType.Text);
        pContratante_Nr_OT.Value = Contratante_Nr;

        SqlParameter pContratante_Complemento_OT = cmd_OT_RAW.Parameters.Add("@Contratante_Complemento", SqlDbType.Text);
        pContratante_Complemento_OT.Value = Contratante_Complemento;

        SqlParameter pContratante_Bairro_OT = cmd_OT_RAW.Parameters.Add("@Contratante_Bairro", SqlDbType.Text);
        pContratante_Bairro_OT.Value = Contratante_Bairro;

        SqlParameter pContratante_RNTC_OT = cmd_OT_RAW.Parameters.Add("@Contratante_RNTC", SqlDbType.Text);
        pContratante_RNTC_OT.Value = Contratante_RNTC;

        SqlParameter pFrete_Valor_OT = cmd_OT_RAW.Parameters.Add("@Frete_Valor", SqlDbType.Decimal);
        pFrete_Valor_OT.Value = Frete_Valor;

        SqlParameter pPropaganda_Valor_OT = cmd_OT_RAW.Parameters.Add("@Propaganda_Valor", SqlDbType.Decimal);
        pPropaganda_Valor_OT.Value = Propaganda_Valor;

        SqlParameter pIRRF_OT = cmd_OT_RAW.Parameters.Add("@IRRF", SqlDbType.Decimal);
        pIRRF_OT.Value = IRRF;
*/
        SqlParameter pID_FOPAG_CIOT_ITEM_OT = cmd_OT_RAW.Parameters.Add("@ID_FOPAG_CIOT_ITEM", SqlDbType.Int);
        pID_FOPAG_CIOT_ITEM_OT.Value = ID_FOPAG_CIOT_ITEM;
/*
        SqlParameter pINSS_OT = cmd_OT_RAW.Parameters.Add("@INSS", SqlDbType.Decimal);
        pINSS_OT.Value = INSS;

        SqlParameter pSEST_SENAT_OT = cmd_OT_RAW.Parameters.Add("@SEST_SENAT", SqlDbType.Decimal);
        pSEST_SENAT_OT.Value = SEST_SENAT;

        SqlParameter pProprietario_CPF_OT = cmd_OT_RAW.Parameters.Add("@Transportador_CPF", SqlDbType.Text);
        pProprietario_CPF_OT.Value = Proprietario_CPF;

        SqlParameter pProprietario_Nome_OT = cmd_OT_RAW.Parameters.Add("@Transportador_Nome", SqlDbType.Text);
        pProprietario_Nome_OT.Value = Proprietario_Nome;

        SqlParameter pDtInicio_OT = cmd_OT_RAW.Parameters.Add("@Dt_Inicio", SqlDbType.DateTime);
        pDtInicio_OT.Value = Convert.ToDateTime(Data_Inicio);

        SqlParameter pDtFim_OT = cmd_OT_RAW.Parameters.Add("@Dt_Fim", SqlDbType.DateTime);
        pDtFim_OT.Value = Convert.ToDateTime(Data_Final);

        SqlParameter pDestinatario_CNPJ_OT = cmd_OT_RAW.Parameters.Add("@Destinatario_CNPJ", SqlDbType.Text);
        pDestinatario_CNPJ_OT.Value = CNPJ_Destino;
*/
        SqlParameter PXML_OT_RAW = new SqlParameter();
        PXML_OT_RAW.ParameterName = "@XML";
        PXML_OT_RAW.SqlDbType = System.Data.SqlDbType.VarChar;
        PXML_OT_RAW.Size = -1;
        PXML_OT_RAW.Direction = ParameterDirection.ReturnValue;
        cmd_OT_RAW.Parameters.Add(PXML_OT_RAW).Direction = ParameterDirection.Output;

        //20190705
        SqlParameter pVersao = cmd_OT_RAW.Parameters.Add("@Versao", SqlDbType.Text);
        pVersao.Value = Versao;

        //20230508
        SqlParameter pSessao = cmd_OT_RAW.Parameters.Add("@session", SqlDbType.Text);
        pSessao.Value = id_sessao;

        if (cmd_OT_RAW.Connection.State == ConnectionState.Closed) cmd_OT_RAW.Connection.Open();

        SqlDataReader dr_OT_RAW = cmd_OT_RAW.ExecuteReader();

        String strXml_OT_RAW;

        //20210618
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

        //20201222
        if (strXml_OT_RAW.IndexOf("Erro:") > -1)
        {
            showError(strXml_OT_RAW);
            return strXml_OT_RAW;
        }

        //20200715
        if(Nome=="")
        {
            //20200615
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

        }

        
        //Assina xml
        //20230721 Permitir efetuar assinatura digital através da API...

        string ativarApi = Request.QueryString.Get("api");

        if (ativarApi == null || ativarApi.Equals("")) {

            ativarApi = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["ATIVAR_MICRO_SERVICO_API"]);
        }
              

        //20230720
        if (ativarApi != null && ativarApi.Equals("1"))
        {

            return getXMLCIOTAssinadoAPI(ID_FOPAG_CIOT_ITEM, strXml_OT_RAW);
        }
        else
        {
            //20220906
            string serverAssinaturaDigital = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["SERVER_ASSINATURA_DIGITAL"]);


            //strXml_OT_RAW =  getRemoteObjectExt("tcp://192.168.1.20:8087/NFeAPPrs").getXMLAssinado(strXml_OT_RAW, "infOT", Nome /*20200715 false*/, false /*20210308*/ );

            strXml_OT_RAW = getRemoteObjectExt(serverAssinaturaDigital).getXMLAssinado(strXml_OT_RAW, "infOT", Nome /*20200715 false*/, false /*20210308*/ );

            return strXml_OT_RAW;
        }
       
    }

    //20210308 Testar assinatura digital informando um XML gerado anteriormente...ORNELLAS
    private string testarAssinatura(int idSeqXML) {


        string strXml_OT_RAW = null;

        strXml_OT_RAW = new QueriesTableAdapter().getXMLCIOTTeste(idSeqXML).ToString();


         //20200715
        if (Nome == "")
        {

            //20200615
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

        }


        //20230721 Permitir efetuar assinatura digital através da API...

        string ativarApi = Request.QueryString.Get("api");

        if (ativarApi == null || ativarApi.Equals("")) {

            ativarApi = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["ATIVAR_MICRO_SERVICO_API"]);
        }

      

        //20230720
        if (ativarApi != null && ativarApi.Equals("1"))
        {
            //Aqui iria falhar, pois o idSEQXML deveria ser o ID_FOPAG_CIOT_ITEM,,,
            return getXMLCIOTAssinadoAPI(idSeqXML, strXml_OT_RAW);
        }
        else
        {


            //20220906
            string serverAssinaturaDigital = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["SERVER_ASSINATURA_DIGITAL"]);



            //Assina xml
            strXml_OT_RAW = getRemoteObjectExt(serverAssinaturaDigital).getXMLAssinado(strXml_OT_RAW, "infOT", Nome /*20200715 false*/, false /*20210308*/ );

            return strXml_OT_RAW;

        }

    }


    //20230721
    private string getXMLCIOTAssinadoAPI(int idFopagCiotItem, string strXml_OT_RAW)
    {


        SqlCommand cmd = new SqlCommand("getXMLCIOTAssinadoAPI", getConn());

        cmd.CommandType = CommandType.StoredProcedure;

        cmd.CommandTimeout = 999999;

        SqlParameter PSession =
            cmd.Parameters.Add("@session", SqlDbType.VarChar, 20);
        PSession.Value = Request.QueryString.Get("session");

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
        

        if (getConn().State == ConnectionState.Closed) getConn().Open();
        
        cmd.ExecuteNonQuery();

        if (Pret_msg.Value.ToString() != "")
        {
            Response.Write(Pret_msg.Value.ToString());
            return null;
        }
        
        return PxmlAssinado.Value.ToString();

    }


    //20230721
    private string getXMLResumoCIOTAssinadoAPI(int idFopagCiotItem, string strXml_OT_RAW)
    {


        SqlCommand cmd = new SqlCommand("getXMLResumoCIOTAssinadoAPI", getConn());

        cmd.CommandType = CommandType.StoredProcedure;

        cmd.CommandTimeout = 999999;

        SqlParameter PSession =
            cmd.Parameters.Add("@session", SqlDbType.VarChar, 20);
        PSession.Value = Request.QueryString.Get("session");

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


        if (getConn().State == ConnectionState.Closed) getConn().Open();


        cmd.ExecuteNonQuery();

        if (Pret_msg.Value.ToString() != "")
        {
            Response.Write(Pret_msg.Value.ToString());
            return null;
        }

        return PxmlAssinado.Value.ToString();

    }




    //20230721
    private string getXMLEncerrarCIOTAssinadoAPI(int idFopagCiotItem, string strXml_OT_RAW)
    {


        SqlCommand cmd = new SqlCommand("getXMLEncerrarCIOTAssinadoAPI", getConn());

        cmd.CommandType = CommandType.StoredProcedure;

        cmd.CommandTimeout = 999999;

        SqlParameter PSession =
            cmd.Parameters.Add("@session", SqlDbType.VarChar, 20);
        PSession.Value = Request.QueryString.Get("session");

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


        if (getConn().State == ConnectionState.Closed) getConn().Open();

        cmd.ExecuteNonQuery();

        if (Pret_msg.Value.ToString() != "")
        {
            Response.Write(Pret_msg.Value.ToString());
            return null;
        }

        return PxmlAssinado.Value.ToString();

    }


    private bool consultarResumoCIOT(string ciot, int id_fopag_ciot_item) {
                     
        string GUID = "";

        GUID = Gera_GUID(CNPJ, Token);

        string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;
        SqlConnection conn = new SqlConnection(strConexao);

        SqlCommand cmd = new SqlCommand("dbo.CIOT_Consultar_Resumo_OT", conn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.CommandTimeout = 999999;

        SqlParameter pCNPJ = cmd.Parameters.Add("@CNPJ", SqlDbType.Text);
        pCNPJ.Value = CNPJ;

        SqlParameter pToken = cmd.Parameters.Add("@Token", SqlDbType.Text);
        pToken.Value = Token;

        SqlParameter pGUID_OT_RAW = cmd.Parameters.Add("@GUID", SqlDbType.Text);
        pGUID_OT_RAW.Value = GUID;

        SqlParameter pCIOT = cmd.Parameters.Add("@CIOT", SqlDbType.Text);
        pCIOT.Value = ciot;

        SqlParameter pSerie = cmd.Parameters.Add("@Serie", SqlDbType.Text);
        pSerie.Value = "1";

        SqlParameter pID_FOPAG_CIOT_ITEM = cmd.Parameters.Add("@ID_FOPAG_CIOT_ITEM", SqlDbType.Int);
        pID_FOPAG_CIOT_ITEM.Value = id_fopag_ciot_item;

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

        //Assina xml


        //20230724 Permitir efetuar assinatura digital através da API...

        string ativarApi = Request.QueryString.Get("api");

        if (ativarApi == null || ativarApi.Equals(""))
        {

            ativarApi = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["ATIVAR_MICRO_SERVICO_API"]);
        }


        //20230720
        if (ativarApi != null && ativarApi.Equals("1"))
        {

            strXml = getXMLResumoCIOTAssinadoAPI(id_fopag_ciot_item, strXml);
        }
        else
        {

            //20220906
            string serverAssinaturaDigital = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["SERVER_ASSINATURA_DIGITAL"]);
            
            strXml = getRemoteObjectExt(serverAssinaturaDigital).getXMLAssinado(strXml, "consultarStatusOT_envio", Nome, false);

        }

        if (strXml.Substring(0, 3) == "NOK")
        {
            showError(strXml);
            
            //20231010 goto Fim;
            return false;
        }

        string strXMLConsulta = GeraEnvio(CNPJ, Token, ref GUID, "ConsultarResumoOT", Versao);

        var NDD_Homologa = new AeroCIOTWeb.NDD_Homologa.ExchangeMessage();
        var NDD_Producao = new AeroCIOTWeb.NDD_Producao.ExchangeMessage();

        string retorno_Consulta;
        if (ConfigurationManager.AppSettings["ambiente"] == "producao")
            retorno_Consulta = NDD_Producao.Send(strXMLConsulta, strXml);
        else
            retorno_Consulta = NDD_Homologa.Send(strXMLConsulta, strXml);


        SalvarXML("90", strXMLConsulta, DateTime.Now); //Consulta de Resumo OT
        SalvarXML("91", strXml, DateTime.Now); //Consulta de Resumo OT - Documento
        SalvarXML("92", retorno_Consulta, DateTime.Now); //Consulta de Resumo OT - Retorno da consulta

        string strRetorno = FindValueCIOT(retorno_Consulta, "CrossTalk_Header", "ResponseCode");
        if (strRetorno == "200")
        {
            XmlDocument oXML = new XmlDocument();
            oXML.LoadXml(retorno_Consulta);
            XmlNode root = oXML.DocumentElement;

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(oXML.NameTable);
            nsmgr.AddNamespace("ndd", "http://www.nddigital.com.br/nddcargo");

            XmlNodeList oNoLista = root.SelectNodes("CrossTalk_Body/retornoConsultaResumo/ndd:retConsultaResumoOT", nsmgr);

            if (oNoLista.Count == 0)
            {
                strRetorno = FindValueCIOT(retorno_Consulta, "CrossTalk_Header", "mensagem");
                showError(strRetorno);
                
                //20231010 goto Fim;
                return false;
            }
            else
            {
                string strCIOT, strCodVerificador, strDataEmissao, strStatus;

                foreach (XmlNode oNo in oNoLista)
                {
                    strCIOT = oNo.SelectSingleNode("ndd:autorizacao/ndd:ciot/ndd:numero", nsmgr).InnerText;
                    strCodVerificador = oNo.SelectSingleNode("ndd:autorizacao/ndd:ciot/ndd:ciotCodVerificador", nsmgr).InnerText;
                    strDataEmissao = oNo.SelectSingleNode("ndd:ide/ndd:dataEmissao", nsmgr).InnerText;
                    strStatus = oNo.SelectSingleNode("ndd:ide/ndd:status", nsmgr).InnerText;

                    //20231124 Status => 2 CIOT cancelado
                    bool bolFinalizado = (strStatus == "1" || strStatus == "2") ? true : false;

                    //salvar as informações
                    if (strCIOT.Trim() != "")
                    {
                        string sql = "SELECT count(1) FROM MOTORISTA_CIOT WHERE ID_FOPAG_CIOT_ITEM = " + id_fopag_ciot_item.ToString();

                        cmd = new SqlCommand(sql, conn);
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandTimeout = 999999;

                        if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();

                        int intRetorno = Convert.ToInt32(cmd.ExecuteScalar());

                        if (intRetorno == 0) //não encontrou
                        {
                            Stored_IAE_Motorista_CIOT("I", "", strCIOT, strCodVerificador, Convert.ToDateTime(strDataEmissao), Convert.ToDateTime(strDataEmissao).AddDays(26), id_fopag_ciot_item, usuario, "", Convert.ToDateTime(strDataEmissao), bolFinalizado);
                        }
                        else
                        {
                            Stored_IAE_Motorista_CIOT("A", "", strCIOT, strCodVerificador, Convert.ToDateTime(strDataEmissao), Convert.ToDateTime(strDataEmissao).AddDays(26), id_fopag_ciot_item, usuario, "", Convert.ToDateTime(strDataEmissao), bolFinalizado);
                        }
                    }
                }

                //20231010
                //consultaResumo = true;

                return true;
            }
        }
        else
            showError(strRetorno);

        //20231010 cb.Checked = false;

        return false;

    }

    private void consultarResumo() {


        bool consultaResumo = false;
        //20230828
        //Versao = "4.2.11.0";
        Versao = "4.2.8.0";

        foreach (GridViewRow row in gvPesquisa.Rows)
        {
            CheckBox cb = (CheckBox)row.FindControl("chkItem");
            if (cb != null && cb.Checked)
            {

                //string CIOT = row.Cells[1].Text;
                LinkButton lk = (LinkButton)row.FindControl("lkCIOT");
                string CIOT = lk.Text;


                int ID_FOPAG_CIOT_ITEM = Convert.ToInt32(row.Cells[31].Text);

                if (CIOT == "&nbsp;")
                    CIOT = "";

                //20231010
                consultaResumo = consultarResumoCIOT(CIOT, ID_FOPAG_CIOT_ITEM); 

                
            }
        }

        if (consultaResumo == true)
        {
            AtualizaGrid();
            showInfo("Dados atualizados.");
        }
        else
            showWarning("Informações não encontradas para consulta.");

    //Fim:

        return;

    }
    protected void btnConsultarResumo_Click(object sender, EventArgs e)
    {
        consultarResumo();

    }
    

    protected void btnFinalizar_Click(object sender, EventArgs e)
    {
        foreach (GridViewRow row in gvPesquisa.Rows)
        {
            CheckBox cb = (CheckBox)row.FindControl("chkItem");
            if (cb != null && cb.Checked)
            {
                if (row.Cells[2].Text.Trim() == "SIM")
                {
                    showError("CIOT já finalizado!");
                    break;
                }

                FinalizaCIOT_Row(row);
            }
        }
    }

    protected void FinalizaCIOT_Row(GridViewRow row)
    {
        if (!row.Cells[31].Text.Equals("") && !row.Cells[31].Text.Equals("&nbsp;"))//2023082801
        {
            int ID_FOPAG_CIOT_ITEM = Convert.ToInt32(row.Cells[31].Text);
            //20230828
            //Versao = "4.2.11.0";
            Versao = "4.2.8.0"; 

            FinalizaCIOT(ID_FOPAG_CIOT_ITEM, Versao);
        }
        else {


            //2023082801 Permitir finalizar CIOT que não foi gerado através da FOPAG...
            if (ckTpForcarEncerramento.Checked){

                LinkButton lkCIOT = (LinkButton)row.Cells[1].Controls[1];

                string retMsg =null;
                new QueriesTableAdapter().Stored_Finalizar_CIOT_By_NrCIOT(Request.QueryString.Get("session"), lkCIOT.Text, ref retMsg);

                if (retMsg != null && !retMsg.Equals(""))
                {
                    showError(retMsg);


                }
                else {

                    AtualizaGrid();
                }
            }
        }
    }

    /**|
     * 20220314 Verifica se o CIOT pode ser finalizado...
     * 
     */
    private bool permiteFinalizarCIOT(int ID_FOPAG_CIOT_ITEM) {

        bool ret = true;

        string retMsg=null;
        
        new QueriesTableAdapter().Stored_Permite_Encerrar_CIOT(ID_FOPAG_CIOT_ITEM , ref retMsg);

        if (retMsg != null && !retMsg.Equals("")) {

            ret = false;

            showError(retMsg);
        }
    
        return ret;
    }
    

    protected void FinalizaCIOT(int ID_FOPAG_CIOT_ITEM, string Versao)
    {

        //20220314
        if (!permiteFinalizarCIOT(ID_FOPAG_CIOT_ITEM)) return;

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


        //20221114
        SqlParameter pTpforcarEncerramento = cmd.Parameters.Add("@tp_forcar_encerramento", SqlDbType.Bit);
        pTpforcarEncerramento.Value = ckTpForcarEncerramento.Checked;

        SqlParameter pSession = cmd.Parameters.Add("@session", SqlDbType.VarChar,20);
        pSession.Value = Request.QueryString.Get("session");
	

        if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();

        SqlDataReader dr = cmd.ExecuteReader();

        String strXml;

        strXml = cmd.Parameters["@XML"].Value.ToString();

        dr.Close();
        cmd.Dispose();
        conn.Close();

        if (strXml=="")
        {
            showError("ID_FOPAG_CIOT_ITEM: " + ID_FOPAG_CIOT_ITEM.ToString() + " inválido!");
            goto Fim;
        }

        //strXml = AssinaXML(strXml, "encerrarOT_envio");

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


        //20230721

          string ativarApi = Request.QueryString.Get("api");

        if (ativarApi == null || ativarApi.Equals("")) {

            ativarApi = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["ATIVAR_MICRO_SERVICO_API"]);
        }

      

        //20230720
        if (ativarApi != null && ativarApi.Equals("1"))
        {

            strXml = getXMLEncerrarCIOTAssinadoAPI(ID_FOPAG_CIOT_ITEM, strXml);
        }
        else
        {


            //20220906
            string serverAssinaturaDigital = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["SERVER_ASSINATURA_DIGITAL"]);



            strXml = getRemoteObjectExt(serverAssinaturaDigital).getXMLAssinado(strXml, "encerrarOT_envio", Nome, false);

        }
        if (strXml.Substring(0, 3) == "NOK")
        {
            showError(strXml);
            goto Fim;
        }

        var NDD_Homologa = new AeroCIOTWeb.NDD_Homologa.ExchangeMessage();
        var NDD_Producao = new AeroCIOTWeb.NDD_Producao.ExchangeMessage();

        string retorno_OT="";

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

                            sql =
                                    "UPDATE " +
                                    "MOTORISTA_CIOT " +
                                    "SET " +
                                    "CIOT_PROTOCOLO_ENCERRAMENTO = '" + strprotocoloEnce + "', " +
                                    "DT_FINALIZACAO=CONVERT(DATETIME,'" + strDataEncerramento + "',120)," +
                                    "TP_FINALIZADO=1, " +
                                    "USUARIO_FINALIZACAO='" + usuario + "' " +//20210830
                                    "WHERE " +
                                    "CIOT = '" + strCIOT + "'";

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
                    //20220211 Identificando CIOT cancelado na NDD para finalizar o mesmo em nosso sistema...
                    if (
                        
                        (strOT.Contains("A Operação de Transporte com o CIOT") && strOT.Contains("está cancelada e não pode ser encerrada")) 
                        //20221019
                        ||
                        (strOT.Contains("Operação de Transporte já está encerrada") ) 
                    )
                    
                    {

                        //20221019 
                        if (strOT.Contains("CIOT"))
                        {
                            strCIOT = strOT.Substring(strOT.IndexOf("CIOT"));
                            strCIOT = strCIOT.Replace("CIOT", "");
                            strCIOT = strCIOT.Replace("está cancelada e não pode ser encerrada.", "");
                            strCIOT = strCIOT.Trim();



                            sql =
                                   "UPDATE " +
                                   "MOTORISTA_CIOT " +
                                   "SET " +
                                   "CIOT_PROTOCOLO_ENCERRAMENTO = 'T00000000000000', " +
                                   "DT_FINALIZACAO=getdate()," +
                                   "TP_FINALIZADO=1, " +
                                   "USUARIO_FINALIZACAO='" + usuario + "' " +//20210830
                                   "WHERE " +
                                   "CIOT = '" + strCIOT + "'";

                        }
                        else {

                            sql =
                                "UPDATE " +
                                "MOTORISTA_CIOT " +
                                "SET " +
                                "CIOT_PROTOCOLO_ENCERRAMENTO = 'T00000000000000', " +
                                "DT_FINALIZACAO=getdate()," +
                                "TP_FINALIZADO=1, " +
                                "USUARIO_FINALIZACAO='" + usuario + "' " +//20210830
                                "WHERE " +
                                "ID_FOPAG_CIOT_ITEM = " + Convert.ToString(ID_FOPAG_CIOT_ITEM ) ;
                        }

                        cmd = new SqlCommand(sql, conn);
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandTimeout = 999999;

                        if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();

                        intRetorno = cmd.ExecuteNonQuery();

                    
                    }
                    showError(strOT);
                    goto Fim;
                }
            }

            AtualizaGrid();
            showInfo("CIOT(s) finalizado(s).");
        }
        
        Fim:
        return;
    }
    

    public string Gera_GUID(string CNPJ,string Token)
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
    

    /// <summary>
    /// Salva xmls enviados e retorno
    /// </summary>
    /// <param name="Tipo">código do tipo de xml envio, retorno, status</param> 
    /// <param name="strXML">xml formatado</param>
    /// <param name="dtEnvio">data da do envio</param>
    private void SalvarXML(string Tipo, string strXML,DateTime dtEnvio)
    {
        strXML = strXML.Replace("'", "''");
        string sql = "INSERT NDD_CIOT_XML (TIPO,XML_NDD,DH_INCLUSAO) VALUES ('" + Tipo + "','" + @strXML+"',CONVERT(DATETIME,'"+dtEnvio.ToString("dd/MM/yyyy HH:mm:ss.fff") + "',103))";

        string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;
        SqlConnection conn = new SqlConnection(strConexao);

        SqlCommand cmd = new SqlCommand(sql, conn);
        cmd.CommandType = CommandType.Text;
        cmd.CommandTimeout = 999999;

        if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();

        cmd.ExecuteNonQuery();
        conn.Close();
    }


    protected string FinalizaCIOTAnterior(GridViewRow row, string Versao)
    {
        int ID_FOPAG_CIOT_ITEM = Convert.ToInt32(row.Cells[31].Text);
        string Motorista_CPF = row.Cells[3].Text;
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


    private void Stored_IAE_Motorista_CIOT(string Tipo, string CPF_Motorista, string CIOT, string CV, DateTime? Inicio_Viagem, DateTime? Final_Viagem, int ID_FOPAG_CIOT_ITEM, string Usuario, string CPF_Proprietario, DateTime Data_Geracao, bool Tp_Finalizado, string erro=null)
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

    public string FindValueCIOT2(string XMLFile, string TAGValue, string TAGValue2)
    {
        string _returnValueCIOTVerificador = string.Empty;

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(XMLFile);

        XmlNodeList _nodeListTAG1 = doc.GetElementsByTagName(TAGValue);

//        XmlNodeList _nodeListTAG2 = doc.GetElementsByTagName(TAGValue2);

        
        foreach (XmlNode nodeTAG1 in _nodeListTAG1)
        {
            for (int i = 0; i < nodeTAG1.ChildNodes.Count;i++ )
                if (nodeTAG1.ChildNodes[i].Name == TAGValue2)
                {
                    _returnValueCIOTVerificador = nodeTAG1.ChildNodes[i].InnerText;
                    break;
                }
        }
/*


        var Nodes = doc.SelectSingleNode(@"//infOT/ide/numero");

        _returnValueCIOTVerificador = Nodes.InnerText;

        /*
        foreach (XmlNode item in Nodes)
        {
            string title = item.SelectSingleNode("./title").InnerText;
            string price = item.SelectSingleNode("./price").InnerText;
            Console.WriteLine("title {0} price: {1}", title, price); //just for demo
        }
        */


        return _returnValueCIOTVerificador;
    }


    private string ConvertSortDirectionToSql(SortDirection sortDirection)
    {
        string newSortDirection = String.Empty;

        switch (sortDirection)
        {
            case SortDirection.Ascending:
                newSortDirection = "ASC";
                break;

            case SortDirection.Descending:
                newSortDirection = "DESC";
                break;
        }

        return newSortDirection;
    }


    protected void btnImportarTerceiros_Click(object sender, EventArgs e)
    {
        if (fupImportarTerceiros.FileName == "")
        {
            showError("Informe o arquivo!");
            return;
        }

        string extension = System.IO.Path.GetExtension(fupImportarTerceiros.FileName);
        if (!extension.Equals(".csv", StringComparison.OrdinalIgnoreCase))
        {
            showError("O arquivo deve ser do tipo csv!");
            return;
        }

        Stream str = fupImportarTerceiros.PostedFile.InputStream;

        StreamReader rdr = new StreamReader(str);

        string inLine = rdr.ReadToEnd();


        string[] Lines = inLine.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

        if (str != null)
        {
            string retMsg = "";
            string sql = "";
            DataTable dt;
            if (Lines[0].Substring(0, 4) != "Emp.")
            {
                showError("Arquivo inválido!");
                return;
            }

            foreach (string line in Lines)
            {
                if (line.Substring(0, 4) != "Emp.")
                {
                    string[] campos = line.Split(';');
                    /*
                    0-Emp. 	
                    1-Filial	
                    2-Nome                                     	
                    3-CPF            	
                    4-Data Pagto  	
                    5-Rend. Bruto    	
                    6-IR Retido      	
                    7-Contribuição INSS	
                    8-INSS Transp.   	
                    9-Banco	
                    10-Nome do Banco                 	
                    11-Agência	
                    12-Conta Corrente	
                    13-Dígito 	
                    14-CPF/CNPJ do proprietário do veiculo	
                    15-PLACA	
                    16-CNPJ do destinatario	
                    17-Inicio da operação	
                    18-Termino da operação
                    19-Saldo                20190723
                    20-Data pagamento saldo 20190723
                  
                    */

                    if (campos.Length<21) //20190723 não tem saldo, ajusta o tamanho da matriz
                    {
                        Array.Resize(ref campos, campos.Length + 3);
                    }

                    //20191217
                    campos[3] = campos[3].PadLeft(11, '0');

                    if (campos[14].Length < 11)
                        campos[14] = campos[14].PadLeft(11, '0');

                    if (campos[14].Length > 11)
                        campos[14] = campos[14].PadLeft(14, '0');

                    if (campos[16].Length < 11)
                        campos[16] = campos[16].PadLeft(11, '0');

                    if (campos[16].Length > 11)
                        campos[16] = campos[16].PadLeft(14, '0');


                    //20191217
                    if ((IsCpf(campos[3]) == false) &&  (IsCnpj(campos[3]) == false))    
                    {
                        retMsg = " Campo 'cpf motorista' inválido !";
                        showError(retMsg);
                        return;
                    }

                    if ((IsCpf(campos[14]) == false) && (IsCnpj(campos[14]) == false))
                    {
                        retMsg = " Campo 'CPF/CNPJ do proprietário do veiculo' inválido !";
                        showError(retMsg);
                        return;
                    }

                    if ((IsCpf(campos[16]) == false) && (IsCnpj(campos[16]) == false))
                    {
                        retMsg = " Campo 'CNPJ do destinatário' inválido !";
                        showError(retMsg);
                        return;
                    }


                
                    //20200715
                    sql = "select 1 from motorista where placa_veiculo='" + campos[15] + "'";
                    DataTable dtPlaca = retDataTableText(sql);
                    if (dtPlaca.Rows.Count == 0)
                    {
                        retMsg = "Placa " + campos[15] + " não encontrada,para o motorista de CPF:" + campos[3] + "  no cadastro de motorista !";
                        showError(retMsg);
                        return;
                    }

                    sql = "select 1 from veiculo where CPF_PROPRIETARIO='" + campos[14] + "'";
                    DataTable dtProprietario = retDataTableText(sql);
                    if (dtProprietario.Rows.Count == 0)
                    {
                        retMsg = "Não foi encontrado nenhum PROPRIETÁRIO  com CPF/CNPJ  " + campos[14] + " no cadastro de veiculo!  !";
                        showError(retMsg);
                        return;
                    }


                    //Verifica se o motorista é terceiro
//20190712                    sql = "select * from motorista where cpf='" + campos[14] + "' and tp_motorista='T'";
                    sql = "select * from motorista where cpf='" + campos[3] + "' and tp_motorista='T'";
                    DataTable dtMotorista = retDataTableText(sql);
                    if (dtMotorista.Rows.Count==0)
                    {
                        retMsg = "Motorista " + campos[3] + " não é terceiro!";
                        showError(retMsg);
                        return;
                    }
                    //Verifica se o motorista tem CIOT não finalizado
                    sql = "select * from motorista_ciot where cpf_motorista='" + campos[3] + "' and tp_finalizado=0 and isnull(ciot,'')<>''";
                    DataTable dtMotoristaCIOT = retDataTableText(sql);
                    if (dtMotoristaCIOT.Rows.Count != 0)
                    {
                        retMsg = "Motorista " + campos[3] + " com CIOT " + dtMotoristaCIOT.Rows[0]["CIOT"].ToString() + " não finalizado!";
                        showError(retMsg);
                        return;
                    }

                    //20191217
                    if (campos[20] != null && !campos[20].Equals(""))
                    {
                        try
                        {

                            Convert.ToDateTime(campos[20]);
                        }
                        catch
                        {
                            retMsg = "Campo 'Data pagamento saldo' inválido ";
                            showError(retMsg);
                            return;
                            
                        }
                    }

                    if ((campos[4] != null) && !campos[4].Equals(""))
                    {
                        try
                        {

                            Convert.ToDateTime(campos[4]);
                        }
                        catch
                        {
                            retMsg = "Campo 'Data pagamento' inválido ";
                            showError(retMsg);
                            return;

                        }
                    }


                    if ((campos[5] != null) && campos[5].Trim() != "")
                    {
                        try
                        {

                            Convert.ToDecimal(campos[5]);
                        }
                        catch
                        {
                            retMsg = "Campo 'Rend. Bruto ' inválido ";
                            showError(retMsg);
                            return;

                        }
                    }

                    if ((campos[6] != null) && campos[6].Trim()!= "")
                    {
                        try
                        {

                            Convert.ToDecimal(campos[6]);
                        }
                        catch
                        {
                            retMsg = "Campo 'IR Retido' inválido ";
                            showError(retMsg);
                            return;

                        }
                    }
                    if ((campos[7] != null) &&  campos[7].Trim() != "")
                    {
                        try
                        {

                            Convert.ToDecimal(campos[7]);
                        }
                        catch
                        {
                            retMsg = "Campo 'Contribuição INSS	' inválido ";
                            showError(retMsg);
                            return;

                        }
                    }
                    if ((campos[8] != null) && campos[8].Trim() != "")
                    {
                        try
                        {

                            Convert.ToDecimal(campos[8]);
                        }
                        catch
                        {
                            retMsg = "Campo 'INSS Transp' inválido ";
                            showError(retMsg);
                            return;

                        }
                    }
                    if ((campos[9] != null) && campos[9].Trim() != "")
                    {
                        try
                        {

                            Convert.ToDecimal(campos[9]);
                        }
                        catch
                        {
                            retMsg = "Campo 'Banco' inválido ";
                            showError(retMsg);
                            return;

                        }
                    }

                    

                    //20190704 Consistir valores numéricos campos[5] a campos[8]
                    if (campos[5].Trim() == "") campos[5] = "0";
                    if (campos[6].Trim() == "") campos[6] = "0";
                    if (campos[7].Trim() == "") campos[7] = "0";
                    if (campos[8].Trim() == "") campos[8] = "0";
                   
                    
                    //20190723 Consistir campos novos
                    if ((campos[19] == null) || (campos[19].Trim() == "")) campos[19] = "0";
                    if ((campos[20] == null) || (campos[20].Trim() == "")) 
                        campos[20] = "null";
                    else 
                        campos[20] = "'" + campos[20] + "'";

                    
                    /*20190723
                    sql = "insert into FOPAG_CIOT_ITEM ("+
				        "ID_IMPORTACAO, empresa, filial, nome, cpf, data_pagto, rend_bruto, IR_Retido, contribuicao_inss, INSS_Transp, Banco, Nome_do_banco, Agencia, Conta_corrente, digito) "+
                        "values (0,'"+campos[0]+"','"+campos[1]+"','"+campos[2]+"','"+campos[3]+"',CONVERT(datetime,'"+campos[4]+"',103),"+campos[5].ToString().Replace(",",".")+","+campos[6].ToString().Replace(",",".")+","+campos[7].ToString().Replace(",",".")+","+campos[8].ToString().Replace(",",".")+",'"+campos[9]+"','"+campos[10]+"','"+campos[11]+"','"+campos[12]+"','"+campos[13]+
                        "')";
                    */

                    sql = "insert into FOPAG_CIOT_ITEM (" +
                        "ID_IMPORTACAO, empresa, filial, nome, cpf, data_pagto, rend_bruto, IR_Retido, contribuicao_inss, INSS_Transp, Banco, Nome_do_banco, Agencia, Conta_corrente, digito, saldo, data_pagto_saldo) " +
                        "values (0,'" + campos[0] + "','" + campos[1] + "','" + campos[2] + "','" + campos[3] + "',CONVERT(datetime,'" + campos[4] + "',103)," + campos[5].ToString().Replace(",", ".") + "," + campos[6].ToString().Replace(",", ".") + "," + campos[7].ToString().Replace(",", ".") + "," + campos[8].ToString().Replace(",", ".") + ",'" + campos[9] + "','" + campos[10] + "','" + campos[11] + "','" + campos[12] + "','" + campos[13] +
                         "'," + campos[19].ToString().Replace(",", ".") + ",CONVERT(datetime," + campos[20] + ",103))";

                    // + campos[21].ToString().Replace(",", ".") + "'";
                    SqlCommand cmd = new SqlCommand(sql, getConn());
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = 999999;

                    if (getConn().State == ConnectionState.Closed) getConn().Open();

                    int retorno = cmd.ExecuteNonQuery();

                    if (retorno < 1)
                    {
                        retMsg = "Problema na inclusão do FOPAG_CIOT_ITEM: " + campos[2].ToString() + " !";
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

                    if (!IsDate(campos[17]))
                        campos[17] = DateTime.Today.ToString();
                    
                    if (!IsDate(campos[18]))
                        campos[18] = DateTime.Today.ToString();


                    sql = "insert into MOTORISTA_CIOT ("+
                        "CPF_MOTORISTA, CIOT, INICIO_VIAGEM,FINAL_VIAGEM, ID_FOPAG_CIOT_ITEM, DATA_GERACAO, DT_INCLUSAO, USUA_INCLUSAO, PLACA, CPF_PROPRIETARIO, CNPJ_DESTINO) "+
                        "values ('" + campos[3] + "','',convert(datetime,'" + campos[17] + "',103),convert(datetime,'" + campos[18] + "',103)," + id_fopag_ciot_item.ToString() + ",getdate(),getdate(),'" + usuario + "','" + campos[15] + "','" + campos[14] + "','" + campos[16] +
                        "')";

                    cmd = new SqlCommand(sql, getConn());
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = 999999;

                    if (getConn().State == ConnectionState.Closed) getConn().Open();

                    retorno = cmd.ExecuteNonQuery();

                    if (retorno < 1)
                    {
                        retMsg = "Problema na inclusão do MOTORISTA_CIOT: " + campos[2].ToString() + " !";
                        showError(retMsg);
                        return;
                    }
                }
            }
            AtualizaGrid();
            showInfo("Importação processada!");
        }
    }


    protected void gvPesquisa_SelectedIndexChanged(object sender, EventArgs e)
    {
        gvPesquisa.SelectedRow.BackColor = System.Drawing.Color.Red;
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
            // "tcp://192.168.1.80:8087/NFeAPPrs"
            Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["SERVER_CTe"])
            );

        return remObject;
    }


    protected void SalvaErro(string numero, string erro)
    {
        erro.Replace("'", "\"").Replace("\n", " ");

        string  sql = "SELECT count(*) FROM MOTORISTA_CIOT WHERE ID_FOPAG_CIOT_ITEM = " + numero;

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


    protected void CheckUncheckAll(object sender, EventArgs e)
    {
        CheckBox chkTotal = (CheckBox)gvPesquisa.HeaderRow.FindControl("chkTotal");

        foreach(GridViewRow row in gvPesquisa.Rows)
        {
            CheckBox cb = (CheckBox)row.FindControl("chkItem");
            cb.Checked = chkTotal.Checked;
        }
    }

    
    protected void btnExcluir_Click(object sender, EventArgs e)
    {
        string strConexao = ConfigurationManager.ConnectionStrings["AEROSOFTConnectionString"].ConnectionString;
        SqlConnection conn = new SqlConnection(strConexao);

        foreach (GridViewRow row in gvPesquisa.Rows)
        {
            CheckBox cb = (CheckBox)row.FindControl("chkItem");
            if (cb != null && cb.Checked)
            {

                SqlCommand cmd = new SqlCommand("dbo.STORED_EXCLUIR_CIOT", conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 999999;

                SqlParameter pID = cmd.Parameters.Add("@ID", SqlDbType.Int);
                pID.Value = Convert.ToInt32(row.Cells[31].Text);

                SqlParameter Pret_msg = new SqlParameter();
                Pret_msg.ParameterName = "@ret_msg";
                Pret_msg.SqlDbType = System.Data.SqlDbType.VarChar;
                Pret_msg.Size = 255;
                Pret_msg.Direction = ParameterDirection.ReturnValue;
                cmd.Parameters.Add(Pret_msg).Direction = ParameterDirection.Output;

                if (cmd.Connection.State == ConnectionState.Closed) cmd.Connection.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                string strRetorno = cmd.Parameters["@ret_msg"].Value.ToString();

                
                if (strRetorno != "")
                {
                    showError("Motorista " + row.Cells[4].Text + ": "+strRetorno);
                    break;
                }

            }
        }

        conn.Close();

        AtualizaGrid();

    }

    
    protected void btnPesquisar_Click(object sender, EventArgs e)
    {
        AtualizaGrid();
    }

    protected void txtCPFTerceiro_TextChanged(object sender, EventArgs e)
    {
        string retorno = "";

        string sql = "select cpf,nome,a.cod_banco,ds_banco, conta,agencia,ativo, tp_motorista from motorista a " +
        "join bancos b on a.cod_banco = b.cod_banco " +
        "where cpf='" + txtCPFTerceiro.Text + "'";

        DataTable dt = retDataTableText(sql);
        
        if (dt.Rows.Count==0)
            retorno = "Motorista " + txtCPFTerceiro.Text + " não encontrado!";
        else
            if (dt.Rows[0]["ATIVO"].ToString() != "S")
                retorno = "Motorista " + txtCPFTerceiro.Text + " inativo!";
            else
                if (dt.Rows[0]["TP_MOTORISTA"].ToString() != "T")
                    retorno = "Motorista " + txtCPFTerceiro.Text + " não é terceiro!";
                else
                {
                    txtNomeTerceiro.Text = dt.Rows[0]["Nome"].ToString();
                    txtCodigoBanco.Text = dt.Rows[0]["Cod_Banco"].ToString();
                    txtNomeBanco.Text = dt.Rows[0]["Ds_Banco"].ToString();
                    txtAgencia.Text = dt.Rows[0]["Agencia"].ToString();
                    txtContaCorrente.Text = dt.Rows[0]["Conta"].ToString().Replace("-", "");
                }

        dt.Dispose();

        if (retorno != "")
        {
            showError(retorno);
            return;
        }
    }

    protected void txtCPFProprietario_TextChanged(object sender, EventArgs e)
    {
        string retorno = "";

        string sql = "select nome from veiculo " +
        "where cnpj_cpf='" + txtCPFProprietario.Text + "'";

        DataTable dt = retDataTableText(sql);

        if (dt.Rows.Count == 0)
            retorno = "Proprietário " + txtCPFProprietario.Text + " não encontrado!";
        else
            txtNomeProprietario.Text = dt.Rows[0]["Nome"].ToString();

        dt.Dispose();

        if (retorno != "")
        {
            showError(retorno);
            return;
        }

    }

    protected void txtPlaca_TextChanged(object sender, EventArgs e)
    {
        string retorno = "";

        string sql = "select cnpj_cpf,nome from veiculo " +
"where placa='" + txtPlaca.Text + "'";

        DataTable dt = retDataTableText(sql);

        if (dt.Rows.Count == 0)
            retorno = "Veículo " + txtPlaca.Text + " não encontrado!";
        else
            if (dt.Rows[0]["cnpj_cpf"].ToString() != txtCPFProprietario.Text)
                retorno = "Proprietário " + txtCPFProprietario.Text + " não possui este veículo!";

        dt.Dispose();

        if (retorno != "")
        {
            showError(retorno);
            return;
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
        string retorno = "";
        //Refaz as validações
        txtCPFTerceiro_TextChanged(sender, e);
        txtCPFProprietario_TextChanged(sender, e);
        txtPlaca_TextChanged(sender, e);
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

    protected bool IsDate(String date)
    {
        try
        {
            DateTime dt = DateTime.Parse(date);
            return true;
        }
        catch
        {
            return false;
        }
    }

    protected void GerarCIOTTerceiro()
    {
        string sql = "";
        string retMsg = "";
        //20230828
        //Versao = "4.2.11.0";
        Versao = "4.2.8.0"; 
                    
        //Verifica se o motorista tem CIOT não finalizado
        sql = "select * from motorista_ciot where cpf_motorista='" + txtCPFTerceiro.Text + "' and tp_finalizado=0 and isnull(ciot,'')<>''";
        DataTable dtMotoristaCIOT = retDataTableText(sql);
        if (dtMotoristaCIOT.Rows.Count != 0)
        {
            retMsg = "Motorista " + txtNomeTerceiro.Text + " com CIOT " + dtMotoristaCIOT.Rows[0]["CIOT"].ToString() + " não finalizado!";
            showError(retMsg);
            return;
        }

        //Inlcuindo os 2 primeiros campos (empresa e filial com 1 e 1 )
        sql = "insert into FOPAG_CIOT_ITEM (" +
            "ID_IMPORTACAO, empresa, filial, nome, cpf, data_pagto, rend_bruto, IR_Retido, contribuicao_inss, INSS_Transp, Banco, Nome_do_banco, Agencia, Conta_corrente, digito) " +
            "values (0,'" + "1" + "','" + "1" + "','" + txtNomeTerceiro.Text + "','" + txtCPFTerceiro.Text + "',CONVERT(datetime,'" + txtDataPagamento.Text + "',103)," + 
            txtRendimentoBruto.Text.ToString().Replace(",",".") + "," + txtIRRetido.Text.ToString().Replace(",", ".") + "," + txtContribuicaoINSS.Text.ToString().Replace(",", ".") + "," + 
            txtINSSTransp.Text.ToString().Replace(",", ".") + ",'" + txtCodigoBanco.Text + "','" + txtNomeBanco.Text + "','" + txtAgencia.Text + "','" + txtContaCorrente.Text + "','" + "" +
            "')";

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
            txtCNPJDestinatario.Text.ToString().Replace(".","").Replace("-","").Replace("/","") + "')";

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

        gvPesquisa.DataSource = dt;
        gvPesquisa.DataBind();


        string strRetorno;
        bool deuErro = false;
                
        strRetorno = GeraOT(CNPJ, Token, gvPesquisa.Rows[0], Versao, IDSessao());
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
                showInfo("CIOT " +  dt.Tables[0].Rows[0]["CIOT"].ToString() + " gerado.");
        }
    }

    protected void btnDownloadDOT_Click(object sender, EventArgs e)
    {
        bool GerouDOT = false;
        string strXMLOT = "";
        string strRetorno = ""; //, strErro = "", strXMLEnvio = "", strXMLConsulta = "";
        bool deuErro = false;
        //20230828
        //Versao = "4.2.11.0";
        Versao = "4.2.8.0";


        foreach (GridViewRow row in gvPesquisa.Rows)
        {
            CheckBox cb = (CheckBox)row.FindControl("chkItem");
            if (cb != null && cb.Checked)
            {
                strXMLOT = "";

                LinkButton lk = (LinkButton)row.FindControl("lkCIOT");

                //if (row.Cells[1].Text.Trim() == "&nbsp;")
                if (lk.Text.Trim() == "")
                    {
                    showError("Motorista " + row.Cells[4].Text + ": registro sem CIOT!");
                    goto Fim;
                }

                strRetorno = DownloadDOT(CNPJ, Token, row, Versao);
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
                        break;
                    //return; //Visualiza um DOT de cada vez.
                }
            }
        }
        if (!GerouDOT && strXMLOT=="")
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

    protected string DownloadDOT(string CNPJ, string Token, GridViewRow row, string Versao)
    {
        string GUID = "";

        //string CIOT = row.Cells[1].Text;

        LinkButton lk = (LinkButton)row.FindControl("lkCIOT");
        string CIOT = lk.Text;

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

        //20230722 0916 ORNELLAS Permitir efetuar assinatura digital através da API...

        string ativarApi = Request.QueryString.Get("api");

        if (ativarApi == null || ativarApi.Equals("")) {

            ativarApi = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["ATIVAR_MICRO_SERVICO_API"]);
        }

      

        //20230720 
        if (ativarApi != null && ativarApi.Equals("1"))
        {
            strXml_DOT = getXMLDownloadCIOTAssinadoAPI(@CIOT,strXml_DOT);
        }
        else
        {
            //20220906
            string serverAssinaturaDigital = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["SERVER_ASSINATURA_DIGITAL"]);

            //Assina xml
            strXml_DOT = getRemoteObjectExt(serverAssinaturaDigital).getXMLAssinado(strXml_DOT, "downloadOperacao_envio", Nome, false);
        }
        return strXml_DOT;
    }



    //20230721
    private string getXMLDownloadCIOTAssinadoAPI(string ciot, string strXml_DOT)
    {


        SqlCommand cmd = new SqlCommand("getXMLDownloadCIOTAssinadoAPI", getConn());

        cmd.CommandType = CommandType.StoredProcedure;

        cmd.CommandTimeout = 999999;

        SqlParameter PSession =
            cmd.Parameters.Add("@session", SqlDbType.VarChar, 20);
        PSession.Value = Request.QueryString.Get("session");

        SqlParameter Pciot =
            cmd.Parameters.Add("@ciot", SqlDbType.VarChar,12);
        Pciot.Value = ciot;


        SqlParameter Pxml =
            cmd.Parameters.Add("@xml", SqlDbType.VarChar, -1);
        Pxml.Value = strXml_DOT;



        SqlParameter Pret_msg =
            cmd.Parameters.Add("@ret_msg", SqlDbType.VarChar, 255);

        Pret_msg.Direction = ParameterDirection.Output;


        SqlParameter PxmlAssinado =
           cmd.Parameters.Add("@ret_xml_assinado", SqlDbType.VarChar, -1);

        PxmlAssinado.Direction = ParameterDirection.Output;


        if (getConn().State == ConnectionState.Closed) getConn().Open();

        cmd.ExecuteNonQuery();

        if (Pret_msg.Value.ToString() != "")
        {
            Response.Write(Pret_msg.Value.ToString());
            return null;
        }

        return PxmlAssinado.Value.ToString();

    }

    

    protected void Envia_Download_DOT(string strXMLOT, string Versao, ref bool deuErro)
    {
        lastError = "";

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

                    //20221221


                    byte[] base64EncodedBytes = System.Convert.FromBase64String(strID);

                    string sPath =  @"D:\Aerosoft\PDF\";
                    string fileName = strCIOT + ".pdf";

                    if (Directory.Exists(sPath))
                    {
                        sPath += fileName;
                    }

                    File.WriteAllBytes(sPath, base64EncodedBytes);

                //D:\aerosoft\pdf

                    //20220915 Problemas ao passar conteúdo base64 por sessão...
                    //Session.Add("dot", strID);

                    string url = @"https://www.aerosoftcargas.com.br/pdf/" + fileName;

                    body1.Attributes.Add("onload", "window.open('" + url + "')");
                    
                    //body1.Attributes.Add("onload", "dot('" + sfile + "')");

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

                    //20210723
                    lastError = strOT;

                    goto Fim;
                }
            }

        }

    Fim:

        return;

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
    
    protected void gvPesquisa_RowCreated(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            LinkButton lk = (LinkButton)e.Row.FindControl("lkCIOT");

            if (lk != null)
            {
                lk.CommandArgument = e.Row.RowIndex.ToString();
                lk.Attributes.Add("onMouseMove", "window.status='Exibir CIOT...'");
            }


            lk = (LinkButton)e.Row.FindControl("lkAddPgto");

            if (lk != null)
            {
                lk.CommandArgument = e.Row.RowIndex.ToString();
                lk.Attributes.Add("onMouseMove", "window.status='Adicionar parcela...'");
            }

            lk = (LinkButton)e.Row.FindControl("lkCancelarCIOT");

            if (lk != null)
            {
                lk.CommandArgument = e.Row.RowIndex.ToString();
                lk.Attributes.Add("onMouseMove", "window.status='Cancelar CIOT...'");
            }
        }
    }

    protected void btnVisulizarXml_Click(object sender, EventArgs e)
    {

        int i = 0;
        foreach (GridViewRow row in gvPesquisa.Rows)
        {

            CheckBox cb = (CheckBox)row.FindControl("chkItem");

            LinkButton lk = (LinkButton)row.FindControl("lkCIOT");
            if (cb.Checked && lk.Text.Trim() == "")
            {
                showError("Motorista " + row.Cells[4].Text + ": registro sem CIOT!");
                return;
            }

            if (cb != null && cb.Checked)
            {
                i++;

                if (i > 1)
                {
                    showError("Selecione apenas um CIOT para alterar!");
                    return;
                }
            }
        }

        foreach (GridViewRow row in gvPesquisa.Rows)
        {
            CheckBox cb = (CheckBox)row.FindControl("chkItem");
            if (cb != null && cb.Checked)
            {
                LinkButton lk = (LinkButton)row.FindControl("lkCIOT");

                if (lk != null)
                {
                    lk.Attributes.Add("onMouseMove", "window.status='Alterar CIOT...'");
                }

                {
                    string CIOT = row.Cells[31].Text;
                    string Cpf = row.Cells[03].Text;
                    string sessionID = Session["IDSession"].ToString();

                    body1.Attributes.Add("onload", "alterarCiot('" + sessionID + "','" + CIOT + "','" + Cpf + "');");

                    break;
                }
            }
            else
                showError("É necessário selecione um CIOT!");
        }
    }


}
