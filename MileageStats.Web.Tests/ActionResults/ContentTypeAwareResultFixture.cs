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
using System.Web;
using System.Web.Mvc;
using Moq;
using Xunit;

namespace MileageStats.Web.Tests.ActionResults
{

    public class ExampleController : Controller
    {
        public ActionResult DefaultActionResult()
        {
            // do some interesting work to produce a view model
            return new ContentTypeAwareResult(new { apples = 1, oranges = "tasty"});
        }

        public ActionResult CustomizedActionResult()
        {
            var viewmodel = new {apples = 1, oranges = "tasty"};
            return new ContentTypeAwareResult(viewmodel)
                       {
                           WhenJson = (data,viewdata) => new JsonResult {Data = data}, 
                           // this way if we don't want the default behavior 
                           // we can customize it
                           WhenHtml = (model, viewdata) =>
                                      {
                                          var result = new ViewResult();
                                          if (model != null)
                                          {
                                              result.ViewData.Model = model;
                                          }
                                          return result;
                                      }
                       };
        }
    }

    public class ContentTypeAwareFixture
    {
        [Fact]
        public void when_no_providers_are_found_for_values_in_the_accepts_header_then_return_415()
        {
            // 415 Unsupported Media Type
            // http://tools.ietf.org/html/rfc2616#section-10.4.16

            var controller = new ExampleController();
            var action = controller.DefaultActionResult();

            var context = new Mock<ControllerContext>();
            var response = new Mock<HttpResponseBase>();

            context.SetupGet(x => x.HttpContext.Response).Returns(() => response.Object);
            context.Setup(x => x.HttpContext.Request.AcceptTypes).Returns(new[] { "not a real type" });

            action.ExecuteResult(context.Object);
            
            response.VerifySet(x=>x.StatusCode = 415);
        }

        [Fact]
        public void when_json_is_accepted_then_return_json()
        {
            var controller = new ExampleController();
            var action = controller.DefaultActionResult();

            var context = MockContextFor(controller);
            var response = new Mock<HttpResponseBase>();

            context.Setup(x => x.HttpContext.Request.AcceptTypes).Returns(new[] { "application/json" });
            context.SetupGet(x => x.HttpContext.Response).Returns(() => response.Object);

            action.ExecuteResult(context.Object);

            response.VerifySet(x => x.ContentType = "application/json");
        }

        [Fact]
        public void when_html_is_accepted_then_return_viewresult()
        {
            var controller = new ExampleController();
            var action = (ContentTypeAwareResult)controller.DefaultActionResult();

            var view = new Mock<ViewResult>();
            view.Setup(x => x.ExecuteResult(It.IsAny<ControllerContext>()))
                .Verifiable();
            action.WhenHtml = (x,v) => view.Object;

            var context = MockContextFor(controller);
            context.Setup(x => x.HttpContext.Request.AcceptTypes).Returns(new[] { "text/html" });

            action.ExecuteResult(context.Object);

            view.VerifyAll();
        }

        [Fact]
        public void when_type_contains_additional_data_it_is_still_recognized()
        {
            var controller = new ExampleController();
            var action = (ContentTypeAwareResult)controller.DefaultActionResult();

            var view = new Mock<ViewResult>();
            view.Setup(x => x.ExecuteResult(It.IsAny<ControllerContext>()))
                .Verifiable();
            action.WhenHtml = (x,v) => view.Object;

            var context = MockContextFor(controller);
            context.Setup(x => x.HttpContext.Request.AcceptTypes).Returns(new[] { "text/html; q=0.90" });

            action.ExecuteResult(context.Object);

            view.VerifyAll();
        }

        private static Mock<ControllerContext> MockContextFor(ControllerBase controller)
        {
            var context = new Mock<ControllerContext>();
            context.Setup(x => x.Controller).Returns(controller);
            return context;
        }
    }
}