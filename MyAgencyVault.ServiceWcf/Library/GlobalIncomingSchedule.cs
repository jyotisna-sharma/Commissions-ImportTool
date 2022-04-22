﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.ServiceModel;
using MyAgencyVault.BusinessLibrary;

namespace MyAgencyVault.WcfService
{
    [ServiceContract]
    interface IGlobalIncomingSchedule
    {
        [OperationContract]
        void AddUpdateGlobalIncomingSchedule(GlobalIncomingSchedule IncomingSch);

        [OperationContract]
        GlobalIncomingSchedule GetGlobalIncomingSchedule(Guid carrierId, Guid coverageId);

        [OperationContract]
        void ChangeScheduleType(Guid carrierId, Guid coverageId, int scheduleType);
    }

    public partial class MavService : IGlobalIncomingSchedule
    {
        #region IGlobalIncomingSchedule Members

       
        public void AddUpdateGlobalIncomingSchedule(GlobalIncomingSchedule IncomingSch)
        {
            IncomingSchedule.AddUpdateGlobalSchedule(IncomingSch);
        }

        public void ChangeScheduleType(Guid carrierId, Guid coverageId, int scheduleType)
        {
            IncomingSchedule.ChangeScheduleType(carrierId, coverageId, scheduleType);
        }

        public GlobalIncomingSchedule GetGlobalIncomingSchedule(Guid carrierId, Guid coverageId)
        {
            return IncomingSchedule.GetGlobalIncomingSchedule(carrierId, coverageId);
        }

        #endregion
    }
}