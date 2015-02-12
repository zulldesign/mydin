using System;
using System.Text;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Collections.ObjectModel;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Bitrix.UI;
using Bitrix.Services;
using Bitrix.Main;
using System.Collections.Generic;
using Bitrix;
using Bitrix.Security;
using Bitrix.DataLayer;
using Bitrix.Services.Text;
using Bitrix.IBlock;
using System.Threading;
using Bitrix.IBlock.PublicEditors;
using Bitrix.IBlock.UI;
using Bitrix.Configuration;
using Bitrix.DataTypes;
using Bitrix.Modules;

public partial class bitrix_admin_IBlockElementEdit : Bitrix.UI.BXAdminDialogPage
{

	List<Bitrix.IBlock.PublicEditors.BXIBlockPublicEditorElementField> editors;

	BXIBlock iblock;
	BXIBlockElement element;

	protected int iblockId = -1;
	protected int typeId = -1;
	protected int elementId = -1;
	private bool currentUserCanModifyElement = false;
	protected int redirectSectionId = 0;

	public BXIBlock IBlock
	{
		get
		{
			if(iblock != null || iblockId >= 0)
				return iblock;

			if (iblockId == -1 && Request.QueryString["iblock_id"] != null)
				int.TryParse(Request.QueryString["iblock_id"], out iblockId);

			if (iblockId == -1)
				iblockId = 0;

			if(iblockId <= 0)
				return null;
			
			return iblock = BXIBlock.GetById(iblockId); 
				
		}
	}
	string settingsPagePath;

	protected string SettingsPagePath
	{
		get
		{
			if (!String.IsNullOrEmpty(settingsPagePath))
				return settingsPagePath;
			settingsPagePath = VirtualPathUtility.ToAbsolute("~/bitrix/admin/iblockelementeditsettings.aspx");
			return settingsPagePath;
		}
	}

	public BXIBlockElement Element
	{
		get
		{
			if (element != null)
				return element;

			if (elementId == 0 || IBlock == null)
				return null;

			if (elementId == -1 && Request.QueryString["id"] != null)
				int.TryParse(Request.QueryString["id"], out elementId);

			if (elementId == -1)
			{
				elementId = 0;
				return element = new BXIBlockElement(iblockId, "");
			}

			var col = BXIBlockElement.GetList(new BXFilter(
				new BXFilterItem(BXIBlockElement.Fields.ID, BXSqlFilterOperators.Equal, elementId)), null, null, null, BXTextEncoder.EmptyTextEncoder);

			if (col.Count > 0)
				return element = col[0];
			else
				return null;
		}
	}

	IList<BXIBlockPublicEditorGroup> groups;

	Predicate<BXIBlockElement.BXInfoBlockElementSection> CheckSection(int value)
	{
		return delegate(BXIBlockElement.BXInfoBlockElementSection i) { return i.SectionId == value; };
	}

	bool isDialogMode;


	protected void TabControlPreInit_Init(object sender, EventArgs e)
	{
		isDialogMode = (Page as BXAdminDialogPage).DialogMode;
		if (IBlock == null)
		{
			errorMessage.AddErrorMessage("IBlock not found");
			return;
		}

		if (!this.BXUser.IsCanOperate("IBlockView", "iblock", iblockId))
			BXAuthentication.AuthenticationRequired();

		currentUserCanModifyElement = this.BXUser.IsCanOperate("IBlockModifyElements", "iblock", iblockId);

		int sectionId = base.GetRequestInt("section_id");
		if (sectionId > 0)
		{
			redirectSectionId = sectionId;
			if (elementId > 0)
			{
				if (element.Sections.Exists(CheckSection(sectionId)))
					redirectSectionId = sectionId;
				else
					redirectSectionId = element.Sections.Count > 0 ? element.Sections[0].SectionId : 0;
			}
		}
		else
		{
			redirectSectionId = 0;
			if (elementId > 0)
				redirectSectionId = element.Sections.Count > 0 ? element.Sections[0].SectionId : 0;
		}
		var page = Page as BXAdminDialogPage;
		editors = BXIBlockPublicEditorStructureBuilder.GetEditors(Bitrix.Security.BXIdentity.Current.Id,
			new BXPublicEditorContext()
			{
				Element = Element,
				FormFieldNamePrefix = "IBlockElementField_",
				iblock = IBlock,
				ParentClientID = ClientID
			}, !isDialogMode);

		groups = BXIBlockPublicEditorStructureBuilder.GetConfig(Bitrix.Security.BXIdentity.Current.Id, iblockId, false).Groups;

		BXTabControlTab tab = null;

		foreach(var g in groups)
		{
				tab = new BXTabControlTab();
				tab.ID = g.GroupId;
				tab.Title = g.Title;
				tab.Text = g.Title;
				BXTabControl1.Tabs.Add(tab);
				AddRenderer(tab, g, editors.FindAll(x=>x.GroupId==g.GroupId));
				
		}
		if(tab == null)
		{
			tab = new BXTabControlTab();
			tab.ID = "Element";
			tab.Title = "Element";
			BXTabControl1.Tabs.Add(tab);
		}		

		bool haveFileField = editors.Exists(
			x=>((x.CustomField!= null && x.CustomField.CustomTypeId == "Bitrix.System.File") || x.Id=="PreviewImage" || x.Id=="DetailImage"));

		HtmlForm form;
		if (Page != null && (form = Page.Form) != null && form.Enctype.Length == 0 && haveFileField)
			form.Enctype = "multipart/form-data";
	}
	
