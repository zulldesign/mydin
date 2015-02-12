using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Bitrix;

using Bitrix.DataLayer;
using Bitrix.IBlock;
using Bitrix.Modules;
using Bitrix.Security;
using Bitrix.UI;
using Bitrix.IO;
using Bitrix.Services.Text;
using Bitrix.Services;

public partial class bitrix_admin_IBlockEdit : Bitrix.UI.BXAdminPage
{
	int iblockId = -1;
	int typeId = -1;
	string CaptionsTbWidth = "250px";

	BXIBlock iblock;
	object state;
	BXRoleCollection allRoles;
	BXRoleCollection Roles
	{
		get
		{
			if (allRoles == null)
			{
				allRoles = BXRoleManager.GetList(
					new BXFormFilter(new BXFormFilterItem("Active", true, BXSqlFilterOperators.Equal)),
					new BXOrderBy_old("RoleName", "Asc")
				);
			}
			return allRoles;
		}
	}

	BXRoleTaskCollection allTasks;
	BXRoleTaskCollection Tasks
	{
		get
		{
			if (allTasks == null)
			{
				allTasks = BXRoleTaskManager.GetList(
					new BXFormFilter(new BXFormFilterItem("Operation.ModuleId", "iblock", BXSqlFilterOperators.Equal)),
					new BXOrderBy_old("TaskName", "Asc")
				);
			}
			return allTasks;
		}
	}

	BXRoleOperationCollection allOperations;
	BXRoleOperationCollection Operations
	{
		get
		{
			if (allOperations == null)
			{
				allOperations = BXRoleOperationManager.GetList(
					new BXFormFilter(new BXFormFilterItem("ModuleId", "iblock", BXSqlFilterOperators.Equal)),
					new BXOrderBy_old("OperationName", "Asc")
				);
			}

			return allOperations;
		}
	}

	bool currentUserCanModifyIBlock = false;

	string propertyEntryName;
	string sectionPropertyEntryName;

	protected bool IsSearchModuleInstalled
	{
		get
		{
			return Bitrix.Modules.BXModuleManager.IsModuleInstalled("search");
		}
	}

