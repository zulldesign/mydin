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
using Bitrix;
using Bitrix.UI;
using System.Text;
using System.Collections.Generic;
using Bitrix.IO;
using Bitrix.DataLayer;
using Bitrix.Services;
using Bitrix.Services.Text;
using System.IO;

public partial class BXCustomTypeFile : BXCustomType
{
	public override string Title
	{
		get
		{
			return GetMessage("Title");
		}
	}

	public override string Description
	{
		get
		{
			return GetMessage("Description");
		}
	}

	public override SqlDbType DbType
	{
		get
		{
			return SqlDbType.Int;
		}
	}


	public override string TypeName
	{
		get
		{
			return "Bitrix.System.File";
		}
	}

	public override Control Settings
	{
		get
		{
			return LoadControl("settings.ascx");
		}
	}

	public override Control Edit
	{
		get
		{
			return LoadControl("edit.ascx");
		}
	}

	public override Control AdvancedSettings
	{
		get
		{
			throw new Exception("The method or operation is not implemented.");
		}
	}

	public override BXCustomTypePublicView CreatePublicView()
	{
		return new PublicView();
	}

	public override Control GetFilter(BXCustomField field)
	{
		return null;
	}
	public override bool IsFile
	{
		get
		{
			return true;
		}
	}


	public override BXCustomTypePublicEdit CreatePublicEditor()
	{
		return new PublicEditor(this);
	}


	class PublicEditor : BXCustomTypePublicEdit
	{
		//FIELDS
		private BXCustomType type;
		private BXCustomField field;
		private BXCustomProperty property;
		private FieldEditor editor;
		private int maxSize;
		private List<string> allowedExtensions;
		private int textboxSize;

		//CONSTRUCTORS
		public PublicEditor(BXCustomType type)
		{
			this.type = type;
		}

