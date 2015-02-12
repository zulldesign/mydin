using System.Web;
using Bitrix.UI;
using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using Bitrix.Components;
using System.Collections.Generic;
using Bitrix.Services.Text;
using Bitrix.DataLayer;
using Bitrix.Services;
using Bitrix.DataTypes;
using Bitrix.IO;
using System.Text;
using System.Collections.Specialized;
using Bitrix.Security;
using Bitrix.Components.Editor;
using System.IO;
using Bitrix.Configuration;
using System.Web.Hosting;

namespace Bitrix.IBlock.Components
{
	public partial class IBlockElementWebFormComponent : BXComponent
	{
		public enum UserElementAssociation
		{
			NoBody = 1,
			CreatedBy,
			IBlockProperty,
		}

		public enum ElementActiveForEdit
		{
			Active = 1,
			NotActive,
			Always
		}

		private BXIBlockElement element;
		public BXIBlockElement Element
		{
			get { return this.element; }
			set { this.element = value; }
		}

		public int ElementId
		{
			get { return Parameters.GetInt("ElementId", 0); }
		}

		private BXIBlock iblock;
		public BXIBlock IBlock
		{
			get { return iblock; }
		}

		private int iblockId = 0;
		public int IBlockId
		{
			get
			{
				return iblockId;
			}
		}

		public int TextBoxSize
		{
			get { return Parameters.GetInt("TextBoxSize", 30); }
		}

		public bool SendEmailAfterCreate
		{
			get
			{
				return Parameters.GetBool("SendEmailAfterCreate", false);
			}
		}

		public string EmailMessageTemplate
		{
			get
			{
				return Parameters.GetString("EmailMessageTemplate", "");
			}
		}

		public string EmailTo
		{
			get
			{
				return Parameters.GetString("EmailTo");
			}
		}

		public string EmailSubject
		{
			get
			{
				return Parameters.GetString("EmailSubject");
			}
		}
		/// <summary>
		/// Способ определения пользователя
		/// </summary>
		public enum MannerOfUserIdentification
		{
			/// <summary>
			/// Текущий
			/// </summary>
			Current = 1,
			/// <summary>
			/// Указанный в параметре CustomUserId
			/// </summary>
			Custom
		}

		private UserElementAssociation? bindingBy = null;
		public UserElementAssociation BindingBy
		{
			get
			{
				return (bindingBy ?? (bindingBy = Parameters.GetEnum<UserElementAssociation>("MannerOfUserAssociation", UserElementAssociation.CreatedBy))).Value;
			}
		}

		private ElementActiveForEdit? elementStatusForEdit = null;
		public ElementActiveForEdit ElementStatusForEdit
		{
			get
			{
				return (elementStatusForEdit ?? (elementStatusForEdit = Parameters.GetEnum<ElementActiveForEdit>("MannerOfIssueModificationPermission", ElementActiveForEdit.Always))).Value;
			}
		}

		private ElementActiveForEdit? elementStatusAfterSave = null;
		public ElementActiveForEdit ElementStatusAfterSave
		{
			get
			{
				return (elementStatusAfterSave ?? (elementStatusAfterSave = Parameters.GetEnum<ElementActiveForEdit>("ElementActiveAfterSave", ElementActiveForEdit.Active))).Value;
			}
		}

		private BXPrincipal currentPricipal = null;
		public BXPrincipal CurrentPricipal
		{
			get
			{
				return currentPricipal ?? (currentPricipal = Context.User as BXPrincipal);
			}
		}

		private string[] currentPricipalRoles = null;
		public string[] CurrentPricipalRoles
		{
			get { return currentPricipalRoles ?? (currentPricipalRoles = CurrentPricipal != null ? CurrentPricipal.GetAllRoles() : new string[0]); }
		}


		private int? currentUserId = null;
		public int CurrentUserId
		{
			get { return currentUserId ?? (currentUserId = CurrentPricipal != null && CurrentPricipal.Identity.IsAuthenticated ? ((BXIdentity)CurrentPricipal.Identity).Id : 0).Value; }
		}

		public List<string> EditFields
		{
			get { return Parameters.GetListString("EditFields"); }
		}

		public List<string> RequiredFields
		{
			get { return Parameters.GetListString("RequiredFields"); }
		}

		private Dictionary<string, ElementField> elementFields = new Dictionary<string, ElementField>();
		public Dictionary<string, ElementField> ElementFields
		{
			get { return elementFields; }
			set { elementFields = value; }
		}

		/// <summary>
		/// Включить управление периодом активации (только в режиме создания)
		/// </summary>
		public bool EnableActivationPeriodProcessing
		{
			get
			{
				return Parameters.GetBool("EnableActivationPeriodProcessing", false);
			}
			set
			{
				Parameters["EnableActivationPeriodProcessing"] = value.ToString();
			}
		}

		public int DaysBeforeActivationPeriodStart
		{
			get
			{
				return Parameters.GetInt("DaysBeforeActivationPeriodStart", 0);
			}
			set
			{
				Parameters["DaysBeforeActivationPeriodStart"] = value > 0 ? value.ToString() : "0";
			}
		}

		public int ActivationPeriodLengthInDays
		{
			get
			{
				return Parameters.GetInt("ActivationPeriodLengthInDays", 0);
			}
			set
			{
				Parameters["ActivationPeriodLengthInDays"] = value > 0 ? value.ToString() : "0";
			}
		}

		private IList<string> _rolesAuthorizedToManage = null;
		/// <summary>
		/// Группы пользователей, имеющие право на добавление/редактирование своих элементов
		/// </summary>
		public IList<string> RolesAuthorizedToManage
		{
			get { return _rolesAuthorizedToManage ?? (_rolesAuthorizedToManage = Parameters.GetListString("RolesAuthorizedToManage")); }
		}

		private IList<string> _rolesAuthorizedToAdminister = null;
		/// <summary>
		/// Группы пользователей, имеющие право на добавление/редактирование чужих элементов
		/// </summary>
		public IList<string> RolesAuthorizedToAdminister
		{
			get { return _rolesAuthorizedToAdminister ?? (_rolesAuthorizedToAdminister = Parameters.GetListString("RolesAuthorizedToAdminister")); }
		}

		public string errorMessage = String.Empty;

		private List<string> summaryErrors = new List<string>();
		public List<string> SummaryErrors
		{
			get { return summaryErrors; }
			set { summaryErrors = value; }
		}

		private bool isPermissionDenied = false;
		public bool IsPermissionDenied
		{
			get { return isPermissionDenied; }
		}

		private bool isSavingElementSuccess = false;
		public bool IsSavingElementSuccess
		{
			get { return isSavingElementSuccess; }
		}

		public string SuccessMessageAfterCreateElement
		{
			get
			{
				return Parameters.GetString("SuccessMessageAfterCreateElement", GetMessage("ElementSuccessCreated"));
			}
		}

		public string SuccessMessageAfterUpdateElement
		{
			get
			{
				return Parameters.GetString("SuccessMessageAfterUpdateElement", GetMessage("ElementSuccessUpdated"));
			}
		}

		public MannerOfUserIdentification UserIdentification
		{
			get
			{
				object obj = null;
				if (ComponentCache.TryGetValue("MannerOfUserIdentification", out obj))
					return (MannerOfUserIdentification)obj;

				MannerOfUserIdentification r = Parameters.GetEnum<MannerOfUserIdentification>("MannerOfUserIdentification", MannerOfUserIdentification.Current);
				ComponentCache["MannerOfUserIdentification"] = r;
				return r;
			}
		}

		public int CustomUserId
		{
			get
			{
				object obj = null;
				if (ComponentCache.TryGetValue("CustomUserId", out obj))
					return (int)obj;

				int r = Parameters.GetInt("CustomUserId", 0);
				ComponentCache["CustomUserId"] = r;
				return r;
			}
		}

		public string GetFieldCustomTitle(string fieldID, string defaultTitle)
		{
			string result = Parameters.GetString(fieldID + "CustomTitle");
			return !String.IsNullOrEmpty(result) ? HttpUtility.HtmlEncode(result) : defaultTitle;
		}

		private Behaviour _internalBehaviour = null;
		private Behaviour InternalBehaviour
		{
			get
			{
				if (_internalBehaviour != null)
					return _internalBehaviour;

				if (UserIdentification == MannerOfUserIdentification.Custom && CustomUserId > 0)
				{
					if (RolesAuthorizedToAdminister.Count > 0)
						return (_internalBehaviour = new AdministerBehaviour(this));
					else if (CurrentUserId == CustomUserId)
						return (_internalBehaviour = new StandardBehaviour(this));
					else
						return (_internalBehaviour = new AccessDeniedBehaviour(this));
				}
				return (_internalBehaviour = new StandardBehaviour(this));
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if (!GetIBlock(Parameters.Get("IBlockId", 0)) || (isPermissionDenied = (!InternalBehaviour.IsCurrentUserPermitted)))
			{
				IncludeComponentTemplate();
				return;
			}

			if (ElementId > 0)
			{
				BXFilter elementFilter = GetElementFilter();
				BXIBlockElementCollection elementCollection = BXIBlockElement.GetList(
					elementFilter,
					null,
					new BXSelectAdd(BXIBlockElement.Fields.CustomFields[iblockId]),
					null,
					BXTextEncoder.EmptyTextEncoder
					);
				if (elementCollection == null || elementCollection.Count == 0)
				{
					errorMessage = GetMessage("ElementNotFound");
					IncludeComponentTemplate();
					return;
				}
				element = elementCollection[0];
			}
			else if (InternalBehaviour.EnableMaxUserElementsCheck)
			{
				int maxElements = Parameters.GetInt("MaxUserElements", 0);
				if (maxElements > 0)
				{
					BXFilter elementFilter = GetElementFilter();
					BXIBlockElementCollection elementCollection = BXIBlockElement.GetList(
						elementFilter,
						null,
						new BXSelect(BXSelectFieldPreparationMode.Normal, BXIBlockElement.Fields.ID),
						null
						);
					if (elementCollection.Count >= maxElements)
					{
						errorMessage = String.Format(GetMessage("MaxUserElementsError"), maxElements);
						IncludeComponentTemplate();
						return;
					}
				}
			}

			IncludeComponentTemplate();

			bool fileFieldExistsInForm = false;
			foreach (string fieldId in EditFields)
			{
				if (fieldId.StartsWith("PROPERTY_", StringComparison.OrdinalIgnoreCase))
				{
					string propertyId = fieldId.Substring(9);
					if (!iblock.CustomFields.ContainsKey(propertyId))
						continue;

					BXCustomField field = iblock.CustomFields[propertyId];
					CustomFieldEditor publicEditor = null;

					string fieldName = "PROPERTY_" + field.CorrectedName;
					string formFieldName = ID + fieldName;
					string fieldTitle = field.EditFormLabel;

					EventHandler<CustomTypePublicEditorEventArgs> handler = CreateCustomTypePublicEditor;
					if (handler != null)
					{
						CustomTypePublicEditorEventArgs publicEditorArgs = new CustomTypePublicEditorEventArgs();
						publicEditorArgs.CustomField = field;
						publicEditorArgs.ID = propertyId;
						publicEditorArgs.FormFieldName = formFieldName;
						publicEditorArgs.Title = fieldTitle;
						handler(this, publicEditorArgs);

						if (publicEditorArgs != null)
						{
							if (publicEditorArgs.PublicEditor != null)
								publicEditor = new CustomFieldEditor(field, publicEditorArgs.PublicEditor);

							formFieldName = publicEditorArgs.FormFieldName;
							fieldTitle = publicEditorArgs.Title;
						}
					}

					if (publicEditor == null)
						publicEditor = new CustomFieldEditor(field);
					publicEditor.Load(element, new BXParamsBag<object>());

					//Create element field
					ElementField elementField = new ElementField();
					elementField.PublicEditor = publicEditor;
					elementField.Title = fieldTitle;
					elementField.FormFieldName = formFieldName;
					elementField.UniqueId = ClientID + fieldName;
					elementField.Required = field.Mandatory;
					elementField.CustomField = field;
					elementField.CustomType = BXCustomTypeManager.GetCustomType(field.CustomTypeId);
					elementFields.Add(fieldName, elementField);

					if (!fileFieldExistsInForm && elementField.CustomType.IsFile)
						fileFieldExistsInForm = true;
				}
				else
				{
					if (string.Equals(fieldId.ToUpperInvariant(), "ACTIVE", StringComparison.InvariantCulture) && !IsUserAutorizedToManageOfActivation())
						continue;
					InitStandardField(fieldId);
					if (fieldId.Equals("PreviewImage", StringComparison.OrdinalIgnoreCase) || fieldId.Equals("DetailImage", StringComparison.OrdinalIgnoreCase))
						fileFieldExistsInForm = true;
				}
			}

			HtmlForm form;
			if (Page != null && (form = Page.Form) != null && form.Enctype.Length == 0 && fileFieldExistsInForm)
				form.Enctype = "multipart/form-data";
		}

		private BXFilter GetElementFilter()
		{
			BXFilter elementFilter = new BXFilter(new BXFilterItem(BXIBlockElement.Fields.IBlock.ID, BXSqlFilterOperators.Equal, IBlockId));

			if (ElementId > 0)
				elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.ID, BXSqlFilterOperators.Equal, ElementId));

