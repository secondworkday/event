﻿using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Threading;
using System.Net;
using System.IO;
using System.Xml.Linq;
using System.Web;

using MS.Utility;

namespace App.Library
{
    public class ActivityType : MS.Utility.ActivityType
    {
        protected ActivityType(int applicationID) :
            base(applicationID)
        { }

        protected static ActivityType create(int id) { return new ActivityType(id); }

        public static readonly ActivityType BulkCheckIn = create(0);
        public static readonly ActivityType BulkUndoCheckIn = create(1);
        public static readonly ActivityType BulkEdit = create(2);
        public static readonly ActivityType CheckIn = create(3);
        public static readonly ActivityType UndoCheckIn = create(4);
        public static readonly ActivityType CheckOut = create(5);
    }
}