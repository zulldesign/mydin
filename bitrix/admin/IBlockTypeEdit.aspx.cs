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
using Bitrix.IBlock;
using Bitrix;
using System.Collections.Generic;
using Bitrix.Modules;
using Bitrix.Security;
using Bitrix.DataLayer;
using Bitrix.Services.Text;

public partial class bitrix_admin_IBlockTypeEdit : BXAdminPage
{
	int typeId = -1;
	bool editType;

	bool currentUserCanModifyType = false;

	private Control WalkThrowControlsSearch(Control cntrl, string controlName)
	{
		foreach (Control subCntrl in cntrl.Controls)
		{
			if (!String.IsNullOrEmpty(subCntrl.ID) && subCntrl.ID.Equals(controlName, StringComparison.InvariantCultureIgnoreCase))
				return subCntrl;

			Control cntrl1 = WalkThrowControlsSearch(subCntrl, controlName);
			if (cntrl1 != null)
				return cntrl1;
		}

		return null;
	}

	private void LoadData()
	{
		BXIBlockType type = null;

		if (typeId > 0)
		{
			BXIBlockTypeCollection typeCollection = BXIBlockType.GetList(
				new BXFilter(new BXFilterItem(BXIBlockType.Fields.ID, BXSqlFilterOperators.Equal, typeId)),
				null,
				null,
				null,
				BXTextEncoder.EmptyTextEncoder
			);

			if (typeCollection != null && typeCollection.Count > 0)
				type = typeCollection[0];
		}


		if (type == null)
		{
			typeId = -1;
			hfTypeId.Value = typeId.ToString();

			tbSort.Text = "500";
			trID.Visible = false;
		}
		else
		{
			Page.Title = string.Format(GetMessage("FormatPageTitle.ModificationOfType"), typeId);
			((BXAdminMasterPage)Page.Master).Title = Page.Title;

			trID.Visible = true;
			lbID.Text = type.Id.ToString();

			cbHaveSections.Checked = type.HaveSections;
			tbSort.Text = type.Sort.ToString();

			foreach (BXLanguage lang in BXLanguage.GetList(null, null))
			{
				TextBox tb = (TextBox)WalkThrowControlsSearch(Form, String.Format("tbName_{0}", lang.Id));
				tb.Text = type.Translations[lang.Id].Name != null ? type.Translations[lang.Id].Name : "";
			}
		}
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		((BXAdminMasterPage)Page.Master).Title = Page.Title;

		typeId = base.GetRequestInt("id");
		if (typeId > 0)
			hfTypeId.Value = typeId.ToString();
		editType = Int32.TryParse(hfTypeId.Value, out typeId);
		if (editType)
			editType = typeId > 0;

		if (!this.BXUser.IsCanOperate("IBlockView"))
			BXAuthentication.AuthenticationRequired();

		currentUserCanModifyType = this.BXUser.IsCanOperate("IBlockAdmin");

		string success = base.GetRequestString("success");
		if ("Y".Equals(success, StringComparison.InvariantCultureIgnoreCase))
			successMessage.Visible = true;
		else
			successMessage.Visible = false;

		foreach (BXLanguage lang in BXLanguage.GetList(null, null))
		{
			HtmlTableRow r1 = new HtmlTableRow();

			HtmlTableCell c1 = new HtmlTableCell();
			c1.InnerHtml = lang.Name;
			r1.Cells.Add(c1);

			c1 = new HtmlTableCell();
			TextBox tb = new TextBox();
			tb.ID = String.Format("tbName_{0}", lang.Id);
			c1.Controls.Add(tb);
			r1.Cells.Add(c1);

			tblLangTranslaters.Rows.Add(r1);
		}

		if (!Page.IsPostBack)
			LoadData();
	}
	protected void Page_LoadComplete(object sender, EventArgs e)
	{
		AddButton.Visible = DeleteButton.Visible = (typeId > 0 && currentUserCanModifyType);
		BXTabControl1.ShowSaveButton = BXTabControl1.ShowApplyButton = currentUserCanModifyType;
	}

	protected void BXTabControl1_Command(object sender, BXTabControlCommandEventArgs e)
	{
		bool noRedirect = false;
		bool successAction = true;
		if (e.CommandName == "save")
		{
			if (!SaveType())
			{
				successAction = false;
				noRedirect = true;
			}
		}
		else if (e.CommandName == "apply")
		{
			if (!SaveType())
				successAction = false;
			noRedirect = true;
		}

		if (!noRedirect)
		{
			Page.Response.Redirect("IBlockTypeList.aspx");
		}
		else
		{
			if (successAction)
			{
				successMessage.Visible = true;
				LoadData();
			}
		}
	}

