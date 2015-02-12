using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.IBlock.PublicEditors;
using Bitrix.UI;

namespace Bitrix.IBlock.UI
{

	public partial class IBlockPublicEditorGroupRenderer : System.Web.UI.UserControl
	{

		protected List<BXIBlockPublicEditorElementField> fields = new List<BXIBlockPublicEditorElementField>();
		protected string title;

		public int IBlockId { get; private set; }
		public int IBlockElementId { get; private set; }

		public string Title { get { return title; } set { title = value; } }

		protected bool DetailTextWebEditorIncluded
		{
			get;
			set;
		}

		protected bool PreviewTextWebEditorIncluded
		{
			get;
			set;
		}	

		protected bool IsAdminPage { get; set; }

		public void SetInfo(IEnumerable<BXIBlockPublicEditorElementField> fields, int iblockId, int iblockElementId, bool isAdminPage)
		{
			if (iblockId <= 0)
				throw new ArgumentException("iblockId must be greater than zero");

			IBlockId = iblockId;

			IBlockElementId = iblockElementId;

			IsAdminPage = isAdminPage;

			if (fields != null)
			{
				foreach (var f in fields)
					this.fields.Add(f);
			}
		}

		protected override void OnInit(EventArgs e)
		{
			
			var catalog = fields.Find(x => x.Id.Equals("Catalog"));

			if (catalog != null)
			{
				var editor = catalog.PublicEditor;
				if (editor != null)
				{
					var catalogEditorControl = LoadControl("~/bitrix/admin/controls/catalog/IBlockElementEditor.ascx") as BXIBlockElementEditor;
					if (catalogEditorControl != null)
					{
						if (IBlockId <= 0 )
							throw new ArgumentException("IBlockId must be greater than zero");
						catalogEditorControl.Attributes["IBlockElementId"] = IBlockElementId.ToString();
						catalogEditorControl.Attributes["IBlockId"] = IBlockId.ToString();
						if (catalogEditorControl.IsEnabled())
						{
							CataloguePlaceHolder.Controls.Add(catalogEditorControl);
							editor.SetControl((catalogEditorControl as Control));
						}
					}
				}
			}


			var detailText =
				fields.Find(x => x.Id.Equals("DetailText"));

			if (detailText != null)
			{
				var editor = detailText.PublicEditor as ElementDetailText;
				if (editor != null && editor.AllowWebEditor)
				{
					editor.AttachWebEditor(DetailTextPlaceHolder);
					DetailTextWebEditorIncluded = true;
				}
			}

			var preview =
			fields.Find(x => x.Id.Equals("PreviewText"));

			if (preview != null)
			{
				var editor = preview.PublicEditor as ElementPreviewText;
				if (editor != null && editor.AllowWebEditor)
				{
					editor.AttachWebEditor(PreviewTextPlaceHolder);
					PreviewTextWebEditorIncluded = true;
				}
			}
			if (IsAdminPage)
			{
				var detailImage =
				fields.Find(x => x.Id.Equals("DetailImage"));

				if (detailImage != null)
				{
					var detailEditor = LoadControl("~/bitrix/admin/controls/main/adminimagefield.ascx") as BXAdminImageField;
					detailEditor.SetMaxImageDisplaySize(400, 400);
					detailImage.PublicEditor.SetControl(detailEditor);
					DetailImagePlaceHolder.Controls.Add(detailEditor);
				}

				var previewImage =
					fields.Find(x => x.Id.Equals("PreviewImage"));

				if (previewImage != null)
				{
					var previewEditor = LoadControl("~/bitrix/admin/controls/main/adminimagefield.ascx") as BXAdminImageField;
					previewEditor.SetMaxImageDisplaySize(400, 400);
					previewImage.PublicEditor.SetControl(previewEditor);
					PreviewImagePlaceHolder.Controls.Add(previewEditor);
				}

				//var previewImage =
				//fields.Find(x => x.Id.Equals("PreviewImage"));

				//if (preview != null)
				//{
				//    var editor = preview.PublicEditor as ElementPreviewText;
				//    if (editor != null && editor.AllowWebEditor)
				//    {
				//        editor.AttachWebEditor(PreviewTextPlaceHolder);
				//        PreviewTextWebEditorIncluded = true;
				//    }
				//}
			}

			base.OnInit(e);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

		}
	}
}
