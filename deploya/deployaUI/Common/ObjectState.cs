﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace deploya.Common
{

    public static class ApplyDetails
    {
        public static string Name { get; set; }
        public static string FileName { get; set; }
        public static int Index { get; set; }
        public static int DiskIndex { get; set; }
        public static bool UseEFI { get; set; }
        public static bool UseNTLDR { get; set; }
    }

}