			if (InternalBehaviour.ElementEditIsPermitted != ElementActiveForEdit.Always)
			{
				elementFilter.Add(new BXFilterItem(
					BXIBlockElement.Fields.Active,
					BXSqlFilterOperators.Equal,
					InternalBehaviour.ElementEditIsPermitted == ElementActiveForEdit.Active ? "Y" : "N")
				);
			}

			if (BindingBy == UserElementAssociation.IBlockProperty)
			{
				string bindingProperty = Parameters.Get<string>("UserAssociatedByCustomIBlockProperty", String.Empty);
				if (!String.IsNullOrEmpty(bindingProperty) && iblock.CustomFields.ContainsKey(bindingProperty))
					elementFilter.Add(
						new BXFilterItem(
							BXIBlockElement.Fields.GetCustomField(iblockId, iblock.CustomFields[bindingProperty].Name),
							BXSqlFilterOperators.Equal,
							InternalBehaviour.ImpersonatedUserId
						)
					);
				else
					bindingBy = UserElementAssociation.CreatedBy;
			}

			if (BindingBy == UserElementAssociation.CreatedBy)
				elementFilter.Add(new BXFilterItem(BXIBlockElement.Fields.CreatedBy, BXSqlFilterOperators.Equal, InternalBehaviour.ImpersonatedUserId));

			return elementFilter;
		}

		private bool GetIBlock(int id)
		{
			bool success = false;
			if (id > 0)
			{
				iblock = BXIBlock.GetById(id, BXTextEncoder.HtmlTextEncoder);
				if (iblock != null)
				{
					success = true;
					iblockId = id;
				}
				else
					errorMessage = GetMessage("WrongIBlockCode");
			}
			else
				errorMessage = GetMessage("EmptyIBlockCode");

			return success;
		}

		/*
		private bool IsUserAutorizedToUseWebform()
		{
			if (CurrentPricipal == null || CurrentUserId <= 0)
				return false;

			List<string> UserRoles = Parameters.GetListString("RolesAuthorizedToManage");
			if (UserRoles.Count == 0)
				return IBlock != null ? IBlock.IsUserCanOperate(BXIBlock.Operations.IBlockModifyElements) : false;

			string[] userRoleCollection = CurrentPricipal.GetAllRoles();
			foreach (string userRole in userRoleCollection)
			{
				if (UserRoles.Contains(userRole))
					return true;
			}
			return false;
		}
		*/
		private bool? isUserAutorizedToManageOfActivation = null;
		private bool IsUserAutorizedToManageOfActivation()
		{
			if (isUserAutorizedToManageOfActivation.HasValue)
				return isUserAutorizedToManageOfActivation.Value;

			isUserAutorizedToManageOfActivation = false;
			List<string> authorizedRoles = Parameters.GetListString("RolesAuthorizedToManageOfActivation");
			if (authorizedRoles.Count > 0)
				foreach (string pricipalRole in CurrentPricipalRoles)
				{
					if (!authorizedRoles.Contains(pricipalRole))
						continue;
					isUserAutorizedToManageOfActivation = true;
					break;
				}
			return isUserAutorizedToManageOfActivation.Value;
		}


		private void InitStandardField(string fieldId)
		{
			BXParamsBag<object> settings = new BXParamsBag<object>();
			fieldId = fieldId.ToUpper();
			if (fieldId == "NAME")
			{
				elementFields.Add("Name", CreateElementField<ElementName>("Name", GetMessage("NameElementField"), settings));
			}
			else if (fieldId == "ACTIVE")
			{
				settings["fieldDefaultValue"] = ElementStatusAfterSave != ElementActiveForEdit.NotActive;
				elementFields.Add("Active", CreateElementField<ElementActive>("Active", GetMessage("ActiveElementField"), settings));
			}
			else if (fieldId == "SECTIONS")
			{
				settings["iblockId"] = IBlockId;
				settings["onlyLeafSelect"] = Parameters.GetBool("OnlyLeafSelect", false);
				settings["maxSectionSelect"] = Parameters.GetInt("MaxSectionSelect", 3);
				elementFields.Add("Sections", CreateElementField<ElementSections>("Sections", GetMessage("SectionsElementField"), settings));
			}
			else if (fieldId == "PREVIEWTEXT")
			{
				elementFields.Add("PreviewText", CreateElementField<ElementPreviewText>("PreviewText", GetMessage("PreviewTextElementField"), settings));
			}
			else if (fieldId == "DETAILTEXT")
			{
				elementFields.Add("DetailText", CreateElementField<ElementDetailText>("DetailText", GetMessage("DetailTextElementField"), settings));
			}
			else if (fieldId == "PREVIEWIMAGE")
			{
				settings["maxFileSizeUpload"] = Parameters.GetInt("MaxFileSizeUpload", 1024);
				elementFields.Add("PreviewImage", CreateElementField<ElementPreviewImage>("PreviewImage", GetMessage("PreviewPictureElementField"), settings));
			}
			else if (fieldId == "DETAILIMAGE")
			{
				settings["maxFileSizeUpload"] = Parameters.GetInt("MaxFileSizeUpload", 1024);
				elementFields.Add("DetailImage", CreateElementField<ElementDetailImage>("DetailImage", GetMessage("DetailPictureElementField"), settings));
			}
			else if (fieldId == "ACTIVEFROMDATE")
			{
				settings["showTime"] = Parameters.GetBool("ActiveFromDateShowTime", false);
				elementFields.Add("ActiveFromDate", CreateElementField<ElementActiveFromDate>("ActiveFromDate", GetMessage("ActiveFromElementField"), settings));
			}
			else if (fieldId == "ACTIVETODATE")
			{
				settings["showTime"] = Parameters.GetBool("ActiveToDateShowTime", false);
				elementFields.Add("ActiveToDate", CreateElementField<ElementActiveToDate>("ActiveToDate", GetMessage("ActiveToElementField"), settings));
			}
			else if (fieldId == "CAPTCHA" && CurrentUserId <= 0)
			{
				settings["required"] = true;
				elementFields.Add("Captcha", CreateElementField<Captcha>("Captcha", GetMessage("CaptchCodeElementField"), settings));
			}
		}

		private ElementField CreateElementField<TEditor>(string fieldName, string title, BXParamsBag<object> settings) where TEditor : FieldPublicEditor, new()
		{
			string fieldTitle = GetFieldCustomTitle(fieldName, title);
			bool required;
			if (settings.ContainsKey("required"))
				required = (bool)settings["required"];
			else
				required = RequiredFields.Contains(fieldName);

			settings["required"] = required;
			settings["fieldTitle"] = fieldTitle;
			settings["textBoxSize"] = TextBoxSize;

			string formFieldName = ID + fieldName;

			//Create public editor
			BXIBlockElementFieldPublicEditor editor = null;
			EventHandler<FieldPublicEditorEventArgs> handler = CreateFieldPublicEditor;
			if (handler != null)
			{
				FieldPublicEditorEventArgs publicEditorArgs = new FieldPublicEditorEventArgs();
				publicEditorArgs.Required = required;
				publicEditorArgs.ID = fieldName;
				publicEditorArgs.FormFieldName = formFieldName;
				publicEditorArgs.Title = fieldTitle;

				handler(this, publicEditorArgs);

				if (publicEditorArgs != null)
				{
					editor = publicEditorArgs.PublicEditor;
					required = publicEditorArgs.Required;
					fieldTitle = publicEditorArgs.Title;
					formFieldName = publicEditorArgs.FormFieldName;
				}
			}

			if (editor == null)
			{
				TEditor defaultEditor = new TEditor();
				defaultEditor.Component = this;
				editor = defaultEditor;
			}

			editor.Load(element, settings);

			//Create element field
			ElementField elementField = new ElementField();
			elementField.PublicEditor = editor;
			elementField.Title = fieldTitle;
			elementField.FormFieldName = formFieldName;
			elementField.UniqueId = ClientID + fieldName;
			elementField.Required = required;

			return elementField;
		}

