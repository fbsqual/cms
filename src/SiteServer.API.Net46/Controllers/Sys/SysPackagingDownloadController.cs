﻿using System.Web.Http;
using SiteServer.API.Common;
using SiteServer.CMS.Api.Sys.Packaging;
using SiteServer.CMS.Core;
using SiteServer.CMS.Packaging;
using SiteServer.Utils;

namespace SiteServer.API.Controllers.Sys
{
    public class SysPackagesDownloadController : ControllerBase
    {
        [HttpPost, Route(ApiRouteDownload.Route)]
        public IHttpActionResult Main()
        {
            var request = GetRequest();

            if (!request.IsAdminLoggin)
            {
                return Unauthorized();
            }

            var packageId = request.GetPostString("packageId");
            var version = request.GetPostString("version");

            try
            {
                PackageUtils.DownloadPackage(packageId, version);
            }
            catch
            {
                PackageUtils.DownloadPackage(packageId, version);
            }

            if (StringUtils.EqualsIgnoreCase(packageId, PackageUtils.PackageIdSsCms))
            {
                CacheDbUtils.RemoveAndInsert(PackageUtils.CacheKeySsCmsIsDownload, true.ToString());
            }

            return Ok();
        }
    }
}
