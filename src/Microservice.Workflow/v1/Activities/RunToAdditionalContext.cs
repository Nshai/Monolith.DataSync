﻿using System;

namespace Microservice.Workflow.v1.Activities
{
    public class RunToAdditionalContext : IAdditionalContext
    {
        public int StepIndex { get; set; }
        public DateTime? DelayTime { get; set; }
        public int? TaskId { get; set; }
    }
}