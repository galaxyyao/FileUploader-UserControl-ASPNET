using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Web.UI.HtmlControls;
using System.Text;
using System.Data;

namespace FileUploader
{
    public partial class FileUploader : System.Web.UI.UserControl
    {
        public FileUploader()
        {
            SupportingFileSuffix = new Dictionary<string, List<string>>();
            UploadedFiles = new List<File>();
            LatestUploadedFiles = new List<File>();
            IsAllowDownload = true;
            IsAllowMultipleFile = true;
            IsAllowDelete = true;
        }

        /// <summary>
        /// Path for uploaded files
        /// Default to be folder "Uploads"
        /// eg.: Uploads
        /// </summary>
        public string UploadPath
        {
            get;
            set;
        }

        public enum LanguageEnum
        {
            zh_cn = 0,
            en_us
        }

        /// <summary>
        /// Language to display
        /// If you want to add more language, add it to LanguageEnum and update InitializeParms() method
        /// </summary>
        public LanguageEnum Language
        {
            get;
            set;
        }

        /// <summary>
        /// If multiple file upload is allowed
        /// //TODO: not done yet
        /// </summary>
        public bool IsAllowMultipleFile
        {
            get;
            set;
        }

        public bool IsAllowDownload
        {
            get;
            set;
        }

        public bool IsAllowDelete
        {
            get;
            set;
        }

        public int Width
        {
            get;
            set;
        }

        public int Height
        {
            get;
            set;
        }

        public int MaxFileSizeInMB
        {
            get;
            set;
        }

        public string PageRelativeUrlToRoot
        {
            get;
            set;
        }

        public Dictionary<string, List<string>> SupportingFileSuffix
        {
            get;
            set;
        }

        public enum RenameMethodEnum
        {
            EntireRandom = 0,
            CustomPrefix_DateSuffix,
            CustomName
        }

        public RenameMethodEnum RenameMethod
        {
            get;
            set;
        }

        public string RenamePrefix
        {
            get;
            set;
        }

        public string RenameName
        {
            get;
            set;
        }

        public class File
        {
            public string Name
            {
                get;
                set;
            }

            public string Location
            {
                get;
                set;
            }

            public DateTime UploadTime
            {
                get;
                set;
            }

            public string UploadUser
            {
                get;
                set;
            }
        }

        public List<File> UploadedFiles
        {
            get;
            set;
        }

        public List<File> LatestUploadedFiles
        {
            get;
            private set;
        }

        public string UploadUser
        {
            get;
            set;
        }

        public delegate void OnFileUploadedDelegate();
        public event OnFileUploadedDelegate OnFileUploaded;


        public UpdatePanelUpdateMode UpdateMode
        {
            get { return this.upFileUploader.UpdateMode; }
            set { this.upFileUploader.UpdateMode = value; }
        }

        public void Update()
        {
            this.upFileUploader.Update();
        }

