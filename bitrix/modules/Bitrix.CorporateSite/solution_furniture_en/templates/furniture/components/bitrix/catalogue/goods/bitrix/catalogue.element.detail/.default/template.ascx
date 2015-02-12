<%@ Reference Control="~/bitrix/components/bitrix/catalogue.element.detail/component.ascx" %>
<%@ Control Language="C#" AutoEventWireup="true" Inherits="Bitrix.IBlock.Components.CatalogueElementDetailTemplate" %>
<%@ Import Namespace="Bitrix.IBlock.Components" %>

<%
if (!String.IsNullOrEmpty(Component.errorMessage))
{ 
	%><span class="errortext"><%=Component.errorMessage%></span><%
}
else if (Component.Element != null)
{
    var price = Component.Element.CustomPublicValues.Get<int>("PRICE", 0);
    
	%>
	<div class="catalog-detail">
	<div class="catalog-item">
		<% 
		if (Component.DetailImage != null) 
		{ 
			%><div class="catalog-item-image">
				<img border="0" src="<%= Component.DetailImage.FilePath %>" width="<%= Component.DetailImage.Width %>" title="<%= Component.ElementName %>" height="<%= Component.DetailImage.Height %>" alt="<%= Component.DetailImage.Description %>" id="image_<%= Component.DetailImage.Id %>"/>
			</div><%
		}
		else if (Component.PreviewImage != null)
		{
			%><div class="catalog-item-image">
			<img class="preview-picture" border="0" title="<%= Component.ElementName %>" src="<%= Component.PreviewImage.FilePath %>" width="<%= Component.PreviewImage.Width %>"
				height="<%= Component.PreviewImage.Height %>" alt="<%= Component.PreviewImage.Description %>" /></div><%
		}
		%>
		<div class="<%=(Component.DetailImage!=null && Component.DetailImage.Width > 300 ? "catalog-item-desc" : "catalog-item-desc-float")  %>">
			<%
	            if (Component.Element.DetailText.Length > 0)
	            {
		            %><%= Component.Element.DetailText%><%
	            }
	            else if (Component.Element.PreviewText.Length > 0)
	            {
		            %><%= Component.Element.PreviewText%><%
	            }
            	
	            %>
            		
		</div>
		<% if ( price > 0 ){ %>
        <div class="catalog-item-price">
            <span><%= GetMessageRaw("Price") %>:</span> <%= price.ToString()+" "+Component.Element.CustomPublicValues.GetHtml("PRICECURRENCY") %>
        </div>
        <% } %>
        <div class="catalog-item-properties">
					<div class="catalog-item-properties-title"><%= GetMessageRaw("Characteristics") %></div>
					<%
		foreach (var field in Component.Element.CustomFields)
		{
            if (Component.ShowProperties.Contains(field.Name))
            { 
                var value = Component.Element.CustomPublicValues.GetHtml(field.Name);
                var fieldDispName = string.Empty;
                if (field.Localization[Bitrix.BXSite.Current.LanguageId] != null)
                    fieldDispName = field.Localization[Bitrix.BXSite.Current.LanguageId].EditFormLabel;
                else
                    fieldDispName = field.Name;

                if (!String.IsNullOrEmpty(fieldDispName) && !String.IsNullOrEmpty(value))
                {
				%><div class="catalog-item-property"><span><%=(fieldDispName ?? field.Name)%></span> <b><%=value%></b></div><%
                        }
            }
		}
		%>
            </div>
</div></div><%
} 
%>

<%
	
%>
<% 
	string title = Parameters.Get("Template_BackTitle");
 %>