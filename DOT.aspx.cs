/*
 ORNELLAS                   15/09/2022 13:42    20220915    Problemas ao passar conteúdo base64 por sessão...
 */
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace AeroCIOTWeb
{
    public partial class DOT : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //20220915
            //byte[] base64EncodedBytes =  System.Convert.FromBase64String(Session["dot"].ToString());
            byte[] base64EncodedBytes = System.Convert.FromBase64String(Request.Form.Get("dot"));

            Response.ContentType = "application/pdf";
            Response.AddHeader("content-length", base64EncodedBytes.Length.ToString());
            Response.BinaryWrite(base64EncodedBytes);

            //Session.Remove("dot");
        }
    }
}