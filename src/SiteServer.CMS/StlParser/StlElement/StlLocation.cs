﻿using System.Text;
using SiteServer.Utils;
using SiteServer.CMS.Core;
using SiteServer.CMS.DataCache;
using SiteServer.CMS.StlParser.Model;
using System.Collections.Specialized;

namespace SiteServer.CMS.StlParser.StlElement
{
    [StlElement(Title = "当前位置", Description = "通过 stl:location 标签在模板中插入页面的当前位置")]
    public class StlLocation
    {
        private StlLocation() { }
        public const string ElementName = "stl:location";

        [StlAttribute(Title = "当前位置分隔符")]
        private const string Separator = nameof(Separator);

        [StlAttribute(Title = "打开窗口的目标")]
        private const string Target = nameof(Target);

        [StlAttribute(Title = "链接CSS样式")]
        private const string LinkClass = nameof(LinkClass);

        [StlAttribute(Title = "链接字数")]
        private const string WordNum = nameof(WordNum);

        [StlAttribute(Title = "是否包含当前栏目")]
        private const string IsContainSelf = nameof(IsContainSelf);

        //对“当前位置”（stl:location）元素进行解析
        public static string Parse(PageInfo pageInfo, ContextInfo contextInfo)
        {
            var separator = " - ";
            var target = string.Empty;
            var linkClass = string.Empty;
            var wordNum = 0;
            var isContainSelf = true;

            foreach (var name in contextInfo.Attributes.AllKeys)
            {
                var value = contextInfo.Attributes[name];

                if (StringUtils.EqualsIgnoreCase(name, Separator))
                {
                    separator = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, Target))
                {
                    target = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, LinkClass))
                {
                    linkClass = value;
                }
                else if (StringUtils.EqualsIgnoreCase(name, WordNum))
                {
                    wordNum = TranslateUtils.ToInt(value);
                }
                else if (StringUtils.EqualsIgnoreCase(name, IsContainSelf))
                {
                    isContainSelf = TranslateUtils.ToBool(value);
                }
            }

            return ParseImpl(pageInfo, contextInfo, separator, target, linkClass, wordNum, isContainSelf);
        }

        private static string ParseImpl(PageInfo pageInfo, ContextInfo contextInfo, string separator, string target, string linkClass, int wordNum, bool isContainSelf)
        {
            if (!string.IsNullOrEmpty(contextInfo.InnerHtml))
            {
                separator = contextInfo.InnerHtml;
            }

            var nodeInfo = ChannelManager.GetChannelInfo(pageInfo.SiteId, contextInfo.ChannelId);

            var builder = new StringBuilder();

            var parentsPath = nodeInfo.ParentsPath;
            var parentsCount = nodeInfo.ParentsCount;
            if (parentsPath.Length != 0)
            {
                var nodePath = parentsPath;
                if (isContainSelf)
                {
                    nodePath = nodePath + "," + contextInfo.ChannelId;
                }
                var channelIdArrayList = TranslateUtils.StringCollectionToStringList(nodePath);
                foreach (var channelIdStr in channelIdArrayList)
                {
                    var currentId = int.Parse(channelIdStr);
                    var currentNodeInfo = ChannelManager.GetChannelInfo(pageInfo.SiteId, currentId);
                    if (currentId == pageInfo.SiteId)
                    {
                        var attributes = new NameValueCollection();
                        if (!string.IsNullOrEmpty(target))
                        {
                            attributes["target"] = target;
                        }
                        if (!string.IsNullOrEmpty(linkClass))
                        {
                            attributes["class"] = linkClass;
                        }
                        var url = PageUtility.GetIndexPageUrl(pageInfo.SiteInfo, pageInfo.IsLocal);
                        if (url.Equals(PageUtils.UnclickedUrl))
                        {
                            attributes["target"] = string.Empty;
                        }
                        attributes["href"] = url;
                        var innerHtml = StringUtils.MaxLengthText(currentNodeInfo.ChannelName, wordNum);

                        TranslateUtils.AddAttributesIfNotExists(attributes, contextInfo.Attributes);

                        builder.Append($@"<a {TranslateUtils.ToAttributesString(attributes)}>{innerHtml}</a>");

                        if (parentsCount > 0)
                        {
                            builder.Append(separator);
                        }
                    }
                    else if (currentId == contextInfo.ChannelId)
                    {
                        var attributes = new NameValueCollection();
                        if (!string.IsNullOrEmpty(target))
                        {
                            attributes["target"] = target;
                        }
                        if (!string.IsNullOrEmpty(linkClass))
                        {
                            attributes["class"] = linkClass;
                        }
                        var url = PageUtility.GetChannelUrl(pageInfo.SiteInfo, currentNodeInfo, pageInfo.IsLocal);
                        if (url.Equals(PageUtils.UnclickedUrl))
                        {
                            attributes["target"] = string.Empty;
                        }
                        attributes["href"] = url;
                        var innerHtml = StringUtils.MaxLengthText(currentNodeInfo.ChannelName, wordNum);

                        TranslateUtils.AddAttributesIfNotExists(attributes, contextInfo.Attributes);

                        builder.Append($@"<a {TranslateUtils.ToAttributesString(attributes)}>{innerHtml}</a>");
                    }
                    else
                    {
                        var attributes = new NameValueCollection();
                        if (!string.IsNullOrEmpty(target))
                        {
                            attributes["target"] = target;
                        }
                        if (!string.IsNullOrEmpty(linkClass))
                        {
                            attributes["class"] = linkClass;
                        }
                        var url = PageUtility.GetChannelUrl(pageInfo.SiteInfo, currentNodeInfo, pageInfo.IsLocal);
                        if (url.Equals(PageUtils.UnclickedUrl))
                        {
                            attributes["target"] = string.Empty;
                        }
                        attributes["href"] = url;
                        var innerHtml = StringUtils.MaxLengthText(currentNodeInfo.ChannelName, wordNum);

                        TranslateUtils.AddAttributesIfNotExists(attributes, contextInfo.Attributes);

                        builder.Append($@"<a {TranslateUtils.ToAttributesString(attributes)}>{innerHtml}</a>");

                        if (parentsCount > 0)
                        {
                            builder.Append(separator);
                        }
                    }
                }
            }

            return builder.ToString();
        }
    }
}
