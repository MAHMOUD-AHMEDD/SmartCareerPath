using SmartCareerPath.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartCareerPath.Application.DTOs.Chat
{
    public record ChatHistoryDto(
     ChatDto Chat,
     PagedResult<MessageDto> Messages);
}
