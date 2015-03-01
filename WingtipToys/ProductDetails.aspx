<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ProductDetails.aspx.cs" Inherits="WingtipToys.ProductDetails" Title="Product Details" MasterPageFile="~/Site.Master" %>
<%@ Register assembly="Navigation" namespace="Navigation" tagprefix="nav" %>
<asp:Content ID="Content2" ContentPlaceHolderID="FeaturedContent" runat="server">
    <asp:FormView ID="productDetails" runat="server" ItemType="WingtipToys.Models.Product" SelectMethod ="GetProduct" RenderOuterTable="false">
        <ItemTemplate>
            <div>
                <h1><%#:Item.ProductName %></h1>
            </div>
            <br />
            <table>
                <tr>
                    <td>
                        <img src="/Catalog/Images/<%#:Item.ImagePath %>" border="1" alt="<%#:Item.ProductName %>" height="300" />
                    </td>
                    <td style="vertical-align: top">
                        <b>Description:</b><br /><%#:Item.Description %>
                        <br />
                        <span><b>Price:</b>&nbsp;<%#: String.Format("{0:c}", Item.UnitPrice) %></span>
                        <br />
                        <span><b>Product Number:</b>&nbsp;<%#:Item.ProductID %></span>
                        <br />
						<nav:NavigationHyperLink runat="server" Action="Add" ToData='<%# new NavigationData(){{ "productID" , Item.ProductID }} %>' >
                            <span class="ProductListItem">
                                <b>Add To Cart<b>
                            </span>           
						</nav:NavigationHyperLink>
                    </td>
                </tr>
            </table>
        </ItemTemplate>
    </asp:FormView>
</asp:Content>
