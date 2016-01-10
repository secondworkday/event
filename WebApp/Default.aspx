<%@ Page Title="" Language="C#" MasterPageFile="~/master/TorqMaster.master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="WebApp.Default" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContentPlaceHolder" runat="Server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContentPlaceHolder" runat="Server">
    <div class="home">
        <div class="col1">
            <div class="row1">
                <div class="project_outer">
                    <div class="buttons">
                        <div id="ButtonSpacer" runat="server" class="button_spacer">&nbsp;</div>
                        <a href="project/CreateProject.aspx" class="create_project_btn">Create Project</a>
                        <a href="project/Default.aspx" class="open_project_btn">Open Project</a>
                        <a href="workshop/" class="workshop_btn" id="WorkshopButton" runat="server" visible="false">Start Workshop</a>
                    </div>
                    <div class="clear"></div>
                </div>
            </div>
            <div class="row2">
                <div class="tutorials_outer">
                    <h2>TORQ TUTORIALS</h2>
                    <div class="tutorials_list">
                        <asp:ListView ID="VideoContentList" runat="server">
                            <LayoutTemplate>
                                <ul>
                                    <asp:PlaceHolder ID="ItemPlaceholder" runat="server" />
                                </ul>
                            </LayoutTemplate>
                            <ItemTemplate>
                                <li>
                                    <div class="play">
                                    </div>
                                    <div class="text">
                                        <a href="<%# Eval("Url")%>" rel="0"
                                            class="title newWindow"><%# Eval("Title")%></a> <%# Eval("Description")%> <a href="<%# Eval("Url")%>"
                                                rel="0" class="newWindow nowrap">[ play >]</a>
                                    </div>
                                    <div class="clear"></div>
                                </li>
                            </ItemTemplate>
                        </asp:ListView>
                    </div>
                </div>
            </div>
        </div>
        <div class="col2">
            <h2>TORQ IN ACTION</h2>
            <div class="news_list">
                <ul>
                    <asp:Repeater ID="TorqInActionContents" runat="server">
                        <ItemTemplate>
                            <li>
                                <div class="icon"></div>
                                <span class="state"><%# DataBinder.Eval(Container.DataItem, "Dateline") %></span>
                                <%# DataBinder.Eval(Container.DataItem, "Subject") %>
                                <a href="News.aspx?article=<%# Container.ItemIndex+1 %>" class="nowrap">[ more >]</a>
                            </li>
                        </ItemTemplate>
                    </asp:Repeater>
                </ul>
            </div>
        </div>
        <div class="clear">
        </div>
    </div>
</asp:Content>