	protected void Page_Init(object sender, EventArgs e)
	{
		if (BXTabControl1.SelectedIndex < 0)
			BXTabControl1.SelectedIndex = 0;
		Form.Enctype = "multipart/form-data";		
	}

	void AddRenderer(Control c, BXIBlockPublicEditorGroup group, IEnumerable<BXIBlockPublicEditorElementField> fields)
	{
		var renderer = (IBlockPublicEditorGroupRenderer)LoadControl("~/bitrix/admin/controls/iblock/IBlockPublicEditorGroupRenderer.ascx");
		renderer.SetInfo(fields, iblockId, Element.Id, !isDialogMode);
		renderer.Title = group.Title;
		c.Controls.Add(renderer);
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		if (iblock != null)
		{
			AddElementButton.Text = iblock.CaptionsInfo.AddElement;
			AddElementButton.Title = iblock.CaptionsInfo.AddElement;
			string titleString = (elementId > 0) ? String.Format("{0} #{1}", iblock.CaptionsInfo.ModifyingElement, elementId.ToString())
			: iblock.CaptionsInfo.NewElement;
			Page.Title = BXHtmlTextEncoder.HtmlTextEncoder.Encode(titleString);
			((BXAdminMasterPage)Page.Master).Title = Page.Title;

			GoToListButton.Text = BXHtmlTextEncoder.HtmlTextEncoder.Encode(iblock.CaptionsInfo.ElementList);
			GoToListButton.Title = GoToListButton.Text;
			AddElementButton.Visible = (elementId > 0 && currentUserCanModifyElement);
			DeleteButton.Visible = (elementId > 0 && currentUserCanModifyElement);

			DeleteButton.Text = BXHtmlTextEncoder.HtmlTextEncoder.Encode(iblock.CaptionsInfo.DeleteElement);
			DeleteButton.Title = DeleteButton.Text;
		}
	}

	bool SaveIBlockElement()
	{
		if (!currentUserCanModifyElement)
		{

			return false;
		}
		bool success = true;
		foreach (var ed in editors)
		{
			List<string> err = new List<string>();
			if (!ed.PublicEditor.Validate(ed.FormFieldName, err))
			{
				if (success)
					success = false;
				ed.ValidateErrors = err;
                foreach (var str in err)
                    errorMessage.AddErrorMessage(HttpUtility.HtmlEncode(str));
			}
		}
		if (success)
		{
			BXIBlockPublicEditorElementField catalogField = null;
			foreach (var ed in editors)
			{
				if (ed.Id == "Catalog")
				{
					catalogField = ed;
					continue;
				}
				ed.PublicEditor.Save(ed.FormFieldName, element, element.CustomValues);
			}
			element.Save();
			if(catalogField!=null)
				catalogField.PublicEditor.Save(catalogField.FormFieldName, element, element.CustomValues);
		}

		return success;
	}

