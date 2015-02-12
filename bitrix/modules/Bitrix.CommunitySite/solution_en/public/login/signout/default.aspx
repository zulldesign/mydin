<%@ Page Language="C#" Inherits="Bitrix.UI.BXPublicPage" Title="Authorization" %>
<%@ Import Namespace="Bitrix.Services.Text" %>

<script runat="server">
	protected override void OnLoad(EventArgs e)
	{		
		base.OnLoad(e);

		//Logout from bitrix		
		Bitrix.Security.BXAuthentication.SignOut();

		var returnUrl = GetReturnUrl();
		
		if (BXStringUtility.IsNullOrTrimEmpty(returnUrl) || Regex.IsMatch(returnUrl, @"^(http|https|ftp)://", RegexOptions.IgnoreCase))
			returnUrl = Bitrix.BXSite.Current.DirectoryAbsolutePath;
		
		this.Redirect(returnUrl);
	}

	public static string GetReturnUrl()
	{
		HttpContext current = HttpContext.Current;
		if (current == null || current.Request == null)
			return "/";

		string str = current.Request.QueryString["ReturnUrl"];
		if (str == null)
		{
			str = current.Request.Form["ReturnUrl"];
			if ((!string.IsNullOrEmpty(str) && !str.Contains("/")) && str.Contains("%"))
				str = HttpUtility.UrlDecode(str);
		}

		if (str == null)
			return Bitrix.BXSite.Current.DirectoryAbsolutePath;

		return str;
	}	
</script>
<asp:Content ID="Content1" ContentPlaceHolderID="BXContent" runat="Server"></asp:Content>