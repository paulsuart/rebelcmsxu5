﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RebelCms.Cms.Web.Model.BackOffice.Editors;
using RebelCms.Cms.Web.Model.BackOffice.PropertyEditors;

namespace RebelCms.Cms.Web.PropertyEditors.Tags
{
    public class TagsPreValueModel : PreValueModel
    {
        [AllowDocumentTypePropertyOverride]
        public bool IsRequired { get; set; }
    }
}