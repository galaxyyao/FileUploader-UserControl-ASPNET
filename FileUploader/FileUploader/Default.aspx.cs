using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FileUploader
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            fileUploader1.IsAllowMultipleFile = false;
            fileUploader1.Width = 600;
            fileUploader1.Height = 600;
            fileUploader1.UploadPath = "/Uploads";
            fileUploader1.PageRelativeUrlToRoot = "/Default.aspx";
            List<string> imageFileSuffixes = new List<string>(new string[] { "jpg", "png", "gif" });
            List<string> zipFileSuffixes = new List<string>(new string[] { "zip" });
            fileUploader1.SupportingFileSuffix.Add("Image Files", imageFileSuffixes);
            fileUploader1.SupportingFileSuffix.Add("Zip Files", zipFileSuffixes);
            fileUploader1.MaxFileSizeInMB = 10;
            fileUploader1.RenameMethod = FileUploader.RenameMethodEnum.CustomPrefix_DateSuffix;
            fileUploader1.RenamePrefix = "c101_testimage";
            if (Session["uploadedFiles"] != null)
            {
                fileUploader1.UploadedFiles = (List<FileUploader.File>)Session["uploadedFiles"];
            }
            fileUploader1.OnFileUploaded += new FileUploader.OnFileUploadedDelegate(fileUploader1_OnFileUploaded);
            
        }

        void fileUploader1_OnFileUploaded()
        {
            Session["uploadedFiles"] = fileUploader1.UploadedFiles;
        }
    }
}