using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Bitrix.DataLayer;
using Bitrix.Forum;
using Bitrix.Services.Text;

namespace Bitrix.IBlock.Components
{
	public partial class CatalogueForum : UserControl, CatalogueComponent.IForum
	{
		public CatalogueComponent.ForumInfo[] GetForums()
		{
			BXForumCollection col = BXForum.GetList(
				null,
				new BXOrderBy(new BXOrderByPair(BXForum.Fields.Name, BXOrderByDirection.Asc)),
				new BXSelect(
					BXSelectFieldPreparationMode.Normal,
					BXForum.Fields.Id,
					BXForum.Fields.Name),
					null,
					BXTextEncoder.EmptyTextEncoder);


			CatalogueComponent.ForumInfo[] r = new CatalogueComponent.ForumInfo[col.Count];
			for(int i = 0; i < col.Count; i++)
				r[i] = new CatalogueComponent.ForumInfo(col[i].Id, col[i].Name);
			
			return r;
		}
	}
}