	private void LoadData()
	{
		cblSites.Items.Clear();
		BXSiteCollection siteColl = BXSite.GetList(null, null);
		foreach (BXSite site in siteColl)
			cblSites.Items.Add(new ListItem("[" + site.Id + "] " + site.Name, site.Id));

		if (iblock == null)
		{
			iblockId = -1;
			hfIBlockId.Value = iblockId.ToString();

			cbActive.Checked = true;
			tbSort.Text = "500";
			cbIndexContent.Checked = true;

			trID.Visible = false;
			trUpdateDate.Visible = false;
			//creating CaptionsInfo object to obtain default values for captions
			//when infoblock does not exist
			BXIBlockCaptionsInfo ci = new BXIBlockCaptionsInfo(BXTextEncoder.HtmlTextEncoder);

			tbSectionName.Text = ci.SectionName;
			tbElementName.Text = ci.ElementName;
			tbSectionsName.Text = ci.SectionsName;
			tbElementsName.Text = ci.ElementsName;

			tbAddElement.Text = ci.AddElement;
			tbChangeElement.Text = ci.ChangeElement;
			tbDeleteElement.Text = ci.DeleteElement;
			tbAddSection.Text = ci.AddSection;
			tbChangeSection.Text = ci.ChangeSection;
			tbDeleteSection.Text = ci.DeleteSection;

			tbNewElement.Text = ci.NewElement;
			tbElementList.Text = ci.ElementList;
			tbModifyingElement.Text = ci.ModifyingElement;
			tbNewSection.Text = ci.NewSection;
			tbSectionList.Text = ci.SectionList;
			tbModifyingSection.Text = ci.ModifyingSection;

		}
		else
		{
			trID.Visible = true;
			lbID.Text = iblock.Id.ToString();
			trUpdateDate.Visible = true;
			lbUpdateDate.Text = iblock.UpdateDate.ToString();

			cbActive.Checked = iblock.Active;
			tbCode.Text = iblock.Code;
			cbIndexContent.Checked = BXModuleManager.IsModuleInstalled("search") ? iblock.IndexContent : true;
			Bitrix.IBlock.BXIBlock.BXInfoBlockSiteCollection iblockSites = iblock.Sites;
			if(iblockSites != null && iblockSites.Count > 0)
				foreach (ListItem item in cblSites.Items)
					if (iblockSites.FindIndex(delegate(Bitrix.IBlock.BXIBlock.BXInfoBlockSite obj) 
					{
						return string.Equals(obj.SiteId, item.Value, StringComparison.Ordinal);
					}
					) >= 0)
						item.Selected = true;

			tbName.Text = iblock.Name;
			tbSort.Text = iblock.Sort.ToString();
			tbXmlId.Text = iblock.XmlId;

			Img.ImageFile = iblock.Image;

			if (BXIBlockModule.UseVisualEditor())
			{
				bxweDescription.Content = iblock.Description;
				bxweDescription.StartMode = ((iblock.DescriptionType == BXTextType.Text) ? BXWebEditor.StartModeType.PlainText : BXWebEditor.StartModeType.HTMLVisual);
			}
			else
			{
				tbDescription.Text = iblock.Description;
				rblDescriptionType.SelectedValue = ((iblock.DescriptionType == BXTextType.Text) ? "text" : "html");
			}

			tbSectionName.Text = iblock.CaptionsInfo.SectionName;
			tbElementName.Text = iblock.CaptionsInfo.ElementName;
			tbSectionsName.Text = iblock.CaptionsInfo.SectionsName;
			tbElementsName.Text = iblock.CaptionsInfo.ElementsName;

			tbAddElement.Text = iblock.CaptionsInfo.AddElement;
			tbChangeElement.Text = iblock.CaptionsInfo.ChangeElement;
			tbDeleteElement.Text = iblock.CaptionsInfo.DeleteElement;
			tbAddSection.Text = iblock.CaptionsInfo.AddSection;
			tbChangeSection.Text = iblock.CaptionsInfo.ChangeSection;
			tbDeleteSection.Text = iblock.CaptionsInfo.DeleteSection;

			tbNewElement.Text = iblock.CaptionsInfo.NewElement;
			tbElementList.Text = iblock.CaptionsInfo.ElementList;
			tbModifyingElement.Text = iblock.CaptionsInfo.ModifyingElement;
			tbNewSection.Text = iblock.CaptionsInfo.NewSection;
			tbSectionList.Text = iblock.CaptionsInfo.SectionList;
			tbModifyingSection.Text = iblock.CaptionsInfo.ModifyingSection;
		}

		cfl.EntityId = propertyEntryName;
		cfls.EntityId = sectionPropertyEntryName;

		//if (!ClientScript.IsStartupScriptRegistered(GetType(), "StartupScript"))
		//    ClientScript.RegisterStartupScript(GetType(), "StartupScript", sscript.ToString(), true);
	}

	protected void Page_Load(object sender, EventArgs e)
	{
		string success = base.GetRequestString("success");
		if ("Y".Equals(success, StringComparison.InvariantCultureIgnoreCase))
			successMessage.Visible = true;
		else
			successMessage.Visible = false;
		
		bxweDescription.Visible = BXIBlockModule.UseVisualEditor();
		tbDescription.Visible = !BXIBlockModule.UseVisualEditor();
		rblDescriptionType.Visible = !BXIBlockModule.UseVisualEditor();

		if (!Page.IsPostBack)
			LoadData();

		cfl.EntityId = propertyEntryName;
		cfls.EntityId = sectionPropertyEntryName;

		tbSectionName.Style.Add("width",CaptionsTbWidth);
		tbElementName.Style.Add("width",CaptionsTbWidth);
		tbSectionsName.Style.Add("width",CaptionsTbWidth);
		tbElementsName.Style.Add("width",CaptionsTbWidth);

		tbAddElement.Style.Add("width",CaptionsTbWidth);
		tbChangeElement.Style.Add("width",CaptionsTbWidth);
		tbDeleteElement.Style.Add("width",CaptionsTbWidth);
		tbAddSection.Style.Add("width",CaptionsTbWidth);
		tbChangeSection.Style.Add("width",CaptionsTbWidth); 
		tbDeleteSection.Style.Add("width",CaptionsTbWidth);

		tbNewElement.Style.Add("width",CaptionsTbWidth);
		tbElementList.Style.Add("width",CaptionsTbWidth);
		tbModifyingElement.Style.Add("width",CaptionsTbWidth);
		tbNewSection.Style.Add("width",CaptionsTbWidth);
		tbSectionList.Style.Add("width",CaptionsTbWidth);
		tbModifyingSection.Style.Add("width",CaptionsTbWidth);
	}

