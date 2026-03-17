using System.Collections.Generic;
using Paralogamadha.Core.Models;

namespace Paralogamadha.Web.Models
{
    public class MassTimingViewModel
    {
        public IEnumerable<MassSchedule> WeeklySchedules { get; set; }
        public IEnumerable<SpecialMass> SpecialMasses { get; set; }
    }
}