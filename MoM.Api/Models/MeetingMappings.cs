namespace MoM.Api.Models
{
    public static class MeetingMappings
    {
        public static MeetingSummaryDto ToSummaryDto(this Meeting meeting)
        {
            return new MeetingSummaryDto
            {
                Id = meeting.Id,
                Title = meeting.Title,
                Subject = meeting.Subject,
                MeetingNumber = meeting.MeetingNumber,
                MeetingType = meeting.MeetingType,
                Description = meeting.Description,
                Date = meeting.Date,
                Venue = meeting.Venue,
                PresentCount = meeting.MeetingUsers.Count(u => u.IsPresent),
                AbsentCount = meeting.MeetingUsers.Count(u => !u.IsPresent),
                TotalAttendees = meeting.MeetingUsers.Count
            };
        }

        public static MeetingDto ToDto(this Meeting meeting)
        {
            return new MeetingDto
            {
                Id = meeting.Id,
                Title = meeting.Title,
                MeetingNumber = meeting.MeetingNumber,
                MeetingType = meeting.MeetingType,
                Subject = meeting.Subject,
                Description = meeting.Description,
                Date = meeting.Date,
                Time = meeting.Time,
                Venue = meeting.Venue,
                Facilitator = meeting.Facilitator,
                Chairperson = meeting.Chairperson,
                Secretary = meeting.Secretary,
                Logo = meeting.Logo is null ? null : Convert.ToBase64String(meeting.Logo),
                PresentCount = meeting.MeetingUsers.Count(u => u.IsPresent),
                AbsentCount = meeting.MeetingUsers.Count(u => !u.IsPresent),
                TotalAttendees = meeting.MeetingUsers.Count,
                MeetingUsers = meeting.MeetingUsers.Select(u => new MeetingUserDto
                {
                    Id = u.Id,
                    MeetingId = u.MeetingId,
                    UserName = u.UserName,
                    IsPresent = u.IsPresent
                }).ToList(),
                Agendas = meeting.Agendas.Select(a => new AgendaItemDto
                {
                    Id = a.Id,
                    MeetingId = a.MeetingId,
                    Topic = a.Topic,
                    Owner = a.Owner
                }).ToList(),
                ActionItems = meeting.ActionItems.Select(a => new ActionItemDto
                {
                    Id = a.Id,
                    MeetingId = a.MeetingId,
                    Task = a.Task,
                    Responsibility = a.Responsibility,
                    Deadline = a.Deadline
                }).ToList()
            };
        }

        public static Meeting ToEntity(this MeetingUpsertDto dto)
        {
            return new Meeting
            {
                Title = dto.Title.Trim(),
                MeetingNumber = TrimOrNull(dto.MeetingNumber),
                MeetingType = TrimOrNull(dto.MeetingType),
                Subject = dto.Subject.Trim(),
                Description = TrimOrNull(dto.Description),
                Date = dto.Date!.Value,
                Time = TrimOrNull(dto.Time),
                Venue = TrimOrNull(dto.Venue),
                Facilitator = TrimOrNull(dto.Facilitator),
                Chairperson = TrimOrNull(dto.Chairperson),
                Secretary = TrimOrNull(dto.Secretary),
                Logo = DecodeLogo(dto.Logo),
                MeetingUsers = dto.MeetingUsers
                    .Where(u => !string.IsNullOrWhiteSpace(u.UserName))
                    .Select(u => new MeetingUser
                    {
                        UserName = u.UserName.Trim(),
                        IsPresent = u.IsPresent
                    })
                    .ToList(),
                Agendas = dto.Agendas
                    .Where(a => !string.IsNullOrWhiteSpace(a.Topic))
                    .Select(a => new AgendaItem
                    {
                        Topic = a.Topic.Trim(),
                        Owner = TrimOrEmpty(a.Owner)
                    })
                    .ToList(),
                ActionItems = dto.ActionItems
                    .Where(a => !string.IsNullOrWhiteSpace(a.Task))
                    .Select(a => new ActionItem
                    {
                        Task = a.Task.Trim(),
                        Responsibility = TrimOrEmpty(a.Responsibility),
                        Deadline = a.Deadline
                    })
                    .ToList()
            };
        }

        public static void ApplyToEntity(this MeetingUpsertDto dto, Meeting meeting)
        {
            meeting.Title = dto.Title.Trim();
            meeting.MeetingNumber = TrimOrNull(dto.MeetingNumber);
            meeting.MeetingType = TrimOrNull(dto.MeetingType);
            meeting.Subject = dto.Subject.Trim();
            meeting.Description = TrimOrNull(dto.Description);
            meeting.Date = dto.Date!.Value;
            meeting.Time = TrimOrNull(dto.Time);
            meeting.Venue = TrimOrNull(dto.Venue);
            meeting.Facilitator = TrimOrNull(dto.Facilitator);
            meeting.Chairperson = TrimOrNull(dto.Chairperson);
            meeting.Secretary = TrimOrNull(dto.Secretary);
            meeting.Logo = DecodeLogo(dto.Logo);

            SyncMeetingUsers(dto, meeting);
            SyncAgendas(dto, meeting);
            SyncActions(dto, meeting);
        }

        private static void SyncMeetingUsers(MeetingUpsertDto dto, Meeting meeting)
        {
            var incoming = dto.MeetingUsers.Where(u => !string.IsNullOrWhiteSpace(u.UserName)).ToList();
            var incomingIds = incoming.Where(u => u.Id > 0).Select(u => u.Id).ToHashSet();

            meeting.MeetingUsers.RemoveAll(u => u.Id > 0 && !incomingIds.Contains(u.Id));

            foreach (var item in incoming)
            {
                var existing = meeting.MeetingUsers.FirstOrDefault(u => u.Id == item.Id && item.Id > 0);
                if (existing is null)
                {
                    meeting.MeetingUsers.Add(new MeetingUser
                    {
                        UserName = item.UserName.Trim(),
                        IsPresent = item.IsPresent
                    });
                    continue;
                }

                existing.UserName = item.UserName.Trim();
                existing.IsPresent = item.IsPresent;
            }
        }

        private static void SyncAgendas(MeetingUpsertDto dto, Meeting meeting)
        {
            var incoming = dto.Agendas.Where(a => !string.IsNullOrWhiteSpace(a.Topic)).ToList();
            var incomingIds = incoming.Where(a => a.Id > 0).Select(a => a.Id).ToHashSet();

            meeting.Agendas.RemoveAll(a => a.Id > 0 && !incomingIds.Contains(a.Id));

            foreach (var item in incoming)
            {
                var existing = meeting.Agendas.FirstOrDefault(a => a.Id == item.Id && item.Id > 0);
                if (existing is null)
                {
                    meeting.Agendas.Add(new AgendaItem
                    {
                        Topic = item.Topic.Trim(),
                        Owner = TrimOrEmpty(item.Owner)
                    });
                    continue;
                }

                existing.Topic = item.Topic.Trim();
                existing.Owner = TrimOrEmpty(item.Owner);
            }
        }

        private static void SyncActions(MeetingUpsertDto dto, Meeting meeting)
        {
            var incoming = dto.ActionItems.Where(a => !string.IsNullOrWhiteSpace(a.Task)).ToList();
            var incomingIds = incoming.Where(a => a.Id > 0).Select(a => a.Id).ToHashSet();

            meeting.ActionItems.RemoveAll(a => a.Id > 0 && !incomingIds.Contains(a.Id));

            foreach (var item in incoming)
            {
                var existing = meeting.ActionItems.FirstOrDefault(a => a.Id == item.Id && item.Id > 0);
                if (existing is null)
                {
                    meeting.ActionItems.Add(new ActionItem
                    {
                        Task = item.Task.Trim(),
                        Responsibility = TrimOrEmpty(item.Responsibility),
                        Deadline = item.Deadline
                    });
                    continue;
                }

                existing.Task = item.Task.Trim();
                existing.Responsibility = TrimOrEmpty(item.Responsibility);
                existing.Deadline = item.Deadline;
            }
        }

        private static string? TrimOrNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static string TrimOrEmpty(string? value)
        {
            return value?.Trim() ?? string.Empty;
        }

        private static byte[]? DecodeLogo(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            try
            {
                return Convert.FromBase64String(value);
            }
            catch (FormatException)
            {
                return null;
            }
        }
    }
}
