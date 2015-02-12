using System;
using System.Text;
using System.Web.UI.WebControls;

using Bitrix.IBlock;
using Bitrix.Modules;
using Bitrix.Security;
using Bitrix.UI;
using Bitrix.IO;
using Bitrix.Services.Text;
using Bitrix;
using Bitrix.DataLayer;
using Bitrix.Configuration;
using Bitrix.DataTypes;
using Bitrix.Services;

public partial class bitrix_admin_IBlockSectionEdit : BXAdminDialogPage
{
	protected int iblockId = -1;
	protected int typeId = -1;
	int sectionId = -1;
	protected int redirectSectionId = 0;

	BXIBlockSection section;
    BXIBlock currentIBlock;
	string propertyEntryName;
	bool currentUserCanModifySection = false;

	private void LoadData()
	{
		ddlParentSection.Items.Clear();
		ddlParentSection.Items.Add(new ListItem(GetMessage("TopLevel"), "0"));
		foreach (BXIBlockSection sectionTmp in BXIBlockSection.GetTree(iblockId,0,false,BXTextEncoder.EmptyTextEncoder))
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 1; i < sectionTmp.DepthLevel; i++)
				sb.Append(" . ");
			sb.Append(sectionTmp.Name);
			ddlParentSection.Items.Add(new ListItem(sb.ToString(), sectionTmp.Id.ToString()));
		}

		if (section == null)
		{
			if (redirectSectionId > 0)
				ddlParentSection.SelectedValue = redirectSectionId.ToString();

			cbActive.Checked = true;
			tbSort.Text = "500";
			
			trID.Visible = false;
			trUpdateDate.Visible = false;
		}
		else
		{
			//Page.Title = string.Format(GetMessage("FormatPageTitle.ModificationOfSection"), sectionId);
			//((BXAdminMasterPage)Page.Master).Title = Page.Title;

			trID.Visible = true;
			lbID.Text = section.Id.ToString();
			trUpdateDate.Visible = true;
			lbUpdateDate.Text = section.UpdateDate.ToString();

			cbActive.Checked = section.Active;
			tbCode.Text = section.Code;
            ddlParentSection.SelectedValue = section.SectionId.ToString();
			tbName.Text = section.Name;
			tbSort.Text = section.Sort.ToString();
			tbXmlId.Text = section.XmlId;

			
			Img.ImageFile = section.Image;
			DetailImg.ImageFile = section.DetailImage;

			if (BXIBlockModule.UseVisualEditor())
			{
				bxweDescription.Content = section.Description;
				bxweDescription.StartMode = ((section.DescriptionType == BXTextType.Text) ? BXWebEditor.StartModeType.PlainText : BXWebEditor.StartModeType.HTMLVisual);
			}
			else
			{
				tbDescription.Text = section.Description;
				rblDescriptionType.SelectedValue = ((section.DescriptionType == BXTextType.Text) ? "text" : "html");
			}
		}
	}

	protected void Page_Load(object sender, EventArgs e)
	{
        BXIBlock block = null;
		sectionId = base.GetRequestInt("id");
		if (sectionId > 0)
			hfSectionId.Value = sectionId.ToString();
        else
		    Int32.TryParse(hfSectionId.Value, out sectionId);

        iblockId = base.GetRequestInt("iblock_id");

		if (sectionId > 0)
		{
			//section = BXIBlockSection.GetById(sectionId,BXTextEncoder.EmptyTextEncoder);
            BXIBlockSectionCollection sectionCol = BXIBlockSection.GetList(
                new BXFilter(new BXFilterItem(BXIBlockSection.Fields.ID, BXSqlFilterOperators.Equal, sectionId)),
                null,
                iblockId > 0 ? new BXSelectAdd(BXIBlockSection.Fields.CustomFields[iblockId]) : null,
                null,
				BXTextEncoder.EmptyTextEncoder
                );
            if ((section = sectionCol.Count > 0 ? sectionCol[0] : null) == null)
			{
				sectionId = 0;
				hfSectionId.Value = sectionId.ToString();
			}
		}

		if (sectionId > 0)
		{
			iblockId = section.IBlockId;
			hfIBlockId.Value = iblockId.ToString();
			block = BXIBlock.GetById(iblockId, BXTextEncoder.EmptyTextEncoder);
			typeId = block.TypeId;
			hfTypeId.Value = typeId.ToString();
		}
		else
		{
			//iblockId = base.GetRequestInt("iblock_id");
			if (iblockId > 0)
				hfIBlockId.Value = iblockId.ToString();
            else
			    Int32.TryParse(hfIBlockId.Value, out iblockId);
			block = null;
			if (iblockId > 0)
			{
				block = BXIBlock.GetById(iblockId, BXTextEncoder.EmptyTextEncoder);
				if (block == null)
				{
					iblockId = 0;
					hfIBlockId.Value = iblockId.ToString();
				}
			}

			if (iblockId > 0)
			{
				typeId = block.TypeId;
				hfTypeId.Value = typeId.ToString();
			}
			else
			{
				typeId = base.GetRequestInt("type_id");
				Page.Response.Redirect(String.Format("IBlockAdmin.aspx?type_id={0}", typeId));
			}
		}
		propertyEntryName = BXIBlockSection.GetCustomFieldsKey(iblockId);
        currentIBlock = block;
		int sectionIdTmp = base.GetRequestInt("section_id");
		if (sectionIdTmp > 0)
		{
			redirectSectionId = sectionIdTmp;
			if (sectionId > 0 && section.SectionId != redirectSectionId)
				redirectSectionId = section.SectionId;
		}
		else
		{
			redirectSectionId = 0;
			if (sectionId > 0)
				redirectSectionId = section.SectionId;
		}

		if (!this.BXUser.IsCanOperate("IBlockView", "iblock", iblockId))
			BXAuthentication.AuthenticationRequired();

		currentUserCanModifySection = this.BXUser.IsCanOperate("IBlockModifyStructure", "iblock", iblockId.ToString());

		string success = base.GetRequestString("success");
		if ("Y".Equals(success, StringComparison.InvariantCultureIgnoreCase))
			successMessage.Visible = true;
		else
			successMessage.Visible = false;

		bxweDescription.Visible = BXIBlockModule.UseVisualEditor();
		tbDescription.Visible = !BXIBlockModule.UseVisualEditor();
		rblDescriptionType.Visible = !BXIBlockModule.UseVisualEditor();

		CustomFieldList1.EntityId = propertyEntryName;
        if (section != null)
            CustomFieldList1.Load(section.CustomValues);
	
		if (!Page.IsPostBack)
			LoadData();
	}

	protected void Page_LoadComplete(object sender, EventArgs e)
	{

        Page.Title = (sectionId > 0)
            ? String.Format("{0} #{1}",currentIBlock.CaptionsInfo.ModifyingSection,sectionId.ToString()) 
            : currentIBlock.CaptionsInfo.NewSection;
        ((BXAdminMasterPage)Page.Master).Title = Page.Title;
        AddButton.Text = currentIBlock.CaptionsInfo.AddSection;
        DeleteButton.Text = currentIBlock.CaptionsInfo.DeleteSection;
        AddButton.Title = currentIBlock.CaptionsInfo.AddSection;
        DeleteButton.Title = currentIBlock.CaptionsInfo.DeleteSection;
        BXTabControlTab1.Title = BXHtmlTextEncoder.HtmlTextEncoder.Decode(Page.Title);
        BXTabControlTab1.Text = BXHtmlTextEncoder.HtmlTextEncoder.Decode(currentIBlock.CaptionsInfo.SectionName);
        ListButton.Text = currentIBlock.CaptionsInfo.SectionList;
        ListButton.Title = currentIBlock.CaptionsInfo.SectionList;
		AddButton.Visible = (sectionId > 0 && currentUserCanModifySection);
		DeleteButton.Visible = (sectionId > 0 && currentUserCanModifySection);
		BXTabControl1.ShowApplyButton = BXTabControl1.ShowSaveButton = currentUserCanModifySection;
	}

	protected void BXTabControl1_Command(object sender, BXTabControlCommandEventArgs e)
	{
		//bool noRedirect = false;
		//bool successAction = true;
		if (e.CommandName == "cancel" || (e.CommandName == "save" && SaveSection()))
			GoBack();
		else if (e.CommandName == "apply" && SaveSection())
			Page.Response.Redirect(String.Format("IBlockSectionEdit.aspx?type_id={0}&iblock_id={1}&section_id={2}&id={3}&tabindex={4}", typeId, iblockId, sectionId, sectionId, BXTabControl1.SelectedIndex));	
	}

	private bool SaveSection()
	{
		if (IsValid)
		{
			if (sectionId > 0)
				return UpdateSection();
			else
				return CreateSection();
		}
		return false;
	}

	protected BXFile SaveFile(string type)
	{
		BXFile fImage = null;

		FileUpload fu;

		if ("detail".Equals(type, StringComparison.InvariantCultureIgnoreCase))
			fu = DetailImg.Upload;
		else
			fu = Img.Upload;

		if (fu.HasFile)
		{
			fImage = new BXFile(fu.PostedFile, "iblock", "iblock", null); 
			BXFileValidationResult fResult = fImage.ValidateAsImage(0, 0, 0);
			if (fResult != BXFileValidationResult.Valid)
			{
				string fError = "";
				if ((fResult & BXFileValidationResult.InvalidContentType) == BXFileValidationResult.InvalidContentType)
					fError += GetMessage("Error.InvalidType");
				if ((fResult & BXFileValidationResult.InvalidExtension) == BXFileValidationResult.InvalidExtension)
				{
					if (!String.IsNullOrEmpty(fError))
						fError += ", ";
					fError += GetMessage("Error.Invalid.Extension");
				}
				if ((fResult & BXFileValidationResult.InvalidImage) == BXFileValidationResult.InvalidImage)
				{
					if (!String.IsNullOrEmpty(fError))
						fError += ", ";
					fError += GetMessage("Error.InvalidImage");
				}
				throw new Exception(fError);
			}
			fImage.DemandFileUpload();
			fImage.Save();
		}

		return fImage;
	}

	private bool CreateSection()
	{
		try
		{
			if (!currentUserCanModifySection)
				throw new Exception(GetMessageRaw("Exception.YouDontHaveRightsToCreateNewSection"));

			BXFile fImage = SaveFile("preview");
			BXFile fDetailImage = SaveFile("detail");

			int sort;
			int.TryParse(tbSort.Text, out sort);

			int parentSectionId;
			int.TryParse(ddlParentSection.SelectedValue, out parentSectionId);
			redirectSectionId = parentSectionId;

			string descr = null;
			BXTextType descrType = BXTextType.Text;
			if (BXIBlockModule.UseVisualEditor())
			{
				descr = bxweDescription.Content;
				descrType = (bxweDescription.StartMode  == BXWebEditor.StartModeType.PlainText) ? BXTextType.Text : BXTextType.Html;
			}
			else
			{
				descr = tbDescription.Text;
				descrType = ("text".Equals(rblDescriptionType.SelectedValue, StringComparison.InvariantCultureIgnoreCase) ? BXTextType.Text : BXTextType.Html);
			}

			section =new BXIBlockSection(
				iblockId,
				parentSectionId,
                tbName.Text );
            section.Active = cbActive.Checked;
			section.Sort = (sort > 0 ? sort : 500);
            section.ImageId = (fImage != null) ? fImage.Id : 0;
			section.DetailImageId = (fDetailImage != null) ? fDetailImage.Id : 0;
			section.Description = descr;
            section.DescriptionType = descrType;
			section.Code = tbCode.Text;
			section.XmlId = tbXmlId.Text;
            section.CreatedBy = (this.BXUser.Identity as BXIdentity).Id;
			if (CustomFieldList1.HasItems)
				section.CustomValues.Override(CustomFieldList1.Save());
            section.Create();
			if (section.Id <= 0 )
				throw new Exception(GetMessageRaw("Exception.SectionCreationFailed"));

			sectionId = section.Id;
			hfSectionId.Value = sectionId.ToString();
			iblockId = section.IBlockId;
			hfIBlockId.Value = iblockId.ToString();
			
			//if (CustomFieldList1.HasItems)
			//	BXCustomEntityManager.SaveEntity(propertyEntryName, section.Id, CustomFieldList1.Save());

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

	private bool UpdateSection()
	{
		try
		{
			if (!currentUserCanModifySection)
				throw new Exception(GetMessageRaw("Exception.YouDontHaveRightsToModifySection"));

			string descr = null;
			BXTextType descrType = BXTextType.Text;
			if (BXIBlockModule.UseVisualEditor())
			{
				descr = bxweDescription.Content;
				descrType = (bxweDescription.StartMode == BXWebEditor.StartModeType.PlainText) ? BXTextType.Text : BXTextType.Html;
			}
			else
			{
				descr = tbDescription.Text;
				descrType = ("text".Equals(rblDescriptionType.SelectedValue, StringComparison.InvariantCultureIgnoreCase) ? BXTextType.Text : BXTextType.Html);
			}

			section.Active = cbActive.Checked;
			section.Code = tbCode.Text;
			section.Description = descr;
			section.DescriptionType = descrType;
			section.Name = tbName.Text;
			section.XmlId = tbXmlId.Text;
            section.ModifiedBy = (this.BXUser.Identity as BXIdentity).Id;

			int sort;
			int.TryParse(tbSort.Text, out sort);
			section.Sort = sort;

			int parentSectionId;
			int.TryParse(ddlParentSection.SelectedValue, out parentSectionId);
			section.SectionId = parentSectionId;
			redirectSectionId = parentSectionId;

			if (Img.DeleteFile)
				BXFile.Delete(section.ImageId);
			BXFile fImage = SaveFile("preview");
			section.ImageId = ((fImage != null) ? fImage.Id : (Img.DeleteFile ? 0 : section.ImageId));

			if (DetailImg.DeleteFile)
				BXFile.Delete(section.DetailImageId);
			BXFile fDetailImage = SaveFile("detail");
			section.DetailImageId = ((fDetailImage != null) ? fDetailImage.Id : (DetailImg.DeleteFile ? 0 : section.DetailImageId));
			if (CustomFieldList1.HasItems)
				section.CustomValues.Override(CustomFieldList1.Save());
            section.Update();

			//if (CustomFieldList1.HasItems)
			//	BXCustomEntityManager.SaveEntity(propertyEntryName, section.Id, CustomFieldList1.Save());

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
			case "DeleteSection":
				try
				{
					if (!currentUserCanModifySection)
						throw new Exception(GetMessageRaw("Exception.YouDontHaveRightsToDeleteThisRecord"));

					if (sectionId > 0)
					{
						redirectSectionId = section.SectionId;

                        section.Delete();
					}
					else
					{
						throw new Exception(GetMessageRaw("Exception.SectionIsNotFound"));
					}

					Page.Response.Redirect(String.Format("IBlockListAdmin.aspx?type_id={0}&iblock_id={1}&filter_sectionid={2}", typeId, iblockId, redirectSectionId));
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

	protected override string BackUrl
	{
		get
		{
			return base.BackUrl ?? string.Format("IBlockListAdmin.aspx?type_id={0}&iblock_id={1}&filter_sectionid={2}", typeId, iblockId, redirectSectionId);
		}
	}

	protected override void PrepareDialogContent(BXDialogData data)
	{
		if(data == null)
			throw new ArgumentNullException("data");

		BXDialogSectionData content = data.CreateSection(BXDialogSectionType.Content);
		BXTabControl1.PublicMode = true;
		BXTabControl1.ButtonsMode = BXTabControl.ButtonsModeType.Hidden;
		BXContextMenuToolbar1.Visible = false;
		content.CreateItemFromControl(this);
	}

	protected override bool ValidateDialogData()
	{
		Validate(BXTabControl1.ValidationGroup);
		return IsValid;
	}

	protected override void SaveDialogData()
	{
		if(!SaveSection())
			return;

		string backUrl = Request[BXConfigurationUtility.Constants.BackUrl];
		if(string.IsNullOrEmpty(backUrl))
			Refresh(string.Empty, BXDialogGoodbyeWindow.LayoutType.Success, 0);

		BXParamsBag<object> replace = new BXParamsBag<object>();
		replace.Add("SectionId", this.sectionId.ToString());
		replace.Add("SECTION_ID", this.sectionId.ToString());
		replace.Add("SectionCode", this.sectionId.ToString());
		replace.Add("SECTION_CODE", this.sectionId.ToString());
		Redirect(BXSefUrlUtility.MakeLink(backUrl, replace), string.Empty, BXDialogGoodbyeWindow.LayoutType.Success, 0);
	}
}
