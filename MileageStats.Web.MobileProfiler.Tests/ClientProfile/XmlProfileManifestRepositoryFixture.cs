/*  
Copyright Microsoft Corporation

Licensed under the Apache License, Version 2.0 (the "License"); you may not
use this file except in compliance with the License. You may obtain a copy of
the License at 

http://www.apache.org/licenses/LICENSE-2.0 

THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED 
ARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, 
MERCHANTABLITY OR NON-INFRINGEMENT. 

See the Apache 2 License for the specific language governing permissions and
limitations under the License. */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using MileageStats.Web.MobileProfiler.ClientProfile;
using System.IO;

namespace MileageStats.Web.MobileProfiler.Tests.ClientProfile
{
    public class XmlProfileManifestRepositoryFixture
    {
        [Fact]
        public void WhenGettingExistingProfile_ThenProfileManifestIsReturned()
        {
            var reader = new XmlProfileManifestRepository("Profiles\\", (path) => path);
            var profile = reader.GetProfile("Generic");

            Assert.NotNull(profile);
        }

        [Fact]
        public void WhenGettingUnexistingProfile_ThenFileNotFoundExceptionIsThrown()
        {
            var reader = new XmlProfileManifestRepository("invalid", (path) => path);
            
            Assert.Throws<FileNotFoundException>(() => reader.GetProfile("Generic"));
        }
    }
}