	private void PrepareAccessState()
	{
		AccessEdit.FillStandardRoles(Roles, false);

		AccessEdit.Operations.AddSeparator(GetMessageRaw("SecuritySeparator.Tasks"));
		foreach (BXRoleTask task in Tasks)
			AccessEdit.Operations.Add("t" + task.TaskId.ToString(), task.Title);

		AccessEdit.Operations.AddSeparator(GetMessageRaw("SecuritySeparator.Operations"));
		foreach (BXRoleOperation op in Operations)
			AccessEdit.Operations.Add("o" + op.OperationId.ToString(), op.OperationTitle);
	}
	private void LoadAccessState()
	{
		//SKIP THIS THING IF AJAX
		ScriptManager sm = ScriptManager.GetCurrent(this);
		if (sm != null && sm.IsInAsyncPostBack)
		{ 
			bool skip = true;
			for (Control parent = AccessEdit.Parent; parent != null; parent = parent.Parent)
			{
				UpdatePanel panel = parent as UpdatePanel;
				if (panel != null && panel.IsInPartialRendering)
				{ 
					skip = false; 
					break;
				}
			}
			if (skip)
				return;
		}

		AccessEdit.State.Clear();
		
		foreach (BXRole role in Roles)
		{
			string stringRoleId = role.RoleId.ToString();

			#region Get Inherited Tasks
			BXRoleTaskCollection inheritedTasks = BXRoleTaskManager.GetList(
				new BXFormFilter(
					new BXFormFilterItem("Role.RoleName", role.RoleName, BXSqlFilterOperators.Equal),
					new BXFormFilterItem("Role.ModuleId", string.Empty, BXSqlFilterOperators.Equal)
				),
				new BXOrderBy_old("TaskName", "Asc")
			);
			inheritedTasks.RemoveAll(delegate(BXRoleTask task)
			{
				return !AccessEdit.Operations.ContainsKey("t" + task.TaskId.ToString());
			});
			#endregion

			#region Get Current Tasks
			BXRoleTaskCollection currentTasks =
				iblock != null
				? BXRoleTaskManager.GetList(
					new BXFormFilter(
						new BXFormFilterItem("Role.RoleName", role.RoleName, BXSqlFilterOperators.Equal),
						new BXFormFilterItem("Role.ModuleId", "iblock", BXSqlFilterOperators.Equal),
						new BXFormFilterItem("Role.ExternalId", iblockId.ToString(), BXSqlFilterOperators.Equal)
					),
					new BXOrderBy_old("TaskName", "Asc")
				)
				: new BXRoleTaskCollection();
			currentTasks.RemoveAll(delegate(BXRoleTask task)
			{
				return !AccessEdit.Operations.ContainsKey("t" + task.TaskId.ToString());
			});
			#endregion

			#region Get Inherited Operations
			BXRoleOperationCollection inheritedOperations = BXRoleOperationManager.GetList(
					new BXFormFilter(
						new BXFormFilterItem("Role.RoleName", role.RoleName, BXSqlFilterOperators.Equal),
						new BXFormFilterItem("Role.ModuleId", string.Empty, BXSqlFilterOperators.Equal)
					),
					new BXOrderBy_old("OperationName", "Asc")
				);
			inheritedOperations.RemoveAll(delegate(BXRoleOperation operation)
			{
				return !AccessEdit.Operations.ContainsKey("o" + operation.OperationId.ToString());
			});
			#endregion

			#region Get Current Operations
			BXRoleOperationCollection currentOperations =
				iblock != null
				? BXRoleOperationManager.GetList(
					new BXFormFilter(
						new BXFormFilterItem("Role.RoleName", role.RoleName, BXSqlFilterOperators.Equal),
						new BXFormFilterItem("Role.ModuleId", "iblock", BXSqlFilterOperators.Equal),
						new BXFormFilterItem("Role.ExternalId", iblockId.ToString(), BXSqlFilterOperators.Equal)
					),
					new BXOrderBy_old("OperationName", "Asc")
				)
				: new BXRoleOperationCollection();
			currentOperations.RemoveAll(delegate(BXRoleOperation operation)
			{
				return !AccessEdit.Operations.ContainsKey("o" + operation.OperationId.ToString());
			});
			#endregion


			if (inheritedTasks.Count > 0 || currentTasks.Count > 0 || inheritedOperations.Count > 0 || currentOperations.Count > 0)
			{
				BXOperationsEditRoleInfo roleInfo = AccessEdit.Roles[stringRoleId];

				foreach (BXRoleTask task in inheritedTasks)
				{
					string stringTaskId = "t" + task.TaskId.ToString();
					BXOperationsEditOperationInfo opInfo;
					if (!roleInfo.Operations.TryGetValue(stringTaskId, out opInfo))
						roleInfo.Operations.Add(stringTaskId, opInfo = new BXOperationsEditOperationInfo());
					opInfo.InheritedState = BXOperationsEditInheritedOperationState.Allowed;
				}
				foreach (BXRoleTask task in currentTasks)
				{
					string stringTaskId = "t" + task.TaskId.ToString();
					BXOperationsEditOperationInfo opInfo;
					if (!roleInfo.Operations.TryGetValue(stringTaskId, out opInfo))
						roleInfo.Operations.Add(stringTaskId, opInfo = new BXOperationsEditOperationInfo());
					opInfo.State = BXOperationsEditOperationState.Allowed;
				}
				foreach (BXRoleOperation operation in inheritedOperations)
				{
					string stringOperationId = "o" + operation.OperationId.ToString();
					BXOperationsEditOperationInfo opInfo;
					if (!roleInfo.Operations.TryGetValue(stringOperationId, out opInfo))
						roleInfo.Operations.Add(stringOperationId, opInfo = new BXOperationsEditOperationInfo());
					opInfo.InheritedState = BXOperationsEditInheritedOperationState.Allowed;
				}
				foreach (BXRoleOperation operation in currentOperations)
				{
					string stringOperationId = "o" + operation.OperationId.ToString();
					BXOperationsEditOperationInfo opInfo;
					if (!roleInfo.Operations.TryGetValue(stringOperationId, out opInfo))
						roleInfo.Operations.Add(stringOperationId, opInfo = new BXOperationsEditOperationInfo());
					opInfo.State = BXOperationsEditOperationState.Allowed;
				}

				AccessEdit.State.SetRoleFromData(stringRoleId);
			}
		}
	}
	private void SaveAccessState()
	{
		List<string> allowed = new List<string>();
		foreach (KeyValuePair<string, Dictionary<string, BXOperationsEditOperationState>> role in AccessEdit.State)
		{
			int id = int.Parse(role.Key);
			BXRole r = Roles.Find(delegate(BXRole item)
			{
				return item.RoleId == id;
			});

			allowed.Clear();
			foreach (BXRoleTask task in Tasks)
				if (role.Value["t" + task.TaskId.ToString()] == BXOperationsEditOperationState.Allowed)
					allowed.Add(task.TaskName);

			if (allowed.Count > 0)
				BXRoleManager.AddRolesToTasks(new string[] { r.RoleName }, allowed.ToArray(), "iblock", iblock.Id.ToString());

			allowed.Clear();
			foreach (BXRoleOperation operation in Operations)
				if (role.Value["o" + operation.OperationId.ToString()] == BXOperationsEditOperationState.Allowed)
					allowed.Add(operation.OperationName);

			if (allowed.Count > 0)
				BXRoleManager.AddRolesToOperations(new string[] { r.RoleName }, allowed.ToArray(), "iblock", iblock.Id.ToString());
		}
	}

