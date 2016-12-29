using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;

using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.UserProfiles;



namespace TestUseTech
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Enter user login (domain\\user):");
            var un = Console.ReadLine();
            if (!string .IsNullOrEmpty(un))
            {
                try
                {
                    var unExact = un.IndexOf("\\") > 0 ? un.Substring(un.IndexOf("\\") + 1) : un;
                    #region URL magic
                    Uri uri = new Uri(Properties.Settings.Default.ListUrl);
                    var idxLst = uri.Segments.ToList().IndexOf("Lists/");
                    if (idxLst == -1 || idxLst == uri.Segments.Length - 1)
                        return;
                    var url = uri.Scheme + "://" + uri.Host + ":" + uri.Port;
                    for (var i = 0; i < idxLst; i++)
                        url += uri.Segments[i];
                    var urlLst = uri.Segments[idxLst] + uri.Segments[idxLst + 1];
                    #endregion

                    using (ClientContext ctx = new ClientContext(url))
                    {
                        #region Get info about user by his login
                        PeopleManager peopleManager = new PeopleManager(ctx);
                        PersonProperties personProperties = peopleManager.GetPropertiesFor(un);
                        ctx.Load(personProperties, p => p.AccountName, p => p.UserProfileProperties);
                        ctx.ExecuteQuery();
                        foreach (var property in personProperties.UserProfileProperties)
                        {
                            Debug.WriteLine(string.Format("{0}: {1}", property.Key.ToString(), property.Value.ToString()));
                        }

                        List siteUserInfoList = ctx.Web.SiteUserInfoList;
                        CamlQuery query2 = new CamlQuery();
                        query2.ViewXml = @"<View  Scope='RecursiveAll'>
<Query>
<Where>
    <Eq>
      <FieldRef Name='UserName'  />
      <Value Type='Text'>"
    + unExact +
          @"</Value>
    </Eq>
</Where>
</Query></View>";
                        IEnumerable<ListItem> itemColl = ctx.LoadQuery(siteUserInfoList.GetItems(query2));
                        ctx.ExecuteQuery();
                        ListItem theUser = null;
                        foreach (var li in itemColl)
                        {
                            theUser = li;
                            break;
                        }

                        #endregion

                        if (theUser != null)
                        {
                            #region Do task search
                            List tasksList;
                            ListItemCollection taskItems;


                            tasksList = ctx.Web.LoadListByUrl(urlLst);
                            CamlQuery query = new CamlQuery();
                            query.ViewXml = @"<View  Scope='RecursiveAll'><Query>
<Where>
<And>
    <Lt>
      <FieldRef Name='DueDate'  />
      <Value Type='DateTime'><Today /></Value>
    </Lt>
    <Includes> 
        <FieldRef Name='AssignedTo' LookupId='TRUE'/> 
        <Value Type='Integer'>"
    + theUser.Id +
    @"</Value>
    </Includes>
</And>
</Where>
</Query></View>";
                            taskItems = tasksList.GetItems(query);
                            ctx.Load(taskItems);
                            ctx.ExecuteQuery();
                            foreach (var li in taskItems)
                            {
                                Console.WriteLine("ID: {0} \nTitle: {1}", li.Id, li["Title"]);
                            }
                            #endregion
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message );
                }
            }
        }
    }
}
