﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcFrontendData
{
    public static class DbFactory
    {
        public static IDb Db()
        {
            return new Db();
        }
    }
}
