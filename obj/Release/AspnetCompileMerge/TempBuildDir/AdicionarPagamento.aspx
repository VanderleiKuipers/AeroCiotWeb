<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdicionarPagamento.aspx.cs" Inherits="AeroCIOTWeb.AdicionarPagamento" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Adicionar pagamento</title>
    <link href="https://www.aerosoftcargas.com.br/aeroctrl/css/dotnetStyles.css" type="text/css" rel="stylesheet" />

</head>
<body runat="server" id="body1">
    <form id="form1" runat="server">
    <div>
        <table style="width:100%" > <tr style="color: #333333; background-color: #e2ded6;" align="left" ><td>
    <table style="width:100%">
         <tr style="font-weight: bold; color: white; background-color: #0a246a; text-align: center">
                <td align="left" colspan="2"><%=msg%></td>
            </tr>
        <tr><td><table align="center">
        <tr style="color: #333333; background-color: #e2ded6" align="left">         
           
            <td>Data de Previsão:</td>
            <td><input type="text" id="dataPrevisao" runat="server" placeholder="dd/mm/yyyy" onkeyup=" var v = this.value; 
                if (v.match(/^\d{2}$/) !== null) 
                    { this.value = v + '/'; } 
                else if (v.match(/^\d{2}\/\d{2}$/) !== null) 
                    {this.value = v + '/';}" class="INPUTTEXTBLACK"/></td>
       
            <td>Valor(R$):</td>
            <td><input type="text" id="valorAplicado" runat="server" class="INPUTTEXTBLACK"/></td>
             <td>
                <asp:Button ID="btnAdicionarParcela" runat="server" Text="Salvar" CssClass="INPUTTEXTBLACK" OnClick="btnAdicionarParcela_Click" /></td>
        </tr>

            </table>
            </td></tr>

         
    </table>
            </td></tr></table>
    </div>
    </form>
</body>


</html>
