<%@ Page Language="C#" AutoEventWireup="true" Inherits="AeroCIOT_Pesquisa" Codebehind="Pesquisa.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head id="head1" runat="server">
     <title>Sistema AEROsoft - CIOT</title>
    <meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1" />
    <link href="https://www.aerosoftcargas.com.br/aeroctrl/css/dotnetStyles.css" type="text/css" rel="stylesheet" />
    
    <link rel="stylesheet" href="https://www.aerosoftcargas.com.br/jquery/plugin/lightbox/jquery.superbox.css" type="text/css" media="all" />
    
    <script type="text/javascript" src="https://www.aerosoftcargas.com.br/jquery/jquery.min.js"></script>
    <script type="text/javascript" src="https://www.aerosoftcargas.com.br/jquery/plugin/lightbox/jquery.superbox-min.js"></script>
   
    <script type="text/javascript" language="javascript">


        function SoNumeros(event, charEsp) {
            var charCode = (event.which) ? event.which : event.keyCode
            if (charEsp.indexOf(String.fromCharCode(charCode)) != -1)
                return true;
            if (charCode == 8)
                return true;
            if (charCode > 31 && (charCode < 48 || charCode > 57))
                return false;
            return true;
        }

        //20220712
        function adicionarParcela(sessionID, ciot, cpf) {

            var url = 'AdicionarPagamento.aspx?session=' + sessionID + '&ciot=' + ciot + '&cpf=' + cpf;

            document.getElementById('grmLnk').href = url;

            clickLink(document.getElementById('grmLnk'));

        }

        function cancelarCIOT(sessionID, ciot, cpf) {

            var url = 'CancelarCIOT.aspx?session=' + sessionID + '&ciot=' + ciot + '&cpf=' + cpf;

            document.getElementById('grmLnk').href = url;

            clickLink(document.getElementById('grmLnk'));

        }

        function alterarCiot(sessionID, ciot, cpf) {

            var url = 'AlterarCiotRol.aspx?session=' + sessionID + '&ciot=' + ciot + '&cpf=' + cpf;

            document.getElementById('grmLnk').href = url;

            clickLink(document.getElementById('grmLnk'));

        }

        function visualizarXml(sessionID, ciot, cpf) {

            var url = 'VisualizarXml.aspx?session=' + sessionID + '&ciot=' + ciot + '&cpf=' + cpf;

            document.getElementById('grmLnk').href = url;

            clickLink(document.getElementById('grmLnk'));

        }

        //20220915 Problemas ao passar o dot em sessão em alguns computadores...ORNELLAS
        function dot(base64) {

            var html = "<html><body>" +
                "<form action='DOT.aspx' method='POST' id='frm'>" +
                "<input type='hidden' name='dot' value='" + base64 + "'>" +

                "</form></body></html>";

            var jan = window.open("_blank");

            with (jan) {
                document.close();
                document.write(html);
                document.getElementById("frm").submit();

            }


        }

        //20220712 simula click em hyperlink...
        function clickLink(link) {

            var cancelled = false;


            if (document.createEvent) {

                var event = document.createEvent("MouseEvents");
                event.initMouseEvent("click", true, true, window,
                    0, 0, 0, 0, 0,
                    false, false, false, false,
                    0, null);
                cancelled = !link.dispatchEvent(event);

            }
            else if (link.fireEvent) {
                cancelled = !link.fireEvent("onclick");
            }

            if (!cancelled) {
                window.location = link.href;

            }
        }


    </script>



     </head>