		public void SaveIBlockElement(object sender, EventArgs e)
		{
			BXCustomPropertyCollection properties = new BXCustomPropertyCollection();
			BXIBlockElement iblockElement;

			if (Element != null)
			{
				iblockElement = Element;
				properties.Assign(Element.CustomValues);
			}
			else
				iblockElement = new BXIBlockElement(IBlockId, String.Empty);

			//Invove Validate
			bool success = true;
			foreach (KeyValuePair<string, IBlockElementWebFormComponent.ElementField> field in ElementFields)
			{
				List<string> validateErrors = new List<string>();
				if (string.Equals(field.Key.ToUpperInvariant(), "ACTIVE", StringComparison.InvariantCulture) && !IsUserAutorizedToManageOfActivation())
				{
					if (success)
						success = false;
					SummaryErrors.Add(GetMessage("ManagementOfActivityIsProhibitedError"));
				}

				if (!field.Value.PublicEditor.Validate(field.Value.FormFieldName, validateErrors))
				{
					if (success)
						success = false;
					field.Value.ValidateErrors = validateErrors;
					SummaryErrors.AddRange(validateErrors);
				}
			}

			if (success)
			{
				//Invoke Save
				foreach (KeyValuePair<string, IBlockElementWebFormComponent.ElementField> field in ElementFields)
					field.Value.PublicEditor.Save(field.Value.FormFieldName, iblockElement, properties);

				//Generate Name For Element
				string nameMacros = Parameters.Get("NameFieldMacros");
				if (!BXStringUtility.IsNullOrTrimEmpty(nameMacros) && !EditFields.Exists(delegate(string str) { return String.Equals("Name", str, StringComparison.OrdinalIgnoreCase); }))
					iblockElement.Name = GenerateNameMacros(nameMacros, iblockElement, properties);

				if (iblockElement.Name != null && iblockElement.Name.Length > 255)
					iblockElement.Name = iblockElement.Name.Substring(0, 255);

				//Update Element
				if (Element != null)
				{
					iblockElement.ModifiedBy = InternalBehaviour.ImpersonatedUserId;
					if (!IsUserAutorizedToManageOfActivation()) //автоустановка активности  
						iblockElement.Active = ElementStatusAfterSave != ElementActiveForEdit.NotActive;
					iblockElement.CustomValues.Assign(properties);
					try
					{
						if (BeforeSave != null)
						{
							var args = new BeforeSaveEventArgs(iblockElement);
							BeforeSave(this, args);
							if (args.errors != null && args.errors.Count > 0)
							{
								SummaryErrors.AddRange(args.errors);
								success = false;
							}
						}


						iblockElement.Update();
						//BXCustomEntityManager.SaveEntity(BXIBlockElement.GetCustomFieldsKey(IBlockId), Element.Id, properties);
						isSavingElementSuccess = true;
					}
					catch (Exception)
					{
						SummaryErrors.Add(GetMessage("ElementUpdateError"));
					}
				}
				else
				{
					//Save Element
					if (BindingBy == UserElementAssociation.IBlockProperty)
					{
						string bindingProperty = Parameters.Get<string>("UserAssociatedByCustomIBlockProperty", String.Empty);
						if (!String.IsNullOrEmpty(bindingProperty) && iblock.CustomFields.ContainsKey(bindingProperty))
						{
							BXCustomField field = iblock.CustomFields[bindingProperty];
							BXCustomType type = BXCustomTypeManager.GetCustomType(field.CustomTypeId);
							if (type != null)
								properties[field.Name] = new BXCustomProperty(field.Name, field.Id, type.DbType, field.Multiple, InternalBehaviour.ImpersonatedUserId);
						}
					}

					iblockElement.CreatedBy = InternalBehaviour.ImpersonatedUserId;
					if (!IsUserAutorizedToManageOfActivation()) //автоустановка активности                    
						iblockElement.Active = ElementStatusAfterSave != ElementActiveForEdit.NotActive;

					if (EnableActivationPeriodProcessing)
					{
						int daysBeforeActivationPeriodStart = DaysBeforeActivationPeriodStart;
						iblockElement.ActiveFromDate = daysBeforeActivationPeriodStart > 0 ? DateTime.Now.Date.AddDays(Convert.ToDouble(daysBeforeActivationPeriodStart)) : DateTime.Now;

						int activationPeriodLengthInDays = ActivationPeriodLengthInDays;
						if (activationPeriodLengthInDays > 0)
							iblockElement.ActiveToDate = iblockElement.ActiveFromDate.AddDays(Convert.ToDouble(activationPeriodLengthInDays));
					}
					iblockElement.CustomValues.Assign(properties);

					try
					{
						if (BeforeSave != null)
						{
							var args = new BeforeSaveEventArgs(iblockElement);
							BeforeSave(this, args);
							if (args.errors != null && args.errors.Count > 0)
							{
								SummaryErrors.AddRange(args.errors);
								success = false;
							}
						}

						if (success)
						{
							iblockElement.Save();
							//BXCustomEntityManager.SaveEntity(BXIBlockElement.GetCustomFieldsKey(IBlockId), iblockElement.Id, properties);
							isSavingElementSuccess = true;
						}
					}
					catch (Exception)
					{
						SummaryErrors.Add(GetMessage("ElementCreateError"));
					}

					if (isSavingElementSuccess && SendEmailAfterCreate && !String.IsNullOrEmpty(EmailTo))
					{
						SendEmail(iblockElement, properties);
					}
				}

				if (AfterSave != null)
					AfterSave(this, new AfterSaveEventArgs(iblockElement, properties));



				//Redirect
				string redirectPage = Parameters.GetString("RedirectPageUrl", String.Empty);
				if (isSavingElementSuccess && !BXStringUtility.IsNullOrTrimEmpty(redirectPage))
				{
					if (redirectPage.StartsWith("~/"))
						redirectPage = ResolveUrl(redirectPage);

					Response.Redirect(redirectPage);
				}
			}
		}

		private string GetCustomPropertyString(BXCustomProperty property, BXCustomField field, string separator)
		{
			BXCustomTypePublicView view = BXCustomTypeManager.GetCustomType(field.CustomTypeId).CreatePublicView();
			view.Init(property, field);
			return view.GetHtml(separator);
		}

		void SendEmail(BXIBlockElement element, BXCustomPropertyCollection properties)
		{
			// все свойства элемента и все его набираемые свойства

			BXMailerMessageData data = new BXMailerMessageData();
			data.Parameters["Active"] = element.Active ? GetMessage("Yes") : GetMessage("No");
			data.Parameters["ActiveFromDate"] = element.ActiveFromDate.ToString();
			data.Parameters["ActiveToDate"] = element.ActiveToDate.ToString();

			data.Parameters["Code"] = element.Code;
			data.Parameters["CreateDate"] = element.CreateDate.ToString();

			if (element.CreatedBy > 0 && element.CreatedByUser != null)
				data.Parameters["CreatedByUser"] = element.CreatedByUser.GetDisplayName();
			else
				data.Parameters["CreatedByUser"] = string.Empty;

			data.Parameters["Tags"] = element.Tags;
			data.Parameters["Id"] = element.Id.ToString();
			data.Parameters["IBlockId"] = element.IBlockId.ToString();
			data.Parameters["Name"] = element.Name;
			data.Parameters["ViewsCount"] = element.ViewsCount.ToString();
			data.Parameters["XmlId"] = element.XmlId;
			data.Parameters["DetailText"] = BXStringUtility.HtmlToText(element.DetailText);
			data.Parameters["IBlockName"] = element.IBlock.Name;
			data.Parameters["IBlockId"] = element.IBlockId.ToString();

			if (element.DetailImageId > 0 && element.DetailImage != null)
				data.Parameters["DetailImageUrl"] = BXPath.ToUri(element.DetailImage.FilePath, true);
			else
				data.Parameters["DetailImageUrl"] = string.Empty;

			if (element.PreviewImageId > 0 && element.PreviewImage != null)
				data.Parameters["PreviewImageUrl"] = BXPath.ToUri(element.PreviewImage.FilePath, true);
			else
				data.Parameters["PreviewImageUrl"] = string.Empty;

			data.Parameters["Name"] = element.Name;

			if (element.ModifiedBy > 0 && element.ModifiedByUser != null)
				data.Parameters["ModifiedByUser"] = element.ModifiedByUser.GetDisplayName();
			else
				data.Parameters["ModifiedByUser"] = string.Empty;

			data.Parameters["PreviewText"] = BXStringUtility.HtmlToText(element.PreviewText);

			foreach (BXCustomProperty prop in properties.Values)
				if (!prop.IsFile)
					data.Parameters["Property_" + prop.Name] = prop.ToString();
				else if (!prop.IsMultiple)
				{
					if (prop.Value == null)
					{
						data.Parameters["Property_" + prop.Name] = string.Empty;
						continue;
					}
					BXFile file = BXFile.GetById(prop.Value);
					if (file != null)
						data.Parameters["Property_" + prop.Name] = BXPath.ToUri(file.FilePath, true);
					else
						data.Parameters["Property_" + prop.Name] = string.Empty;
				}
				else
				{
					StringBuilder sb = new StringBuilder();
					for (int i = 0; i < prop.Values.Count; i++)
					{
						if (prop.Values[i] == null) continue;
						BXFile f = BXFile.GetById(prop.Values[i]);
						if (f != null)
						{
							sb.Append(BXPath.ToUri(f.FilePath, true));
							if (i != prop.Values.Count - 1)
								sb.AppendLine(",");
						}

					}
					data.Parameters["Property_" + prop.Name] = sb.ToString();
				}

			data.Site = BXSite.Current.Id;
			data.Parameters.Add("CurrentUserName", CurrentUserId > 0 ? BXUser.GetById(CurrentUserId).GetDisplayName() : string.Empty);
			BXMailerTemplate template = new BXMailerTemplate();
			template.Message = EmailMessageTemplate;
			template.BodyType = string.Equals(Parameters.GetString("EmailBodyType"), "html", StringComparison.OrdinalIgnoreCase) ? BXMailerTemplateBodyType.Html : BXMailerTemplateBodyType.Text;
			template.EmailTo = EmailTo;
			template.EmailFrom = "#DEFAULT_EMAIL_FROM#";
			if (!String.IsNullOrEmpty(EmailSubject))
				template.Subject = EmailSubject;

			BXMailer.SendMessageQueue(template, data);

		}

		private string GenerateNameMacros(string nameMacros, BXIBlockElement iblockElement, BXCustomPropertyCollection properties)
		{
			if (String.IsNullOrEmpty(nameMacros) || iblockElement == null)
				return String.Empty;

			BXParamsBag<object> replaceItems = new BXParamsBag<object>();
			replaceItems.Add("DateCreate", DateTime.Now.ToString());
			replaceItems.Add("ActiveFromDate", iblockElement.ActiveFromDate != DateTime.MinValue ? iblockElement.ActiveFromDate.ToString() : String.Empty);
			replaceItems.Add("ActiveToDate", iblockElement.ActiveToDate != DateTime.MinValue ? iblockElement.ActiveToDate.ToString() : String.Empty);
			replaceItems.Add("PreviewText", iblockElement.PreviewText ?? String.Empty);
			replaceItems.Add("DetailText", iblockElement.DetailText ?? String.Empty);
			replaceItems.Add("Code", iblockElement.Code ?? String.Empty);
			replaceItems.Add("XmlId", iblockElement.XmlId ?? String.Empty);
			replaceItems.Add("Sort", iblockElement.Sort.ToString());

			string createdBy = String.Empty;
			if (InternalBehaviour.ImpersonatedUserId > 0)
			{
				BXUser user = BXUserManager.GetById(InternalBehaviour.ImpersonatedUserId, false);
				if (user != null)
					createdBy = String.Concat(user.FirstName, " ", user.LastName, " [", user.UserName, "]");
			}
			replaceItems.Add("CreatedBy", createdBy);

			if (properties != null)
			{
				foreach (KeyValuePair<string, BXCustomProperty> property in properties)
					replaceItems.Add("PROPERTY_" + property.Key, property.Value.Value ?? String.Empty);
			}

			return BXComponentManager.MakeLink(nameMacros, replaceItems);
		}

