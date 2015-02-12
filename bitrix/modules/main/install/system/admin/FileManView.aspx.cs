using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Text;
using System.Text.RegularExpressions;

using Bitrix.Services;
using Bitrix;

using Bitrix.Security;
using Bitrix.Services.Js;
using Bitrix.DataTypes;
using Bitrix.IO;
using Bitrix.Configuration;
using Bitrix.UI;


public partial class bitrix_admin_FileManView : BXAdminPage, IBXFileManPage
{
	string curPath;
	static Regex _WhiteSpaceRegex = new Regex(@"[\s]+");
	View activeView;
	string encoding;

	string backUrl;
	protected override string BackUrl
	{
		get
		{
			backUrl = base.BackUrl;
			if (backUrl == null)
			{
				string dir = String.Empty;
				string entity = String.Empty;
				if (!BXPath.BreakPath(curPath, ref dir, ref entity))
					return (backUrl = "FileMan.aspx");
				backUrl = "FileMan.aspx?path=" + HttpUtility.UrlEncode(dir);
			}
			return backUrl;
		}
	}
	
	void FillEncodings(DropDownList list)
	{
		List<ListItem> enList = new List<ListItem>();
		foreach (EncodingInfo en in Encoding.GetEncodings())
			enList.Add(new ListItem(en.DisplayName, en.Name));
		enList.Sort(
			delegate(ListItem a, ListItem b)
			{
				return String.Compare(a.Text, b.Text, StringComparison.InvariantCultureIgnoreCase);
			}
		);
		list.Items.AddRange(enList.ToArray());
	}
	void RegisterJavaScript()
	{
		string script;
		
		script = String.Format(
			@"
			function fileMan_GoBack()
			{{
				location.href='{0}'
			}}",
			BXJSUtility.Encode(BackUrl)
		);

		if (!ClientScript.IsClientScriptBlockRegistered(GetType(), "fileMan_GoBack"))
			ClientScript.RegisterClientScriptBlock(GetType(), "fileMan_GoBack", script, true);
	}

	View SelectView()
	{
		BXFileType type = BXFileInfo.GetFileType(curPath);

		if ((type & BXFileType.Image) != BXFileType.Unknown)
			return imageView;
		else if ((type & BXFileType.TextFormat) != BXFileType.Unknown)
			return textView;
		else
			return defaultView;
	}

	void InitImageView()
	{

	}
	void InitTextView()
	{

		FillEncodings(textViewEncodings);
		textViewEncodings.SelectedValue = BXConfigurationUtility.DefaultEncoding.WebName;
		if (textViewEncodings.Items.FindByValue(encoding) != null)
			textViewEncodings.SelectedValue = encoding;
		textViewEdit.Enabled = BXSecureIO.CheckWrite(curPath);
	}
	void InitDefaultView()
	{
	}

	void PrepareImageView()
	{
		imageContent.ImageUrl = BXSite.GetUrlForPath(curPath, null);
		MasterTitle = GetMessage("MasterTitle.ImageView") + ": <font style=\"font-weight: normal;\">" + Encode(curPath) + "</font>";
		mainTab.Text = GetMessage("TabText.View");
		mainTab.Title = GetMessage("TabTitle.ImageView");
	}
	void PrepareTextView()
	{
		Encoding fileEncoding;
		try
		{
			fileEncoding = Encoding.GetEncoding(textViewEncodings.SelectedValue);
		}
		catch (ArgumentException)
		{
			fileEncoding = BXConfigurationUtility.DefaultEncoding;
		}
		string text = String.Empty;
		try
		{
			text = BXSecureIO.FileReadAllText(curPath, fileEncoding);
		}
		catch
		{
			GoBack();
		}
        if (text == null)
            GoBack();
        text = HttpUtility.HtmlEncode(text).Replace("\n", " <br/>").Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp; ").Replace("\r", String.Empty);
		//Replace whitespaces with &nbsp; and spaces
		int startPos = 0;
		StringBuilder s = new StringBuilder();
		Match m = _WhiteSpaceRegex.Match(text);
		while (m.Success)
		{
			s.Append(text.Substring(startPos, m.Index - startPos));
			for (int i = 0; i < m.Length - 1; i++)
				s.Append("&nbsp;");
			s.Append(" ");
			startPos = m.Index + m.Length;
			m = m.NextMatch();
		}
		s.Append(text.Substring(startPos));
		textContent.Text = s.ToString();

		if (textViewEdit.Enabled)
		{
			StringBuilder url = new StringBuilder();
			url.Append("FileManEdit.aspx?path=");
			url.Append(HttpUtility.UrlEncode(curPath));
			url.Append("&encoding=");
			url.Append(HttpUtility.UrlEncode(fileEncoding.WebName));
			textViewEdit.OnClientClick = "location.href='" + url.ToString() + "'; return false;";
		}
		MasterTitle = GetMessage("MasterTitle.TextView") + ": <font style=\"font-weight: normal;\">" + HttpUtility.HtmlEncode(curPath) + "</font>";
		mainTab.Text = GetMessage("TabText.View");
		mainTab.Title = GetMessage("TabTitle.TextView");
	}
	void PrepareDefaultView()
	{
		string text = String.Empty;
		try
		{
			text = BXSecureIO.FileReadAllText(curPath, BXConfigurationUtility.DefaultEncoding);
		}
		catch
		{
			GoBack();
		}
		if (text == null)
			GoBack();

		defaultContent.Text = string.Format("<pre>{0}</pre>", HttpUtility.HtmlEncode(text));

		MasterTitle = GetMessage("MasterTitle.DefaultView") + ": <font style=\"font-weight: normal;\">" + HttpUtility.HtmlEncode(curPath) + "</font>";
		mainTab.Text = GetMessage("TabText.View");
		mainTab.Title = GetMessage("TabTitle.DefaultView");
	}

	protected void Page_Init(object sender, EventArgs e)
	{
		if (!this.BXUser.IsCanOperate(BXRoleOperation.Operations.FileManage))
			BXAuthentication.AuthenticationRequired();

		curPath = BXPath.ToVirtualRelativePath(Request["path"]);
		if (!BXSecureIO.FileExists(curPath) || !BXSecureIO.CheckView(curPath))
			GoBack();

		encoding = Request["encoding"] == null ? String.Empty : Request["encoding"]; 

		RegisterJavaScript();

		activeView = SelectView();
		if (activeView == null)
			GoBack();

		if (activeView == imageView)
			InitImageView();
		else if (activeView == textView)
			InitTextView();
		else if (activeView == defaultView)
			InitDefaultView();

		fileViews.SetActiveView(activeView);
	}
	protected void Page_LoadComplete(object sender, EventArgs e)
	{
		if (activeView == imageView)
			PrepareImageView();
		else if (activeView == textView)
			PrepareTextView();
		else if (activeView == defaultView)
			PrepareDefaultView();
	}

	#region IBXFileManPage Members

	public string ProvidePath()
	{
		string path = BXPath.ToVirtualRelativePath(Request["path"] ?? "~");
		string dir = string.Empty;
		string file = string.Empty;
		if (!BXPath.BreakPath(path, ref dir, ref file))
			return "~";
		return BXPath.ToVirtualRelativePath(dir).ToLowerInvariant();
	}

	#endregion
}
