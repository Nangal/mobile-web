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
using MileageStats.Domain.Contracts.Data;
using MileageStats.Domain.Models;

namespace MileageStats.Domain.Handlers
{
    public class UpdateUser
    {
        IUserRepository _userRepository;

        public UpdateUser(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public virtual void Execute(User user)
        {
            if (user == null) throw new ArgumentNullException("user");

            User userToUpdate = new User
            {
                UserId = user.UserId,
                AuthorizationId = user.AuthorizationId,
                DisplayName = user.DisplayName,
                Country = user.Country,
                HasRegistered = user.HasRegistered,
            };

            _userRepository.Update(userToUpdate);
        }
    }
}