		//METHODS
		private string RenderFile(BXFile file, string formName, string id, bool allowDelete, bool addPlaceholder, bool showUpload)
		{
			return
				(file != null
				? string.Format(
					allowDelete
						? string.Format(
							@"<input type=""checkbox"" name=""{0}$save"" id=""{1}"" value=""{2}"" checked=""checked"" /><label for=""{1}""> {{0}}</label><br/>",
							HttpUtility.HtmlEncode(formName),
							HttpUtility.HtmlEncode(id),
							file.Id
						)
						: @"{0}<br/>",
					string.Format(
						@"{0} ({1}) {2}",
						HttpUtility.HtmlEncode(file.FileNameOriginal ?? string.Empty),
						HttpUtility.HtmlEncode(file.ContentType ?? string.Empty),
						HttpUtility.HtmlEncode(BXStringUtility.BytesToString(file.FileSize))
					)
				)
				: string.Empty)
				+ (showUpload
					? string.Format(
						@"<input type=""file"" class=""bx-custom-field custom-field-file"" name=""{0}"" {1}/>",
						HttpUtility.HtmlEncode(formName),
						textboxSize > 0 ? (@"size=""" + textboxSize + @""" ") : string.Empty
					)
					: string.Empty)
				+ (addPlaceholder ? @" {0}" : null);
		}
		private BXFile ValidateFile(HttpPostedFile file, ICollection<string> errors)
		{
			bool result = true;
			if (maxSize > 0 && file.ContentLength > maxSize)
			{
				result = false;
				errors.Add(HttpUtility.HtmlEncode(string.Format(
					BXLoc.GetMessage(type, "Error.FileSizeExceeded"),
					file.FileName,
					BXStringUtility.BytesToString(maxSize)
				)));
			}
			if (allowedExtensions.Count != 0)
			{
				string ext = Path.GetExtension(file.FileName);
				if (ext != null)
					ext = ext.TrimStart('.').ToLowerInvariant();
				if (!allowedExtensions.Contains(ext))
				{
					result = false;
					errors.Add(HttpUtility.HtmlEncode(string.Format(
						BXLoc.GetMessage(type, "Error.InvalidFileExtension"),
						file.FileName,
						string.Join(", ", allowedExtensions.ToArray())
					)));
				}
			}
			if (!result)
				return null;

			BXFile f = new BXFile(file, "uf", "main", string.Empty);
			if (!BXSecureIO.CheckUpload(f.FileVirtualPath))
			{
				errors.Add(HttpUtility.HtmlEncode(string.Format(BXLoc.GetMessage(type, "Error.InsufficientRightsToUpload"), file.FileName)));
				return null;
			}
			return f;
		}

		public override void Init(BXCustomField field)
		{
			this.field = field;
			maxSize = field.Settings.GetInt("MaxSize");
			IEnumerable<string> ae = field.Settings.Get<IEnumerable<string>>("AllowedExtensions");
			allowedExtensions = ae != null ? new List<string>(ae) : new List<string>();
			for (int i = 0; i < allowedExtensions.Count; i++)
				allowedExtensions[i] = allowedExtensions[i].Trim().ToLowerInvariant();
			textboxSize = field.Settings.GetInt("TextBoxSize");
			editor = field.Multiple ? (FieldEditor)new MultipleFieldEditor(this) : (FieldEditor)new SingleFieldEditor(this);
		}
		public override void Load(BXCustomProperty property)
		{
			this.property = property;
			editor.Load();
		}
		public override string Render(string formFieldName, string uniqueID)
		{
			return editor.Render(formFieldName, uniqueID);
		}
		protected override void Save(string formFieldName, BXCustomPropertyCollection storage)
		{
			editor.Save(formFieldName, storage);
		}
		protected override bool Validate(string formFieldName, ICollection<string> errors)
		{
			return editor.Validate(formFieldName, errors);
		}


		//NESTED CLASSES
		abstract class FieldEditor
		{
			protected PublicEditor editor;

			public FieldEditor(PublicEditor editor)
			{
				this.editor = editor;
			}

			public abstract void Load();
			public abstract string Render(string formFieldName, string uniqueID);
			public abstract void Save(string formFieldName, BXCustomPropertyCollection storage);
			public abstract bool Validate(string formFieldName, ICollection<string> errors);
		}
		sealed class SingleFieldEditor : FieldEditor
		{
			BXFile file;
			BXFile store;
			BXFile save;
			BXFile delete;

			public SingleFieldEditor(PublicEditor editor)
				: base(editor)
			{

			}

			public override void Load()
			{
				if (editor.property.Value == null)
					return;

				file = BXFile.GetById((int)editor.property.Value);
			}
			public override string Render(string formFieldName, string uniqueID)
			{
				return editor.RenderFile(file, formFieldName, uniqueID, !editor.field.Mandatory, false, true);
			}
			public override void Save(string formFieldName, BXCustomPropertyCollection storage)
			{
				if (delete != null)
				{
					delete.Delete();
					delete = null;
				}

				file = store;

				if (save != null)
				{
					save.Save();

					if (file != null)
						file.Delete();
					file = save;
				}

				BXCustomProperty p = new BXCustomProperty(editor.field.Name, editor.field.Id, editor.type.DbType, false, null, true);
				if (file != null)
					p.Values.Add(file.Id);
				storage[editor.field.Name] = p;
			}
			public override bool Validate(string formFieldName, ICollection<string> errors)
			{
				HttpRequest request = HttpContext.Current.Request;

				// If the field is mandatory, then there is no checkbox
				store = file;
				if (!editor.field.Mandatory)
				{
					string save = request.Form[formFieldName + "$save"];
					int i;
					if (store != null && (!int.TryParse(save, out i) || store.Id != i))
					{
						delete = store;
						store = null;
					}
				}

				HttpPostedFile upload = request.Files[formFieldName];
				if (upload != null && upload.ContentLength != 0)
				{
					BXFile uploaded = editor.ValidateFile(upload, errors);
					if (uploaded == null)
						return false;

					save = uploaded;
				}

				if (editor.field.Mandatory && store == null && save == null)
				{
					errors.Add(HttpUtility.HtmlEncode(string.Format(BXLoc.GetMessage(editor.type, "Error.FieldIsRequired"), editor.field.TextEncoder.Decode(editor.field.EditFormLabel))));
					return false;
				}

				return true;
			}
		}
		sealed class MultipleFieldEditor : FieldEditor
		{
			List<BXFile> files = new List<BXFile>();
			List<BXFile> store = new List<BXFile>();
			List<BXFile> delete = new List<BXFile>();
			List<BXFile> save = new List<BXFile>();


			public MultipleFieldEditor(PublicEditor editor)
				: base(editor)
			{

			}

			public override void Load()
			{
				if (editor.property.Values.Count == 0)
					return;

				int[] files = editor.property.Values.ConvertAll<int>(delegate(object input)
				{
					return (int)input;
				}).ToArray();

				this.files = BXFile.GetList(
					new BXFilter(new BXFilterItem(BXFile.Fields.Id, BXSqlFilterOperators.In, files)),
					null
				);
			}
			public override string Render(string formFieldName, string uniqueID)
			{
				int counter = 0;
				List<string> items = files.ConvertAll<string>(delegate(BXFile input)
				{
					return editor.RenderFile(input, formFieldName, uniqueID + counter++, true, false, false);
				});
				return BXCustomTypeHelper.GetMultipleView
				(
					items,
					editor.RenderFile(null, formFieldName, uniqueID, false, true, true),
					uniqueID
				);
			}
			public override void Save(string formFieldName, BXCustomPropertyCollection storage)
			{
				foreach (BXFile d in delete)
					d.Delete();

				foreach (BXFile s in save)
					s.Save();

				files.Clear();
				files.AddRange(store);
				files.AddRange(save);

				save.Clear();

				BXCustomProperty p = new BXCustomProperty(editor.field.Name, editor.field.Id, editor.type.DbType, true, null, true);
				foreach (BXFile f in files)
					p.Values.Add(f.Id);
				storage[editor.field.Name] = p;
			}
			public override bool Validate(string formFieldName, ICollection<string> errors)
			{
				HttpRequest request = HttpContext.Current.Request;

				string[] saves = request.Form.GetValues(formFieldName + "$save");
				List<int> saveList = new List<int>();

				if (saves != null && saves.Length != 0)
				{
					int i;
					foreach (string save in saves)
						if (int.TryParse(save, out i))
							saveList.Add(i);
				}
				foreach (BXFile f in files)
				{
					if (saveList.Contains(f.Id))
						store.Add(f);
					else
						delete.Add(f);
				}

				bool result = true;
				string[] uploads = request.Files.AllKeys;
				if (uploads != null)
				{
					for (int i = 0; i < uploads.Length; i++)
					{
						if (uploads[i] != formFieldName)
							continue;

						HttpPostedFile upload = request.Files[i];
						if (upload.ContentLength == 0)
							continue;

						BXFile uploaded = editor.ValidateFile(upload, errors);

						if (uploaded == null)
						{
							result = false;
							continue;
						}

						save.Add(uploaded);
					}
				}

				if (editor.field.Mandatory && store.Count + save.Count == 0)
				{
					result = false;
					errors.Add(HttpUtility.HtmlEncode(string.Format(BXLoc.GetMessage(editor.type, "Error.FieldIsRequired"), editor.field.TextEncoder.Decode(editor.field.EditFormLabel))));
				}

				return result;
			}
		}
	}
	class PublicView : BXCustomTypePublicView
	{
		private Dictionary<int, BXFile> files;

		private Dictionary<int, BXFile> BuildFiles()
		{
			Dictionary<int, BXFile> files = new Dictionary<int, BXFile>();
			if(property.Values.Count > 0)
			{
				if (property.IsMultiple && property.Values.Count > 1)
				{
					BXFileCollection fileCol = BXFile.GetList(
						new BXFilter(new BXFilterItem(BXFile.Fields.Id, BXSqlFilterOperators.In, property.Values)),
						null, null, null, BXTextEncoder.EmptyTextEncoder);

					foreach (BXFile file in fileCol)
						files[file.Id] = file;
				}
				else
				{
					BXFile file = BXFile.GetById((int)property.Values[0], BXTextEncoder.EmptyTextEncoder);
					if(file != null)
						files[file.Id] = file;
				}
			}
			return files;
		}

		public override string GetHtml(string uniqueId, string separatorHtml)
		{
			if (property == null || property.Values.Count == 0)
				return string.Empty;

			files = files ?? BuildFiles();

			StringBuilder s = new StringBuilder();
			foreach (object value in property.Values)
			{
				if (value == null)
					continue;

				BXFile f;
				if (!files.TryGetValue((int)value, out f))
					continue;

				if (s.Length > 0)
					s.Append(separatorHtml);

				s.AppendFormat(@"<a href=""{0}"">{1}</a>", HttpUtility.HtmlEncode(f.FilePath), HttpUtility.HtmlEncode(f.FileName));
			}
			return s.ToString();
		}
		public override void Render(string uniqueId, string separatorHtml, HtmlTextWriter writer)
		{
			if (property == null || property.Values.Count == 0)
				return;

			files = files ?? BuildFiles();

			bool separate = false;
			foreach (object value in property.Values)
			{
				if (value == null)
					continue;

				BXFile f;
				if (!files.TryGetValue((int)value, out f))
					continue;
				
				if (separate)
					writer.Write(separatorHtml);
				else
					separate = true;

				writer.Write(@"<a");
				writer.WriteAttribute("href", f.FilePath, true);
				writer.Write(@">");
				writer.WriteEncodedText(f.FileName);
				writer.Write(@"</a>");

			}
		}
		public override bool IsEmpty
		{
			get
			{
				if (property == null || property.Values.Count == 0)
					return true;

				files = files ?? BuildFiles();

				foreach (object value in property.Values)
				{
					if (value == null)
						continue;

					if (files.ContainsKey((int)value))
						return false;
				}
				return true;
			}
		}
	}
}
