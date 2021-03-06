﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Umbraco.Cms.Web.Context;
using Umbraco.Cms.Web.Editors;
using Umbraco.Cms.Web.Model.BackOffice.Trees;
using Umbraco.Cms.Web.Trees.MenuItems;
using Umbraco.Framework;

using Umbraco.Framework.Persistence;
using Umbraco.Framework.Persistence.Model.Attribution.MetaData;
using Umbraco.Framework.Persistence.Model.Constants;
using Umbraco.Framework.Persistence.Model;
using Umbraco.Framework.Persistence.Model.Constants.Entities;
using Umbraco.Hive;
using Umbraco.Hive.RepositoryTypes;
using FixedHiveIds = Umbraco.Framework.Security.Model.FixedHiveIds;
using MembershipUser = System.Web.Security.MembershipUser;


namespace Umbraco.Cms.Web.Trees
{
    /// <summary>
    /// Tree controller to render out the data types
    /// </summary>
    [Tree(CorePluginConstants.UserTreeControllerId, "Users")]
    [UmbracoTree]
    public class UserTreeController : SupportsEditorTreeController
    {

        public UserTreeController(IBackOfficeRequestContext requestContext)
            : base(requestContext)
        { }       

        /// <summary>
        /// Defines the data type editor
        /// </summary>
        public override Guid EditorControllerId
        {
            get { return new Guid(CorePluginConstants.UserEditorControllerId); }
        }

        /// <summary>
        /// Customize root node
        /// </summary>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        protected override TreeNode CreateRootNode(FormCollection queryStrings)
        {
            var n = base.CreateRootNode(queryStrings);
            //add some menu items to the created root node
            n.AddEditorMenuItem<CreateItem>(this, "createUrl", "CreateNew");
            n.AddMenuItem<Reload>();
            return n;
        }

        protected override UmbracoTreeResult GetTreeData(HiveId parentId, FormCollection queryStrings)
        {
            //if its the first level
            if (parentId == RootNodeId)
            {
                var membershipService = BackOfficeRequestContext.Application.Security.Users;
                var users = membershipService.GetAll().OrderBy(x => x.Username).ToList();

                foreach (var treeNode in users.Select(user => 
                            (TreeNode)CreateTreeNode(
                                user.Id, 
                                queryStrings,
                                //user.Name.OrIfNullOrWhiteSpace(user.Username),
                                user.Username,
                                GetEditorUrl(user.Id, queryStrings))))
                {
                    treeNode.Icon = "tree-user";
                    treeNode.HasChildren = false;

                    //add the menu items
                    treeNode.AddEditorMenuItem<Delete>(this, "deleteUrl", "Delete");

                    NodeCollection.Add(treeNode);
                }
            }
            else
            {
                throw new NotSupportedException("The User tree does not support more than 1 level");
            }

            return UmbracoTree();
        }

        protected override HiveId RootNodeId
        {
            get
            {
                ////BUG: Because of the current way that Hive is (30/09/2011), Hive cannot return the real Id representation,
                //// so in the meantime to be consistent so that tree syncing works properly, we need to lookup the root id 
                //// from hive and use it's returned value as the root node id. SD.
                //using (var uow = BackOfficeRequestContext.Application.Hive.OpenReader<ISecurityStore>(new Uri("security://users")))
                //{
                //    var root = uow.Repositories.Get(FixedHiveIds.UserVirtualRoot);
                //    return root.Id;
                //}

                return new HiveId(new Uri("security://users"), "", FixedHiveIds.UserVirtualRoot.Value);

            }
        }

    }
}