		protected override void PreLoadComponentDefinition()
		{
			Title = GetMessageRaw("Title");
			Description = GetMessageRaw("Description");
			Icon = "images/icon.gif";
			Group = new BXComponentGroup("iblock.elements", GetMessageRaw("Group"), 100, BXComponentGroup.Content);

			BXCategory mainCategory = BXCategory.Main;
			BXCategory accessCategory = new BXCategory(GetMessageRaw("ParametersAccessCategory"), "AccessCategory", 150);
			BXCategory fieldSettingsCategory = new BXCategory(GetMessageRaw("ParametersFieldSettingsCategory"), "FieldSettingsCategory", 160);
			BXCategory customNameCategory = new BXCategory(GetMessageRaw("ParametersCustomNameCategory"), "CustomNameCategory", 270);
			BXCategory addSettingsCategory = BXCategory.AdditionalSettings;
			BXCategory emailCategory = new BXCategory(GetMessageRaw("ParametersEmailCategory"), "EmailCategory", 220);


			#region Main Settings
			ParamsDefinition.Add(
				"IBlockTypeId",
				new BXParamSingleSelection(
					GetMessageRaw("InfoBlockType"),
					String.Empty,
					mainCategory
				)
			);

			ParamsDefinition.Add(
				"IBlockId",
				new BXParamSingleSelection(
					GetMessageRaw("InfoBlockCode"),
					String.Empty,
					mainCategory
				)
			);

			ParamsDefinition.Add(
				"ElementId",
				new BXParamText(
					GetMessageRaw("EditElementId"),
					"0",
					BXCategory.Main
				)
			);

			ParamsDefinition.Add(
				"EditFields",
				new BXParamDoubleList(
					GetMessageRaw("EditFields"),
					"Name",
					mainCategory
				)
			);

			ParamsDefinition.Add(
				"RequiredFields",
				new BXParamMultiSelection(
					GetMessageRaw("RequiredFields"),
					"",
					mainCategory
				)
			);

			ParamsDefinition.Add(
				"EnableActivationPeriodProcessing",
				new BXParamYesNo(
					GetMessageRaw("EnableActivationPeriodProcessing"),
					false,
					mainCategory,
					new ParamClientSideActionGroupViewSwitch(ClientID, "EnableActivationPeriodProcessing", "ActivityManagement", string.Empty)
				)
			);

			ParamsDefinition.Add(
				"DaysBeforeActivationPeriodStart",
				new BXParamText(
					GetMessageRaw("DaysBeforeActivationPeriodStart"),
					"0",
					mainCategory,
					new ParamClientSideActionGroupViewMember(ClientID, "DaysBeforeActivationPeriodStart", new string[] { "ActivityManagement" })
				)
			);

			ParamsDefinition.Add(
				"ActivationPeriodLengthInDays",
				new BXParamText(
					GetMessageRaw("ActivationPeriodLengthInDays"),
					"0",
					mainCategory,
					new ParamClientSideActionGroupViewMember(ClientID, "ActivationPeriodLengthInDays", new string[] { "ActivityManagement" })
				)
			);
			#endregion

			#region Access Settings
			ParamsDefinition.Add(
				"MannerOfUserIdentification",
				new BXParamSingleSelection(
					GetMessageRaw("MannerOfUserIdentification"),
					MannerOfUserIdentification.Current.ToString("G"),
					accessCategory,
					null,
					new ParamClientSideActionGroupViewSelector(ClientID, "MannerOfUserIdentification")
				));

			ParamsDefinition.Add(
				"CustomUserId",
				new BXParamText(
					GetMessageRaw("CustomUserId"),
					"0",
					accessCategory,
				new ParamClientSideActionGroupViewMember(ClientID, "CustomUserId", new string[] { MannerOfUserIdentification.Custom.ToString("G") })
				));
			//управление своими записями
			ParamsDefinition.Add(
				"RolesAuthorizedToManage",
				new BXParamMultiSelection(
					GetMessageRaw("RolesAuthorizedToManage"),
					"Admin",
					accessCategory
				));
			//управление чужими записями
			ParamsDefinition.Add(
				"RolesAuthorizedToAdminister",
				new BXParamMultiSelection(
					GetMessageRaw("RolesAuthorizedToAdminister"),
					"Admin",
					accessCategory
				));
			ParamsDefinition.Add(
				"MaxUserElements",
				new BXParamText(
					GetMessageRaw("MaxUserElements"),
					"0",
					accessCategory,
					new ParamClientSideActionGroupViewMember(ClientID, "MaxUserElements", new string[] { MannerOfUserIdentification.Current.ToString("G") })
				));

			ParamsDefinition.Add(
				"MannerOfIssueModificationPermission",
				new BXParamSingleSelection(
					GetMessageRaw("ElementActiveForEdit"),
					ElementActiveForEdit.Active.ToString(),
					accessCategory,
					null,
					new ParamClientSideActionGroupViewMember(ClientID, "MannerOfIssueModificationPermission", new string[] { MannerOfUserIdentification.Current.ToString("G") })
				));

			//управление активацией
			ParamsDefinition.Add(
				"RolesAuthorizedToManageOfActivation",
				new BXParamMultiSelection(
					GetMessageRaw("RolesAuthorizedToManageOfActivation"),
					"Admin",
					accessCategory
				));

			ParamsDefinition.Add(
				"ElementActiveAfterSave",
				new BXParamSingleSelection(
					GetMessageRaw("ElementActiveAfterSave"),
					ElementActiveForEdit.Active.ToString(),
					accessCategory
				));

			ParamsDefinition.Add(
				"MannerOfUserAssociation",
				new BXParamSingleSelection(
					GetMessageRaw("UserBindingBy"),
					UserElementAssociation.CreatedBy.ToString(),
					accessCategory,
					null,
					new ParamClientSideActionGroupViewSelector(ClientID, "MannerOfUserAssociation")
				));

			ParamsDefinition.Add(
				"UserAssociatedByCustomIBlockProperty",
				new BXParamSingleSelection(
					GetMessageRaw("IBlockPropertyBindingBy"),
					String.Empty,
					accessCategory,
					null,
					new ParamClientSideActionGroupViewMember(ClientID, "UserAssociatedByCustomIBlockProperty", new string[] { UserElementAssociation.IBlockProperty.ToString() })
				));
			#endregion

			#region Fields Settings
			ParamsDefinition.Add(
				"MaxSectionSelect",
				new BXParamText(
					GetMessageRaw("MaxSectionSelect"),
					"3",
					fieldSettingsCategory
				)
			);

			ParamsDefinition.Add(
				"OnlyLeafSelect",
				new BXParamYesNo(
					GetMessageRaw("OnlyLeafSelect"),
					false,
					fieldSettingsCategory
				)
			);

			ParamsDefinition.Add(
				"MaxFileSizeUpload",
				new BXParamText(
					GetMessageRaw("MaxFileSizeUpload"),
					"1024",
					fieldSettingsCategory
				)
			);

			ParamsDefinition.Add(
				"ActiveFromDateShowTime",
				new BXParamYesNo(
					GetMessageRaw("ActiveFromDateShowTime"),
					false,
					fieldSettingsCategory
				)
			);

			ParamsDefinition.Add(
				"ActiveToDateShowTime",
				new BXParamYesNo(
					GetMessageRaw("ActiveToDateShowTime"),
					false,
					fieldSettingsCategory
				)
			);

			ParamsDefinition.Add(
				"NameFieldMacros",
					new BXParamText(
					GetMessageRaw("NameFieldMacros"),
					"#DetailText#-#DateCreate#-#CreatedBy#",
					fieldSettingsCategory
				)
			);

			#endregion

			#region Additional
			ParamsDefinition.Add(
				"TextBoxSize",
				new BXParamText(
					GetMessageRaw("FieldsTextBoxSize"),
					"30",
					addSettingsCategory
				)
			);

			ParamsDefinition.Add(
				"CreateButtonTitle",
					new BXParamText(
					GetMessageRaw("CreateButtonTitle"),
					GetMessageRaw("CreateButtonTitleDefault"),
					addSettingsCategory
				)
			);

			ParamsDefinition.Add(
				"UpdateButtonTitle",
					new BXParamText(
					GetMessageRaw("UpdateButtonTitle"),
					GetMessageRaw("UpdateButtonTitleDefault"),
					addSettingsCategory
				)
			);

			ParamsDefinition.Add(
				"SuccessMessageAfterCreateElement",
				new BXParamText(
					GetMessageRaw("SuccessMessageAfterCreateElement"),
					GetMessageRaw("ElementSuccessCreated"),
					addSettingsCategory
				)
			);

			ParamsDefinition.Add(
				"SuccessMessageAfterUpdateElement",
				new BXParamText(
					GetMessageRaw("SuccessMessageAfterUpdateElement"),
					GetMessageRaw("ElementSuccessUpdated"),
					addSettingsCategory
				)
			);

			ParamsDefinition.Add(
				"RedirectPageUrl",
				new BXParamText(
					GetMessageRaw("RedirectPageUrl"),
					"",
					addSettingsCategory
				)
			);

			#endregion

			#region EmailSettings

			ParamsDefinition.Add(
				"SendEmailAfterCreate",
				new BXParamYesNo(
					GetMessageRaw("SendEmailAfterCreate"),
					false,
					emailCategory
				)
			);

			ParamsDefinition.Add(
				"EmailSubject",
				new BXParamText(
						GetMessageRaw("EmailSubject"),
						GetMessageRaw("DefaultEmailSubjectTemplate"),
						emailCategory
				)
			);

			ParamsDefinition.Add(
				"EmailTo",
				new BXParamText(
					GetMessageRaw("EmailTo"),
					"",
					emailCategory
				)
			);

			ParamsDefinition.Add(
				"EmailMessageTemplate",
				new BXParamMultilineText(
					GetMessageRaw("EmailMessageTemplate"),
					GetMessageRaw("DefaultEmailMessageTemplate"),
					emailCategory
				)
			);

			#endregion

			#region Custom Title
			ParamsDefinition.Add(
				"ActiveCustomTitle",
				new BXParamText(
					GetMessageRaw("ActiveCustomTitle"),
					String.Empty,
					customNameCategory
				)
			);

			ParamsDefinition.Add(
				"NameCustomTitle",
				new BXParamText(
					GetMessageRaw("NameCustomTitle"),
					String.Empty,
					customNameCategory
				)
			);

			ParamsDefinition.Add(
				"ActiveFromDateCustomTitle",
				new BXParamText(
					GetMessageRaw("ActiveFromDateCustomTitle"),
					String.Empty,
					customNameCategory
				)
			);

			ParamsDefinition.Add(
				"ActiveToDateCustomTitle",
				new BXParamText(
					GetMessageRaw("ActiveToDateCustomTitle"),
					String.Empty,
					customNameCategory
				)
			);

			ParamsDefinition.Add(
				"SectionsCustomTitle",
				new BXParamText(
					GetMessageRaw("SectionsCustomTitle"),
					String.Empty,
					customNameCategory
				)
			);

			ParamsDefinition.Add(
				"PreviewTextCustomTitle",
				new BXParamText(
					GetMessageRaw("PreviewTextCustomTitle"),
					String.Empty,
					customNameCategory
				)
			);

			ParamsDefinition.Add(
				"PreviewImageCustomTitle",
				new BXParamText(
					GetMessageRaw("PreviewPictureCustomTitle"),
					String.Empty,
					customNameCategory
				)
			);

			ParamsDefinition.Add(
				"DetailTextCustomTitle",
				new BXParamText(
					GetMessageRaw("DetailTextCustomTitle"),
					String.Empty,
					customNameCategory
				)
			);


			ParamsDefinition.Add(
				"DetailImageCustomTitle",
				new BXParamText(
					GetMessageRaw("DetailPictureCustomTitle"),
					String.Empty,
					customNameCategory
				)
			);


			ParamsDefinition.Add(
				"CaptchaCustomTitle",
				new BXParamText(
					GetMessageRaw("CaptchaCustomTitle"),
					String.Empty,
					customNameCategory
				)
			);
			#endregion
		}

