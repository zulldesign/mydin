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
using Bitrix.IBlock;
using System.Text;
using Bitrix.Security;
using Bitrix.DataLayer;
using System.Collections.Generic;
using Bitrix.UI;
using Bitrix.Services;
using Bitrix.Modules;
using Bitrix.Services.Text;
using Bitrix.IO;
using Bitrix;

public partial class bitrix_admin_IBlockListAdmin : Bitrix.UI.BXAdminPage
{
	protected int typeId = -1;
	protected int iblockId = -1;
	bool initCalled;
	//int sectionId = -1;

	BXIBlockType type;
	BXIBlock iblock;
    BXIBlockSection currentSection;

	bool canModifySections = false;
	bool canModifyElements = false;

	public bool CanModifySections
	{
		get { return canModifySections; }
	}

	public bool CanModifyElements
	{
		get { return canModifyElements; }
	}

	protected void Page_Init(object sender, EventArgs e)
	{
		InitPage();

		var entityId = BXIBlockElement.GetCustomFieldsKey(iblockId);
		GridView1.CreateCustomColumns(entityId, null, x => "#" + x.Name);
		BXAdminFilter1.CreateCustomFilters(entityId);
	}

	private void AddCustomFieldColumns(DataColumnCollection columns)
	{
		var fields = Bitrix.BXCustomEntityManager.GetFields(BXIBlockElement.GetCustomFieldsKey(iblockId));
		foreach (var field in fields)
			columns.Add("#" + field.Name, typeof(BXCustomProperty));
	}

	private void AddCustomFieldValues(DataRow row, BXEntity entity)
	{
		var fields = Bitrix.BXCustomEntityManager.GetFields(BXIBlockElement.GetCustomFieldsKey(iblockId));
		foreach (var field in fields)
			row["#" + field.Name] = entity.CustomValues[field.Name];
	}

