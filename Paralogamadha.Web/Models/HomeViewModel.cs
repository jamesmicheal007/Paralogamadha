using System.Collections.Generic;

namespace Paralogamadha.Web.Models
{
    public class HomeViewModel
    {
        // Ensure this property is public and spelled correctly
        public IEnumerable<Paralogamadha.Core.Models.HeroSlide> HeroSlides { get; set; }
        public IEnumerable<Paralogamadha.Core.Models.Announcement> Announcements { get; set; }
        public IEnumerable<Paralogamadha.Core.Models.MassSchedule> TodayMasses { get; set; }
        public Paralogamadha.Core.Models.FeastOfDay TodayFeast { get; set; }
        public Paralogamadha.Core.Models.ThoughtOfDay TodayThought { get; set; }
        public Paralogamadha.Core.Models.ReadingOfDay TodayReading { get; set; }
        public IEnumerable<Paralogamadha.Core.Models.SpecialMass> UpcomingMasses { get; set; }
        public IEnumerable<Paralogamadha.Core.Models.Testimonial> Testimonials { get; set; }
    }
}