		protected override void LoadComponentDefinition()
		{
			//Iblock type
			List<BXParamValue> typeParamValue = new List<BXParamValue>();
			typeParamValue.Add(new BXParamValue(GetMessageRaw("SelectIBlockType"), ""));

			BXIBlockTypeCollection iblockTypes = BXIBlockType.GetList(null, new BXOrderBy(new BXOrderByPair(BXIBlockType.Fields.Name, BXOrderByDirection.Asc)), null, null, BXTextEncoder.EmptyTextEncoder);
			foreach (BXIBlockType iblockType in iblockTypes)
				typeParamValue.Add(new BXParamValue(iblockType.Translations[BXLoc.CurrentLocale].Name, iblockType.Id.ToString()));

			ParamsDefinition["IBlockTypeId"].Values = typeParamValue;
			ParamsDefinition["IBlockTypeId"].RefreshOnDirty = true;

			//Iblock
			int selectedIBlockType = 0;
			if (Parameters.ContainsKey("IBlockTypeId"))
				int.TryParse(Parameters["IBlockTypeId"], out selectedIBlockType);

			BXFilter filter = new BXFilter();
			if (selectedIBlockType > 0)
				filter.Add(new BXFilterItem(BXIBlock.Fields.Type.ID, BXSqlFilterOperators.Equal, selectedIBlockType));
			if (!String.IsNullOrEmpty(DesignerSite))
				filter.Add(new BXFilterItem(BXIBlock.Fields.Sites.SiteId, BXSqlFilterOperators.Equal, DesignerSite));

			List<BXParamValue> iblockParamValue = new List<BXParamValue>();
			iblockParamValue.Add(new BXParamValue(GetMessageRaw("SelectIBlockID"), "-"));
			BXIBlockCollection iblocks = BXIBlock.GetList(filter, new BXOrderBy(new BXOrderByPair(BXIBlock.Fields.Name, BXOrderByDirection.Asc)), null, null, BXTextEncoder.EmptyTextEncoder);
			foreach (BXIBlock iblock in iblocks)
				iblockParamValue.Add(new BXParamValue(iblock.Name, iblock.Id.ToString()));

			ParamsDefinition["IBlockId"].Values = iblockParamValue;
			ParamsDefinition["IBlockId"].RefreshOnDirty = true;

			//Properties
			List<BXParamValue> iblockProperty = new List<BXParamValue>();
			int selectedIblockId = 0;
			if (Parameters.ContainsKey("IBlockId"))
				int.TryParse(Parameters["IBlockId"], out selectedIblockId);

			List<BXParamValue> fields = new List<BXParamValue>();
			fields.Add(new BXParamValue(GetMessageRaw("IBlockElementActive"), "Active"));
			fields.Add(new BXParamValue(GetMessageRaw("IBlockElementName"), "Name"));
			fields.Add(new BXParamValue(GetMessageRaw("IBlockElementActiveFrom"), "ActiveFromDate"));
			fields.Add(new BXParamValue(GetMessageRaw("IBlockElementActiveTo"), "ActiveToDate"));
			fields.Add(new BXParamValue(GetMessageRaw("IBlockElementSections"), "Sections"));
			fields.Add(new BXParamValue(GetMessageRaw("IBlockElementPreviewText"), "PreviewText"));
			fields.Add(new BXParamValue(GetMessageRaw("IBlockElementPreviewPicture"), "PreviewImage"));
			fields.Add(new BXParamValue(GetMessageRaw("IBlockElementDetailText"), "DetailText"));
			fields.Add(new BXParamValue(GetMessageRaw("IBlockElementDetailPicture"), "DetailImage"));

			ParamsDefinition["RequiredFields"].Values = new List<BXParamValue>(fields);

			fields.Add(new BXParamValue(GetMessageRaw("IBlockElementCaptcha"), "Captcha"));

			List<BXParamValue> bindingProperties = new List<BXParamValue>();
			bindingProperties.Add(new BXParamValue(GetMessageRaw("SelectIBlockPropertyForBinding"), ""));

			if (selectedIblockId > 0)
			{
				BXCustomFieldCollection customFields = BXIBlock.GetCustomFields(selectedIblockId);
				foreach (BXCustomField customField in customFields)
				{
					//if (customField.EditInList)
					//{
					string title = BXTextEncoder.HtmlTextEncoder.Decode(customField.EditFormLabel);
					string code = customField.Name.ToUpper();
					fields.Add(new BXParamValue(title, "PROPERTY_" + code));
					//}

					if (String.Equals(customField.CustomTypeId, "Bitrix.System.Int", StringComparison.Ordinal))
						bindingProperties.Add(new BXParamValue(title, code));
				}
			}

			ParamsDefinition["EditFields"].Values = fields;

			ParamsDefinition["SendEmailAfterCreate"].ClientSideAction = new ParamClientSideActionGroupViewSwitch(ClientID, "Email", "EnableEmail", "DisableEmail");

			ParamsDefinition["EmailTo"].ClientSideAction = new ParamClientSideActionGroupViewMember(ClientID, "EmailTo", new string[] { "EnableEmail" });
			ParamsDefinition["EmailSubject"].ClientSideAction = new ParamClientSideActionGroupViewMember(ClientID, "EmailSubject", new string[] { "EnableEmail" });
			ParamsDefinition["EmailMessageTemplate"].ClientSideAction = new ParamClientSideActionGroupViewMember(ClientID, "EmailMessageTemplate", new string[] { "EnableEmail" });

			#region Access

			//RolesAuthorizedToManage && RolesAuthorizedToAdminister && RolesAuthorizedToManageOfActivation
			IList<BXParamValue> rolesAuthorizedToManage = ParamsDefinition["RolesAuthorizedToManage"].Values;
			if (rolesAuthorizedToManage.Count > 0)
				rolesAuthorizedToManage.Clear();

			IList<BXParamValue> rolesAuthorizedToAdminister = ParamsDefinition["RolesAuthorizedToAdminister"].Values;
			if (rolesAuthorizedToAdminister.Count > 0)
				rolesAuthorizedToAdminister.Clear();

			IList<BXParamValue> rolesAuthorizedToManageOfActivation = ParamsDefinition["RolesAuthorizedToManageOfActivation"].Values;
			if (rolesAuthorizedToManageOfActivation.Count > 0)
				rolesAuthorizedToManageOfActivation.Clear();

			foreach (BXRole r in BXRoleManager.GetList(new BXFormFilter(new BXFormFilterItem("Active", true, BXSqlFilterOperators.Equal)), new BXOrderBy_old("RoleName", "Asc")))
			{
				BXParamValue v = new BXParamValue(r.Title, r.RoleName);
				rolesAuthorizedToManage.Add(v);
				rolesAuthorizedToAdminister.Add(v);
				rolesAuthorizedToManageOfActivation.Add(v);
			}

			List<BXParamValue> statusList = new List<BXParamValue>();
			string[] statusNames = Enum.GetNames(typeof(ElementActiveForEdit));
			int namesCount = statusNames != null ? statusNames.Length : 0;
			for (int j = 0; j < namesCount; j++)
			{
				string name = statusNames[j];
				statusList.Add(new BXParamValue(GetMessageRaw(string.Concat("MannerOfIssueModificationPermission", name)), name));
			}
			ParamsDefinition["MannerOfIssueModificationPermission"].Values = statusList;

			List<BXParamValue> associationList = new List<BXParamValue>();
			string[] userElementAssociation = Enum.GetNames(typeof(UserElementAssociation));
			int assocSize = userElementAssociation != null ? userElementAssociation.Length : 0;
			for (int k = 0; k < assocSize; k++)
			{
				string name = userElementAssociation[k];
				associationList.Add(new BXParamValue(GetMessageRaw(string.Concat("MannerOfUserAssociation", name)), name));
			}
			ParamsDefinition["MannerOfUserAssociation"].Values = associationList;

			List<BXParamValue> statusAfterSave = new List<BXParamValue>();
			statusAfterSave.Add(new BXParamValue(GetMessageRaw("ActiveStatusAfterSave"), ElementActiveForEdit.Active.ToString()));
			statusAfterSave.Add(new BXParamValue(GetMessageRaw("NotActiveStatusAfterSave"), ElementActiveForEdit.NotActive.ToString()));
			ParamsDefinition["ElementActiveAfterSave"].Values = statusAfterSave;

			ParamsDefinition["UserAssociatedByCustomIBlockProperty"].Values = bindingProperties;
			//MannerOfUserIdentification
			IList<BXParamValue> mannerOfUserIdentificationValues = ParamsDefinition["MannerOfUserIdentification"].Values;
			if (mannerOfUserIdentificationValues.Count > 0)
				mannerOfUserIdentificationValues.Clear();

			foreach (string n in Enum.GetNames(typeof(MannerOfUserIdentification)))
				mannerOfUserIdentificationValues.Add(new BXParamValue(GetMessageRaw(string.Concat("MannerOfUserIdentification.", n)), n));
			//---
			#endregion
		}

		public event EventHandler<CustomTypePublicEditorEventArgs> CreateCustomTypePublicEditor;
		public event EventHandler<FieldPublicEditorEventArgs> CreateFieldPublicEditor;
		public event EventHandler<AfterSaveEventArgs> AfterSave;
		public event EventHandler<BeforeSaveEventArgs> BeforeSave;

		public class CustomTypePublicEditorEventArgs : EventArgs
		{
			internal CustomTypePublicEditorEventArgs()
			{

			}

			private string id;
			public string ID
			{
				get { return id; }
				set { id = value; }
			}

			private string formFieldName;
			public string FormFieldName
			{
				get { return formFieldName; }
				set { formFieldName = value; }
			}

			private string title;
			public string Title
			{
				get { return title; }
				set { title = value; }
			}

			private BXCustomField field;
			public BXCustomField CustomField
			{
				get { return field; }
				set { field = value; }
			}

			private BXCustomTypePublicEdit publicEditor;
			public BXCustomTypePublicEdit PublicEditor
			{
				get { return publicEditor; }
				set { publicEditor = value; }
			}
		}

		public class FieldPublicEditorEventArgs : EventArgs
		{
			internal FieldPublicEditorEventArgs()
			{

			}

			private BXIBlockElementFieldPublicEditor publicEditor;
			public BXIBlockElementFieldPublicEditor PublicEditor
			{
				get { return publicEditor; }
				set { publicEditor = value; }
			}

			private string id;
			public string ID
			{
				get { return id; }
				set { id = value; }
			}

			private string formFieldName;
			public string FormFieldName
			{
				get { return formFieldName; }
				set { formFieldName = value; }
			}

			private string title;
			public string Title
			{
				get { return title; }
				set { title = value; }
			}

			private BXParamsBag<object> settings;
			public BXParamsBag<object> Settings
			{
				get { return settings; }
				set { settings = value; }
			}

			private bool required;
			public bool Required
			{
				get { return required; }
				set { required = value; }
			}

		}

		public class BeforeSaveEventArgs : EventArgs
		{
			internal BeforeSaveEventArgs(BXIBlockElement element)
			{
				this.element = element;
			}

			internal List<string> errors;
			public List<string> Errors
			{
				get
				{
					return errors ?? (errors = new List<string>());
				}
			}

			private BXIBlockElement element;
			public BXIBlockElement Element
			{
				get
				{
					return element;
				}
			}
		}


		public class AfterSaveEventArgs : EventArgs
		{
			internal AfterSaveEventArgs(BXIBlockElement element, BXCustomPropertyCollection properties)
			{
				this.element = element;
				this.properties = properties;
			}

			private BXIBlockElement element;
			public BXIBlockElement Element
			{
				get
				{
					return element;
				}
			}
			private BXCustomPropertyCollection properties;
			public BXCustomPropertyCollection Properties
			{
				get
				{
					return properties;
				}
			}

		}

		public abstract class FieldPublicEditor : BXIBlockElementFieldPublicEditor
		{
			private BXComponent component;
			public BXComponent Component
			{
				get { return component; }
				set { component = value; }
			}
		}

		private sealed class CustomFieldEditor : FieldPublicEditor
		{
			BXCustomTypePublicEdit publicEditor;
			BXCustomField field;

			public CustomFieldEditor(BXCustomField field, BXCustomTypePublicEdit publicEditor)
			{
				this.field = field;
				this.publicEditor = publicEditor;

				if (this.field == null)
					throw new ArgumentNullException("field");
				else if (this.publicEditor == null)
					throw new ArgumentNullException("publicEditor");

				publicEditor.Init(field);
			}

			public CustomFieldEditor(BXCustomField field)
			{
				this.field = field;
				if (this.field == null)
					throw new ArgumentNullException("field");

				BXCustomType type = BXCustomTypeManager.GetCustomType(field.CustomTypeId);
				publicEditor = type.CreatePublicEditor();
				publicEditor.Init(field);
			}

			public override void Load(BXIBlockElement iblockElement, BXParamsBag<object> settings)
			{
				if (iblockElement == null)
					return;

				BXCustomProperty value;
				if (iblockElement.CustomValues.TryGetValue(field.CorrectedName, out value))
					publicEditor.Load(value);
			}

			public override string Render(string formFieldName, string uniqueID)
			{
				return publicEditor.Render(formFieldName, uniqueID);
			}

