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
using System.Web.Mvc;
using Microsoft.Practices.ServiceLocation;
using MileageStats.Domain.Handlers;
using MileageStats.Domain.Models;
using MileageStats.Web.Models;
using MileageStats.Web.Properties;
using System.Net;
using System.Web;

namespace MileageStats.Web.Controllers
{
    [Authorize]
    public class ReminderController : BaseController
    {
        public ReminderController(GetUserByClaimId getUser, IServiceLocator serviceLocator)
            : base(getUser, serviceLocator)
        {
        }

        [HttpPost]
        public ActionResult OverdueList()
        {
            var list = Using<GetOverdueRemindersForUser>().Execute(CurrentUserId);

            var reminders = from reminder in list
                            let vehicle = Using<GetVehicleById>().Execute(CurrentUserId, reminder.VehicleId)
                            let title = GetFullTitle(reminder, vehicle)
                            select new OverdueReminderViewModel { FullTitle = title, Reminder = reminder };

            var viewModel = new JsonRemindersOverdueListViewModel { Reminders = reminders.ToList() };

            return Json(viewModel);
        }

        public ActionResult Details(int vehicleId, int id)
        {
            var reminder = Using<GetReminder>().Execute(id);
            var reminders = GetUnfulfilledRemindersByVehicle(vehicleId);
            var reminderSummaryViewModels = reminders
                .Select(x => new ReminderSummaryModel(x))
                .ToList();

            var viewModel = new ReminderDetailsViewModel
            {
                Reminder = new ReminderSummaryModel(reminder),
                Reminders = new SelectedItemList<ReminderSummaryModel>(reminderSummaryViewModels, x => x.FirstOrDefault(item => item.ReminderId == id)),
            };

            return new ContentTypeAwareResult(viewModel);
        }

        public ActionResult List(int vehicleId)
        {
            var reminders = GetUnfulfilledRemindersByVehicle(vehicleId);
            var reminderSummaryViewModels = reminders
                .Select(x => new ReminderSummaryModel(x))
                .ToList();

            var viewModel = new ReminderDetailsViewModel
            {
                Reminder = new ReminderSummaryModel(reminders.FirstOrDefault()),
                Reminders = new SelectedItemList<ReminderSummaryModel>(reminderSummaryViewModels, Enumerable.FirstOrDefault),
            };

            return View(viewModel);
        }

        public ActionResult ListPartial(int vehicleId, int? id = null)
        {
            var reminders = GetUnfulfilledRemindersByVehicle(vehicleId)
                .Select(r => new ReminderSummaryModel(r));

            var reminderItemList = new SelectedItemList<ReminderSummaryModel>(reminders, list=> list.FirstOrDefault(x=>x.ReminderId == id));

            return new ContentTypeAwareResult(reminderItemList);
        }

        public ActionResult ListByGroup(int vehicleId)
        {
            var vehicle = Using<GetVehicleById>().Execute(CurrentUserId, vehicleId);

            var reminders = Using<GetAllRemindersForVehicle>().Execute(vehicleId);
            foreach (var reminder in reminders)
            {
                reminder.CalculateIsOverdue(vehicle.Odometer ?? 0);
            }

            var reminderSummaryModels = reminders.Select(r => new ReminderSummaryModel(r));
            var groups = from reminder in reminderSummaryModels
                         group reminder by reminder.Status
                         into grouping
                         select new ReminderListViewModel
                                    {
                                        Status = grouping.Key,
                                        Reminders = grouping
                                    };

            return new ContentTypeAwareResult(groups.ToList());
        }

        public ActionResult Add(int vehicleId)
        {
            var vehicle = Using<GetVehicleById>()
                .Execute(CurrentUserId, vehicleId);

            if (vehicle == null)
                throw new HttpException((int)HttpStatusCode.NotFound,
                    Messages.ReminderController_VehicleNotFound);

            var reminder = new ReminderFormModel 
            { 
                VehicleId = vehicleId,
                DueDateDay = DateTime.Now.Day.ToString(),
                DueDateMonth = DateTime.Now.Month.ToString(),
                DueDateYear = DateTime.Now.Year.ToString()
            };

            return View(reminder);
        }

        [HttpPost]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Add(int vehicleId, ReminderFormModel reminder)
        {
            if ((reminder != null) && ModelState.IsValid)
            {
                var errors = Using<CanAddReminder>().Execute(CurrentUserId, reminder);
                ModelState.AddModelErrors(errors, "Add");

                if (ModelState.IsValid)
                {
                    Using<AddReminderToVehicle>().Execute(CurrentUserId, vehicleId, reminder);

                    this.SetConfirmationMessage(Messages.ReminderController_ReminderAdded);

                    return RedirectToAction("ListByGroup", "Reminder", new { Id = vehicleId });
                }
            }
            this.SetAlertMessage(Messages.PleaseFixInvalidData);
            return View(reminder);
        }

        public ActionResult Fulfill(int vehicleId, int id)
        {
            Using<FulfillReminder>().Execute(CurrentUserId, id);

            this.SetConfirmationMessage(Messages.ReminderController_ReminderFulfilled);

            return new ContentTypeAwareResult
                       {
                           WhenHtml = (x,v,t) => RedirectToAction("ListByGroup", "Reminder", new {vehicleId})
                       };
        }

        [HttpPost]
        public ActionResult ImminentReminders()
        {
            var imminentReminders = Using<GetImminentRemindersForUser>().Execute(CurrentUserId, DateTime.UtcNow);
            return new ContentTypeAwareResult(imminentReminders);
        }

        private IEnumerable<Reminder> GetUnfulfilledRemindersByVehicle(int vehicleId)
        {
            var vehicle = Using<GetVehicleById>()
                .Execute(CurrentUserId, vehicleId);

            if (vehicle == null)
                throw new HttpException((int)HttpStatusCode.NotFound,
                    Messages.ReminderController_VehicleNotFound);

            var reminders = Using<GetUnfulfilledRemindersForVehicle>()
                .Execute(CurrentUserId, vehicleId, vehicle.Odometer ?? 0);

            return reminders;
        }

        private static string GetFullTitle(ReminderModel overdueReminder, VehicleModel vehicle)
        {
            string fullTitle = overdueReminder.Title + " | " + vehicle.Name + " @ ";

            if (overdueReminder.DueDate != null)
            {
                fullTitle += overdueReminder.DueDate.Value.ToString("d");
            }

            if (overdueReminder.DueDistance != null)
            {
                if (overdueReminder.DueDate != null)
                {
                    fullTitle += " or ";
                }

                fullTitle += overdueReminder.DueDistance.Value.ToString();
            }

            return fullTitle;
        }
    }
}