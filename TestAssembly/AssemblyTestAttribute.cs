﻿//  Copyright 2019 Florian Gather <florian.gather@tngtech.com>
// 	Copyright 2019 Fritz Brandhuber <fritz.brandhuber@tngtech.com>
// 	Copyright 2020 Pavel Fischer <rubbiroid@gmail.com>
// 
// 	SPDX-License-Identifier: Apache-2.0
// 

using System;

namespace TestAssembly
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class AssemblyTestAttribute : Attribute
    {
        private string _testValue;

        public AssemblyTestAttribute(string testValue)
        {
            _testValue = testValue;
        }
    }
}