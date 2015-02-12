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
using Bitrix.UI;
using System.Text;

public partial class bitrix_components_bitrix_photogallery_photo_templates__default_template : BXComponentTemplate<PhotogalleryPhoto>
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Component.Photo != null)
        {
            lButtonDelete.Visible = true;
            lButtonDelete.OnClientClick = string.Format("return confirm(\"{0}\");", JSEncode(GetMessageRaw("DoYouReallyWantRemoveThisPhoto")));
        }

    }

    protected void lbDelete_Click(object sender, EventArgs e)
    {
        Component.DeletePhoto();
    }
}
