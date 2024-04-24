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
using AeroCIOTWeb.DBO;
using AeroCIOTWeb.Model;

namespace AeroCIOTWeb
{
    public partial class VisualizarXml : System.Web.UI.Page
    {
        protected string msg = "Visualizar XML CIOT";
        string Versao = "";  //ou 4.2.11.0
        private string ID_CIOT = "";
        private string Cpf_Motorista = "";        
        string CNPJ = "";
        string Token = "";
        string Nome = "";
        string usuario = "";
        bool atualiza = true;
        private bool jaExibiuMensagem = false;
        string session = "";
        protected string XmlCiot = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            DboCIOT dboCiot = new DboCIOT();

            ID_CIOT = Request.QueryString.Get("ciot");
            Cpf_Motorista = Request.QueryString.Get("cpf");
            session = Request.QueryString.Get("session");

            bool enablemenu;
            /*
            if (!dboCiot.sessaoAtiva(session, getDtSessao(), ref usuario, "Visualizar XML CIOT"))
            {
                Response.Write("Sessão vencida ou usuário sem acesso!");
                Response.End();
            }
            */
            var motoristaCiot = dboCiot.Get_Motorista_Ciot(ID_CIOT, Cpf_Motorista);            

            var configCiot = dboCiot.Get_Config_CIOT();
            CNPJ = configCiot.Cnpj;
            Token = configCiot.Token;
            Versao = configCiot.Versao;

            if (!IsPostBack)
            {
                msg = "Visualizar XML do CIOT " + motoristaCiot.Ciot;                
            }

            string sXmlCiot = dboCiot.GetXmlCiot(ID_CIOT);

            if (sXmlCiot != "")
            {
                XmlCiot = sXmlCiot;
            }

        }
           
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

        private void showError(string msg)
        {
            string str = msg.Replace('"', ' ').Replace("'", "").Replace('\n', ' ').Replace('\t', ' ').Replace("\n", "").Replace("\t", "").Replace("\r", "").Trim();
            body0.Attributes.Add("onload", "alert('" + str + "')");
        }

        private void showInfo(string msg)
        {
            string str = msg.Replace('"', ' ').Replace("'", "").Replace('\n', ' ').Replace('\t', ' ').Replace("\n", "").Replace("\t", "").Replace("\r", "").Trim();
            body0.Attributes.Add("onload", "alert('" + str + "')");
        }

        private void showWarning(string msg)
        {
            string str = msg.Replace('"', ' ').Replace("'", "").Replace('\n', ' ').Replace('\t', ' ').Replace("\n", "").Replace("\t", "").Replace("\r", "").Trim();
            body0.Attributes.Add("onload", "alert('" + msg + "')");
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
            session = Request.QueryString.Get("session");
            //armazena ID de sessao ....
            Session["IDSession"] = session;

            return session;
        }

    }
}