			public override void Save(string formFieldName, BXIBlockElement iblockElement, BXCustomPropertyCollection properties)
			{
				publicEditor.DoSave(formFieldName, properties, null);
			}

			public override bool Validate(string formFieldName, ICollection<string> errors)
			{
				return publicEditor.DoValidate(formFieldName, errors);
			}
		}

		public class ElementField
		{
			private string title;
			public string Title
			{
				set { title = value; }
				get { return title; }
			}

			private string formFieldName;
			public string FormFieldName
			{
				get { return formFieldName; }
				set { formFieldName = value; }
			}

			private string uniqueId;
			public string UniqueId
			{
				set { uniqueId = value; }
				get { return uniqueId; }
			}

			private bool required;
			public bool Required
			{
				set { required = value; }
				get { return required; }
			}

			private List<string> validateErrors;
			public List<string> ValidateErrors
			{
				get { return validateErrors; }
				set { validateErrors = value; }
			}

			private BXIBlockElementFieldPublicEditor publicEditor;
			public BXIBlockElementFieldPublicEditor PublicEditor
			{
				set { publicEditor = value; }
				get { return publicEditor; }
			}

			private BXCustomField customField;
			public BXCustomField CustomField
			{
				set { customField = value; }
				get { return customField; }
			}

			private BXCustomType customType;
			public BXCustomType CustomType
			{
				set { customType = value; }
				get { return customType; }
			}

			public string Render()
			{
				if (publicEditor != null)
					return publicEditor.Render(FormFieldName, UniqueId);
				else
					return String.Empty;
			}
		}

		private abstract class ElementTextField : FieldPublicEditor
		{
			private string fieldValue;
			private bool required;
			private string fieldTitle;
			private int textBoxSize;

			public string Value
			{
				get { return fieldValue; }
			}

			public bool Required
			{
				get { return required; }
			}

			public string Title
			{
				get { return fieldTitle; }
			}

			public int BoxSize
			{
				get { return textBoxSize; }
			}

			public override void Load(BXIBlockElement iblockElement, BXParamsBag<object> settings)
			{
				required = settings.ContainsKey("required") ? (bool)settings["required"] : true;
				fieldTitle = settings.ContainsKey("fieldTitle") ? (string)settings["fieldTitle"] : String.Empty;
				textBoxSize = settings.ContainsKey("textBoxSize") ? (int)settings["textBoxSize"] : 30;

				if (iblockElement != null)
					fieldValue = GetField(iblockElement);
			}

			public override string Render(string formFieldName, string uniqueID)
			{
				return String.Format(@"<input name=""{0}"" value=""{1}"" id=""{2}"" size=""{3}"" maxlength=""255"" class=""element-field element-text-field"" />", HttpUtility.HtmlEncode(formFieldName), HttpUtility.HtmlEncode(fieldValue), HttpUtility.HtmlEncode(uniqueID), textBoxSize);
			}

			public override void Save(string formFieldName, BXIBlockElement iblockElement, BXCustomPropertyCollection properties)
			{
				if (iblockElement == null)
					return;

				SetField(iblockElement);
			}

			public override bool Validate(string formFieldName, ICollection<string> errors)
			{
				fieldValue = HttpContext.Current.Request.Form[formFieldName];
				if (required && BXStringUtility.IsNullOrTrimEmpty(fieldValue))
				{
					errors.Add(String.Format(Component.GetMessage("EmptyFieldError"), fieldTitle));
					return false;
				}

				return true;
			}

			public abstract string GetField(BXIBlockElement iblockElement);
			public abstract void SetField(BXIBlockElement iblockElement);
		}

		/// <summary>
		/// Поле для св-ва типа Boolean
		/// </summary>
		private abstract class ElementFlagField : FieldPublicEditor
		{
			private bool fieldValue = false;
			public bool Value
			{
				get { return fieldValue; }
				protected set { fieldValue = value; }
			}


			private string fieldTitle = string.Empty;
			public string Title
			{
				get { return fieldTitle; }
				protected set { fieldTitle = value; }
			}

			#region FieldPublicEditor
			public override void Load(BXIBlockElement iblockElement, BXParamsBag<object> settings)
			{
				object o;
				fieldTitle = settings.TryGetValue("fieldTitle", out o) ? (string)o : string.Empty;
				GetFieldValue(iblockElement, settings);
			}

			public override string Render(string formFieldName, string uniqueID)
			{
				return string.Format("<input type=\"checkbox\" id=\"{0}\" name=\"{1}\" {2} value=\"active\" class=\"element-field element-flag-field\" />", HttpUtility.HtmlAttributeEncode(uniqueID), HttpUtility.HtmlAttributeEncode(formFieldName), fieldValue ? "checked=\"checked\"" : string.Empty);
			}

			public override void Save(string formFieldName, BXIBlockElement iblockElement, BXCustomPropertyCollection properties)
			{
				SetFieldValue(iblockElement);
			}

			public override bool Validate(string formFieldName, ICollection<string> errors)
			{
				fieldValue = !string.IsNullOrEmpty(HttpContext.Current.Request.Form[formFieldName]) && (string.Equals(HttpContext.Current.Request.Form[formFieldName].ToUpperInvariant(), "ON", StringComparison.InvariantCulture) || string.Equals(HttpContext.Current.Request.Form[formFieldName].ToUpperInvariant(), "ACTIVE", StringComparison.InvariantCulture));
				return true;
			}
			#endregion

			/// <summary>
			/// Значение из св-ва эл-та инфоблока в редактор
			/// </summary>
			/// <param name="iblockElement">эл-т инфоблока (требуется обработка значения NULL)</param>
			/// <param name="settings">настройки</param>
			protected abstract void GetFieldValue(BXIBlockElement iblockElement, BXParamsBag<object> settings);
			/// <summary>
			/// Значение из редактора в св-во эл-та инфоблока
			/// </summary>
			/// <param name="iblockElement">эл-т инфоблока (требуется обработка значения NULL)</param>
			protected abstract void SetFieldValue(BXIBlockElement iblockElement);
		}

		private abstract class ElementDateField : FieldPublicEditor
		{
			private DateTime fieldValue = DateTime.MinValue;
			private string postValue;
			private bool isValidated = true;
			private bool required;
			private string fieldTitle;
			private int textBoxSize;
			private bool showTime;

			public DateTime Value
			{
				get { return fieldValue; }
			}

			public bool Required
			{
				get { return required; }
			}

			public string Title
			{
				get { return fieldTitle; }
			}

			public int BoxSize
			{
				get { return textBoxSize; }
			}

			public override void Load(BXIBlockElement iblockElement, BXParamsBag<object> settings)
			{
				required = settings.ContainsKey("required") ? (bool)settings["required"] : true;
				fieldTitle = settings.ContainsKey("fieldTitle") ? (string)settings["fieldTitle"] : String.Empty;
				textBoxSize = settings.ContainsKey("textBoxSize") ? (int)settings["textBoxSize"] : 30;
				showTime = settings.ContainsKey("showTime") ? (bool)settings["showTime"] : false;

				BXCalendarHelper.RegisterScriptFiles();

				if (iblockElement != null)
					fieldValue = GetField(iblockElement);
			}

			public override string Render(string formFieldName, string uniqueID)
			{
				string value;
				if (!isValidated && postValue != null)
					value = postValue;
				else
					value = fieldValue == DateTime.MinValue ? String.Empty : fieldValue.ToString(showTime ? "G" : "d");

				return String.Format(
					@"<input name=""{0}"" value=""{1}"" id=""{2}"" size=""{3}"" class=""element-field element-date-field"" />{4}",
					HttpUtility.HtmlEncode(formFieldName),
					HttpUtility.HtmlEncode(value),
					uniqueID,
					20,
					BXCalendarHelper.GetMarkupByInputElement("this.previousSibling", showTime)
				);
			}

			public override void Save(string formFieldName, BXIBlockElement iblockElement, BXCustomPropertyCollection properties)
			{
				if (iblockElement == null)
					return;

				SetField(iblockElement);
			}

			public override bool Validate(string formFieldName, ICollection<string> errors)
			{
				isValidated = true;
				postValue = HttpContext.Current.Request.Form[formFieldName];

				if (!BXStringUtility.IsNullOrTrimEmpty(postValue))
				{
					if (!DateTime.TryParse(postValue, out fieldValue))
					{
						errors.Add(String.Format(Component.GetMessage("FieldValueMustBeDate"), fieldTitle));
						isValidated = false;
					}

					fieldValue = showTime ? fieldValue : fieldValue.Date;
				}
				else if (required)
				{
					errors.Add(String.Format(Component.GetMessage("EmptyFieldError"), fieldTitle));
					isValidated = false;
				}
				else
					fieldValue = DateTime.MinValue;

				return isValidated;
			}

			public abstract DateTime GetField(BXIBlockElement iblockElement);
			public abstract void SetField(BXIBlockElement iblockElement);
		}
		private abstract class ElementImageField : FieldPublicEditor
		{
			enum Status
			{
				NoFileUpload,
				UploadTempFile,
				UploadOriginFile
			}

			private Status status = Status.NoFileUpload;
			private int imageId = 0;
			private BXFile image;
			private string tempFileGuid;
			private bool deleteFile = false;

			//Settings
			private bool required;
			private string fieldTitle;
			private int textBoxSize;
			private int maxFileUpload;

			public bool Required
			{
				get { return required; }
			}

			public string Title
			{
				get { return fieldTitle; }
			}

			public int BoxSize
			{
				get { return textBoxSize; }
			}

			public int ImageId
			{
				get { return imageId; }
			}

			public abstract BXFile GetField(BXIBlockElement iblockElement);
			public abstract void SetField(BXIBlockElement iblockElement);

			public override void Load(BXIBlockElement iblockElement, BXParamsBag<object> settings)
			{
				required = settings.ContainsKey("required") ? (bool)settings["required"] : true;
				fieldTitle = settings.ContainsKey("fieldTitle") ? (string)settings["fieldTitle"] : String.Empty;
				textBoxSize = settings.ContainsKey("textBoxSize") ? (int)settings["textBoxSize"] : 30;
				maxFileUpload = settings.ContainsKey("maxFileSizeUpload") ? (int)settings["maxFileSizeUpload"] : 1024;
				maxFileUpload = maxFileUpload * 1024; //To Bytes

				if (iblockElement != null)
				{
					image = GetField(iblockElement);
					imageId = image != null ? image.Id : 0;

					if (imageId > 0 && image != null)
						status = Status.UploadOriginFile;
				}
			}

