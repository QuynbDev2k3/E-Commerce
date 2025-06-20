﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.DbManagement
{
    public class MetaKeyValue
    {
        public virtual string Key { get; set; }

        public virtual string Code { get; set; }

        public virtual string Value { get; set; }

        public virtual int Order { get; set; }
    }
}
