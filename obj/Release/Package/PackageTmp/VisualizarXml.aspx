<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="VisualizarXml.aspx.cs" Inherits="AeroCIOTWeb.VisualizarXml" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">

    <title>Visualizar XML CIOT</title>
    <link href="https://www.aerosoftcargas.com.br/aeroctrl/css/dotnetStyles.css" type="text/css" rel="stylesheet" />
     
    <style type="text/css">
        #TextArea1 {
            height: 386px;
            width: 677px;
        }
        #TxtXml {
            height: 375px;
            width: 673px;
        }
    </style>
    <script>
        var tx = document.getElementsById('TxtXml');
        for (var i = 0; i < tx.length; i++) {
          tx[i].setAttribute('style', 'height:' + (tx[i].scrollHeight) + 'px;overflow-y:hidden;');
          tx[i].addEventListener("input", OnInput, false);
        }
        
        function OnInput(e) {
          this.style.height = 'auto';
          this.style.height = (this.scrollHeight) + 'px';
        }
     </script>    

</head>
<body runat="server" id="body0">
    <form id="form1" runat="server">
    <div>
        <table style="width: 100%; height: 100%;"> 
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
                                        <td><textarea id="TxtXml" name="XmlCiot" readonly="readonly" > <%=XmlCiot%> </textarea></td>                                            
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