        protected void btnUpdate_OnClick(object sender, EventArgs e)
        {
            RefreshUploadedFileGridView();
            upFileUploader.Update();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            LoadScript();

            string outMessage;
            bool isPassed = InitializeParms(out outMessage);
            if (!isPassed)
            {
                throw new Exception(outMessage);
                return;
            }

            //set gridview width
            uploaderContainerDiv.Attributes.Add("style", "width: " + Width + "px; overflow: auto; margin-top: 5px; float:left;height:" + Height + "px;");
            btnUpdateContainerDiv.Attributes.Add("style", "width: " + Width + "px; height:" + Height + "px; z-index: 10; position: absolute;");
            StringBuilder relativePathPrefix = new StringBuilder();
            for (int i = 0; i < PageRelativeUrlToRoot.Count(c => c == '/') - 1; i++)
            {
                relativePathPrefix.Append("/..");
            }
            btnUpdate.ImageUrl = string.Format("~{0}/Scripts/plupload/jquery.plupload.queue/img/refresh.png", relativePathPrefix); ;

            // Check to see whether there are uploaded files to process them
            if (Request.Files.Count > 0)
            {
                if (UploadedFiles.Count >= 1 && IsAllowMultipleFile == false)
                {
                    return;
                }

                int chunk = Request["chunk"] != null ? int.Parse(Request["chunk"]) : 0;
                HttpPostedFile fileUpload = Request.Files[0];
                string fileName = GetFileName(fileUpload.ContentType);

                var uploadPath = Server.MapPath(string.Format("~{0}", UploadPath));
                using (var fs = new FileStream(Path.Combine(uploadPath, fileName), chunk == 0 ? FileMode.Create : FileMode.Append))
                {
                    var buffer = new byte[fileUpload.InputStream.Length];
                    fileUpload.InputStream.Read(buffer, 0, buffer.Length);
                    fs.Write(buffer, 0, buffer.Length);

                    File uploadedFile = new File();
                    uploadedFile.Name = fileName;
                    uploadedFile.Location = Path.Combine(uploadPath, fileName);
                    uploadedFile.UploadUser = UploadUser;
                    uploadedFile.UploadTime = DateTime.Now;
                    LatestUploadedFiles.Add(uploadedFile);
                    if (chunk == 0)
                    {
                        UploadedFiles.Remove(UploadedFiles.Find(f => f.Name == fileName));
                    }
                    UploadedFiles.Add(uploadedFile);
                    OnFileUploaded();
                }
            }

            if (!IsPostBack)
            {
                RefreshUploadedFileGridView();
            }
        }

        private string GetFileName(string contentType)
        {
            string fileName = string.Empty;
            string s = Request.Params.ToString();
            switch (RenameMethod)
            {
                case RenameMethodEnum.EntireRandom:
                    fileName = Request["name"] != null ? Request["name"] : string.Empty;
                    break;
                case RenameMethodEnum.CustomPrefix_DateSuffix:
                    fileName = string.Format("{0}_{1}.{2}"
                        , RenamePrefix
                        , DateTime.Now.ToString("yyyyMMddHHmmssff")
                        , Request["name"].Substring(Request["name"].LastIndexOf('.') + 1));
                    break;
                case RenameMethodEnum.CustomName:
                    fileName = string.Format("{0}.{1}"
                        , RenameName
                        , Request["name"].Substring(Request["name"].LastIndexOf('.') + 1));
                    break;
                default:
                    break;
            }
            return fileName;
        }