	protected void BXTabControl1_Command(object sender, BXTabControlCommandEventArgs e)
	{		
		switch (e.CommandName.ToLower())
		{
			case "save":
				if (!SaveIBlockElement())
					return;
				GoBack("IBlockListAdmin.aspx?iblock_id=" + iblockId.ToString());				
				break;
			case "apply":				
				if (!SaveIBlockElement())
					return;
				Reload(BXTabControl1.SelectedIndex);
				break;
			case "cancel":
				GoBack("IBlockListAdmin.aspx?iblock_id=" + iblockId.ToString());
				break;
		}
	}

	protected void Toolbar_CommandClick(object sender, CommandEventArgs e)
	{
		if (iblock == null)
			return;
		switch (e.CommandName.ToLower())
		{
			case "edit":

					Response.Redirect(String.Format("IBlockElementEditSettings.aspx?iblock_id={0}", iblock.Id));
				break;
			case "addelement":

					Response.Redirect(String.Format("IBlockElementEdit.aspx?type_id={1}&iblock_id={0}", iblockId, typeId));
				break;
			case "go2list":
				Response.Redirect(String.Format("IBlockListAdmin.aspx?type_id={1}&iblock_id={0}", iblockId, typeId));
				break;
			case "delete":
			case "deleteelement":
				try
				{
					if (!currentUserCanModifyElement)
						throw new Exception(GetMessageRaw("Exception.YouDontHaveRightsToDeleteThisRecord"));

					if (elementId > 0)
					{
						if (redirectSectionId < 0 || !element.Sections.Exists(CheckSection(redirectSectionId)))
						{
							if (element.Sections.Count > 0)
								redirectSectionId = element.Sections[0].SectionId;
							else
								redirectSectionId = 0;
						}

						try
						{
							BXIBlockElement.Delete(elementId);
						}
						catch (Exception ex)
						{
							throw new Exception(GetMessageRaw("Exception.AnErrorHasOccurredWhileDeletingElement"), ex);
						}
					}
					else
					{
						throw new Exception(GetMessageRaw("Exception.ElementIsNotFound"));
					}

					Page.Response.Redirect(String.Format("IBlockListAdmin.aspx?type_id={0}&iblock_id={1}&filter_sectionid={2}", typeId, iblockId, redirectSectionId));
				}
				catch (BXEventException ex)
				{
					foreach (string s in ex.Messages)
						errorMessage.AddErrorMessage(s);
				}
				catch (Exception ex)
				{
					errorMessage.AddErrorMessage(ex.Message);
				}
				break;
			default:
				break;
		}
	}

	protected override void PrepareDialogContent(BXDialogData data)
	{
		if (data == null)
			throw new ArgumentNullException("data");

		BXDialogSectionData content = data.CreateSection(BXDialogSectionType.Content);
		BXTabControl1.PublicMode = true;
		BXTabControl1.ButtonsMode = BXTabControl.ButtonsModeType.Hidden;
		Toolbar.Visible = false;
		content.CreateItemFromControl(this);
	}

	protected override bool ValidateDialogData()
	{
		Validate(BXTabControl1.ValidationGroup);
		return IsValid;
	}

	protected override void SaveDialogData()
	{
		if (!SaveIBlockElement())
			return;

		string backUrl = Request[BXConfigurationUtility.Constants.BackUrl];
		if (string.IsNullOrEmpty(backUrl))
			Refresh(string.Empty, BXDialogGoodbyeWindow.LayoutType.Success, 0);

		if (element == null)
			throw new InvalidOperationException("Could not find element!");

		string sectionId = this.element.Sections != null && this.element.Sections.Count > 0 ? this.element.Sections[0].SectionId.ToString() : string.Empty;

		BXParamsBag<object> replace = new BXParamsBag<object>();
		replace.Add("IblockId", this.element.IBlockId);
		replace.Add("IBLOCK_ID", this.element.IBlockId);
		replace.Add("IblockCode", this.element.Code);
		replace.Add("IBLOCK_CODE", this.element.Code);
		replace.Add("ELEMENT_ID", this.element.Id);
		replace.Add("ElementId", this.element.Id);
		replace.Add("ElementCode", this.element.Code);
		replace.Add("ELEMENT_CODE", this.element.Code);
		replace.Add("SectionId", sectionId);
		replace.Add("SECTION_ID", sectionId);
		replace.Add("SectionCode", sectionId);
		replace.Add("SECTION_CODE", sectionId);
		Redirect(BXSefUrlUtility.MakeLink(backUrl, replace), string.Empty, BXDialogGoodbyeWindow.LayoutType.Success, 0);
	}
}