	protected void Page_LoadComplete(object sender, EventArgs e)
	{
		if (iblockId != -1)
			Page.Title = string.Format(GetMessage("FormatPageTitle.ModificationOfBlock"), iblockId);
		((BXAdminMasterPage)Page.Master).Title = Page.Title;

		LoadAccessState();
		AddButton.Visible = (iblockId > 0 && BXUser.IsCanOperate(BXIBlock.Operations.IBlockManageAdmin));
		DeleteButton.Visible = (iblockId > 0 && currentUserCanModifyIBlock);
		BXTabControl1.ShowSaveButton = BXTabControl1.ShowApplyButton = currentUserCanModifyIBlock;
	}

	protected void BXTabControl1_Command(object sender, BXTabControlCommandEventArgs e)
	{
		bool noRedirect = false;
		bool successAction = true;
		if (e.CommandName == "save")
		{
			if (!SaveIBlock())
			{
				successAction = false;
				noRedirect = true;
			}
		}
		else if (e.CommandName == "apply")
		{
			if (!SaveIBlock())
				successAction = false;
			noRedirect = true;
		}

		if (!noRedirect)
		{
			Page.Response.Redirect(String.Format("IBlockAdmin.aspx?type_id={0}", typeId));
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

	private bool SaveIBlock()
	{
		try
		{
			Page.Validate("IBlockEdit");
			if (!Page.IsValid)
				throw new Exception();

			AccessEdit.State.Validate(false);

			if (!(iblockId > 0 ? UpdateIBlock() : CreateIBlock()))
				throw new Exception();

			if (cbRebuildSearchIndex.Checked)
			{
				BXSchedulerAgent a = new BXSchedulerAgent();
				a.SetClassNameAndAssembly(typeof(BXIBlock.IndexSynchronizer));
				a.Parameters.Add("Action", BXIBlock.IndexSynchronizerAction.Rebuild.ToString("G"));
				a.Parameters.Add("IBlockId", iblockId);
				a.StartTime = DateTime.Now.AddSeconds(3D);
				a.Save();
			}

			propertyEntryName = BXIBlockElement.GetCustomFieldsKey(iblockId);
			sectionPropertyEntryName = BXIBlockSection.GetCustomFieldsKey(iblockId);
			cfl.EntityId = propertyEntryName;
			cfls.EntityId = sectionPropertyEntryName;

			try
			{
				cfl.Save();
				cfls.Save();
			}
			catch (Exception ex)
			{
				errorMassage.AddErrorMessage(ex.Message);
				throw;
			}
			return true;
		}
		catch
		{
			return false;
		}
	}

	protected BXFile SaveFile()
	{
		BXFile fImage = null;

		if (Img.Upload.HasFile)
		{
			fImage = new BXFile(Img.Upload.PostedFile, "iblock", "iblock", null);
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
					fError += GetMessage("Error.InvalidExtension");
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

	private bool CreateIBlock()
	{
		try
		{
			if (!currentUserCanModifyIBlock)
				throw new Exception(GetMessageRaw("Exception.YouDontHaveRightsToCreateNewTask"));

			BXFile fImage = SaveFile();

			List<string> sitesList = new List<string>();
			foreach (ListItem item in cblSites.Items)
				if (item.Selected)
					sitesList.Add(item.Value);

			int sort;
			int.TryParse(tbSort.Text, out sort);

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

			iblock = new BXIBlock(typeId, tbName.Text);
			iblock.Code = tbCode.Text;
			iblock.Active = cbActive.Checked;
			iblock.Sort = sort > 0 ? sort : 500;
			iblock.ImageId = (fImage != null) ? fImage.Id : 0;
			iblock.DescriptionType = descrType;
			iblock.Description = descr;
			iblock.XmlId = tbXmlId.Text;
			//iblock.SectionName = tbSectionName.Text;
			//iblock.SectionsName = tbSectionsName.Text;
			//iblock.ElementName = tbElementName.Text;
			//iblock.ElementsName = tbElementsName.Text;

			iblock.CaptionsInfo.SectionName = tbSectionName.Text;
			iblock.CaptionsInfo.ElementName = tbElementName.Text;
			iblock.CaptionsInfo.SectionsName = tbSectionsName.Text;
			iblock.CaptionsInfo.ElementsName = tbElementsName.Text;

			iblock.CaptionsInfo.AddElement = tbAddElement.Text;
			iblock.CaptionsInfo.ChangeElement = tbChangeElement.Text;
			iblock.CaptionsInfo.DeleteElement = tbDeleteElement.Text;
			iblock.CaptionsInfo.AddSection = tbAddSection.Text;
			iblock.CaptionsInfo.ChangeSection = tbChangeSection.Text;
			iblock.CaptionsInfo.DeleteSection = tbDeleteSection.Text;

			iblock.CaptionsInfo.NewElement = tbNewElement.Text;
			iblock.CaptionsInfo.ElementList = tbElementList.Text;
			iblock.CaptionsInfo.ModifyingElement = tbModifyingElement.Text;
			iblock.CaptionsInfo.NewSection = tbNewSection.Text;
			iblock.CaptionsInfo.SectionList = tbSectionList.Text;
			iblock.CaptionsInfo.ModifyingSection = tbModifyingSection.Text;

			iblock.IndexContent = BXModuleManager.IsModuleInstalled("search") ? cbIndexContent.Checked : true;
			iblock.SetSiteIds(sitesList.ToArray());
			iblock.Create();
			
			if (iblock == null)
				throw new Exception(GetMessageRaw("Exception.BlockCreationFailed"));

			SaveAccessState();

			iblockId = iblock.Id;
			hfIBlockId.Value = iblockId.ToString();
			typeId = iblock.TypeId;
			hfTypeId.Value = typeId.ToString();

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

	private bool UpdateIBlock()
	{
		try
		{
			if (!currentUserCanModifyIBlock)
				throw new Exception(GetMessageRaw("Exception.YouDontHaveRightsToModifyBlock"));

			iblock.Active = cbActive.Checked;
			iblock.Code = tbCode.Text;

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

			iblock.Description = descr;
			iblock.DescriptionType = descrType;
			//iblock.ElementName = tbElementName.Text;
			//iblock.CaptionsInfo.ElementName = tbElementName.Text;
			//iblock.ElementsName = tbElementsName.Text;
			iblock.Name = tbName.Text;
			//iblock.SectionName = tbSectionName.Text;
			//iblock.SectionsName = tbSectionsName.Text;

			iblock.CaptionsInfo.SectionName = tbSectionName.Text;
			iblock.CaptionsInfo.ElementName = tbElementName.Text;
			iblock.CaptionsInfo.SectionsName = tbSectionsName.Text;
			iblock.CaptionsInfo.ElementsName = tbElementsName.Text;

			iblock.CaptionsInfo.AddElement = tbAddElement.Text;
			iblock.CaptionsInfo.ChangeElement = tbChangeElement.Text;
			iblock.CaptionsInfo.DeleteElement = tbDeleteElement.Text;
			iblock.CaptionsInfo.AddSection = tbAddSection.Text;
			iblock.CaptionsInfo.ChangeSection = tbChangeSection.Text;
			iblock.CaptionsInfo.DeleteSection = tbDeleteSection.Text;

			iblock.CaptionsInfo.NewElement = tbNewElement.Text;
			iblock.CaptionsInfo.ElementList = tbElementList.Text;
			iblock.CaptionsInfo.ModifyingElement = tbModifyingElement.Text;
			iblock.CaptionsInfo.NewSection = tbNewSection.Text;
			iblock.CaptionsInfo.SectionList = tbSectionList.Text;
			iblock.CaptionsInfo.ModifyingSection = tbModifyingSection.Text;

			int sort;
			int.TryParse(tbSort.Text, out sort);
			iblock.Sort = (sort > 0 ? sort : 500);
			iblock.XmlId = tbXmlId.Text;
			iblock.IndexContent = BXModuleManager.IsModuleInstalled("search") ? cbIndexContent.Checked : true;

			if (Img.DeleteFile)
				BXFile.Delete(iblock.ImageId);

			BXFile fImage = SaveFile();
			iblock.ImageId = ((fImage != null) ? fImage.Id : (Img.DeleteFile ? 0 : iblock.ImageId));

			List<string> sitesList = new List<string>();
			foreach (ListItem item in cblSites.Items)
				if (item.Selected)
					sitesList.Add(item.Value);
			iblock.SetSiteIds(sitesList.ToArray());

			iblock.Update();

			BXRoleManager.RemoveRoleFromTasks("iblock", iblock.Id.ToString());
			BXRoleManager.RemoveRoleFromOperations("iblock", iblock.Id.ToString());


			SaveAccessState();

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

	protected void Page_Init(object sender, EventArgs e)
	{
		//AccessEdit.FillStandardRoles(false);

		//Context.Items["supa_editor"] = bxweDescription;
		hfIBlockId.Value = Request.Form[hfIBlockId.UniqueID];
		iblockId = GetRequestInt("id");
		if (iblockId > 0)
			hfIBlockId.Value = iblockId.ToString();
		Int32.TryParse(hfIBlockId.Value, out iblockId);
		if (iblockId > 0)
		{
			iblock = BXIBlock.GetById(iblockId, BXTextEncoder.EmptyTextEncoder);
			if (iblock == null)
			{
				iblockId = 0;
				hfIBlockId.Value = "0";
			}
		}

		if (iblockId > 0)
		{
			propertyEntryName = BXIBlockElement.GetCustomFieldsKey(iblockId);
			sectionPropertyEntryName = BXIBlockSection.GetCustomFieldsKey(iblockId);
			typeId = iblock.TypeId;
			hfTypeId.Value = typeId.ToString();
		}
		else
		{
			propertyEntryName = BXIBlockElement.GetCustomFieldsKey(0);
			sectionPropertyEntryName = BXIBlockSection.GetCustomFieldsKey(0);
			typeId = base.GetRequestInt("type_id");
			if (typeId > 0)
				hfTypeId.Value = typeId.ToString();
			Int32.TryParse(hfTypeId.Value, out typeId);
			BXInfoBlockTypeOld type = null;
			if (typeId > 0)
			{
				type = BXInfoBlockTypeManagerOld.GetById(typeId);
				if (type == null)
				{
					typeId = 0;
					hfTypeId.Value = typeId.ToString();
				}
			}

			if (typeId <= 0)
			{
				Page.Response.Redirect(String.Format("IBlockAdmin.aspx?type_id={0}", typeId));
			}
		}

		if (!this.BXUser.IsCanOperate(BXIBlock.Operations.IBlockAdminRead, "iblock", iblockId))
			BXAuthentication.AuthenticationRequired();

		if (iblockId > 0)
			currentUserCanModifyIBlock = this.BXUser.IsCanOperate(BXIBlock.Operations.IBlockManageAdmin, "iblock", iblockId);
		else
			currentUserCanModifyIBlock = this.BXUser.IsCanOperate(BXIBlock.Operations.IBlockManageAdmin);

		cfl.AllowDelete = currentUserCanModifyIBlock;
		cfls.AllowDelete = currentUserCanModifyIBlock;

		PrepareForInsertScript();

		PrepareAccessState();

		cfl.DialogPlaceholder = Dialogs;
		cfls.DialogPlaceholder = Dialogs;
	}

	void PrepareForInsertScript()
	{
		if (!ClientScript.IsClientScriptBlockRegistered(GetType(), "InsertScript"))
		{
			StringBuilder sscript = new StringBuilder();

			sscript.AppendLine("function CheckSites(oSrc, args)");
			sscript.AppendLine("{");
			sscript.AppendLine("	var result = false;");
			sscript.AppendLine("	var i = 0;");
			sscript.AppendLine(String.Format("	var obj = document.getElementById('{0}_' + i);", cblSites.ClientID));
			sscript.AppendLine("	while (obj)");
			sscript.AppendLine("	{");
			sscript.AppendLine("		if (obj.checked)");
			sscript.AppendLine("		{");
			sscript.AppendLine("			result = true;");
			sscript.AppendLine("			break;");
			sscript.AppendLine("		}");
			sscript.AppendLine("		i++;");
			sscript.AppendLine(String.Format("		var obj = document.getElementById('{0}_' + i);", cblSites.ClientID));
			sscript.AppendLine("	}");
			sscript.AppendLine("	args.IsValid = result;");
			sscript.AppendLine("}");
			sscript.AppendLine("");
			sscript.AppendLine("function ShowRolesPerms(val)");
			sscript.AppendLine("{");
			sscript.AppendLine(String.Format("	document.getElementById(\"{0}_trRolesPerms_0_\" + val).className = \"heading\";", BXTabControlTab3.ClientID));
			//sscript.AppendLine(String.Format("	document.getElementById(\"{0}_trRolesPerms_1_\" + val).style[\"display\"] = \"block\";", BXTabControlTab3.ClientID));
			sscript.AppendLine(String.Format("	document.getElementById(\"{0}_trRolesPerms_2_\" + val).style[\"display\"] = \"block\";", BXTabControlTab3.ClientID));
			sscript.AppendLine(String.Format("	document.getElementById(\"{0}_trRolesPerms_3_\" + val).style[\"display\"] = \"block\";", BXTabControlTab3.ClientID));
			sscript.AppendLine("}");
			sscript.AppendLine("");
			sscript.AppendLine("function HideRolesPerms(val)");
			sscript.AppendLine("{");
			sscript.AppendLine(String.Format("	document.getElementById(\"{0}_trRolesPerms_0_\" + val).className = \"field-name\";", BXTabControlTab3.ClientID));
			//sscript.AppendLine(String.Format("	document.getElementById(\"{0}_trRolesPerms_1_\" + val).style[\"display\"] = \"none\";", BXTabControlTab3.ClientID));
			sscript.AppendLine(String.Format("	document.getElementById(\"{0}_trRolesPerms_2_\" + val).style[\"display\"] = \"none\";", BXTabControlTab3.ClientID));
			sscript.AppendLine(String.Format("	document.getElementById(\"{0}_trRolesPerms_3_\" + val).style[\"display\"] = \"none\";", BXTabControlTab3.ClientID));
			sscript.AppendLine("}");

			ClientScript.RegisterClientScriptBlock(GetType(), "InsertScript", sscript.ToString(), true);
		}
	}

	protected void cvSites_ServerValidate(object source, ServerValidateEventArgs args)
	{
		args.IsValid = false;
		foreach (ListItem item in cblSites.Items)
		{
			if (item.Selected)
			{
				args.IsValid = true;
				break;
			}
		}
	}

	protected void BXContextMenuToolbar1_CommandClick(object sender, CommandEventArgs e)
	{
		switch (e.CommandName)
		{
			case "DeleteIBlock":
				try
				{
					if (!currentUserCanModifyIBlock)
						throw new Exception(GetMessageRaw("Exception.YouDontHaveRightsToDeleteThisRecord"));

					if (iblockId > 0)
					{
						if (!BXInfoBlockManagerOld.Delete(iblockId))
						{
							throw new Exception(GetMessageRaw("Exception.BlockDeletionFailed"));
						}
					}
					else
					{
						throw new Exception(GetMessageRaw("Exception.BlockIsNotFound"));
					}

					Page.Response.Redirect(String.Format("IBlockAdmin.aspx?type_id={0}", typeId));
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