	private bool SaveType()
	{
		if (Page.IsValid)
		{
			if (typeId > 0)
				return UpdateType();
			else
				return CreateType();
		}
		return false;
	}

	private bool CreateType()
	{
		try
		{
			if (!currentUserCanModifyType)
				throw new Exception(GetMessageRaw("Exception.YouDontHaveRightsToCreateNewType"));

			int sort;
			int.TryParse(tbSort.Text, out sort);

			List<BXInfoBlockTypeOld.BXIBlockTypeLang> langs = new List<BXInfoBlockTypeOld.BXIBlockTypeLang>();
			foreach (BXLanguage lang in BXLanguage.GetList(null, null))
			{
				TextBox tb = (TextBox)WalkThrowControlsSearch(Form, String.Format("tbName_{0}", lang.Id));
				string s1 = tb.Text;

				if (String.IsNullOrEmpty(s1))
					throw new Exception(String.Format(GetMessageRaw("Exception.NameTraslationNotExist"), lang.Name));

				langs.Add(new BXInfoBlockTypeOld.BXIBlockTypeLang(lang.Id, s1, String.Empty, String.Empty));
			}

			BXInfoBlockTypeOld type = BXInfoBlockTypeManagerOld.Create(
				null,
				cbHaveSections.Checked,
				(sort > 0 ? sort : 500),
				langs.ToArray()
			);
			if (type == null)
				throw new Exception(GetMessageRaw("Exception.TypeCreationFailed"));

			typeId = type.TypeId;
			if (typeId > 0)
				hfTypeId.Value = typeId.ToString();
			editType = Int32.TryParse(hfTypeId.Value, out typeId);
			if (editType)
				editType = typeId > 0;

			return true;
		}
		catch (BXEventException e)
		{
			foreach (string s in e.Messages)
				errorMassage.AddErrorMessage(s);
		}
		catch (Exception e)
		{
			errorMassage.AddErrorMessage(e.Message);
		}
		return false;
	}

	private bool UpdateType()
	{
		try
		{
			if (!currentUserCanModifyType)
				throw new Exception(GetMessageRaw("Exception.YouDontHaveRightsToModifyThisType"));

			BXInfoBlockTypeOld type = BXInfoBlockTypeManagerOld.GetById(typeId);
			if (type == null)
				throw new Exception(GetMessageRaw("Exception.TypeIsNotFound"));

			int sort;
			int.TryParse(tbSort.Text, out sort);
			type.Sort = sort;

			type.HaveSections = cbHaveSections.Checked;

			type.TypeLang.Clear();
			foreach (BXLanguage lang in BXLanguage.GetList(null, null))
			{
				TextBox tb = (TextBox)WalkThrowControlsSearch(Form, String.Format("tbName_{0}", lang.Id));
				string s1 = tb.Text;

				if (String.IsNullOrEmpty(s1))
					throw new Exception(String.Format(GetMessageRaw("Exception.NameTraslationNotExist"), lang.Name));

				type.TypeLang[lang.Id] = new BXInfoBlockTypeOld.BXIBlockTypeLang(lang.Id, s1, String.Empty,String.Empty );
			}

			type.Update();

			return true;
		}
		catch (BXEventException e)
		{
			foreach (string s in e.Messages)
				errorMassage.AddErrorMessage(s);
		}
		catch (Exception e)
		{
			errorMassage.AddErrorMessage(e.Message);
		}

		return false;
	}

	protected void BXContextMenuToolbar1_CommandClick(object sender, CommandEventArgs e)
	{
		switch (e.CommandName)
		{
			case "DeleteType":
				try
				{
					if (!currentUserCanModifyType)
						throw new Exception(GetMessageRaw("Exception.YouDontHaveRightsToDeleteThisRecord"));

					BXInfoBlockTypeOld r = BXInfoBlockTypeManagerOld.GetById(typeId);
					if (r != null)
					{
						if (!BXInfoBlockTypeManagerOld.Delete(typeId))
						{
							throw new Exception(GetMessageRaw("Exception.AnErrorHasOccurredWhileDeletingType"));
						}
					}
					else
					{
						throw new Exception(GetMessageRaw("Exception.TypeIsNotFound"));
					}

					Page.Response.Redirect("IBlockTypeList.aspx");
				}
				catch (BXEventException ex)
				{
					foreach (string s in ex.Messages)
						errorMassage.AddErrorMessage(s);
				}
				catch (Exception ex)
				{
					errorMassage.AddErrorMessage(ex.Message);
				}

				break;
		}
	}
}
