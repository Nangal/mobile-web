/*  
Copyright Microsoft Corporation

Licensed under the Apache License, Version 2.0 (the "License"); you may not
use this file except in compliance with the License. You may obtain a copy of
the License at 

http://www.apache.org/licenses/LICENSE-2.0 

THIS CODE IS PROVIDED ON AN *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED 
WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, 
MERCHANTABLITY OR NON-INFRINGEMENT. 

See the Apache 2 License for the specific language governing permissions and
limitations under the License. */

using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using MileageStats.Data.InMemory;
using MileageStats.Domain.Contracts;
using MileageStats.Domain.Handlers;

namespace MileageStats.Web.Controllers
{
    public class SampleController: BaseController
    {
        private readonly IServiceLocator _locator;

        public SampleController(GetUserByClaimId getUser, IServiceLocator locator)
            : base(getUser, locator)
        {
            _locator = locator;
        }

        [Authorize]
        public ActionResult Generate()
        {
            _locator.GetInstance<PopulateSampleData>().Seed(CurrentUser.UserId);
            return RedirectToAction("Index", "Dashboard");
        }
    }
}