<body runat="server" id="body1" >
    <form id="form1" runat="server">

        <div visible="true" id="CIOT" runat="server" style="overflow: auto; height: 100%; width:4600px;">

        <table style="background-color: #e2ded6;width:100%">
            <tr style="font-weight: bold; color: white; background-color: #0a246a;">
                <td align="left" >
                    Sistema AEROsoft - CIOT</td>
            </tr>
            <tr><td>
                <table align="left">
            <tr style="color: #333333; background-color: #e2ded6" align="left">
                <td >
                    CPF
                </td>
                <td >
                    Nome Motorista
                </td>
                <td >
                    Dt. Importação Arquivo
                </td>
                <td >
                    Número do CIOT</td>
                <td>
                    <a href="#" rel="superbox[iframe][760x610]" id="grmLnk"></a>
                </td>
                <td>
                    &nbsp;</td>
                <td>
                    &nbsp;</td>
                <td>
                    &nbsp;</td>
                <td>
                    &nbsp;</td>
                <td>
                    &nbsp;</td>
                <td>
                    &nbsp;</td>
            </tr>
            <tr style="color: #333333; background-color: #e2ded6" align="left" >
    			<td >
                    <asp:TextBox ID="txtCPF" runat="server" CssClass="INPUTTEXTBLACK" Width="100px" TabIndex="1"></asp:TextBox>
                </td>
                <td >
                    <asp:TextBox ID="txtNomeMotorista" runat="server" CssClass="INPUTTEXTBLACK" Width="232px" TabIndex="2"></asp:TextBox>
                </td>
                <td >
                    <asp:DropDownList ID="ddImportacao" runat="server" CssClass="INPUTTEXTBLACK" Width="200px" DataSourceID="dsImportacao" DataTextField="column1" DataValueField="id_importacao" OnSelectedIndexChanged="ddImportacao_SelectedIndexChanged" AutoPostBack="True" TabIndex="3">
                    </asp:DropDownList>
                    <asp:SqlDataSource ID="dsImportacao" runat="server" ConnectionString="<%$ ConnectionStrings:AEROSOFTConnectionString %>" SelectCommand="getDtImportacaoFOPAGCIOT" SelectCommandType="StoredProcedure">
                        <SelectParameters>
                            <asp:Parameter Direction="InputOutput" Name="id" Type="Int32" />
                        </SelectParameters>
                    </asp:SqlDataSource>
                </td>
                <td >
                    <asp:TextBox ID="txtCIOT" runat="server" CssClass="INPUTTEXTBLACK" Width="120px" TabIndex="4"></asp:TextBox>
                </td>
                <td >
                    <asp:Button ID="btnPesquisar" runat="server" CssClass="INPUTTEXTBLACK" Text="Pesquisar" Width="120px" OnClick="btnPesquisar_Click" TabIndex="5"/>
                </td>
                <td >
                    <asp:Button ID="btnGerarCIOT" runat="server" CssClass="INPUTTEXTBLACK" Text="Gerar CIOT" OnClick="btnGerarCIOT_Click" Width="120px" TabIndex="6" />
                </td>
                <td >
                    <asp:Button ID="btnFinalizarCIOT" runat="server" CssClass="INPUTTEXTBLACK" Text="Finalizar CIOT" OnClick="btnFinalizar_Click" Width="120px" TabIndex="7" />
                </td>
                <td style="margin-left: 120px" >
                    &nbsp;</td>
                <td style="margin-left: 120px" >
                    <asp:CheckBox ID="chkFinalizarAnterior" runat="server" Text="Finalizar Anterior" TabIndex="8" />
                </td>
                <td >
                    <asp:CheckBox ID="ckTpForcarEncerramento" runat="server" Text="Forçar Finalizar" TabIndex="9" />
                </td>                
                <td >
                    &nbsp;</td>   
                <td >
                    &nbsp;</td>
                <td >
                    &nbsp;</td>
                <td>            
                    &nbsp;</td>
                <td >
                    &nbsp;</td>
            </tr>
            <tr style="color: #333333; background-color: #e2ded6" align="left" >
    			<td >
                    Tp.Motorista</td>
    			<td >
                    Situação CIOT</td>
                <td colspan="2" rowspan="2" >
                    <asp:FileUpload ID="fupImportarTerceiros" runat="server" TabIndex="12" />
                </td>
                <td rowspan="2" >
                    <asp:Button ID="btnImportarTerceiros" runat="server" CssClass="INPUTTEXTBLACK" Text="Importar Terceiros" Width="120px" OnClick="btnImportarTerceiros_Click"  TabIndex="13" />
                </td>
                <td rowspan="2" >
                   <asp:Button ID="btnExcluir" runat="server" CssClass="INPUTTEXTBLACK" OnClick="btnExcluir_Click" Text="Excluir" Width="120px" TabIndex="14" /> 
                </td>
                <td rowspan="2" >
                   <asp:Button ID="btnConsultarResumo"  runat="server" CssClass="INPUTTEXTBLACK" Text="Consultar Resumo" Width="120px" OnClick="btnConsultarResumo_Click" TabIndex="15" /> 
                </td>                
                <td >
                    &nbsp;</td>   
                <td >
                    <asp:Button ID="btnDownloadDOT" runat="server" CssClass="INPUTTEXTBLACK" Text="Visualizar DOT" OnClick="btnDownloadDOT_Click" Width="120px" TabIndex="11" />
                </td>   
                <td >
                    &nbsp;</td>
                <td >
                    &nbsp;</td>
                <td>            
                    &nbsp;</td>
                <td >
                    &nbsp;</td>
            </tr>
            <tr style="color: #333333; background-color: #e2ded6" align="left" >
    			<td >
                    <asp:DropDownList ID="ddTpMotorista" runat="server"  CssClass="INPUTTEXTBLACK" Width="105px" TabIndex="10" >
                        <asp:ListItem Value="">Todos</asp:ListItem>
                        <asp:ListItem Value="T">Terceiro</asp:ListItem>
                        <asp:ListItem Value="A">Agregado</asp:ListItem>
                    </asp:DropDownList>
                </td>
    			<td >
                    <asp:DropDownList ID="ddSituacaoCIOT" runat="server"  CssClass="INPUTTEXTBLACK" Width="235px" TabIndex="11" >
                        <asp:ListItem Value="T">Todos</asp:ListItem>
                        <asp:ListItem Value="V">Vencidos</asp:ListItem>
                        <asp:ListItem Value="A">Ativos</asp:ListItem>
                    </asp:DropDownList>
                </td>
                <%-- 
                <td >
                    <asp:Button ID="btnVisualizarXml" runat="server" CssClass="INPUTTEXTBLACK" Text="Visualizar XML" OnClick="btnVisualizarXml_Click" Width="120px" TabIndex="9" />
                </td> --%>
                <td >
                    &nbsp;</td>   
                <td >
                    &nbsp;</td>   
                <td >
                    &nbsp;</td>
                <td >
                    &nbsp;</td>
                <td>            
                    &nbsp;</td>
                <td >
                    &nbsp;</td>
            </tr>
                   </table> </td></tr>
        </table>

              
                 
            <table>
                                               
                                                
            <tr style="font-weight: bold; color: #333333; background-color: #e2ded6">
                <td align="left" class="INPUTTEXTBLACK">
                    <div style="overflow: auto; width: 100%; height: 670px" id="DIV1" class="INPUTTEXTBLACK">
                    <asp:GridView ID="gvPesquisa" runat="server" AllowSorting="True" AutoGenerateColumns="False"
                        ForeColor="#333333" GridLines="None" OnRowCommand="gvPesquisa_RowCommand" Width="100%" AllowPaging="True" PageSize="200" OnPageIndexChanging="gvPesquisa_PageIndexChanging" OnSorting="gvPesquisa_Sorting" OnSelectedIndexChanged="gvPesquisa_SelectedIndexChanged" TabIndex="16" OnRowCreated="gvPesquisa_RowCreated">
                        <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />

                        <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                        <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
                        <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                        <Columns>
                            <asp:TemplateField>
                                <HeaderTemplate>
                                    <asp:CheckBox ID="chkTotal" runat="server" AutoPostBack="true" OnCheckedChanged="CheckUncheckAll"/>
                                </HeaderTemplate>
                                <ItemTemplate>
                                    <asp:CheckBox ID="chkItem" runat="server" Font-Size="8px" Font-Strikeout="False" ForeColor="White" />
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="CIOT" SortExpression="CIOT">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="lkCIOT" runat="server" Text='<%# Bind("CIOT") %>' CommandName="VisualizarCIOT"></asp:LinkButton>
                                    </ItemTemplate>
                                    <ItemStyle HorizontalAlign="Center" />
                            </asp:TemplateField>


                            <asp:BoundField DataField="Finalizado" HeaderText="Finalizado" />
                            <asp:BoundField DataField="CPF" HeaderText="CPF" SortExpression="CPF" />
                            <asp:BoundField DataField="Nome_Motorista" HeaderText="Nome Motorista" SortExpression="Nome_Motorista" >
                            <ItemStyle Wrap="False" />
                            </asp:BoundField>
                             <asp:BoundField DataField="MSG_ERRO" HeaderText="Mensagem de Erro" ItemStyle-Width="100px" SortExpression="MSG_ERRO"/>             
                            <asp:BoundField DataField="Data_Pagto" HeaderText="Dt.Pagto." DataFormatString="{0:d}" />
                            <asp:BoundField DataField="Rend_Bruto" HeaderText="Rend.Bruto" DataFormatString="{0:n2}" >
                            <ItemStyle HorizontalAlign="Right" />
                            </asp:BoundField>
                            <asp:BoundField DataField="Propaganda" HeaderText="Propaganda" >
                            <ItemStyle HorizontalAlign="Right" />
                            </asp:BoundField>
                            <asp:BoundField DataField="Cod_Banco" HeaderText="Cód.Banco" />
                            <asp:BoundField DataField="Agencia" HeaderText="Agência" >
                            <ItemStyle Wrap="False" />
                            </asp:BoundField>
                            <asp:BoundField DataField="Conta" HeaderText="C/C" >
                            <ItemStyle Wrap="False" />
                            </asp:BoundField>
                            <asp:BoundField DataField="Cod_RNTRC" HeaderText="RNTRC Motorista" />
                            <asp:BoundField DataField="Placa_Veiculo" HeaderText="Placa" />
                            <asp:BoundField DataField="Contratante" HeaderText="Contratante" >
                            <ItemStyle Wrap="False" />
                            </asp:BoundField>
                            <asp:BoundField DataField="CGC" HeaderText="CNPJ" />
                            <asp:BoundField DataField="CEP" HeaderText="CEP" >
                            <ItemStyle Wrap="False" />
                            </asp:BoundField>
                            <asp:BoundField DataField="Cidade" HeaderText="Cidade" >
                            <ItemStyle Wrap="False" />
                            </asp:BoundField>
                            <asp:BoundField DataField="SIG_ESTADO" HeaderText="Estado" />
                            <asp:BoundField DataField="ENDERECO_LOG" HeaderText="Log." />
                            <asp:BoundField DataField="ENDERECO" HeaderText="Endereço" >
                            <ItemStyle Wrap="False" />
                            </asp:BoundField>
                            <asp:BoundField DataField="ENDERECO_NR" HeaderText="Nº" />
                            <asp:BoundField DataField="ENDERECO_COMPL" HeaderText="Compl." />
                            <asp:BoundField DataField="BAIRRO" HeaderText="Bairro" >
                            <ItemStyle Wrap="False" />
                            </asp:BoundField>
                            <asp:BoundField DataField="RNTC" HeaderText="RNTRC Contratante" />
                            <asp:BoundField DataField="DS_BANCO" HeaderText="Banco" >
                            <ItemStyle Wrap="False" />
                            </asp:BoundField>
                            <asp:BoundField DataField="Ativo" HeaderText="Mot.Ativo" />
                            <asp:BoundField DataField="tipo" HeaderText="Tipo Mot." />
                            <asp:BoundField DataField="data_geracao" HeaderText="Dt.Geração CIOT" DataFormatString="{0:d}" />
                            <asp:BoundField DataField="INICIO_VIAGEM" HeaderText="Início Viagem" DataFormatString="{0:d}" />
                            <asp:BoundField DataField="FINAL_VIAGEM" HeaderText="Final Viagem" DataFormatString="{0:d}" />
                            <asp:BoundField DataField="ID_FOPAG_CIOT_ITEM" HeaderText="ID" SortExpression="ID" />
                            <asp:BoundField DataField="IR" HeaderText="IR" >
                            <ItemStyle HorizontalAlign="Right" />
                            </asp:BoundField>
                            <asp:BoundField DataField="INSS" HeaderText="INSS" >
                            <ItemStyle HorizontalAlign="Right" />
                            </asp:BoundField>
                            <asp:BoundField DataField="SEST_SENAT" HeaderText="SEST/SENAT" >
                            <ItemStyle HorizontalAlign="Right" />
                            </asp:BoundField>
                            <asp:BoundField DataField="ID" HeaderText="ID CIOT"/>
                            <asp:BoundField DataField="CPF_PROPRIETARIO" HeaderText="CPF Proprietário" SortExpression="CPF_PROPRIETARIO" />
                            <asp:BoundField DataField="NOME_PROPRIETARIO" HeaderText="Nome Proprietário" SortExpression="NOME_PROPRIETARIO" >
                            <ItemStyle Wrap="False" />
                            </asp:BoundField>
                            <asp:BoundField DataField="CNPJ_DESTINO" HeaderText="CNPJ Destino" />
                            <asp:BoundField DataField="saldo" DataFormatString="{0:C}" HeaderText="Adiantamento" SortExpression="saldo" />
                                       
                            
                            <asp:TemplateField>
                                <ItemStyle Wrap="False" />
                                <ItemTemplate>                                    
                                 <asp:LinkButton ID="lkCancelarCIOT" runat="server" 
                                     CommandName= <%# Convert.ToInt32(Eval("TP_CANCELAMENTO")) == 1 ? "cancelarCIOT" : "" %> >
                                      <%# Convert.ToInt32(Eval("TP_CANCELAMENTO")) == 1 ? "Cancelar CIOT" : "Cancelado" %>&nbsp;</asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField>
                                <ItemTemplate>
                                    <asp:LinkButton ID="lkViewXml" runat="server" CommandName="visualizarXml">Visualizar XML</asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:BoundField DataField="Referencias" HeaderText="Manifesto/ROL" />     
                        </Columns>
                        <EditRowStyle BackColor="#999999" />
                        <AlternatingRowStyle BackColor="White" ForeColor="#284775" />

                             <SelectedRowStyle BackColor="#D1DDF1" Font-Bold="True" ForeColor="#333333" />

                    </asp:GridView>
                    
                     <asp:HiddenField ID="hdImportacao" runat="server" Value="139" />
                        </div>
                    <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" CellPadding="4" DataSourceID="getFOPAG_CIOT_HISTORICOODS" EnableModelValidation="True" ForeColor="#333333" GridLines="None" AllowPaging="True" PageSize="15">
                        <AlternatingRowStyle BackColor="White" ForeColor="#284775" />
                        <Columns>
                            <asp:BoundField DataField="DT_INCLUSAO" HeaderText="Data/Hora" SortExpression="DT_INCLUSAO" />
                            <asp:BoundField DataField="CM_IMPORTACAO" HeaderText="Comentários" SortExpression="CM_IMPORTACAO" />
                        </Columns>
                        <EditRowStyle BackColor="#999999" />
                        <FooterStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                        <HeaderStyle BackColor="#5D7B9D" Font-Bold="True" ForeColor="White" />
                        <PagerStyle BackColor="#284775" ForeColor="White" HorizontalAlign="Center" />
                        <RowStyle BackColor="#F7F6F3" ForeColor="#333333" />
                        <SelectedRowStyle BackColor="#E2DED6" Font-Bold="True" ForeColor="#333333" />
                    </asp:GridView>
                    <asp:ObjectDataSource ID="getFOPAG_CIOT_HISTORICOODS" runat="server" OldValuesParameterFormatString="original_{0}" SelectMethod="GetData" TypeName="AeroCIOTWeb.dsCIOTTableAdapters.getFOPAG_CIOT_HISTORICOTableAdapter">
                        <SelectParameters>
                            <asp:ControlParameter ControlID="ddImportacao" Name="id_importacao" PropertyName="SelectedValue" Type="Int32" />
                            <asp:ControlParameter ControlID="txtCIOT" Direction="InputOutput" Name="CIOT" PropertyName="Text" Type="String" />
                        </SelectParameters>
                    </asp:ObjectDataSource>
                </td>
            </tr>
              
        </table>
            </div>

        

        <div visible="false" id="Terceiro" runat="server" style="overflow: auto; height: 100%; width:100%;">
        <!--
        <iframe src="file:\\192.168.1.18\j$\Integracao\DOT_083000124026XXXX.pdf" width="1000" height="500"></iframe>
        -->

        <table  style="vertical-align: middle; text-align: left; border-right: black 1px solid;
                border-top: black 1px solid; border-left: black 1px solid; border-bottom: black 1px solid;
                font-size: 10pt;">
            <tr style="font-weight: bold; color: white; background-color: #0a246a;">
                <td align="center">Gerar CIOT Terceiro</td>
            </tr>
            <tr style="color: #333333; background-color: #E2DED6;">
                <td align="left" class="auto-style1">
                    <table>
                        <tr>
                            <td align="left">
                                <table>
                                    <tr>
                                        <td colspan="2">ND's geradas pela Coleta</td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:DropDownList ID="ddGrupoND" runat="server" CssClass="INPUTTEXTBLACK" Enabled="False" Width="55px">
                                                <asp:ListItem Selected="True" Value="1">SAO</asp:ListItem>
                                                <asp:ListItem Value="2">BHZ</asp:ListItem>
                                                <asp:ListItem Value="3">RIO</asp:ListItem>
                                                <asp:ListItem Value="6">BSB</asp:ListItem>
                                                <asp:ListItem Value="12">MAO</asp:ListItem>
                                                <asp:ListItem Value="102">AGE</asp:ListItem>
                                            </asp:DropDownList>
                                        </td>
                                        <td>
                                            <asp:DropDownList ID="ddND" runat="server" CssClass="INPUTTEXTBLACK" Width="120px">
                                            </asp:DropDownList>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr>
                            <td>CPF do Motorista</td>
                            <td colspan="4">Motorista</td>
                        </tr>
                        <tr>
                            <td>
                                <asp:TextBox ID="txtCPFTerceiro" runat="server" AutoPostBack="True" CssClass="INPUTTEXTBLACK" Width="122px" onkeypress="return SoNumeros(event,'');" MaxLength="11" OnTextChanged="txtCPFTerceiro_TextChanged" TabIndex="15"></asp:TextBox>
                            </td>
                            <td colspan="1" valign="top">
                                <asp:TextBox ID="txtNomeTerceiro" runat="server" CssClass="INPUTTEXTBLACK" Width="400px" Enabled="False"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td>Código do Banco</td>
                            <td colspan="1">Banco</td>
                        </tr>
                        <tr>
                            <td>
                                <asp:TextBox ID="txtCodigoBanco" runat="server" AutoPostBack="True" CssClass="INPUTTEXTBLACK" Width="50px" onkeypress="return SoNumeros(event,'');" MaxLength="4" Enabled="False"></asp:TextBox>
                            </td>
                            <td colspan="1">
                                <asp:TextBox ID="txtNomeBanco" runat="server" CssClass="INPUTTEXTBLACK" Width="400px" Enabled="False"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td>Agência</td>
                            <td>Conta Corrente</td>
                        </tr>
                        <tr>
                            <td>
                                <asp:TextBox ID="txtAgencia" runat="server" CssClass="INPUTTEXTBLACK" Width="50px" onkeypress="return SoNumeros(event,'');" MaxLength="5" Enabled="False"></asp:TextBox>
                            </td>
                            <td>
                                <asp:TextBox ID="txtContaCorrente" runat="server" CssClass="INPUTTEXTBLACK" Width="100px" onkeypress="return SoNumeros(event,'');" MaxLength="10" Enabled="False"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="5">
                                <table>
                                    <tr>
                                        <td>Data de Pagamento</td>
                                        <td>Rendimento Bruto</td>
                                        <td>IR Retido</td>
                                        <td>Contribuição INSS</td>
                                        <td>INSS Transp.</td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:TextBox ID="txtDataPagamento" runat="server" AutoPostBack="True" CssClass="INPUTTEXTBLACK" Width="100px" onkeypress="return SoNumeros(event,'/');" MaxLength="10" TabIndex="16"></asp:TextBox>
                                        </td>
                                        <td>
                                            <asp:TextBox ID="txtRendimentoBruto" runat="server" CssClass="INPUTTEXTBLACK" Width="100px" onkeypress="return SoNumeros(event,',');" MaxLength="10" TabIndex="17"></asp:TextBox>
                                        </td>
                                        <td>
                                            <asp:TextBox ID="txtIRRetido" runat="server" CssClass="INPUTTEXTBLACK" Width="100px" onkeypress="return SoNumeros(event,',');" MaxLength="10" TabIndex="18"></asp:TextBox>
                                        </td>
                                        <td>
                                            <asp:TextBox ID="txtContribuicaoINSS" runat="server" CssClass="INPUTTEXTBLACK" Width="100px" onkeypress="return SoNumeros(event,',');" MaxLength="10" TabIndex="19"></asp:TextBox>
                                        </td>
                                        <td>
                                            <asp:TextBox ID="txtINSSTransp" runat="server" CssClass="INPUTTEXTBLACK" Width="100px" onkeypress="return SoNumeros(event,',');" MaxLength="10" TabIndex="20"></asp:TextBox>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                        <tr>
                            <td>CPF do Proprietário do Veículo</td>
                            <td>Proprietário</td>
                        </tr>
                        <tr>
                            <td>
                                <asp:TextBox ID="txtCPFProprietario" runat="server" AutoPostBack="True" CssClass="INPUTTEXTBLACK" Width="100px" onkeypress="return SoNumeros(event,'');" MaxLength="11" OnTextChanged="txtCPFProprietario_TextChanged" TabIndex="21"></asp:TextBox>
                            </td>
                            <td colspan="1" valign="top">
                                <asp:TextBox ID="txtNomeProprietario" runat="server" CssClass="INPUTTEXTBLACK" Width="400px" Enabled="False"></asp:TextBox>
                            </td>

                        </tr>
                        <tr>
                            <td class="auto-style7">Placa do Veículo</td>
                            <td class="auto-style7"></td>
                        </tr>
                        <tr>
                            <td>
                                <asp:TextBox ID="txtPlaca" runat="server" AutoPostBack="True" CssClass="INPUTTEXTBLACK" Width="100px" OnTextChanged="txtPlaca_TextChanged" MaxLength="7" TabIndex="22"></asp:TextBox>
                            </td>
                            <td></td>
                        </tr>
                        <tr>
                            <td>CNPJ/CPF do Destinatário</td>
                            <td>Destinatário</td>
                        </tr>
                        <tr>
                            <td>
                                <asp:TextBox ID="txtCNPJDestinatario" runat="server" AutoPostBack="True"
                                    CssClass="INPUTTEXTBLACK" Width="122px" OnTextChanged="txtCNPJDestinatario_TextChanged" TabIndex="23"></asp:TextBox>
                            </td>
                            <td>
                                <asp:TextBox ID="txtNomeDestinatario" runat="server" CssClass="INPUTTEXTBLACK" Width="400px" Enabled="False"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td>Início da Operação</td>
                            <td>Término da Operação</td>
                        </tr>
                        <tr>
                            <td>
                                <asp:TextBox ID="txtInicioOperacao" runat="server" AutoPostBack="True" CssClass="INPUTTEXTBLACK" Width="100px" onkeypress="return SoNumeros(event,'/');" MaxLength="10" TabIndex="24"></asp:TextBox>
                            </td>
                            <td colspan="1" valign="top">
                                <asp:TextBox ID="txtTerminoOperacao" runat="server" CssClass="INPUTTEXTBLACK" Width="100px" onkeypress="return SoNumeros(event,'/');" MaxLength="10" TabIndex="25"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="5" align="center">
                                <asp:Button ID="btnGerarCIOTTerceiro" runat="server" CssClass="INPUTTEXTBLACK" Text="Gerar CIOT" Width="85px" OnClick="btnGerarCIOTTerceiro_Click" TabIndex="26" />
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
        </div>
    </form>
        
</body>

    
<script language="javascript" type="text/javascript">

    $(function () {
        $.superbox();
    });

</script>
</html>