			public override string Render(string formFieldName, string uniqueID)
			{
				string result;

				if (status == Status.UploadOriginFile)
				{
					if (required)

						result = String.Format(
							@"{0} ({1}) {2}<br /><input type=""file"" name=""{3}"" id=""{4}"" size=""{5}"" class=""element-field element-image-field"" />",
							image.FileNameOriginal,
							image.ContentType,
							BXStringUtility.BytesToString(image.FileSize),
							HttpUtility.HtmlEncode(formFieldName),
							HttpUtility.HtmlEncode(uniqueID),
							textBoxSize
						);
					else
						result = String.Format(
							@"<input type=""checkbox"" id=""{4}_attach"" name=""{0}_attach"" value=""Y"" checked=""Y"" class=""element-field element-image-checkbox-field"" />
							<label for=""{4}_attach"">{1} ({2}) {3}</label><br /><input type=""file"" name=""{0}"" id=""{4}"" size=""{5}"" class=""element-field element-image-field"" />",
							HttpUtility.HtmlEncode(formFieldName),
							image.FileNameOriginal,
							image.ContentType,
							BXStringUtility.BytesToString(image.FileSize),
							HttpUtility.HtmlEncode(uniqueID),
							textBoxSize
						);
				}
				else if (status == Status.UploadTempFile)
				{
					if (required)

						result = String.Format(
@"{0} ({1}) {2}</label><br />
<input type=""file"" name=""{3}"" id=""{4}"" size=""{5}"" class=""element-field element-image-field"" />
<input type=""hidden"" name=""{3}_guid"" value=""{6}"" />",
							HttpUtility.HtmlEncode(image.FileNameOriginal ?? ""),
							HttpUtility.HtmlEncode(image.ContentType ?? ""),
							BXStringUtility.BytesToString(image.FileSize),
							HttpUtility.HtmlEncode(formFieldName),
							HttpUtility.HtmlEncode(uniqueID),
							textBoxSize,
							HttpUtility.HtmlEncode(tempFileGuid)
						);
					else

						result = String.Format(
@"<input type=""checkbox"" id=""{4}_attach"" name=""{0}_attach"" value=""Y"" checked=""Y"" class=""element-field element-image-checkbox-field"" /><label for=""{4}_attach"">{1} ({2}) {3}</label><br />
<input type=""file"" name=""{0}"" id=""{4}"" size=""{5}"" class=""element-field element-image-field"" />
<input type=""hidden"" name=""{0}_guid"" value=""{6}"" />",
							HttpUtility.HtmlEncode(formFieldName),
							HttpUtility.HtmlEncode(image.FileNameOriginal ?? ""),
							HttpUtility.HtmlEncode(image.ContentType ?? ""),
							BXStringUtility.BytesToString(image.FileSize),
							HttpUtility.HtmlEncode(uniqueID),
							textBoxSize,
							HttpUtility.HtmlEncode(tempFileGuid)
						);
				}
				else
				{
					result = String.Format(@"<input type=""file"" name=""{0}"" id=""{1}"" size=""{2}"" class=""element-field element-image-field"" />", HttpUtility.HtmlEncode(formFieldName), HttpUtility.HtmlEncode(uniqueID), textBoxSize);
				}

				return result;
			}

			public override void Save(string formFieldName, BXIBlockElement iblockElement, BXCustomPropertyCollection properties)
			{
				if (iblockElement == null)
					return;

				if (status == Status.UploadTempFile)
				{
					if (image != null)
					{
						image.TempGuid = "";
						image.Save();
						imageId = image.Id;
						status = Status.UploadOriginFile;
					}
				}
				else if (status == Status.UploadOriginFile && !required && deleteFile)
				{
					imageId = 0;
					image = null;
					status = Status.NoFileUpload;
				}

				SetField(iblockElement);
			}

			public override bool Validate(string formFieldName, ICollection<string> errors)
			{
				bool isRaized = RaiseFileFromHiddenFields(formFieldName);
				if (isRaized)
					status = Status.UploadTempFile;

				bool isFileSave = false;
				HttpPostedFile postFile = HttpContext.Current.Request.Files[formFieldName];
				if (postFile != null && postFile.ContentLength != 0)
				{
					isFileSave = SaveUploadFile(postFile, errors);
					if (isFileSave)
						status = Status.UploadTempFile;
					else
						return false;
				}

				string attachFile = HttpContext.Current.Request.Form[formFieldName + "_attach"];
				deleteFile = attachFile == null || attachFile != "Y";

				if (required)
				{
					if (status == Status.NoFileUpload)
					{
						errors.Add(String.Format(Component.GetMessage("EmptyFieldError"), fieldTitle));
						return false;
					}
				}
				else if (isRaized && !isFileSave && deleteFile)
				{
					DeleteOldFile();
					imageId = 0;
					image = null;
					status = Status.NoFileUpload;
				}

				return true;
			}

			private bool SaveUploadFile(HttpPostedFile postFile, ICollection<string> errors)
			{
				BXFile newfile = new BXFile(postFile, "iblock", "iblock", String.Empty);
				if (!ValidateImage(newfile, errors))
					return false;


				bool success = false;

				newfile.TempGuid = Guid.NewGuid().ToString("N");

				try
				{
					newfile.Save();
					success = true;
				}
				catch
				{
				}


				if (success)
				{
					DeleteOldFile();
					tempFileGuid = newfile.TempGuid;
					image = newfile;
				}

				return success;
			}

			private bool RaiseFileFromHiddenFields(string formFieldName)
			{
				string loadedFileGuid = HttpContext.Current.Request.Form[formFieldName + "_guid"];

				if (String.IsNullOrEmpty(loadedFileGuid))
					return false;

				bool success = false;
				var tempFile = BXFile.GetByTempGuid(loadedFileGuid, BXTextEncoder.EmptyTextEncoder);
				if (tempFile != null)
				{
					tempFileGuid = tempFile.TempGuid;
					image = tempFile;
					success = true;
				}
				else
				{
					tempFileGuid = null;
				}

				return success;
			}

			private void DeleteOldFile()
			{
				if (tempFileGuid != null)
				{
					try
					{
						var tempFile = BXFile.GetByTempGuid(tempFileGuid, BXTextEncoder.EmptyTextEncoder);
						if (tempFileGuid != null)
							tempFile.Delete();
					}
					catch
					{

					}
				}
			}

			private bool ValidateImage(BXFile image, ICollection<string> errors)
			{
				BXFileValidationResult result = image.ValidateAsImage(maxFileUpload, 0, 0);

				if (result != BXFileValidationResult.Valid)
				{
					StringBuilder errorMessage = new StringBuilder();

					if ((result & BXFileValidationResult.InvalidContentType) == BXFileValidationResult.InvalidContentType)
						errorMessage.Append(Component.GetMessage("UploadImage.IncorrectType"));
					if ((result & BXFileValidationResult.InvalidExtension) == BXFileValidationResult.InvalidExtension)
					{
						if (errorMessage.Length > 0)
							errorMessage.Append(", ");
						errorMessage.Append(Component.GetMessage("UploadImage.IncorrectExtension"));
					}
					if ((result & BXFileValidationResult.InvalidImage) == BXFileValidationResult.InvalidImage)
					{
						if (errorMessage.Length > 0)
							errorMessage.Append(", ");
						errorMessage.Append(Component.GetMessage("UploadImage.Incorrect"));
					}

					if ((result & BXFileValidationResult.InvalidSize) == BXFileValidationResult.InvalidSize)
					{
						if (errorMessage.Length > 0)
							errorMessage.Append(", ");
						errorMessage.AppendFormat(Component.GetMessage("MaxFileSizeError"), BXStringUtility.BytesToString(maxFileUpload));
					}

					errors.Add(String.Format(Component.GetMessage("UploadFileIsNotImage"), image.FileNameOriginal, errorMessage.ToString()));
					return false;
				}

				try
				{
					image.DemandFileUpload();
				}
				catch
				{
					errors.Add(Component.GetMessage("FileUploadPermissionError"));
					return false;
				}

				return true;
			}
		}
		private class ElementActiveFromDate : ElementDateField
		{
			public override DateTime GetField(BXIBlockElement iblockElement)
			{
				return iblockElement.ActiveFromDate;
			}

			public override void SetField(BXIBlockElement iblockElement)
			{
				iblockElement.ActiveFromDate = Value;
			}
		}
		private class ElementActiveToDate : ElementDateField
		{
			public override DateTime GetField(BXIBlockElement iblockElement)
			{
				return iblockElement.ActiveToDate;
			}

			public override void SetField(BXIBlockElement iblockElement)
			{
				iblockElement.ActiveToDate = Value;
			}
		}
		private class ElementName : ElementTextField
		{
			public override string GetField(BXIBlockElement iblockElement)
			{
				return iblockElement.Name;
			}

			public override void SetField(BXIBlockElement iblockElement)
			{
				iblockElement.Name = Value;
			}
		}

		/// <summary>
		/// Поле "Активность"
		/// </summary>
		private class ElementActive : ElementFlagField
		{
			protected override void GetFieldValue(BXIBlockElement iblockElement, BXParamsBag<object> settings)
			{
				if (iblockElement != null)
					Value = iblockElement.Active;
				else if (settings != null && settings.ContainsKey("fieldDefaultValue"))
					Value = (bool)settings["fieldDefaultValue"];
				else
					Value = false;
			}

			protected override void SetFieldValue(BXIBlockElement iblockElement)
			{
				if (iblockElement != null)
					iblockElement.Active = Value;
			}
		}
		private class ElementPreviewText : ElementTextField
		{
			public override string Render(string formFieldName, string uniqueID)
			{
				return String.Format(
					@"<textarea name=""{0}"" id=""{2}"" cols=""{3}"" rows=""10"" class=""element-field element-text-field"">{1}</textarea>",
					HttpUtility.HtmlEncode(formFieldName),
					HttpUtility.HtmlEncode(Value),
					HttpUtility.HtmlEncode(uniqueID),
					BoxSize
				);
			}

			public override string GetField(BXIBlockElement iblockElement)
			{
				return iblockElement.PreviewText;
			}

			public override void SetField(BXIBlockElement iblockElement)
			{
				iblockElement.PreviewText = Value;
			}
		}
		private class ElementDetailText : ElementTextField
		{
			public override string Render(string formFieldName, string uniqueID)
			{
				return String.Format(
					@"<textarea name=""{0}"" id=""{2}"" cols=""{3}"" rows=""10"" class=""element-field element-text-field"">{1}</textarea>",
					HttpUtility.HtmlEncode(formFieldName),
					HttpUtility.HtmlEncode(Value),
					HttpUtility.HtmlEncode(uniqueID),
					BoxSize
				);
			}

			public override string GetField(BXIBlockElement iblockElement)
			{
				return iblockElement.DetailText;
			}

			public override void SetField(BXIBlockElement iblockElement)
			{
				iblockElement.DetailText = Value;
			}
		}
		private class ElementSections : FieldPublicEditor
		{
			private List<int> fieldValues = new List<int>();

			private int iblockId;
			private int maxSectionSelect;
			private bool onlyLeafSelect;
			private bool multiple;
			private bool required;
			private string fieldTitle;
			private int textBoxSize;
			private string[] postValues;
			private OrderedDictionary sectionTree;

			private class SectionTreeItem
			{
				public int Id;
				public string Name;
				public int DepthLevel;
				public bool HasChildren = false;
			}

			public override void Load(BXIBlockElement iblockElement, BXParamsBag<object> settings)
			{
				required = settings.ContainsKey("required") ? (bool)settings["required"] : true;
				fieldTitle = settings.ContainsKey("fieldTitle") ? (string)settings["fieldTitle"] : String.Empty;
				textBoxSize = settings.ContainsKey("textBoxSize") ? (int)settings["textBoxSize"] : 30;
				iblockId = settings.ContainsKey("iblockId") ? (int)settings["iblockId"] : 0;
				onlyLeafSelect = settings.ContainsKey("onlyLeafSelect") ? (bool)settings["onlyLeafSelect"] : false;
				maxSectionSelect = settings.ContainsKey("maxSectionSelect") ? (int)settings["maxSectionSelect"] : 3;
				multiple = maxSectionSelect > 1;


				BXIBlockSectionCollection sections = BXIBlockSection.GetList(
					new BXFilter(
						new BXFilterItem(BXIBlockSection.Fields.ActiveGlobal, BXSqlFilterOperators.Equal, "Y"),
						new BXFilterItem(BXIBlockSection.Fields.IBlock.ID, BXSqlFilterOperators.Equal, iblockId)
					),
					new BXOrderBy(
						new BXOrderByPair(BXIBlockSection.Fields.LeftMargin, BXOrderByDirection.Asc)
					),
					new BXSelect(
						BXIBlockSection.Fields.ID, BXIBlockSection.Fields.Name, BXIBlockSection.Fields.DepthLevel
					),
					null,
					BXTextEncoder.HtmlTextEncoder
				);

				int currentIndex = 0;
				int previousDepthLevel = 1;
				sectionTree = new OrderedDictionary();
				foreach (BXIBlockSection section in sections)
				{
					if (currentIndex > 0)
						((SectionTreeItem)sectionTree[currentIndex - 1]).HasChildren = section.DepthLevel > previousDepthLevel;
					previousDepthLevel = section.DepthLevel;

					SectionTreeItem treeItem = new SectionTreeItem();
					treeItem.Id = section.Id;
					treeItem.DepthLevel = section.DepthLevel;
					treeItem.Name = section.Name;
					sectionTree.Insert(currentIndex++, section.Id, treeItem);
				}

				if (iblockElement != null)
				{
					foreach (BXIBlockElement.BXInfoBlockElementSection section in iblockElement.Sections)
						fieldValues.Add(section.SectionId);
				}
			}

