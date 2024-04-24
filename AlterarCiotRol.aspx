<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AlterarCiotRol.aspx.cs" Inherits="AeroCIOTWeb.AlterarCiotRol" %>

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
             .auto-style4 {
                 width: 757px;
             }
             .auto-style5 {
                 height: 19px;
                 width: 757px;
             }
             .auto-style6 {
                 height: 19px;
                 width: 116px;
             }
             .auto-style7 {
                 width: 116px;
             }
     </style>
    </head>
<body  runat="server" id="body1" >
    
    <form id="form1" runat="server">
 
      <table  style="border: 1px solid black; vertical-align: middle; text-align: left; font-size: 10pt; width: 700px; height: 221px;">
            <tr style="font-weight: bold; color: white; background-color: #0a246a;">
                <!--'<td align="center">Alterar CIOT Terceiro</td>-->
                <td align="center" colspan="2" class="auto-style4"><%=msg%></td>
            </tr>
            <tr style="color: #333333; background-color: #E2DED6;">
                <td align="left" class="auto-style5">
                    <table style="width: 690px">
                        <tr>
                            <td class="auto-style6">Banco</td>
                            <td colspan="1" class="auto-style1">&nbsp;</td>
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:DropDownList ID="DdLstBancos" CssClass="INPUTTEXTBLACK" runat="server" DataTextField="Ds_Banco" DataValueField="Cod_banco"  Width="100%">
                                </asp:DropDownList>
                            </td>
                        </tr>
                        <tr>
                            <td class="auto-style7">Agência</td>
                            <td>Conta Corrente</td>
                        </tr>
                        <tr>
                            <td class="auto-style7">
                                <asp:TextBox ID="txtAgencia" runat="server" CssClass="INPUTTEXTBLACK" Width="50px" onkeypress="return SoNumeros(event,'');" MaxLength="5" Enabled="True" ReadOnly="False"></asp:TextBox>
                            </td>
                            <td>
                                <asp:TextBox ID="txtContaCorrente" runat="server" CssClass="INPUTTEXTBLACK" Width="100px" onkeypress="return SoNumeros(event,'');" MaxLength="10" Enabled="True" ReadOnly="False"></asp:TextBox>
                            </td>
                        </tr>                        
                        <tr>
                            <td class="auto-style7">Motivo da alteração</td>                            
                        </tr>
                        <tr>
                            <td colspan="2">
                                <asp:TextBox ID="txtMotivoAlteracao" runat="server" Class="INPUTTEXTBLACK" Width="100%" Height="41px" TextMode="MultiLine" MaxLength="255"></asp:TextBox>                                        
                            </td>
                        </tr>
                        <tr>
                            <td colspan="5" align="center">
                                <asp:Button ID="btnGravarCIOT" runat="server" CssClass="INPUTTEXTBLACK" Text="Gravar alteração CIOT" Width="170px"  onClientClick="return confirm('Confirma geração das alterações do CIOT para os dados informados?')"  OnClick="btnGravarCIOT_Click" TabIndex="26" Enabled="False" />
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
