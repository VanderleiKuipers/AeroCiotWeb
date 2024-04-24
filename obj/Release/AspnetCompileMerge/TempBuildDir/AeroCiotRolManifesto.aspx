<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AeroCiotRolManifesto.aspx.cs" Inherits="AeroCIOTWeb.AeroCiotRolManifesto" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">

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

    </script>

    <script language="javascript">
        function mascara_data(data, controle) {
            var mydata = '';
            mydata = mydata + data;
            if (mydata.length == 2) {
                mydata = mydata + '/';
                if (controle == 0)
                    document.form1.txtDataPagamento.value = mydata;
                if (controle == 1)
                    document.form1.txtDtpagamentoSaldo.value = mydata;
                if (controle == 2)
                    document.form1.txtInicioOperacao.value = mydata;
                if (controle == 3)
                    document.form1.txtTerminoOperacao.value = mydata;
                }
            
            if (mydata.length == 5) {
                mydata = mydata + '/';
                if (controle == 0)
                    document.form1.txtDataPagamento.value = mydata;
                if (controle == 1)
                    document.form1.txtDtpagamentoSaldo.value = mydata;
                if (controle == 2)
                    document.form1.txtInicioOperacao.value = mydata;
                if (controle == 3)
                    document.form1.txtTerminoOperacao.value = mydata;
            }
        }
    </script>

    <script type="text/javascript">
        /*Função Pai de Mascaras*/
        function Mascara(o, f) {
            v_obj = o
            v_fun = f
            setTimeout("execmascara()", 1)
        }

        /*Função que Executa os objetos*/
        function execmascara() {
            v_obj.value = v_fun(v_obj.value)
        }

        /*Função que padroniza valor monétario*/
        function Valor(v) {
            v = v.replace(/\D/g, "") //Remove tudo o que não é dígito
            v = v.replace(/^([0-9]{3}\.?){3}-[0-9]{2}$/, "$1,$2");
            //v=v.replace(/(\d{3})(\d)/g,"$1,$2")
            v = v.replace(/(\d)(\d{2})$/, "$1,$2") //Coloca ponto antes dos 2 últimos digitos
            return v
        }

</script>
    <title></title>
     <style type="text/css">
         .auto-style1 {
             height: 19px;
         }
         .INPUTTEXTBLACK {}
         .auto-style2 {
             width: 108px;
         }
         .auto-style3 {
             width: 108px;
             height: 19px;
         }
     </style>
