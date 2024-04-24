using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace AeroCIOTWeb.Services
{
    public class FuncoesXMLs
    {
        public string getMsgErro(string sXml)
        {
            string msgErro = "";

            XmlDocument xmldoc = new XmlDocument();
            xmldoc.LoadXml(sXml);

            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(xmldoc.NameTable);
            namespaceManager.AddNamespace("ns", "http://www.nddigital.com.br/nddcargo");

            try
            {
                var xmlElement = xmldoc.SelectNodes("//ns:infOT", namespaceManager)[0].ChildNodes[0].LastChild;

                var codigo = xmlElement["codigo"].InnerXml;
                var observacao = xmlElement["observacao"].InnerXml;

                msgErro = codigo + " - " + observacao;

            }
            catch (Exception ex)
            {
                msgErro = "";
            }            

            return msgErro; 
        }
    }
}