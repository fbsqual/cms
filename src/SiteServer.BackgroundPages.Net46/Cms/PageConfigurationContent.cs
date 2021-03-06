﻿using System;
using System.Collections.Specialized;
using System.Web.UI.WebControls;
using SiteServer.BackgroundPages.Core;
using SiteServer.Utils;
using SiteServer.CMS.Core;
using SiteServer.CMS.DataCache;
using SiteServer.Utils.Enumerations;

namespace SiteServer.BackgroundPages.Cms
{
    public class PageConfigurationContent : BasePageCms
    {
        public DropDownList DdlIsSaveImageInTextEditor;
        public DropDownList DdlIsAutoPageInTextEditor;
        public PlaceHolder PhAutoPage;
        public TextBox TbAutoPageWordNum;
        public DropDownList DdlIsContentTitleBreakLine;
        public DropDownList DdlIsCheckContentUseLevel;
        public PlaceHolder PhCheckContentLevel;
        public DropDownList DdlCheckContentLevel;
        public DropDownList DdlIsAutoCheckKeywords;

        public static string GetRedirectUrl(int siteId)
        {
            return PageUtilsEx.GetCmsUrl(siteId, nameof(PageConfigurationContent), null);
        }

        public void Page_Load(object sender, EventArgs e)
        {
            if (IsForbidden) return;

            FxUtils.CheckRequestParameter("siteId");

            if (IsPostBack) return;

            VerifySitePermissions(ConfigManager.WebSitePermissions.Configration);

            FxUtils.AddListItems(DdlIsSaveImageInTextEditor, "保存", "不保存");
            ControlUtils.SelectSingleItemIgnoreCase(DdlIsSaveImageInTextEditor, SiteInfo.IsSaveImageInTextEditor.ToString());

            FxUtils.AddListItems(DdlIsAutoPageInTextEditor, "自动分页", "手动分页");
            ControlUtils.SelectSingleItemIgnoreCase(DdlIsAutoPageInTextEditor, SiteInfo.IsAutoPageInTextEditor.ToString());

            PhAutoPage.Visible = SiteInfo.IsAutoPageInTextEditor;
            TbAutoPageWordNum.Text = SiteInfo.AutoPageWordNum.ToString();

            FxUtils.AddListItems(DdlIsContentTitleBreakLine, "启用标题换行", "不启用");
            ControlUtils.SelectSingleItemIgnoreCase(DdlIsContentTitleBreakLine, SiteInfo.IsContentTitleBreakLine.ToString());

            //保存时，敏感词自动检测
            FxUtils.AddListItems(DdlIsAutoCheckKeywords, "启用敏感词自动检测", "不启用");
            ControlUtils.SelectSingleItemIgnoreCase(DdlIsAutoCheckKeywords, SiteInfo.IsAutoCheckKeywords.ToString());

            DdlIsCheckContentUseLevel.Items.Add(new ListItem("默认审核机制", false.ToString()));
            DdlIsCheckContentUseLevel.Items.Add(new ListItem("多级审核机制", true.ToString()));

            ControlUtils.SelectSingleItem(DdlIsCheckContentUseLevel, SiteInfo.IsCheckContentLevel.ToString());
            if (SiteInfo.IsCheckContentLevel)
            {
                ControlUtils.SelectSingleItem(DdlCheckContentLevel, SiteInfo.CheckContentLevel.ToString());
                PhCheckContentLevel.Visible = true;
            }
            else
            {
                PhCheckContentLevel.Visible = false;
            }
        }

        public void DdlIsAutoPageInTextEditor_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            PhAutoPage.Visible = EBooleanUtils.Equals(DdlIsAutoPageInTextEditor.SelectedValue, EBoolean.True);
        }

        public void DdlIsCheckContentUseLevel_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            PhCheckContentLevel.Visible = EBooleanUtils.Equals(DdlIsCheckContentUseLevel.SelectedValue, EBoolean.True);
        }

        public override void Submit_OnClick(object sender, EventArgs e)
        {
            if (!Page.IsPostBack || !Page.IsValid) return;

            SiteInfo.IsSaveImageInTextEditor = TranslateUtils.ToBool(DdlIsSaveImageInTextEditor.SelectedValue, true);

            var isReCaculate = false;
            if (TranslateUtils.ToBool(DdlIsAutoPageInTextEditor.SelectedValue, false))
            {
                if (SiteInfo.IsAutoPageInTextEditor == false)
                {
                    isReCaculate = true;
                }
                else if (SiteInfo.AutoPageWordNum != TranslateUtils.ToInt(TbAutoPageWordNum.Text, SiteInfo.AutoPageWordNum))
                {
                    isReCaculate = true;
                }
            }

            SiteInfo.IsAutoPageInTextEditor = TranslateUtils.ToBool(DdlIsAutoPageInTextEditor.SelectedValue, false);

            SiteInfo.AutoPageWordNum = TranslateUtils.ToInt(TbAutoPageWordNum.Text, SiteInfo.AutoPageWordNum);

            SiteInfo.IsContentTitleBreakLine = TranslateUtils.ToBool(DdlIsContentTitleBreakLine.SelectedValue, true);

            SiteInfo.IsAutoCheckKeywords = TranslateUtils.ToBool(DdlIsAutoCheckKeywords.SelectedValue, true);

            SiteInfo.IsCheckContentLevel = TranslateUtils.ToBool(DdlIsCheckContentUseLevel.SelectedValue);
            if (SiteInfo.IsCheckContentLevel)
            {
                SiteInfo.CheckContentLevel = TranslateUtils.ToInt(DdlCheckContentLevel.SelectedValue);
            }

            DataProvider.SiteDao.Update(SiteInfo);
            if (isReCaculate)
            {
                SiteInfo.ContentDaoList.ForEach(x => x.SetAutoPageContentToSite(SiteId));
            }

            AuthRequest.AddSiteLog(SiteId, "修改内容设置");

            SuccessMessage("内容设置修改成功！");
        }
    }
}
