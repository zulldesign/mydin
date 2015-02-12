<%@ WebHandler Language="C#" Class="ImageResizer" %>

using System;
using System.Web;
using Bitrix.IO;
using Bitrix.Configuration;

public class ImageResizer : IHttpHandler, System.Web.SessionState.IRequiresSessionState
{
    
    public void ProcessRequest (HttpContext context) {

		if (context.Request["filename"] == null && context.Request["path"] == null)
            return;

        int height = int.TryParse(context.Request["height"],out height) ? height : 75;
        int width = int.TryParse(context.Request["width"], out width) ? width : 75;
        bool fit = bool.TryParse(context.Request["fit"], out fit) ? fit : false;
		string path = (context.Request["path"] != null) ? context.Request["path"] : BXPath.ToVirtualRelativePath(BXConfigurationUtility.DefaultEncoding.GetString(Convert.FromBase64String(context.Request["filename"])));
        path = BXPath.ToPhysicalPath(path);
		if(!System.IO.File.Exists(path))
            return;
        
        context.Response.ContentType = "image/jpeg";
 
        using (System.Drawing.Image img = System.Drawing.Image.FromFile(path))
        {
            int actHeight = 0;
            int actWidth = 0;

            Bitrix.Services.Image.BXImageUtility.CalculateImageSize(img.Width, img.Height, width, height, out actWidth, out actHeight, fit);

            using (System.Drawing.Bitmap bmp = ResizeBitmap(img, actWidth, actHeight))
            {
                bmp.Save(context.Response.OutputStream, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
        }
    }
 
    public bool IsReusable {
        get {
            return false;
        }
    }

    public System.Drawing.Bitmap ResizeBitmap(System.Drawing.Image b, int nWidth, int nHeight)
    {
        System.Drawing.Bitmap result = new System.Drawing.Bitmap(nWidth, nHeight);
        using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage((System.Drawing.Image)result))
            g.DrawImage(b, 0, 0, nWidth, nHeight);
        return result;
    }

}