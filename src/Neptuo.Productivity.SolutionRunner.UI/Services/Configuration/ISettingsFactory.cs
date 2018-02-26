﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neptuo.Productivity.SolutionRunner.Services.Configuration
{
    public interface ISettingsFactory
    {
        ISettingsService CreateForFile(string filePath);
    }
}
