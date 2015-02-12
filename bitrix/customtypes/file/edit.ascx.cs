using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

using Bitrix;
using Bitrix.Services;
using System.Collections.Generic;

using Bitrix.IO;
using Bitrix.DataTypes;
using Bitrix.Configuration;
using Bitrix.Services.Text;

public partial class BXCustomTypeFileEdit : Bitrix.UI.BXControl, IBXCustomTypeEdit, INamingContainer
{
	//FIELDS
	private BXCustomField field;
	private int? maxSize;
	private string[] extensions;
	private BXCustomProperty value;
	private bool addDescription;
	private BXFile originalFile;
	private BXFile currentFile;
	private bool raised;

	//PROPERTIES
	public bool HasValue
	{
		get { return !(string.IsNullOrEmpty(CachedId.Value) && string.IsNullOrEmpty(StoredId.Value)); }
	}
	public string UploadBoxStyle
	{
		get { return HasValue ? "display:none;visibility:hidden;" : "display:inline;visibility:visible;"; }
	}
	public string ClearButtonStyle
	{
		get { return HasValue ? "display:inline;visibility:visible;" : "display:none;visibility:hidden;"; }
	}
	public string SaveFileGuid
	{
		get { return CachedId.Value ?? string.Empty; }
	}

	//METHODS
	protected void Page_Load(object sender, EventArgs e)
	{
		DataBindChildren();		
	}	
	protected void Page_PreRender(object sender, EventArgs e)
	{
		RaiseFile();
		
		SavedName.InnerHtml = string.Empty;
		if (currentFile != null)
		{
			SavedName.InnerHtml += GetMessage("File") + ": " + HttpUtility.HtmlEncode(currentFile.FileNameOriginal) + "<br/>";
			SavedName.InnerHtml += GetMessage("Size") + ": " + HttpUtility.HtmlEncode(BXStringUtility.BytesToString(currentFile.FileSize)) + "<br/>";
		}

		StoredId.Value = currentFile == originalFile && originalFile != null ? "Y" : "";
		CachedId.Value = currentFile != null && currentFile != originalFile ? currentFile.TempGuid : "";
		
		if (!Page.ClientScript.IsClientScriptIncludeRegistered(GetType(), "Upload"))
			Page.ClientScript.RegisterClientScriptInclude(GetType(), "Upload", ResolveUrl("./edit.aspx") + "?lang=" + HttpUtility.UrlEncode(BXLoc.CurrentLocale));
	}

	private void RaiseFile()
	{
		if (raised)
			return;
		raised = true;

		if (!IsPostBack)
			return;

		var f = !string.IsNullOrEmpty(CachedId.Value) ? BXFile.GetByTempGuid(CachedId.Value, BXTextEncoder.EmptyTextEncoder) : null;
		
		if (StoredId.Value == "Y")
		{
			if (f != null)
				f.Delete();
			currentFile = originalFile;
		}
		else 
			currentFile = f;	
	}

	#region IBXCustomTypeEdit Members

	private string validationGroup = String.Empty;
	public string ValidationGroup
	{
		get { return validationGroup; }
		set { validationGroup = value; }
	}

	public void Initialize(BXCustomField currentField, BXCustomProperty currentValue)
	{
		field = currentField;
		if (field == null)
			return;

		BXParamsBag<object> settings = new BXParamsBag<object>(currentField.Settings);

		RequiredValidator.Enabled = currentField.Mandatory;
		if (RequiredValidator.Enabled)
			RequiredValidator.ErrorMessage = GetMessageFormat("Error.Required", field.EditFormLabel);

		SizeValidator.Enabled = settings.ContainsKey("MaxSize");
		if (SizeValidator.Enabled)
		{
			maxSize = (int)settings["MaxSize"];
			SizeValidator.ErrorMessage = GetMessageFormat("Error.IllegalSize", field.EditFormLabel, settings["MaxSize"]);
		}

		if (settings.ContainsKey("AllowedExtensions"))
			extensions = settings["AllowedExtensions"] as string[];
		ExtensionValidator.Enabled = (extensions != null && extensions.Length > 0);
		if (ExtensionValidator.Enabled)
			ExtensionValidator.ErrorMessage = GetMessageFormat("Error.IllegalExtension", field.EditFormLabel, string.Join(", ", extensions));
		value = currentValue;

		RequiredValidator.Enabled = field.Mandatory;
		if (RequiredValidator.Enabled)
			RequiredValidator.ErrorMessage = GetMessageFormat("Error.Required", field.EditFormLabel);

		addDescription = settings.GetBool("AddDescription");
		DescriptionBlock.Visible = addDescription;

		BXFile f = null;
		if (currentValue != null && (currentValue.Value != null && (int)currentValue.Value != 0) && (f = BXFile.GetById((int)currentValue.Value, BXTextEncoder.EmptyTextEncoder)) != null)
		{
			currentFile = originalFile = f;
			StoredId.Value = "Y";
			//DisplayName.Value = f.FileNameOriginal;
			//DisplaySize.Value = BXStringUtility.BytesToString(f.FileSize);
			//ContentType.Value = f.ContentType;
			if (addDescription)
				Description.Text = f.Description;
		}
	}

	public void Save(BXCustomPropertyCollection storage)
	{
		if (field == null)
			return;
		
		RaiseFile();

		object value = null;
		BXCustomType t = BXCustomTypeManager.GetCustomType(field.CustomTypeId);
		
		if (currentFile != null)
		{
			value = currentFile.Id;
			
			if (addDescription)
				currentFile.Description = Description.Text;
			currentFile.TempGuid = "";
			currentFile.Save();
		}

		if (originalFile != null && (currentFile == null || originalFile.Id != currentFile.Id))
			originalFile.Delete();

		originalFile = currentFile;
		 
		if (value != null)
		{
			storage[field.Name] = new BXCustomProperty(field.Name, field.Id, t.DbType, t.IsClonable ? false : field.Multiple, value, true);			
		}
		else if (string.IsNullOrEmpty(StoredId.Value))
		{
			storage[field.Name] = new BXCustomProperty(field.Name, field.Id, t.DbType, t.IsClonable ? false : field.Multiple, null, true);			
		}
		else 
			storage[field.Name] = this.value;
	}

	#endregion

	protected void SizeValidator_ServerValidate(object source, ServerValidateEventArgs args)
	{
		RaiseFile();
		args.IsValid = false;
		if (maxSize.HasValue)
		{
			if (currentFile != null && (originalFile == null || currentFile.Id != originalFile.Id))
			{				
				if (currentFile.FileSize > maxSize.Value)
					return;				
			}
		}
		args.IsValid = true;
	}
	protected void ExtensionValidator_ServerValidate(object source, ServerValidateEventArgs args)
	{
		RaiseFile();
		args.IsValid = false;
		if (extensions != null && extensions.Length > 0 && currentFile != null && (originalFile == null || currentFile.Id != originalFile.Id))
		{
			string ext = Path.GetExtension(currentFile.FileNameOriginal).ToLowerInvariant();
			while (ext.StartsWith("."))
				ext = ext.Substring(1);
			if (!BXSet.Contains(extensions, ext))
				return;
		}
		args.IsValid = true;
	}
	protected void RequiredValidator_ServerValidate(object source, ServerValidateEventArgs args)
	{
		RaiseFile();
		args.IsValid = HasValue;
	}
}