        private void LoadScript()
        {
            if (PageRelativeUrlToRoot.Length == 0)
            {
                throw new Exception("Please set PageRelativeUrlToRoot property");
                return;
            }
            if (PageRelativeUrlToRoot[0] != '/')
            {
                PageRelativeUrlToRoot = string.Format("/{0}", PageRelativeUrlToRoot);
            }
            StringBuilder relativePathPrefix = new StringBuilder();
            for (int i = 0; i < PageRelativeUrlToRoot.Count(c => c == '/') - 1; i++)
            {
                relativePathPrefix.Append("../");
            }


            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<script type='text/javascript'>");
            sb.AppendLine("$(function () {");
            sb.Append("$('#fileUploader1_uploader').pluploadQueue({");
            sb.AppendLine();
            sb.AppendLine("runtimes: 'flash,silverlight,html5',");

            //url: '/Test.aspx',
            sb.Append("url: '");
            sb.Append(PageRelativeUrlToRoot);
            sb.AppendLine("',");

            //max_file_size: '10mb',
            MaxFileSizeInMB = (MaxFileSizeInMB <= 0) ? 10 : MaxFileSizeInMB;
            sb.Append("max_file_size: '");
            sb.Append(MaxFileSizeInMB);
            sb.AppendLine("mb',");
            sb.AppendLine("chunk_size: '1mb',");
            sb.AppendLine("unique_names: true,");
            //comment image resize
            //sb.AppendLine("resize: { width: 320, height: 240, quality: 90 },");

            //filters: [{ title: "Image files", extensions: "jpg,gif,png" },],
            sb.AppendLine("filters: [ ");
            foreach (var suffixTitle in SupportingFileSuffix.Keys)
            {
                sb.Append("{ title: \"");
                sb.Append(suffixTitle);
                sb.Append("\", extensions: \"");
                foreach (var suffix in SupportingFileSuffix[suffixTitle])
                {
                    sb.Append(suffix);
                    if (suffix != SupportingFileSuffix[suffixTitle].Last())
                    {
                        sb.Append(",");
                    }
                }
                sb.Append("\" }");
                if (suffixTitle != SupportingFileSuffix.Keys.Last())
                {
                    sb.Append(",");
                }
                sb.AppendLine();
            }
            sb.AppendLine("],");

            //flash_swf_url: 'Scripts/plupload/Moxie.swf',
            sb.Append("flash_swf_url: '");
            sb.Append(relativePathPrefix);
            sb.AppendLine("Scripts/plupload/Moxie.swf',");

            //silverlight_xap_url: 'Scripts/plupload/Moxie.xap'
            sb.Append("silverlight_xap_url: '");
            sb.Append(relativePathPrefix);
            sb.AppendLine("Scripts/plupload/Moxie.xap'");
            sb.AppendLine("});");
            sb.AppendLine("$('form').submit(function (e) {");
            sb.AppendLine("var uploader = $('#uploader').pluploadQueue();");
            sb.AppendLine("if (uploader.total.uploaded == 0) {");
            sb.AppendLine("if (uploader.files.length > 0) {");
            sb.AppendLine("uploader.bind('UploadProgress', function () {");
            sb.AppendLine("if (uploader.total.uploaded == uploader.files.length) ");
            sb.AppendLine("$('form').submit();");
            sb.AppendLine("});");
            sb.AppendLine("uploader.start();");
            sb.AppendLine("} else ");
            sb.AppendLine("alert('You must at least upload one file.');");
            sb.AppendLine("e.preventDefault(); ");
            sb.AppendLine("}});});");
            sb.AppendLine("</script>");

            LiteralControl jsResource1 = new LiteralControl();
            jsResource1.Text = string.Format("<script type=\"text/javascript\" src=\"{0}Scripts/jquery-1.8.2.js\"></script>", relativePathPrefix);
            Page.Header.Controls.Add(jsResource1);

            LiteralControl jsResource2 = new LiteralControl();
            jsResource2.Text = string.Format("<script type=\"text/javascript\" src=\"{0}Scripts/plupload/plupload.full.min.js\"></script>", relativePathPrefix);
            Page.Header.Controls.Add(jsResource2);

            HtmlLink stylesLink = new HtmlLink();
            stylesLink.Href = string.Format("{0}Scripts/plupload/jquery.plupload.queue/css/jquery.plupload.queue.css", relativePathPrefix);
            stylesLink.Attributes["rel"] = "stylesheet";
            stylesLink.Attributes["type"] = "text/css";
            Page.Header.Controls.Add(stylesLink);

            LiteralControl jsResource3 = new LiteralControl();
            jsResource3.Text = string.Format("<script type=\"text/javascript\" src=\"{0}Scripts/plupload/jquery.plupload.queue/jquery.plupload.queue.min.js\"></script>", relativePathPrefix);
            Page.Header.Controls.Add(jsResource3);

            LiteralControl jsResource4 = new LiteralControl();
            jsResource4.Text = sb.ToString();
            Page.Header.Controls.Add(jsResource4);

            //Language
            string languageJSFileName = null;
            switch (Language)
            {
                case LanguageEnum.zh_cn:
                    languageJSFileName = "zh_CN";
                    break;
                case LanguageEnum.en_us:
                    languageJSFileName = "en";
                    break;
                default:
                    languageJSFileName = "en";
                    break;
            }
            LiteralControl jsResource5 = new LiteralControl();
            jsResource5.Text = string.Format("<script type=\"text/javascript\" src=\"{0}Scripts/plupload/i18n/{1}.js\"></script>", relativePathPrefix, languageJSFileName);
            Page.Header.Controls.Add(jsResource5);
        }

