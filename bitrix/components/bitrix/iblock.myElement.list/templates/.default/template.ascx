<%@ Reference VirtualPath="~/bitrix/components/bitrix/iblock.myElement.list/component.ascx" %>
<%@ Import Namespace="Bitrix.IBlock" %>
<%@ Import Namespace="Bitrix.IBlock.Components" %>
<%@ Implements Interface="System.Web.UI.IPostBackEventHandler" %>
<%@ Control Language="C#" AutoEventWireup="false" Inherits="Bitrix.IBlock.Components.IBlockMyElementListTemplate"%>

<script runat="server">
    protected override void OnPreRender(EventArgs e)
    {
        if (!Component.IsUserAutorizedToManageMyElementList())
        {
            rpt.Visible = false;
            addItemLnk.Visible = false;
        }
        else
        {
            rpt.DataSource = Component.Items;
            rpt.DataBind();

            if (!IsElementCreationAllowed())
                addItemLnk.Visible = false;
            else
            {
                string url4Creation = Component.GetUrlForElementCreation();
                if (string.IsNullOrEmpty(url4Creation))
                    addItemLnk.Visible = false;
                else
                {
                    addItemLnk.NavigateUrl = url4Creation;
                    addItemLnk.Text = HttpUtility.HtmlEncode(Component.ElementCreationUrlTitle);
                }
            }
        }   
        base.OnPreRender(e);
    }

    private StringBuilder _sb = null;
    public string RenderButtonPanel(IBlockMyElementListComponent.Item item) 
    {
        bool permitMod = item.IsElementModificationAllowed();
        bool permitDel = item.IsElementDeletionAllowed();


        if (!permitMod && !permitDel)
            return string.Empty;

        if (_sb == null)
            _sb = new StringBuilder();
        else
            _sb.Length = 0; 
        
        _sb.Append("<div class=\"myElementListButtonListContainer\">");
        if (permitMod)
            _sb.AppendFormat("<span class=\"myElementListButtonContainer\"><a href=\"{3}\" title=\"{0}\" ><img src=\"{2}\"/>&nbsp;{1}</a></span>", HttpUtility.HtmlAttributeEncode(GetMessageRaw("EditElement")), HttpUtility.HtmlAttributeEncode(GetMessageRaw("EditElement").ToLower()), ResolveUrl("images/edit_button.gif"), HttpUtility.HtmlAttributeEncode(GetUrlForModification(item)));            
        if (permitDel)
            _sb.AppendFormat("<span class=\"myElementListButtonContainer\"><a href=\"\" onclick=\"if(!window.confirm('{4}')) return false; {3}; return false;\" title=\"{0}\" ><img src=\"{2}\"/>&nbsp;{1}</a></span>", HttpUtility.HtmlAttributeEncode(GetMessageRaw("DeleteElement")), HttpUtility.HtmlAttributeEncode(GetMessageRaw("DeleteElement")).ToLower(), ResolveUrl("images/delete_button.gif"), GetPostBackEventReferenceForDeletion(item), GetMessageJS("ElementDeletionConfirmation"));
        _sb.Append("</div>");
        
        return _sb.ToString();
    }
    public string RenderName(IBlockMyElementListComponent.Item item)
    {
        bool permitMod = item.IsElementModificationAllowed();
        
        if (_sb == null)
            _sb = new StringBuilder();
        else
            _sb.Length = 0;

        if (!permitMod)
            _sb.Append(item.GetElementName());
        else
            _sb.AppendFormat("<a href=\"{0}\">{1}</a>", GetUrlForModification(item), item.GetElementName() );

        return _sb.ToString();
    }
    
    public string GetUrlForModification(IBlockMyElementListComponent.Item item)
    {
        return ResolveUrl(Component.GetUrlForModification(item.GetElementId()));
    }

    public string GetPostBackEventReferenceForDeletion(IBlockMyElementListComponent.Item item)
    {
        return Page.ClientScript.GetPostBackEventReference(this, string.Concat("delete,", item.GetElementId().ToString()), false);        
    }

    protected override void Render(HtmlTextWriter writer)
    {
        Page.ClientScript.RegisterForEventValidation(this.UniqueID, Session.SessionID);
        base.Render(writer);
    }
    
    public void RaisePostBackEvent(string eventArgument)
    {
        try
        {
            Page.ClientScript.ValidateEvent(this.UniqueID, Session.SessionID);
        }
        catch (ArgumentException /*exc*/)
        {
            return;
        }
        
        if (string.IsNullOrEmpty(eventArgument))
            return;
        
        string[] paramArray = eventArgument.Split(new char[] { ',' });
        if (paramArray == null || paramArray.Length != 2)
            return;
        if (string.Equals(paramArray[0], "delete", StringComparison.Ordinal))
        {
            int? elementID = null;
            try
            {
                elementID = Convert.ToInt32(paramArray[1]);
            }
            catch (FormatException /*exc*/) { }
            catch (OverflowException /*exc*/) { }
            if (elementID.HasValue)
                Component.DeleteElement(elementID.Value);
        }
    }
</script>
<% 
    if (Component.IsPermissionDenied)
        return;
%>
<div class="myElementListLinkContainer">
    <asp:HyperLink CssClass="myElementListAddLink" runat="server" ID="addItemLnk"></asp:HyperLink>
</div>
<br />
<div class="myElements-list-pager">
<bx:IncludeComponent runat="server" ID="HeaderPager" ComponentName="bitrix:system.pager" Template="<%$ Parameters:PagingTemplate %>" CurrentPosition="top" />
</div>
<br />
<asp:Repeater runat="server" ID="rpt">
    <ItemTemplate>
        <div>
            <div class="myElementListBody">
                <%# HttpUtility.HtmlEncode(((IBlockMyElementListComponent.Item)Container.DataItem).GetElementCreationDateTime().ToShortDateString())%>
            </div>
            <div class="myElementListBody">
               <%# RenderName((IBlockMyElementListComponent.Item)Container.DataItem)%>
            </div>            
            <div class="myElementListText">
                <%# !string.IsNullOrEmpty(((IBlockMyElementListComponent.Item)Container.DataItem).GetElementPreviewText()) ? ((IBlockMyElementListComponent.Item)Container.DataItem).GetElementPreviewText() : ((IBlockMyElementListComponent.Item)Container.DataItem).GetElementDetailText() %>
            </div>
            <%# RenderButtonPanel((IBlockMyElementListComponent.Item)Container.DataItem) %>
        </div>
    </ItemTemplate>
    <SeparatorTemplate>
        <div class="myElementListSeparator"></div>
    </SeparatorTemplate>
</asp:Repeater>
<br />
<div class="myElements-list-pager">
<bx:IncludeComponent runat="server" ID="FooterPager" ComponentName="bitrix:system.pager" CurrentPosition="bottom" Template="<%$ Parameters:PagingTemplate %>"/>
</div>