	private void InitPage()
	{
		if (initCalled)
			return;
		initCalled = true;

		iblockId = base.GetRequestInt("iblock_id");
		if (iblockId > 0)
			hfIBlockId.Value = iblockId.ToString();
		Int32.TryParse(hfIBlockId.Value, out iblockId);
		if (iblockId > 0)
		{
			iblock = BXIBlock.GetById(iblockId);
			if (iblock == null)
			{
				iblockId = 0;
				hfIBlockId.Value = iblockId.ToString();
			}
		}

		if (iblockId > 0)
		{
			typeId = iblock.TypeId;
			hfTypeId.Value = typeId.ToString();
			type = BXIBlockType.GetById(typeId);
			if (type == null)
				Response.Redirect("IBlockTypeList.aspx");
		}
		else
		{
			typeId = base.GetRequestInt("type_id");
			if (typeId > 0)
				hfTypeId.Value = typeId.ToString();
			Int32.TryParse(hfTypeId.Value, out typeId);
			Page.Response.Redirect(String.Format("IBlockAdmin.aspx?type_id={0}", typeId));
		}

		if (!BXIBlock.IsUserCanOperate(iblockId, BXIBlock.Operations.IBlockAdminRead))
			BXAuthentication.AuthenticationRequired();

		filterSectionId.Values.Add(new ListItem(GetMessageRaw("Any"), ""));
		filterSectionId.Values.Add(new ListItem(GetMessageRaw("TopLevel"), "0"));

		BXIBlockSectionCollection sections =  BXIBlockSection.GetList(
			new BXFilter(new BXFilterItem(BXIBlockSection.Fields.IBlock.ID, BXSqlFilterOperators.Equal, iblockId)),
			new BXOrderBy(new BXOrderByPair(BXIBlockSection.Fields.LeftMargin, BXOrderByDirection.Asc)),
			null,
			null,
			BXTextEncoder.EmptyTextEncoder
		);

		foreach (BXIBlockSection section in sections)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < section.DepthLevel; i++)
				sb.Append(" . ");
			sb.Append(section.Name);
			filterSectionId.Values.Add(new ListItem(sb.ToString(), section.Id.ToString()));
		}

		canModifySections = BXIBlock.IsUserCanOperate(iblockId, BXIBlock.Operations.IBlockModifySections);
		canModifyElements = BXIBlock.IsUserCanOperate(iblockId, BXIBlock.Operations.IBlockModifyElements);

		AddSectionButton.Visible = canModifySections;
		AddElementButton.Visible = canModifyElements;
        int sectionId = GetElementSectionId(0);
        currentSection = (sectionId == 0) ? null :BXIBlockSection.GetById(sectionId);
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		successMessage.Visible = false;
        Page.Title = 
              String.Format("{0}{1}", iblock.Name,
              currentSection == null ? String.Empty : ": "+currentSection.Name);
            //(!String.IsNullOrEmpty(iblock.SectionsName) ? iblock.SectionsName : (!String.IsNullOrEmpty(type.Translations[BXLoc.CurrentLocale].SectionName) ? type.Translations[BXLoc.CurrentLocale].SectionName : GetMessage("Groups"))));
        ((BXAdminMasterPage)Page.Master).Title = Page.Title;
        
        AddElementButton.Text = iblock.CaptionsInfo.AddElement;
        AddSectionButton.Text = iblock.CaptionsInfo.AddSection;

        AddElementButton.Title = iblock.CaptionsInfo.AddElement;
        AddSectionButton.Title = iblock.CaptionsInfo.AddSection;

        filterSectionId.Text = BXHtmlTextEncoder.HtmlTextEncoder.Decode(iblock.CaptionsInfo.SectionName);

        System.Collections.ObjectModel.Collection<CommandItem> items = PopupPanel1.Commands;

	}

	protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
	{
		BXGridView grid = (BXGridView)sender;
		BXGridViewRow row = (BXGridViewRow)e.Row;
		DataRowView drv = (DataRowView)row.DataItem;
		if (e.Row.RowType != DataControlRowType.DataRow)
			return;
		List<string> allowed = new List<string>();

		if ((string)drv["CommonElementType"] == "S" && BXIBlock.IsUserCanOperate(iblockId, BXIBlock.Operations.IBlockModifySections)
			|| (string)drv["CommonElementType"] == "E" && BXIBlock.IsUserCanOperate(iblockId, BXIBlock.Operations.IBlockModifyElements))
		{
			allowed.Add("edit");
			allowed.Add("");
			allowed.Add("delete");
		}
		else
		{
			allowed.Add("view");
		}

		row.AllowedCommandsList = allowed.ToArray();
	}

	protected void GridView1_RowUpdating(object sender, GridViewUpdateEventArgs e)
	{
		BXGridView grid = (BXGridView)sender;
		GridViewRow row = grid.Rows[e.RowIndex];
		bitrix_ui_AdminImageField detailPicture = (bitrix_ui_AdminImageField)row.FindControl("DetailPicture");
		bitrix_ui_AdminImageField previewPicture = (bitrix_ui_AdminImageField)row.FindControl("PreviewPicture");

		if (detailPicture != null)
			e.NewValues["DetailPicture"] = detailPicture;

		if (previewPicture != null)
			e.NewValues["PreviewPicture"] = previewPicture;
	}

	protected void GridView1_Select(object sender, Bitrix.UI.BXSelectEventArgs e)
	{
		List<string> visibleColumnsList = new List<string>(GridView1.GetVisibleColumnsKeys());
		BXIBlockCommonCollection collection = BXIBlockSection.GetMixedList(MakeCurrentFilter(), e.SortExpression, new BXQueryParams(e.PagingOptions), BXTextEncoder.EmptyTextEncoder);
		e.Data = new DataView(FillTable(collection, visibleColumnsList));
	}

	protected void GridView1_GetSettingsQueryString(object sender, BXGridViewGetSettingsQueryStringEventArgs e)
	{
		InitPage();
		if (iblockId != 0)
			e.Query["iblock_id"] = iblockId.ToString();
	}

	private DataTable FillTable(BXIBlockCommonCollection collection, List<string> visibleColumnsList)
	{
		
		if (collection == null)
			collection = new BXIBlockCommonCollection();

		DataTable result = new DataTable();

		result.Columns.Add("num", typeof(int));
		result.Columns.Add("Name", typeof(string));
		result.Columns.Add("Sort", typeof(int));
		result.Columns.Add("Code", typeof(string));
		result.Columns.Add("XmlId", typeof(string));
		result.Columns.Add("Tags", typeof(string));
		result.Columns.Add("Active", typeof(bool));
		result.Columns.Add("ElementsCount", typeof(int));
		result.Columns.Add("SectionsCount", typeof(int));
		result.Columns.Add("UpdateDate", typeof(DateTime));
		result.Columns.Add("ActiveFromDate", typeof(string));
		result.Columns.Add("ActiveToDate", typeof(string));
		result.Columns.Add("ModifiedBy", typeof(string));
		result.Columns.Add("CreateDate", typeof(DateTime));
		result.Columns.Add("CreatedBy", typeof(string));

		result.Columns.Add("PreviewText", typeof(string));
		result.Columns.Add("PreviewTextType", typeof(char));
		result.Columns.Add("PreviewPictureId", typeof(int));

		result.Columns.Add("DetailText", typeof(string));
		result.Columns.Add("DetailTextType", typeof(char));
		result.Columns.Add("DetailPictureId", typeof(int));
		result.Columns.Add("ID", typeof(int));
		result.Columns.Add("CommonElementType", typeof(string));

		AddCustomFieldColumns(result.Columns);

		bool canViewUsers = this.BXUser.IsCanOperate(BXRoleOperation.Operations.UserView);

		foreach (BXEntity t in collection)
		{
			DataRow r = result.NewRow();
			if (t is BXIBlockSection)
			{
				BXIBlockSection s = (BXIBlockSection)t;
				r["Name"] = s.Name;
				r["Sort"] = s.Sort;
				r["Code"] = s.Code;
				r["XmlId"] = s.XmlId;
				r["Tags"] = "";
				r["Active"] = s.Active;
				r["ElementsCount"] = (visibleColumnsList.Contains("ElementsCount") ? s.ElementsCount : 0);
				r["SectionsCount"] = (visibleColumnsList.Contains("SectionsCount") ? s.SectionsCount : 0);
				r["UpdateDate"] = s.UpdateDate;
				r["ActiveFromDate"] = "";
				r["ActiveToDate"] = "";

                if (visibleColumnsList.Contains("ModifiedBy") && s.ModifiedByUser != null)
				{
					if (canViewUsers)
						r["ModifiedBy"] = String.Format("(<a href=\"AuthUsersEdit.aspx?id={3}\">{0}</a>) {1} {2}", Encode(s.ModifiedByUser.UserName), Encode(s.ModifiedByUser.FirstName), Encode(s.ModifiedByUser.LastName), s.ModifiedBy);
					else
						r["ModifiedBy"] = String.Format("({0}) {1} {2}", Encode(s.ModifiedByUser.UserName), Encode(s.ModifiedByUser.FirstName), Encode(s.ModifiedByUser.LastName));
                }
				else
                    r["ModifiedBy"] = "";

                if (visibleColumnsList.Contains("CreatedBy") && s.CreatedByUser != null)
				{
					if (canViewUsers)
						r["CreatedBy"] = String.Format("(<a href=\"AuthUsersEdit.aspx?id={3}\">{0}</a>) {1} {2}", Encode(s.CreatedByUser.UserName), Encode(s.CreatedByUser.FirstName), Encode(s.CreatedByUser.LastName), s.CreatedBy);
					else
						r["CreatedBy"] = String.Format("({0}) {1} {2}", Encode(s.CreatedByUser.UserName), Encode(s.CreatedByUser.FirstName), Encode(s.CreatedByUser.LastName));
				}
                else
                    r["CreatedBy"] = "";

				r["CreateDate"] = s.CreateDate;
				r["PreviewText"] = "";
				r["PreviewTextType"] = 'H';
				r["DetailText"] = s.Description;
				r["DetailTextType"] = (s.DescriptionType == BXTextType.Html ? 'H' : 'T');
				r["DetailPictureId"] = s.DetailImageId;
				r["PreviewPictureId"] = s.ImageId;
				r["ID"] = s.Id;
				r["CommonElementType"] = "S";
			}
			else if (t is BXIBlockElement)
			{
				BXIBlockElement s = (BXIBlockElement)t;
				r["Name"] = s.Name;
				r["Sort"] = s.Sort;
				r["Code"] = s.Code;
				r["XmlId"] = s.XmlId;
				r["Tags"] = s.Tags;
				r["Active"] = s.Active;
				r["ElementsCount"] = 0;
				r["SectionsCount"] = 0;
				r["CreateDate"] = s.CreateDate;
				r["UpdateDate"] = s.UpdateDate;

				string activeFromDate = String.Empty;
				if (s.ActiveFromDate != DateTime.MinValue)
				{
					if (s.ActiveFromDate.TimeOfDay == TimeSpan.Zero)
						activeFromDate = s.ActiveFromDate.ToString("d");
					else
						activeFromDate = s.ActiveFromDate.ToString();
				}

				r["ActiveFromDate"] = activeFromDate;

				string activeToDate = String.Empty;
				if (s.ActiveToDate != DateTime.MinValue)
				{
					if (s.ActiveToDate.TimeOfDay == TimeSpan.Zero)
						activeToDate = s.ActiveToDate.ToString("d");
					else
						activeToDate = s.ActiveToDate.ToString();
				}

				r["ActiveToDate"] = activeToDate;
				
				if (visibleColumnsList.Contains("ModifiedBy") && s.ModifiedByUser != null)
				{
					if (canViewUsers)
						r["ModifiedBy"] = String.Format("(<a href=\"AuthUsersEdit.aspx?id={3}\">{0}</a>) {1} {2}", Encode(s.ModifiedByUser.UserName), Encode(s.ModifiedByUser.FirstName), Encode(s.ModifiedByUser.LastName), s.ModifiedBy);
					else
						r["ModifiedBy"] = String.Format("({0}) {1} {2}", Encode(s.ModifiedByUser.UserName), Encode(s.ModifiedByUser.FirstName), Encode(s.ModifiedByUser.LastName));
                }
				else
                    r["ModifiedBy"] = "";

                if (visibleColumnsList.Contains("CreatedBy") && s.CreatedByUser != null)
				{
					if (canViewUsers)
						r["CreatedBy"] = String.Format("(<a href=\"AuthUsersEdit.aspx?id={3}\">{0}</a>) {1} {2}", Encode(s.CreatedByUser.UserName), Encode(s.CreatedByUser.FirstName), Encode(s.CreatedByUser.LastName), s.CreatedBy);
					else
						r["CreatedBy"] = String.Format("({0}) {1} {2}", Encode(s.CreatedByUser.UserName), Encode(s.CreatedByUser.FirstName), Encode(s.CreatedByUser.LastName));
				}
                else
                    r["CreatedBy"] = "";


				r["PreviewText"] = s.PreviewText;
				r["PreviewTextType"] = (s.PreviewTextType == BXTextType.Html ? 'H' : 'T');

				r["DetailText"] = s.DetailText;
				r["DetailTextType"] = (s.DetailTextType == BXTextType.Html ? 'H' : 'T');
				r["DetailPictureId"] = s.DetailImageId;
				r["PreviewPictureId"] = s.PreviewImageId;

				r["ID"] = s.Id;
				r["CommonElementType"] = "E";

				AddCustomFieldValues(r, s);
			}
			else
				continue;

			result.Rows.Add(r);
		}

		return result;
	}

	

	private int GetElementSectionId(int elementId)
	{
		int sectionId = base.GetRequestInt("filter_sectionid");
		if (sectionId > 0)
			return sectionId;

		sectionId = 0;
		if (elementId > 0)
		{
			BXIBlockElement.BXInfoBlockElementSectionCollection sections = BXIBlockElement.BXInfoBlockElementSection.GetList(
				new BXFilter(
					new BXFilterItem(BXIBlockElement.BXInfoBlockElementSection.Fields.ElementId, BXSqlFilterOperators.Equal, elementId)
				),
				null
			);

			if (sections != null && sections.Count > 0)
				sectionId = sections[0].SectionId;
		}

		return sectionId;
	}

	protected string GetElementName(object param)
	{
		DataRowView dataRowView = param as DataRowView;

		if (dataRowView == null)
			return String.Empty;

		if ((string)dataRowView["CommonElementType"] == "S")
			return String.Format("<div class=\"iblock_menu_icon_sections\"></div><a href=\"IBlockListAdmin.aspx?type_id={1}&iblock_id={0}&filter_sectionid={2}\" title=\"{4}\">{3}</a>", iblockId, typeId, (int)dataRowView["ID"], BXTextEncoder.HtmlTextEncoder.Encode((string)dataRowView["Name"]), GetMessage("Go2SubsectionList"));
		else
			return String.Format("<div class=\"iblock_menu_icon_elements\"></div><a href=\"IBlockElementEdit.aspx?type_id={2}&iblock_id={1}&section_id={5}&id={0}\" title=\"{4}\">{3}</a>", (int)dataRowView["ID"], iblockId, typeId, BXTextEncoder.HtmlTextEncoder.Encode((string)dataRowView["Name"]), GetMessage("Go2ElementModification"), GetElementSectionId((int)dataRowView["ID"]));
	}

	protected string GetElementText(object param, string type)
	{
		DataRowView dataRowView = param as DataRowView;
		string text = dataRowView[type] as String;

		if (String.IsNullOrEmpty(text) || dataRowView == null || type == null)
			return "&nbsp;";

		if ((char)dataRowView[type + "Type"] == 'H')
			return BXStringUtility.HtmlToText(text);
		else
			return BXTextEncoder.HtmlTextEncoder.Encode(text);
			
	}

	protected void GridView1_PopupMenuClick(object sender, Bitrix.UI.BXPopupMenuClickEventArgs e)
	{
		BXGridView grid = sender as BXGridView;

		DataKey drv = grid.DataKeys[e.EventRowIndex];
		if (drv == null)
			return;

		int tempId;
		Int32.TryParse(drv.Values[0].ToString(), out tempId);
		string tempKey = drv.Values[1].ToString();

		if ((tempId > 0) && ("E".Equals(tempKey, StringComparison.InvariantCultureIgnoreCase) || "S".Equals(tempKey, StringComparison.InvariantCultureIgnoreCase)))
		{
			switch (e.CommandName)
			{
				case "view":
				case "edit":
					e.Cancel = true;
					if ("E".Equals(tempKey, StringComparison.InvariantCultureIgnoreCase))
						Response.Redirect(String.Format("IBlockElementEdit.aspx?type_id={2}&iblock_id={1}&section_id={3}&id={0}", tempId, iblockId, typeId, GetElementSectionId(tempId)));
					else
						Response.Redirect(String.Format("IBlockSectionEdit.aspx?type_id={2}&iblock_id={1}&section_id={0}&id={0}", tempId, iblockId, typeId));
					break;

				default:
					break;
			}
		}
		else
		{
			errorMessage.AddErrorMessage(GetMessage("Error.ElementCodeIsNotFound"));
		}
	}

	protected void BXContextMenuToolbar1_CommandClick(object sender, CommandEventArgs e)
	{
		int sectionId = 0;
		switch (e.CommandName)
		{
			case "addSection":
				foreach (BXFormFilterItem fi in BXAdminFilter1.CurrentFilter)
				{
					if (fi.filterName.Equals("SectionId", StringComparison.InvariantCultureIgnoreCase))
					{
						int.TryParse(fi.filterValue.ToString(), out sectionId);
						break;
					}
				}
				Response.Redirect(String.Format("IBlockSectionEdit.aspx?type_id={1}&iblock_id={0}&section_id={2}", iblockId, typeId, sectionId));
				break;
			case "addElement":
				foreach (BXFormFilterItem fi in BXAdminFilter1.CurrentFilter)
				{
					if (fi.filterName.Equals("SectionId", StringComparison.InvariantCultureIgnoreCase))
					{
						int.TryParse(fi.filterValue.ToString(), out sectionId);
						break;
					}
				}
				Response.Redirect(String.Format("IBlockElementEdit.aspx?type_id={1}&iblock_id={0}&section_id={2}", iblockId, typeId, sectionId));
				break;
			case "iblock":
				Response.Redirect(String.Format("IBlockEdit.aspx?type_id={1}&id={0}", iblockId, typeId));
				break;
		}
	}

	protected void GridView1_Delete(object sender, BXDeleteEventArgs e)
	{
		BXGridView grid = (BXGridView)sender;
		try
		{
			BXIBlockCommonCollection elements;
			if (e.Keys != null) //Delete one element
			{
				elements = new BXIBlockCommonCollection();
				
				int tempId;
				Int32.TryParse(e.Keys["ID"].ToString(), out tempId);
				string tempKey = e.Keys["CommonElementType"].ToString();
				if (tempId <= 0 
					|| 	(!string.Equals(tempKey, "E", StringComparison.OrdinalIgnoreCase) 
						&& !string.Equals(tempKey, "S", StringComparison.OrdinalIgnoreCase)))
				{
					throw new PublicException(GetMessageRaw("Error.ElementCodeIsNotFound"));
				}
				
				if (string.Equals(tempKey, "E", StringComparison.OrdinalIgnoreCase))
				{
					BXIBlockElement elem = BXIBlockElement.GetById(tempId);
					if (elem == null)
						throw new PublicException(GetMessageRaw("Exception.ElementIsNotFound"));
					elements.Add(elem);
				}
				else
				{
					BXIBlockSection sect = BXIBlockSection.GetById(tempId);
					if (sect == null)
						throw new PublicException(GetMessageRaw("Exception.SectionIsNotFound"));
					elements.Add(sect);
				}
			}
			else //All elements
			{
				elements = BXIBlockSection.GetMixedList(MakeCurrentFilter(), null);
			}

			foreach (BXEntity element in elements)
			{
				if (element == null)
					throw new PublicException(GetMessageRaw("Exception.ElementIsNotFound"));
				if (element is BXIBlockElement)
				{
					BXIBlockElement elem = (BXIBlockElement)element;
					if (!BXIBlock.IsUserCanOperate(elem.IBlockId, BXIBlock.Operations.IBlockModifyElements))
						throw new PublicException(GetMessageRaw("ExceptionText.YouDontHaveRightsToDeleteThisRecord"));
					elem.Delete();					
				}
				else
				{
					BXIBlockSection sect = (BXIBlockSection)element;
					if (!BXIBlock.IsUserCanOperate(sect.IBlockId, BXIBlock.Operations.IBlockModifySections))
						throw new PublicException(GetMessageRaw("Exception.YouDontHaveRightsToDeleteThisRecord"));

                    sect.Delete();
					//if (!BXInfoBlockSectionManagerOld.Delete(sect.Id))
						//throw new Exception(GetMessageRaw("Exception.AnErrorHasOccurredWhileDeletingSection"));
				}
				e.DeletedCount++;
			}
			successMessage.Visible = true;
		}
		catch (Exception ex)
		{
			ProcessException(ex, errorMessage.AddErrorText);
		}
		grid.MarkAsChanged();
	}

	protected void GridView1_Update(object sender, BXUpdateEventArgs e)
	{
		//UpdatePanel1.Triggers.Add(

		int elementID = (int)e.Keys["ID"];
		string elementType = (string)e.Keys["CommonElementType"];

		if (elementID < 1)
			return;

		try
		{
			if (elementType == "S" && canModifySections)
			{
				BXIBlockSectionCollection sectionCollection = BXIBlockSection.GetList(
					new BXFilter(
						new BXFilterItem(BXIBlockSection.Fields.ID, BXSqlFilterOperators.Equal, elementID),
						new BXFilterItem(BXIBlockSection.Fields.IBlock.ID, BXSqlFilterOperators.Equal, iblockId)
					),
					null
				);


				if (sectionCollection == null || sectionCollection.Count != 1)
					return;

				BXIBlockSection section = sectionCollection[0];

				if (e.NewValues.Contains("Active"))
					section.Active = (bool)e.NewValues["Active"];

				if (e.NewValues.Contains("Name") && !BXStringUtility.IsNullOrTrimEmpty((string)e.NewValues["Name"]))
					section.Name = (string)e.NewValues["Name"];

				if (e.NewValues.Contains("Code"))
					section.Code = (string)e.NewValues["Code"];

				if (e.NewValues.Contains("XmlId"))
					section.XmlId = (string)e.NewValues["XmlId"];

				if (e.NewValues.Contains("DetailText"))
					section.Description = (string)e.NewValues["DetailText"];

				if (e.NewValues.Contains("DetailTextType"))
					section.DescriptionType = (string)e.NewValues["DetailTextType"] == "H" ? BXTextType.Html : BXTextType.Text;


				if (e.NewValues.Contains("DetailPicture"))
				{
					bitrix_ui_AdminImageField adminImageField = (bitrix_ui_AdminImageField)e.NewValues["DetailPicture"];

					BXFile detailPicture = SaveFile(adminImageField, sectionCollection[0].DetailImageId);
					if (detailPicture != null)
						section.DetailImageId = detailPicture.Id;
					else if (adminImageField.DeleteFile)
						section.DetailImageId = 0;
				}

				if (e.NewValues.Contains("PreviewPicture"))
				{
					bitrix_ui_AdminImageField adminImageField = (bitrix_ui_AdminImageField)e.NewValues["PreviewPicture"];

					BXFile previewPicture = SaveFile(adminImageField, sectionCollection[0].ImageId);
					if (previewPicture != null)
						section.ImageId = previewPicture.Id;
					else if (adminImageField.DeleteFile)
						section.ImageId = 0;
				}

				int sortIndex;
				if (e.NewValues.Contains("Sort") && int.TryParse((string)e.NewValues["Sort"], out sortIndex))
					section.Sort = sortIndex;


				section.ModifiedBy = (this.BXUser.Identity as BXIdentity).Id;
				section.Update();
			}
			else if (elementType == "E" && canModifyElements)
			{
				BXIBlockElementCollection elementCollection = BXIBlockElement.GetList(
					new BXFilter(
						new BXFilterItem(BXIBlockElement.Fields.ID, BXSqlFilterOperators.Equal, elementID),
						new BXFilterItem(BXIBlockElement.Fields.IBlock.ID, BXSqlFilterOperators.Equal, iblockId)
					),
					null
				);

				if (elementCollection == null || elementCollection.Count != 1)
					return;

				BXIBlockElement element = elementCollection[0];
				//element.Id = elementID;

				if (e.NewValues.Contains("Name") && !BXStringUtility.IsNullOrTrimEmpty((string)e.NewValues["Name"]))
					element.Name = (string)e.NewValues["Name"];

				if (e.NewValues.Contains("Code"))
					element.Code = (string)e.NewValues["Code"];

				if (e.NewValues.Contains("XmlId"))
					element.XmlId = (string)e.NewValues["XmlId"];

				if (e.NewValues.Contains("Tags"))
					element.Tags = (string)e.NewValues["Tags"];

				if (e.NewValues.Contains("Active"))
					element.Active = (bool)e.NewValues["Active"];

				if (e.NewValues.Contains("PreviewText"))
					element.PreviewText = (string)e.NewValues["PreviewText"];

				if (e.NewValues.Contains("PreviewTextType"))
					element.PreviewTextType = (string)e.NewValues["PreviewTextType"] == "H" ? BXTextType.Html : BXTextType.Text;

				if (e.NewValues.Contains("DetailText"))
					element.DetailText = (string)e.NewValues["DetailText"];

				if (e.NewValues.Contains("DetailTextType"))
					element.DetailTextType = (string)e.NewValues["DetailTextType"] == "H" ? BXTextType.Html : BXTextType.Text;
	
				if (e.NewValues.Contains("DetailPicture"))
				{
					bitrix_ui_AdminImageField adminImageField = (bitrix_ui_AdminImageField)e.NewValues["DetailPicture"];

					BXFile detailPicture = SaveFile(adminImageField, elementCollection[0].DetailImageId);
					if (detailPicture != null)
						element.DetailImageId = detailPicture.Id;
					else if (adminImageField.DeleteFile)
						element.DetailImageId = 0;
				}

				if (e.NewValues.Contains("PreviewPicture"))
				{
					bitrix_ui_AdminImageField adminImageField = (bitrix_ui_AdminImageField)e.NewValues["PreviewPicture"];

					BXFile previewPicture = SaveFile(adminImageField, elementCollection[0].PreviewImageId);
					if (previewPicture != null)
						element.PreviewImageId = previewPicture.Id;
					else if (adminImageField.DeleteFile)
						element.PreviewImageId = 0;
				}

				if (e.NewValues.Contains("ActiveFromDate"))
				{
					DateTime activeFrom;
					DateTime.TryParse((string)e.NewValues["ActiveFromDate"], out activeFrom);
					element.ActiveFromDate = activeFrom;
				}

				if (e.NewValues.Contains("ActiveToDate"))
				{
					DateTime activeTo;
					DateTime.TryParse((string)e.NewValues["ActiveToDate"], out activeTo);
					element.ActiveToDate = activeTo;
				}

				int sortIndex;
				if (e.NewValues.Contains("Sort") && int.TryParse((string)e.NewValues["Sort"], out sortIndex))
					element.Sort = sortIndex;


				BXGridView.FillCustomValues(e.NewValues, element, x => x.StartsWith("#"), x => x.Substring(1));

				element.ModifiedBy = (this.BXUser.Identity as BXIdentity).Id;

				element.Update();
			}

		}
		catch (BXEventException exception)
		{
			foreach (string s in exception.Messages)
				errorMessage.AddErrorMessage(s);
		}
		catch (Exception exception)
		{
			errorMessage.AddErrorMessage(exception.Message);
		}
	}

	protected BXFile SaveFile(bitrix_ui_AdminImageField adminImageField, int currentImageId)
	{
		if (adminImageField == null)
			return null;

		BXFile image = null;
		FileUpload fileUpload = adminImageField.Upload;

		if (adminImageField.Upload.HasFile)
		{
			image = new BXFile(fileUpload.PostedFile, "iblock", "iblock", adminImageField.Description);
			BXFileValidationResult result = image.ValidateAsImage(0, 0, 0);
			if (result != BXFileValidationResult.Valid)
			{
				string errorMessage = "";
				if ((result & BXFileValidationResult.InvalidContentType) == BXFileValidationResult.InvalidContentType)
					errorMessage += GetMessageRaw("InvalidType");
				if ((result & BXFileValidationResult.InvalidExtension) == BXFileValidationResult.InvalidExtension)
				{
					if (!String.IsNullOrEmpty(errorMessage))
						errorMessage += ", ";
					errorMessage += GetMessageRaw("InvalidExtension");
				}
				if ((result & BXFileValidationResult.InvalidImage) == BXFileValidationResult.InvalidImage)
				{
					if (!String.IsNullOrEmpty(errorMessage))
						errorMessage += ", ";
					errorMessage += GetMessageRaw("InvalidImage");
				}
				throw new Exception(String.Format(GetMessageRaw("Error.InvalidImage"), errorMessage));
			}
            image.DemandFileUpload();
			image.Save();
			return image;
		}
		else if (currentImageId > 0 && (image = BXFile.GetById(currentImageId)) != null)
		{
			image.Description = adminImageField.Description;
			image.Save();
		}

		return null;
	}

	protected void GridView1_SelectCount(object sender, BXSelectCountEventArgs e)
	{
		e.Count = BXIBlockSection.CountMixed(MakeCurrentFilter());
	}

	protected void filterUser_CustomBuildFilter(object sender, BXTextBoxAndDropDownFilter.BuildFilterEventArgs e)
	{
		BXTextBoxAndDropDownFilter filter = (BXTextBoxAndDropDownFilter)sender;
		e.FilterItems.Clear();
		int id;
		if (int.TryParse(e.DropDownValue, out id))
			e.FilterItems.Add(new BXFormFilterItem(filter.Key, id, BXSqlFilterOperators.Equal));
		if (int.TryParse(e.TextBoxValue, out id))
			e.FilterItems.Add(new BXFormFilterItem(filter.Key, id, BXSqlFilterOperators.Equal));
	}

	BXFormFilter MakeCurrentFilter()
	{
		BXFormFilter filter = new BXFormFilter(BXAdminFilter1.CurrentFilter);
		filter.Add(new BXFormFilterItem("IBlockId", iblockId, BXSqlFilterOperators.Equal));
		return filter;
	}

    protected void Page_PreRender(object sender, EventArgs e)
    {
        createByAutocomplete.AttachScript();
        modifiedByAutocomplete.AttachScript();
    }

}
