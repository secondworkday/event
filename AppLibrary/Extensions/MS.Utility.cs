using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Net.Mail;
using System.Net.Mime;

using MS.Utility;

namespace App.Library
{
    public static partial class LocalExtensions
    {
        public static bool IsInRole(this Identity identity, AppRole appRole)
        {
            if (identity != null)
            {
                return identity.IsInRole(appRole.ToString());
            }
            return false;
        }

        public static bool IsInExclusiveRole(this Identity identity, AppRole appRole)
        {
            if (identity != null)
            {
                return identity.IsInExclusiveAppRole(appRole.ToString());
            }
            return false;
        }
    }
}