			public override string Render(string formFieldName, string uniqueID)
			{
				string dropDownStart = "<select name=\"{0}\" id=\"{1}\" style=\"width:{2}px;\" class=\"custom-field-list\" size=\"{3}\"{4}>";
				string dropDownOption = "<option value=\"{0}\"{1}>{2}</option>";
				string dropDownEnd = "</select>";

				StringBuilder result = new StringBuilder(String.Empty);
				result.AppendFormat(
					dropDownStart,
					HttpUtility.HtmlEncode(formFieldName),
					HttpUtility.HtmlEncode(formFieldName),
					textBoxSize * 7,
					multiple ? (sectionTree.Count <= 10 ? sectionTree.Count : 10) : 1,
					multiple ? " multiple=\"multiple\"" : String.Empty
				);

				if (!multiple)
					result.AppendFormat(dropDownOption, 0, String.Empty, Component.GetMessage("ValueNotSelected"));

				for (int i = 0; i < sectionTree.Count; i++)
				{
					SectionTreeItem section = (SectionTreeItem)sectionTree[i];
					StringBuilder sectionName = new StringBuilder();
					for (int index = 0; index < section.DepthLevel; index++)
						sectionName.Append(". ");
					sectionName.Append(section.Name);

					result.AppendFormat(
						dropDownOption,
						section.Id,
						fieldValues.Contains(section.Id) ? " selected=\"selected\"" : String.Empty,
						sectionName
					);
				}

				result.Append(dropDownEnd);
				return result.ToString();
			}

			public override void Save(string formFieldName, BXIBlockElement iblockElement, BXCustomPropertyCollection properties)
			{
				if (iblockElement == null)
					return;

				BXIBlockElement.BXInfoBlockElementSectionCollection sections = iblockElement.Sections;
				sections.Clear();
				foreach (int sectionId in fieldValues)
					sections.Add(sectionId);
			}

			public override bool Validate(string formFieldName, ICollection<string> errors)
			{
				postValues = HttpContext.Current.Request.Form.GetValues(formFieldName);
				if (postValues == null || postValues.Length < 1)
				{
					fieldValues.Clear();
					if (required)
					{
						errors.Add(String.Format(Component.GetMessage("FieldRequiredSelectedValue"), fieldTitle));
						return false;
					}
					return true;
				}

				bool success = true;
				fieldValues = new List<int>(postValues.Length);
				foreach (string value in postValues)
				{
					int sectionId;
					if (!int.TryParse(value, out sectionId) || sectionId < 1 || !sectionTree.Contains(sectionId))
						continue;

					if (onlyLeafSelect && ((SectionTreeItem)sectionTree[(object)sectionId]).HasChildren)
						success = false;

					fieldValues.Add(sectionId);
				}

				if (!success)
				{
					errors.Add(String.Format(Component.GetMessage("OnlyLeafSelectError"), fieldTitle));
				}
				else if (required && fieldValues.Count < 1)
				{
					errors.Add(String.Format(Component.GetMessage("FieldRequiredSelectedValue"), fieldTitle));
					success = false;
				}
				else if (fieldValues.Count > maxSectionSelect)
				{
					errors.Add(String.Format(Component.GetMessage("MaxSectionSelectError"), fieldTitle, maxSectionSelect));
					success = false;
				}

				return success;
			}
		}
		private class ElementPreviewImage : ElementImageField
		{
			public override BXFile GetField(BXIBlockElement iblockElement)
			{
				return iblockElement.PreviewImage;
			}

			public override void SetField(BXIBlockElement iblockElement)
			{
				iblockElement.PreviewImageId = ImageId;
			}
		}
		private class ElementDetailImage : ElementImageField
		{
			public override BXFile GetField(BXIBlockElement iblockElement)
			{
				return iblockElement.DetailImage;
			}

			public override void SetField(BXIBlockElement iblockElement)
			{
				iblockElement.DetailImageId = ImageId;
			}
		}
		private class Captcha : FieldPublicEditor
		{
			private BXCaptchaEngine captcha;

			public override void Load(BXIBlockElement iblockElement, BXParamsBag<object> settings)
			{

			}

			public override string Render(string formFieldName, string uniqueID)
			{
				captcha = captcha ?? BXCaptchaEngine.Create();
				captcha.MaxTimeout = 1800;

				string imageSrc = captcha.Store();

				return String.Format(@"<input type=""hidden"" name=""{0}_guid"" value=""{1}""/><input type=""text"" name=""{0}"" value="""" maxlength=""{5}"" size=""{6}"" class=""element-field element-text-field""/><br />
						<img src=""{2}"" width=""{3}"" height=""{4}"" alt=""{7}""/>
						
					",
					 HttpUtility.HtmlEncode(formFieldName),
					 captcha.Id,
					 HttpUtility.HtmlEncode(imageSrc),
					 captcha.Captcha.Width,
					 captcha.Captcha.Height,
					 captcha.Captcha.Length,
					 captcha.Captcha.Length * 2,
					 Component.GetMessage("CaptchaCustomTitle")
				);
			}

			public override void Save(string formFieldName, BXIBlockElement iblockElement, BXCustomPropertyCollection properties)
			{

			}

			public override bool Validate(string formFieldName, ICollection<string> errors)
			{
				captcha = BXCaptchaEngine.Get(HttpContext.Current.Request.Form[formFieldName + "_guid"]);
				string error = captcha.Validate(HttpContext.Current.Request.Form[formFieldName]);
				if (error != null)
				{
					errors.Add(error);
					return false;
				}
				return true;
			}
		}

		/// <summary>
		/// Управление
		/// </summary>
		private abstract class Behaviour
		{
			public Behaviour(IBlockElementWebFormComponent parent)
			{
				if (parent == null)
					throw new ArgumentNullException("parent");
				_parent = parent;
			}

			private IBlockElementWebFormComponent _parent = null;
			protected IBlockElementWebFormComponent Parent
			{
				get { return _parent; }
			}

			public int CurrentUserId
			{
				get { return _parent.CurrentUserId; }
			}

			public BXPrincipal CurrentPricipal
			{
				get { return _parent.CurrentPricipal; }
			}

			private bool? _isCurrentUserAuthenticated = null;
			public bool IsCurrentUserAuthenticated()
			{
				return (_isCurrentUserAuthenticated ?? (_isCurrentUserAuthenticated = CurrentPricipal != null ? CurrentPricipal.Identity.IsAuthenticated : false)).Value;
			}

			public int CustomUserId
			{
				get { return _parent.CustomUserId; }
			}

			protected bool IsCurrentUserInRoles(IList<string> roles)
			{
				if (roles != null && roles.Count > 0 && CurrentPricipal != null)
					foreach (string r in CurrentPricipal.GetAllRoles())
						if (roles.Contains(r))
							return true;
				return false;
			}

			/// <summary>
			/// Разрешение пользоваться данной веб-формой
			/// </summary>
			public abstract bool IsCurrentUserPermitted { get; }
			/// <summary>
			/// Ид представляемого пользователя
			/// </summary>
			public abstract int ImpersonatedUserId { get; }
			/// <summary>
			/// Ограничения на редактирование элемента
			/// </summary>
			public abstract ElementActiveForEdit ElementEditIsPermitted { get; }
			/// <summary>
			/// Выполнять проверку максимально допустимого количества элементов для пользователя
			/// </summary>
			public abstract bool EnableMaxUserElementsCheck { get; }
		}
		/// <summary>
		/// Управление в режиме "Стандартный" (редактирование своих записей)
		/// </summary>
		private sealed class StandardBehaviour : Behaviour
		{
			public StandardBehaviour(IBlockElementWebFormComponent parent)
				: base(parent)
			{
			}

			private bool? _isCurrentUserPermitted = null;
			public override bool IsCurrentUserPermitted
			{
				get
				{
					if (_isCurrentUserPermitted.HasValue)
						return _isCurrentUserPermitted.Value;

					_isCurrentUserPermitted = false;
					//неаутентифицированные могут быть допущены к управлению "своими" эл-тами
					if (CurrentPricipal != null)
					{
						IList<string> permittedRoles = Parent.RolesAuthorizedToManage;
						if (permittedRoles.Count == 0)
							_isCurrentUserPermitted = Parent.IBlock != null ? Parent.IBlock.IsUserCanOperate(BXIBlock.Operations.IBlockModifyElements) : false;
						else
							_isCurrentUserPermitted = IsCurrentUserInRoles(permittedRoles);
					}
					return _isCurrentUserPermitted.Value;
				}
			}

			public override int ImpersonatedUserId
			{
				get { return CurrentUserId; }
			}

			public override ElementActiveForEdit ElementEditIsPermitted
			{
				get { return Parent.ElementStatusForEdit; }
			}

			public override bool EnableMaxUserElementsCheck
			{
				get { return true; }
			}
		}
		/// <summary>
		/// Управление в режиме "Администрирование" (редактирование чужих записей)
		/// </summary>
		private sealed class AdministerBehaviour : Behaviour
		{
			public AdministerBehaviour(IBlockElementWebFormComponent parent)
				: base(parent)
			{
			}

			private bool? _isCurrentUserPermitted = null;
			public override bool IsCurrentUserPermitted
			{
				get
				{
					//неаутентифицированные не могут быть допущены к управлению чужими эл-тами
					return (_isCurrentUserPermitted ?? (_isCurrentUserPermitted = CurrentPricipal != null && IsCurrentUserAuthenticated() && IsCurrentUserInRoles(Parent.RolesAuthorizedToAdminister))).Value;
				}
			}

			public override int ImpersonatedUserId
			{
				get { return CustomUserId; }
			}

			public override ElementActiveForEdit ElementEditIsPermitted
			{
				get { return ElementActiveForEdit.Always; }
			}

			public override bool EnableMaxUserElementsCheck
			{
				get { return false; }
			}
		}
		/// <summary>
		/// Управление в режиме "Доступ запрещен"
		/// </summary>
		private sealed class AccessDeniedBehaviour : Behaviour
		{
			public AccessDeniedBehaviour(IBlockElementWebFormComponent parent) : base(parent) { }
			public override bool IsCurrentUserPermitted { get { return false; } }
			public override int ImpersonatedUserId { get { throw new NotSupportedException(); } }
			public override ElementActiveForEdit ElementEditIsPermitted { get { throw new NotSupportedException(); } }
			public override bool EnableMaxUserElementsCheck { get { throw new NotSupportedException(); } }
		}
	}

	public partial class IBlockElementWebFormTemplate : BXComponentTemplate<IBlockElementWebFormComponent>
	{
		public void SaveWebForm(object sender, EventArgs e)
		{
			Component.SaveIBlockElement(sender, e);
		}

		protected override void Render(HtmlTextWriter writer)
		{
			StartWidth = "100%";
			if (IsComponentDesignMode && Component.IBlock == null)
				writer.Write(BXLoc.GetMessage(Component, "YouHaveToAdjustTheComponent"));
			else
				base.Render(writer);
		}
	}
}
