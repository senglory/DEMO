using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;


namespace TestUseTech
{
    static class WebExtensions
    {
        /// <summary>
        /// Load List by server relative Url
        /// </summary>
        /// <param name="web"></param>
        /// <param name="listUrl"></param>
        /// <returns></returns>
        public static List LoadListByUrl(this Web web, string listUrl)
        {
            var ctx = web.Context;
            var listFolder = web.GetFolderByServerRelativeUrl(listUrl);
            ctx.Load(listFolder.Properties);
            ctx.ExecuteQuery();
            var listId = new Guid(listFolder.Properties["vti_listname"].ToString());
            var list = web.Lists.GetById(listId);
            ctx.Load(list);
            ctx.ExecuteQuery();
            return list;
        }

        /// <summary>
        /// Resolve client context  
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="context"></param>
        /// <param name="credentials"></param>
        /// <returns></returns>
        public static bool TryResolveClientContext(Uri requestUri, out ClientContext context )
        {
            context = null;
            var baseUrl = requestUri.GetLeftPart(UriPartial.Authority);
            for (int i = requestUri.Segments.Length; i >= 0; i--)
            {
                var path = string.Join(string.Empty, requestUri.Segments.Take(i));
                string url = string.Format("{0}{1}", baseUrl, path);
                try
                {
                    context = new ClientContext(url);
                    context.ExecuteQuery();
                    return true;
                }
                catch (Exception ex) { }
            }
            return false;
        }
    }
}