        private bool InitializeParms(out string outMessage)
        {
            //TODO: do better multi language message support
            if (Language == LanguageEnum.en_us)
            {
                lblUnsupportedMessage.Text = "Please ensure your browser has Flash installed.";
            }
            else
            {
                lblUnsupportedMessage.Text = "请确认你的浏览器已安装了Flash插件";
            }
            outMessage = null;

            //UploadPath
            if (string.IsNullOrEmpty(UploadPath))
            {
                UploadPath = "/Uploads";
            }

            gvUploadedFiles.EmptyDataText = "尚无已上传的文件";

            return true;
        }

        public void RefreshUploadedFileGridView()
        {
            lblUploadedFile.Text = "已上传的文件";
            lblUploadedFile2.Text = "可以下载与删除已上传的文件。";

            DataTable dtUploadedFiles = new DataTable();
            dtUploadedFiles.Columns.Add("FileName");
            dtUploadedFiles.Columns.Add("UploadTime");

            foreach (File file in UploadedFiles)
            {
                DataRow fileRow = dtUploadedFiles.NewRow();
                fileRow["FileName"] = file.Name;
                fileRow["UploadTime"] = file.UploadTime.ToString("yyyy-MM-dd");
                dtUploadedFiles.Rows.Add(fileRow);
            }
            gvUploadedFiles.DataSource = dtUploadedFiles;
            gvUploadedFiles.DataBind();
        }

        protected void gvUploadedFiles_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                StringBuilder relativePathPrefix = new StringBuilder();
                for (int i = 0; i < PageRelativeUrlToRoot.Count(c => c == '/') - 1; i++)
                {
                    relativePathPrefix.Append("/..");
                }
                ImageButton ibtnDownload = (ImageButton)e.Row.FindControl("ibtnDownload");
                ibtnDownload.ImageUrl = string.Format("~{0}/Scripts/plupload/jquery.plupload.queue/img/download.png", relativePathPrefix);
                if (IsAllowDownload == false)
                {
                    ibtnDownload.Visible = false;
                }
                ImageButton ibtnDelete = (ImageButton)e.Row.FindControl("ibtnDelete");
                ibtnDelete.ImageUrl = string.Format("~{0}/Scripts/plupload/jquery.plupload.queue/img/delete.gif", relativePathPrefix);
                if (IsAllowDelete == false)
                {
                    ibtnDelete.Visible = false;
                }
            }
        }

        protected void gvUploadedFiles_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "delete")
            {
                int index = Convert.ToInt32(e.CommandArgument);
                //GridViewRow row = gvUploadedFiles.Rows[index];
                string fileName = gvUploadedFiles.DataKeys[index].Value.ToString();
                File fileToDelete = UploadedFiles.Find(file => file.Name == fileName);
                UploadedFiles.Remove(fileToDelete);
                LatestUploadedFiles.Remove(fileToDelete);
                RefreshUploadedFileGridView();
            }
            if (e.CommandName == "download")
            {
                int index = Convert.ToInt32(e.CommandArgument);
                //GridViewRow row = gvUploadedFiles.Rows[index];
                string fileName = gvUploadedFiles.DataKeys[index].Value.ToString();
                Response.ContentType = "application/octet-stream";
                Response.AppendHeader("Content-Disposition", string.Format("attachment; filename={0}", fileName));
                Response.TransmitFile(Server.MapPath(string.Format("~{0}/{1}", UploadPath, fileName)));
                Response.End();
            }
        }

        public void gvUploadedFiles_RowDeleting(Object sender, GridViewDeleteEventArgs e)
        {

        }
    }
}