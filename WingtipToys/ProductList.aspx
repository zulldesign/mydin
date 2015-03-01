<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ProductList.aspx.cs" Inherits="WingtipToys.ProductList" Title="Products" MasterPageFile="~/Site.Master" %>
<%@ Register assembly="Navigation" namespace="Navigation" tagprefix="nav" %>
<asp:Content ID="Content2" ContentPlaceHolderID="FeaturedContent" runat="server">
    <section class="featured">
        <div class="content-wrapper">
            <hgroup class="title">
                <h1><%: Page.Title %></h1>
            </hgroup>

                 <section class="featured">
                    <ul> 
                        <asp:ListView ID="productList" runat="server"
                            DataKeyNames="ProductID"
                            GroupItemCount="3" ItemType="WingtipToys.Models.Product" SelectMethod="GetProducts">
                            <EmptyDataTemplate>      
                                <table id="Table1" runat="server">        
                                    <tr>          
                                        <td>No data was returned.</td>        
                                    </tr>     
                                </table>  
                            </EmptyDataTemplate>  
                            <EmptyItemTemplate>     
                                <td id="Td1" runat="server" />  
                            </EmptyItemTemplate>  
                            <GroupTemplate>    
                                <tr ID="itemPlaceholderContainer" runat="server">      
                                    <td ID="itemPlaceholder" runat="server"></td>    
                                </tr>  
                            </GroupTemplate>  
                            <ItemTemplate>    
                                <td id="Td2" runat="server">      
                                    <table>        
                                        <tr>          
                                            <td>&nbsp;</td>          
                                            <td>
												<nav:NavigationHyperLink runat="server" Action="Select" ToData='<%# new NavigationData(){{ "productName" , Item.ProductName }} %>' >
                                                    <image src='/Catalog/Images/Thumbs/<%#:Item.ImagePath%>'
                                                        width="100" height="75" border="1"/>
												</nav:NavigationHyperLink>
                                            </td>
                                            <td>
												<nav:NavigationHyperLink runat="server" Action="Select" ToData='<%# new NavigationData(){{ "productName" , Item.ProductName }} %>' >
                                                    <%#:Item.ProductName%>
												</nav:NavigationHyperLink>
                                                <br />
                                                <span class="ProductPrice">           
                                                    <b>Price: </b><%#:String.Format("{0:c}", Item.UnitPrice)%>
                                                </span>
                                                <br />
												<nav:NavigationHyperLink runat="server" Action="Add" ToData='<%# new NavigationData(){{ "productID" , Item.ProductID }} %>' >
                                                    <span class="ProductListItem">
                                                        <b>Add To Cart<b>
                                                    </span>           
												</nav:NavigationHyperLink>
                                            </td>        
                                        </tr>      
                                    </table>    
                                </td>  
                            </ItemTemplate>  
                            <LayoutTemplate>    
                                <table id="Table2" runat="server">      
                                    <tr id="Tr1" runat="server">        
                                        <td id="Td3" runat="server">          
                                            <table ID="groupPlaceholderContainer" runat="server">            
                                                <tr ID="groupPlaceholder" runat="server"></tr>          
                                            </table>        
                                        </td>      
                                    </tr>      
                                    <tr id="Tr2" runat="server"><td id="Td4" runat="server"></td></tr>    
                                </table>  
                            </LayoutTemplate>
                        </asp:ListView>
                    </ul>
               </section>
        </div>
    </section>
</asp:Content>