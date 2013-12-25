<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="FileUploader.ascx.cs"
    Inherits="FileUploader.FileUploader" %>
<style type="text/css">
    .fileUploader_Table
    {
        border: solid 0px;
        font-size: 12px;
        font-family: Arial, 微软雅黑, 'Microsoft YaHei' ‎;
    }
    .fileUploader_Table td
    {
        border: solid 0px;
        max-width: 190px;
        overflow: hidden;
        text-overflow: ellipsis;
        -o-text-overflow: ellipsis;
        text-align: left;
        padding: 8px 8px;
    }
    .fileUploader_Table th
    {
        border: solid 0px;
        max-width: 190px;
        overflow: hidden;
        text-overflow: ellipsis;
        -o-text-overflow: ellipsis;
        text-align: left;
        font-weight: normal;
        padding: 8px 8px;
    }
    .fileUploader_Header
    {
        background-color: #DFDFDF;
        color: #444444;
        height: 40px;
    }
    .fileUploader_Footer
    {
        background-color: #DFDFDF;
        color: #444444;
        height: 40px;
    }
    .fileUploader_Rows
    {
        height: 30px;
        vertical-align: middle;
    }
    .refreshButton_Hide
    {
        display: none;
    }
</style>
<script type="text/javascript">
    $(document).ready(function () {
        hideRefreshButton()
    });

    function showRefreshButton() {
        $("[id*='btnUpdate']").show();
        $("[id*='btnUpdateMask']").show();
    }
    function hideRefreshButton() {
        $("[id*='btnUpdate']").hide();
        $("[id*='btnUpdateMask']").hide();
    }
</script>
<div id="uploaderContainerDiv" runat="server" style="position: relative">
    <div id="btnUpdateContainerDiv" runat="server">
        <div id="btnUpdateMask" style="background-color: rgba(0, 0, 0, 0.5); width: 100%;
            height: 100%;">
            <div id="inner" style="padding-top: 40%; padding-left: 40%;">
                <asp:ImageButton ID="btnUpdate" runat="server" OnClick="btnUpdate_OnClick" />
            </div>
        </div>
    </div>
    <div style="margin-left: 7px; margin-right: 7px; padding-top: 10px; padding-bottom: 7px;
        background-color: #3F4246; font-family: Arial, 微软雅黑, 'Microsoft YaHei'‎; height: 38px;">
        <span style="color: #FFF; margin-left: 61px; font-size: 16px;">
            <asp:Label ID="lblUploadedFile" runat="server"></asp:Label>
        </span>
        <br />
        <span style="color: #FFF; margin-left: 61px; font-size: 12px;">
            <asp:Label ID="lblUploadedFile2" runat="server"></asp:Label>
        </span>
    </div>
    <div id="uploadedFile" style="margin-left: 7px; margin-right: 7px; font-family: Arial, 微软雅黑, 'Microsoft YaHei'‎;">
        <asp:UpdatePanel ID="upUploadedFiles" runat="server">
            <Triggers>
                <asp:PostBackTrigger ControlID="gvUploadedFiles" />
            </Triggers>
            <ContentTemplate>
                <asp:GridView ID="gvUploadedFiles" runat="server" AllowPaging="False" AutoGenerateColumns="False"
                    Width="100%" CssClass="fileUploader_Table" ShowHeaderWhenEmpty="true" OnRowDataBound="gvUploadedFiles_RowDataBound"
                    OnRowCommand="gvUploadedFiles_RowCommand" OnRowDeleting="gvUploadedFiles_RowDeleting"
                    DataKeyNames="FileName">
                    <Columns>
                        <asp:BoundField DataField="FileName" HeaderText="文件名" HeaderStyle-Width="50%" />
                        <asp:BoundField DataField="UploadTime" HeaderText="上传时间" HeaderStyle-Width="30%" />
                        <asp:TemplateField HeaderText="下载" HeaderStyle-Width="10%">
                            <ItemTemplate>
                                <asp:ImageButton ID="ibtnDownload" runat="server" CommandArgument="<%# Container.DataItemIndex %>"
                                    CommandName="download" />
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="删除" HeaderStyle-Width="10%">
                            <ItemTemplate>
                                <asp:ImageButton ID="ibtnDelete" runat="server" CommandArgument="<%# Container.DataItemIndex %>"
                                    CommandName="delete" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <HeaderStyle CssClass="fileUploader_Header"></HeaderStyle>
                    <RowStyle CssClass="fileUploader_Rows" Wrap="False" HorizontalAlign="Center"></RowStyle>
                </asp:GridView>
            </ContentTemplate>
        </asp:UpdatePanel>
    </div>
    <div id="uploader" runat="server" onclick="showRefreshButton()">
        <p>
            <asp:Label ID="lblUnsupportedMessage" runat="server"></asp:Label></p>
    </div>
    <asp:UpdatePanel ID="upFileUploader" runat="server">
    </asp:UpdatePanel>
</div>
