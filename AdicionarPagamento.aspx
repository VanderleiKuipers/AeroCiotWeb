<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AdicionarPagamento.aspx.cs" Inherits="AeroCIOTWeb.AdicionarPagamento" %>

<!DOCTYPE html>


<html xmlns="http://www.w3.org/1999/xhtml">

    
<head runat="server">
    <title>Alterar valor do CIOT</title>
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />    

    <link href="https://www.aerosoftcargas.com.br/aeroctrl/css/dotnetStyles.css" type="text/css" rel="stylesheet" />

<!-- -->
    
    <link rel="stylesheet" href="https://www.aerosoftcargas.com.br/jquery/plugin/lightbox/jquery.superbox.css" type="text/css" media="all" />
    
    <script type="text/javascript" src="https://www.aerosoftcargas.com.br/jquery/jquery.min.js"></script>
    <script type="text/javascript" src="https://www.aerosoftcargas.com.br/jquery/plugin/lightbox/jquery.superbox-min.js"></script>

<!-- -->
<script type="text/javascript">
    /*Fun��o Pai de Mascaras*/
    function Mascara(o, f) {
        v_obj = o
        v_fun = f
        setTimeout("execmascara()", 1)
    }

    /*Fun��o que Executa os objetos*/
    function execmascara() {
        v_obj.value = v_fun(v_obj.value)
    }

    /*Fun��o que padroniza valor mon�tario*/
    function Valor(v) {
        v = v.replace(/\D/g, "") //Remove tudo o que n�o � d�gito
        v = v.replace(/^([0-9]{3}\.?){3}-[0-9]{2}$/, "$1,$2");
        //v=v.replace(/(\d{3})(\d)/g,"$1,$2")
        v = v.replace(/(\d)(\d{2})$/, "$1,$2") //Coloca ponto antes dos 2 �ltimos digitos
        return v
    }
    </script>


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
            <td>Data do Pagamento:</td>
            <td><asp:TextBox ID="txtdataPrevisao" runat="server" CssClass="INPUTTEXTBLACK" Width="100px"  MaxLength="10" Enabled="True" placeholder="dd/mm/yyyy" onkeyup=" var v = this.value; 
                if (v.match(/^\d{2}$/) !== null) 
                    { this.value = v + '/'; } 
                else if (v.match(/^\d{2}\/\d{2}$/) !== null) 
                    {this.value = v + '/';}" class="INPUTTEXTBLACK"/></asp:TextBox>
            </td>
       
            <td align="right">Valor do Frete:</td>
            <td align="right"><asp:TextBox ID="txtValorAplicado" runat="server" CssClass="INPUTTEXTBLACK" Width="100px"  MaxLength="10" Enabled="True" ></asp:TextBox>
            </td>
            <td  align="center" class="auto-style12" colspan="2">
                <asp:Button ID="btnCalcular" runat="server" Text="Calcular" CssClass="INPUTTEXTBLACK" OnClick="btnCalcular_Click" />
            </td>
        </tr>
        <tr>
            <td>Vl.Bruto</td>
            <td>INSS</td>
            <td>IR Retido</td>
            <td>Vl.ISS</td>
            <td>INSS Transp.</td>                                    
        </tr>
        <tr>
            <td >
                <asp:TextBox ID="txtVlrBruto" runat="server" CssClass="INPUTTEXTBLACK" Width="100px"  MaxLength="10" Enabled="False" ReadOnly="True" ></asp:TextBox>
            </td>
            <td >
                <asp:TextBox ID="txtVlrInss" runat="server" CssClass="INPUTTEXTBLACK" Width="100px"  MaxLength="10" Enabled="False" ReadOnly="True"></asp:TextBox>
            </td>
            <td >
                <asp:TextBox ID="txtIRr" runat="server" CssClass="INPUTTEXTBLACK" Width="100px"  MaxLength="10" Enabled="False" ReadOnly="True"></asp:TextBox>
            </td>
            <td >
                <asp:TextBox ID="txtVlrIss" runat="server" CssClass="INPUTTEXTBLACK" Width="100px"  MaxLength="10" Enabled="False" ReadOnly="True"></asp:TextBox>
            </td>
            <td >
                <asp:TextBox ID="txtVlrInssTransp" runat="server" CssClass="INPUTTEXTBLACK" Width="100px"  MaxLength="10" Enabled="False" ReadOnly="True"></asp:TextBox>
            </td>                                    
        </tr>
        <tr>
            <td>Propaganda</td>
            <td>Outros Desc.</td>
            <td>Vl.L&iacute;quido</td>
            <td>Saldo</td>
            <td>Dt.Pgto.Saldo</tVl.Líquido</td>
            <td>Saldo   </tr>
        <tr>
            <td >
                <asp:TextBox ID="txtVlrPropaganda" runat="server" CssClass="INPUTTEXTBLACK" Width="100px"  MaxLength="10" Enabled="False" ReadOnly="True"></asp:TextBox>
            </td>
            <td >
                <asp:TextBox ID="txtVlrOutroDesc" runat="server" CssClass="INPUTTEXTBLACK" Width="100px"  MaxLength="10" Enabled="False" ReadOnly="True"></asp:TextBox>
            </td>
            <td >
                <asp:TextBox ID="txtVlrLiquido" runat="server" CssClass="INPUTTEXTBLACK" Width="100px"  MaxLength="10" Enabled="False" ReadOnly="True"></asp:TextBox>
            </td>
            <td >
                <asp:TextBox ID="txtVlrSaldo" runat="server" CssClass="INPUTTEXTBLACK" Width="100px"  MaxLength="10" Enabled="False" ReadOnly="True"></asp:TextBox>
            </td>
            <td >
                <asp:TextBox ID="txtDtaPgtoSaldo" runat="server" CssClass="INPUTTEXTBLACK" Width="100px"  MaxLength="10" Enabled="False" ReadOnly="True"></asp:TextBox>
            </td>                                    
        </tr>
        <tr>
            <td  align="center" class="auto-style12" colspan="5">
                <asp:Button ID="btnAdicionarParcela" runat="server" Text="Gravar" CssClass="INPUTTEXTBLACK" OnClick="btnAdicionarParcela_Click" />

            </td></tr>
       </table>
            </td></tr>         
    </table>
            </td></tr></table>
    </div>
    </form>
</body>


</html>
