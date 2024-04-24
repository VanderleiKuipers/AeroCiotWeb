<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CancelarCIOT.aspx.cs" Inherits="AeroCIOTWeb.CancelarCIOT" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">

    <title>Cancelar CIOT</title>
    <link href="https://www.aerosoftcargas.com.br/aeroctrl/css/dotnetStyles.css" type="text/css" rel="stylesheet" />
     
</head>
<body runat="server" id="body1">
    <form id="form1" runat="server">
    <div>
        <table style="width: 700px; height: 147px;"> 
            <tr style="color: #333333; background-color: #e2ded6;" align="left" >
                <td>
                    <table style="width:100%; margin-bottom: 34px;">
                        <tr style="font-weight: bold; color: white; background-color: #0a246a; text-align: center">
                            <td align="left" colspan="2"><%=msg%></td>
                        </tr>
                        <tr>
                            <td>
                                <table align="center">
                                    <tr style="color: #333333; background-color: #e2ded6" align="left">                      
                                        <td>Motivo cancelamento:</td>
                                        <td>
                                            <asp:TextBox ID="txtMotivoCancelamento" runat="server" Class="INPUTTEXTBLACK" Width="601px" Height="61px" TextMode="MultiLine" MaxLength="255"></asp:TextBox>
                                        </td>       
                                        <tr><td align="center" colspan ="2" >
                                            <asp:Button ID="btnCancelarCIOT" runat="server" Text="Salvar" Class="INPUTTEXTBLACK" OnClick="btnCandelar_Click" /></td>
                                        </tr>
                                    </tr>
                                </table>
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
