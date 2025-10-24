using Quiz_Web.Models.EF;
using Quiz_Web.Services.IServices;

namespace Quiz_Web.Services
{
    public class TextService : ITextService
    {
        private readonly LearningPlatformContext _context;
        public TextService(LearningPlatformContext context)
        {
            _context = context;
        }
    }
}
