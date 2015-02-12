using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using Bitrix.UI;
using Bitrix.Services.Marketplace;
using Bitrix.Services.Text;
using Bitrix.Security;
using Bitrix.Services;

public partial class bitrix_admin_MarketplaceDetail : BXAdminPage
{
	public bool HasError { get; set; }
	public BXMarketplaceModule Data { get; set; }
	BXMarketplaceModuleDownloader downloader;

	protected override void OnInit(EventArgs e)
	{
		base.OnInit(e);
		
		if (!this.BXUser.IsCanOperate(BXRoleOperation.Operations.UpdateSystem))
			BXAuthentication.AuthenticationRequired();

		MasterTitle = "Marketplace";

		var module = Request.QueryString["module"];
		if (BXStringUtility.IsNullOrTrimEmpty(module))
			Response.Redirect("Marketplace.aspx");


		BXMarketplaceQueryResult result;
		try
		{
			result = new BXMarketplaceQuery { ModuleId = module }.Execute();
		}
		catch
		{
			HasError = true;
			ErrorMessage.AddErrorText(string.Format(GetMessageRaw("Error.UnableToGetModuleInfo"), module));
			return;
		}

		if (result.Modules.Count == 0)
		{
			HasError = true;
			ErrorMessage.AddErrorText(string.Format(GetMessageRaw("Error.NoSuchModule"), module));
			return;
		}
		Data = result.Modules[0];
	}

	IAsyncResult BeginAsyncOperation(object sender, EventArgs e, AsyncCallback callback, object state)
    {
		downloader = new BXMarketplaceModuleDownloader();
		return downloader.BeginDownloadPackage(Data.Id, callback, state);
    }

    void EndAsyncOperation(IAsyncResult result)
    {
		try
		{
			downloader.EndDownloadPackage(result);
		}
		catch(BXMarketplaceModuleDownloaderException ex)
		{
			ErrorMessage.AddErrorMessage(ex.HtmlMessage);
		}
		catch(Exception ex)
		{
			ErrorMessage.AddErrorText(GetMessageRaw("Error.Unknown"));
			BXLogService.LogAll(ex, BXLogMessageType.Error, "MarketplaceDetail.aspx");
		}

		Response.Redirect("ModulesInstall.aspx?action=install&module=" + Data.Id + "&" + BXCsrfToken.BuildQueryStringPair());
    }


	protected void DownloadAndInstall_Click(object sender, EventArgs e)
	{
		if (Data != null)
			AddOnPreRenderCompleteAsync(BeginAsyncOperation, EndAsyncOperation);
	}

	protected void Cancel_Click(object sender, EventArgs e)
	{
		Response.Redirect("Marketplace.aspx");
	}
}
