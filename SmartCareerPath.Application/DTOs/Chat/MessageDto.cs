using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.DTOs.Chat
{
    public record MessageDto(
    int Id, int ChatId,
    string SenderId, string SenderName, string SenderRole,
    string Content, DateTime Timestamp);
}