</head>
<body  runat="server" id="body1" >
    
    <form id="form1" runat="server">
    <div>
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
                                        <td colspan="2">ROL/Manifesto</td>
                                    </tr>
                                    <tr>
                                        <td>
                                <asp:TextBox ID="txtGrpRol" runat="server" CssClass="INPUTTEXTBLACK" Width="50px" onkeypress="return SoNumeros(event,'');" MaxLength="5" Enabled="False" ReadOnly="True"></asp:TextBox>
                                        </td>
                                        <td>
                                <asp:TextBox ID="txtNrRol" runat="server" CssClass="INPUTTEXTBLACK" Width="90px" onkeypress="return SoNumeros(event,'');" MaxLength="5" Enabled="False" ReadOnly="True"></asp:TextBox>
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
                                <asp:TextBox ID="txtCPFTerceiro" runat="server" AutoPostBack="True" CssClass="INPUTTEXTBLACK" Width="122px" onkeypress="return SoNumeros(event,'');" MaxLength="11" TabIndex="15" ReadOnly="True"></asp:TextBox>
                            </td>
                            <td colspan="1" valign="left">
                                <asp:TextBox ID="txtNomeTerceiro" runat="server" CssClass="INPUTTEXTBLACK" Width="601px" Enabled="False" ReadOnly="True"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td class="auto-style1">Código do Banco</td>
                            <td colspan="1" class="auto-style1">Banco</td>
                        </tr>
                        <tr>
                            <td>
                                <asp:TextBox ID="txtCodigoBanco" runat="server" AutoPostBack="True" CssClass="INPUTTEXTBLACK" Width="50px" onkeypress="return SoNumeros(event,'');" MaxLength="4" Enabled="False" ReadOnly="True"></asp:TextBox>
                            </td>
                            <td colspan="1">
                                <asp:TextBox ID="txtNomeBanco" runat="server" CssClass="INPUTTEXTBLACK" Width="601px" Enabled="False" ReadOnly="True"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td>Agência</td>
                            <td>Conta Corrente</td>
                        </tr>
                        <tr>
                            <td>
                                <asp:TextBox ID="txtAgencia" runat="server" CssClass="INPUTTEXTBLACK" Width="50px" onkeypress="return SoNumeros(event,'');" MaxLength="5" Enabled="False" ReadOnly="True"></asp:TextBox>
                            </td>
                            <td>
                                <asp:TextBox ID="txtContaCorrente" runat="server" CssClass="INPUTTEXTBLACK" Width="100px" onkeypress="return SoNumeros(event,'');" MaxLength="10" Enabled="False" ReadOnly="True"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="7">
                                <table>
                                    <tr>
                                        <td class="auto-style1">Data de Pagamento</td>
                                        <td class="auto-style3">Rendimento Bruto</td>
                                        <td class="auto-style1">IR Retido</td>
                                        <td class="auto-style1">Contribuição INSS</td>
                                        <td class="auto-style1">INSS Transp.</td>
                                        <td class="auto-style1">Saldo</td>
                                        <td class="auto-style1">Dt. Pagto Saldo</td>
                                    </tr>
                                    <tr>
                                        <td>
                                            <asp:TextBox ID="txtDataPagamento" runat="server" AutoPostBack="True" CssClass="INPUTTEXTBLACK" Width="100px" onkeypress="return SoNumeros(event,'/');" onkeyDown="javascript:return mascara_data(this.value, 0);"  MaxLength="10" TabIndex="16"></asp:TextBox>
                                        </td>
                                        <td class="auto-style2">
                                            <asp:TextBox ID="txtRendimentoBruto" runat="server" CssClass="INPUTTEXTBLACK" Width="100px" onKeyDown="Mascara(this,Valor);" onKeyPress="Mascara(this,Valor);" onKeyUp="Mascara(this,Valor);" MaxLength="10" TabIndex="17"></asp:TextBox>
                                        </td>
                                        <td>
                                            <asp:TextBox ID="txtIRRetido" runat="server" CssClass="INPUTTEXTBLACK" Width="100px" onKeyDown="Mascara(this,Valor);" onKeyPress="Mascara(this,Valor);" onKeyUp="Mascara(this,Valor);" MaxLength="10" TabIndex="18"></asp:TextBox>
                                        </td>
                                        <td>
                                            <asp:TextBox ID="txtContribuicaoINSS" runat="server" CssClass="INPUTTEXTBLACK" Width="100px" onKeyDown="Mascara(this,Valor);" onKeyPress="Mascara(this,Valor);" onKeyUp="Mascara(this,Valor);" MaxLength="10" TabIndex="19"></asp:TextBox>
                                        </td>
                                        <td>
                                            <asp:TextBox ID="txtINSSTransp" runat="server" CssClass="INPUTTEXTBLACK" Width="100px" onKeyDown="Mascara(this,Valor);" onKeyPress="Mascara(this,Valor);" onKeyUp="Mascara(this,Valor);" MaxLength="10" TabIndex="20"></asp:TextBox>
                                        </td>
                                        <td>
                                            <asp:TextBox ID="txtSaldo" runat="server" CssClass="INPUTTEXTBLACK" Width="100px" onKeyDown="Mascara(this,Valor);" onKeyPress="Mascara(this,Valor);" onKeyUp="Mascara(this,Valor);" MaxLength="10" TabIndex="19"></asp:TextBox>
                                        </td>
                                        <td>
                                            <asp:TextBox ID="txtDtpagamentoSaldo" runat="server" CssClass="INPUTTEXTBLACK" Width="100px" onkeypress="return SoNumeros(event,',');" onkeyDown="javascript:return mascara_data(this.value, 1);" MaxLength="10" TabIndex="19"></asp:TextBox>
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
                                <asp:TextBox ID="txtCPFProprietario" runat="server" AutoPostBack="True" CssClass="INPUTTEXTBLACK" Width="100px" onkeypress="return SoNumeros(event,'');" MaxLength="11"  TabIndex="21" ReadOnly="True"></asp:TextBox>
                            </td>
                            <td colspan="1" valign="top">
                                <asp:TextBox ID="txtNomeProprietario" runat="server" CssClass="INPUTTEXTBLACK" Width="281px" Enabled="False" ReadOnly="True"></asp:TextBox>
                                
                            </td>
                             
                                
                           

                        </tr>
                        <tr>
                            <td class="auto-style7">Placa do Veículo</td>
                            <td class="auto-style7"></td>
                        </tr>
                        <tr>
                            <td>
                                <asp:TextBox ID="txtPlaca" runat="server" AutoPostBack="True" CssClass="INPUTTEXTBLACK" Width="100px"  MaxLength="7" TabIndex="22" ReadOnly="True"></asp:TextBox>
                            </td>
                            <td></td>
                        </tr>
                        <tr>
                            <td class="auto-style1">CNPJ/CPF do Destinatário</td>
                            <td class="auto-style1">Destinatário</td>
                        </tr>
                        <tr>
                            <td>
                                <asp:TextBox ID="txtCNPJDestinatario" runat="server" AutoPostBack="True"
                                    CssClass="INPUTTEXTBLACK" Width="122px" OnTextChanged="txtCNPJDestinatario_TextChanged" TabIndex="23"></asp:TextBox>
                            </td>
                            <td>
                                <asp:TextBox ID="txtNomeDestinatario" runat="server" CssClass="INPUTTEXTBLACK" Width="600px" Enabled="False"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td>Início da Operação</td>
                            <td>Término da Operação</td>
                        </tr>
                        <tr>
                            <td>
                                <asp:TextBox ID="txtInicioOperacao" runat="server" AutoPostBack="True" CssClass="INPUTTEXTBLACK" Width="100px" onkeyDown="javascript:return mascara_data(this.value, 2);"  MaxLength="10" TabIndex="24"></asp:TextBox>
                            </td>
                            <td colspan="1" valign="top">
                                <asp:TextBox ID="txtTerminoOperacao" runat="server" CssClass="INPUTTEXTBLACK" Width="100px" onkeyDown="javascript:return mascara_data(this.value, 3);" MaxLength="10" TabIndex="25"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="5" align="center">
                                <asp:Button ID="btnGerarCIOTTerceiro" runat="server" CssClass="INPUTTEXTBLACK" Text="Gerar CIOT" Width="85px"  onClientClick="return confirm('Confirma geração do CIOT para os dados informados?')"  OnClick="btnGerarCIOTTerceiro_Click" TabIndex="26" Enabled="False" />
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
    </div>
    </form>
</body>
</html>
