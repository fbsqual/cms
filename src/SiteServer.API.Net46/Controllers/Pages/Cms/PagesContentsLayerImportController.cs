﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Http;
using SiteServer.API.Common;
using SiteServer.CMS.Core;
using SiteServer.CMS.DataCache;
using SiteServer.CMS.ImportExport;
using SiteServer.Utils;
using SiteServer.Utils.Enumerations;

namespace SiteServer.API.Controllers.Pages.Cms
{
    [RoutePrefix("pages/cms/contentsLayerImport")]
    public class PagesContentsLayerImportController : ControllerBase
    {
        private const string Route = "";
        private const string RouteUpload = "actions/upload";

        [HttpGet, Route(Route)]
        public IHttpActionResult GetConfig()
        {
            try
            {
                var request = GetRequest();

                var siteId = request.GetQueryInt("siteId");
                var channelId = request.GetQueryInt("channelId");

                if (!request.IsAdminLoggin ||
                    !request.AdminPermissionsImpl.HasChannelPermissions(siteId, channelId,
                        ConfigManager.ChannelPermissions.ContentAdd))
                {
                    return Unauthorized();
                }

                var siteInfo = SiteManager.GetSiteInfo(siteId);
                if (siteInfo == null) return BadRequest("无法确定内容对应的站点");

                var channelInfo = ChannelManager.GetChannelInfo(siteId, channelId);
                if (channelInfo == null) return BadRequest("无法确定内容对应的栏目");

                var isChecked = CheckManager.GetUserCheckLevel(request.AdminPermissionsImpl, siteInfo, siteId, out var checkedLevel);
                var checkedLevels = CheckManager.GetCheckedLevels(siteInfo, isChecked, checkedLevel, true);

                return Ok(new
                {
                    Value = checkedLevel,
                    CheckedLevels = checkedLevels
                });
            }
            catch (Exception ex)
            {
                LogUtils.AddErrorLog(ex);
                return InternalServerError(ex);
            }
        }

        [HttpPost, Route(RouteUpload)]
        public IHttpActionResult Upload()
        {
            try
            {
                var request = GetRequest();

                var siteId = request.GetQueryInt("siteId");
                var channelId = request.GetQueryInt("channelId");

                if (!request.IsAdminLoggin ||
                    !request.AdminPermissionsImpl.HasChannelPermissions(siteId, channelId,
                        ConfigManager.ChannelPermissions.ContentAdd))
                {
                    return Unauthorized();
                }

                var fileName = request.GetPostString("fileName");

                var fileCount = request.Files.Count;

                string filePath = null;

                if (fileCount > 0)
                {
                    var file = request.Files[0];

                    if (string.IsNullOrEmpty(fileName)) fileName = Path.GetFileName(file.FileName);

                    var extendName = fileName.Substring(fileName.LastIndexOf(".", StringComparison.Ordinal)).ToLower();
                    if (extendName == ".zip" || extendName == ".csv" || extendName == ".txt")
                    {
                        filePath = PathUtils.GetTemporaryFilesPath(fileName);
                        DirectoryUtils.CreateDirectoryIfNotExists(filePath);
                        file.SaveAs(filePath);
                    }
                }

                FileInfo fileInfo = null;
                if (!string.IsNullOrEmpty(filePath))
                {
                    fileInfo = new FileInfo(filePath);
                }
                if (fileInfo != null)
                {
                    return Ok(new
                    {
                        fileName,
                        length = fileInfo.Length,
                        ret = 1
                    });
                }

                return Ok(new
                {
                    ret = 0
                });
            }
            catch (Exception ex)
            {
                LogUtils.AddErrorLog(ex);
                return InternalServerError(ex);
            }
        }

        [HttpPost, Route(Route)]
        public IHttpActionResult Submit()
        {
            try
            {
                var request = GetRequest();

                var siteId = request.GetPostInt("siteId");
                var channelId = request.GetPostInt("channelId");
                var importType = request.GetPostString("importType");
                var checkedLevel = request.GetPostInt("checkedLevel");
                var isOverride = request.GetPostBool("isOverride");
                var fileNames = request.GetPostObject<List<string>>("fileNames");

                if (!request.IsAdminLoggin ||
                    !request.AdminPermissionsImpl.HasChannelPermissions(siteId, channelId,
                        ConfigManager.ChannelPermissions.ContentAdd))
                {
                    return Unauthorized();
                }

                var siteInfo = SiteManager.GetSiteInfo(siteId);
                if (siteInfo == null) return BadRequest("无法确定内容对应的站点");

                var channelInfo = ChannelManager.GetChannelInfo(siteId, channelId);
                if (channelInfo == null) return BadRequest("无法确定内容对应的栏目");

                var isChecked = checkedLevel >= siteInfo.CheckContentLevel;

                if (importType == "zip")
                {
                    foreach (var fileName in fileNames)
                    {
                        var localFilePath = PathUtils.GetTemporaryFilesPath(fileName);

                        if (!EFileSystemTypeUtils.Equals(EFileSystemType.Zip, PathUtils.GetExtension(localFilePath)))
                            continue;

                        var importObject = new ImportObject(siteId, request.AdminName);
                        importObject.ImportContentsByZipFile(channelInfo, localFilePath, isOverride, isChecked, checkedLevel, request.AdminId, 0, SourceManager.Default);
                    }
                }

                else if (importType == "csv")
                {
                    foreach (var fileName in fileNames)
                    {
                        var localFilePath = PathUtils.GetTemporaryFilesPath(fileName);

                        if (!EFileSystemTypeUtils.Equals(EFileSystemType.Csv, PathUtils.GetExtension(localFilePath)))
                            continue;

                        var importObject = new ImportObject(siteId, request.AdminName);
                        importObject.ImportContentsByCsvFile(channelInfo, localFilePath, isOverride, isChecked, checkedLevel, request.AdminId, 0, SourceManager.Default);
                    }
                }
                else if (importType == "txt")
                {
                    foreach (var fileName in fileNames)
                    {
                        var localFilePath = PathUtils.GetTemporaryFilesPath(fileName);
                        if (!EFileSystemTypeUtils.Equals(EFileSystemType.Txt, PathUtils.GetExtension(localFilePath)))
                            continue;

                        var importObject = new ImportObject(siteId, request.AdminName);
                        importObject.ImportContentsByTxtFile(channelInfo, localFilePath, isOverride, isChecked, checkedLevel, request.AdminId, 0, SourceManager.Default);
                    }
                }

                request.AddChannelLog(siteId, channelId, "导入内容");

                return Ok(new
                {
                    Value = true
                });
            }
            catch (Exception ex)
            {
                LogUtils.AddErrorLog(ex);
                return InternalServerError(ex);
            }
        }
    }
}
