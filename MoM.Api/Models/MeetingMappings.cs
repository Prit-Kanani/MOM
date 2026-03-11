using Microsoft.EntityFrameworkCore;

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
                Date = meeting.Date,
                VenueName = meeting.VenueMappings.Select(v => v.Venue.VenueName).FirstOrDefault(),
                PresentCount = meeting.UserMappings.Count(u => u.Role == MeetingRoles.Attendee && u.IsPresent),
                AbsentCount = meeting.UserMappings.Count(u => u.Role == MeetingRoles.Attendee && !u.IsPresent)
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
                Venue = meeting.VenueMappings
                    .Select(v => new LookupSelectionDto
                    {
                        Id = v.VenueId,
                        Name = v.Venue.VenueName
                    })
                    .FirstOrDefault(),
                Facilitator = meeting.UserMappings
                    .Where(u => u.Role == MeetingRoles.Facilitator)
                    .Select(u => new LookupSelectionDto
                    {
                        Id = u.UserId,
                        Name = u.User.UserName
                    })
                    .FirstOrDefault(),
                Chairperson = meeting.UserMappings
                    .Where(u => u.Role == MeetingRoles.Chairperson)
                    .Select(u => new LookupSelectionDto
                    {
                        Id = u.UserId,
                        Name = u.User.UserName
                    })
                    .FirstOrDefault(),
                Secretary = meeting.UserMappings
                    .Where(u => u.Role == MeetingRoles.Secretary)
                    .Select(u => new LookupSelectionDto
                    {
                        Id = u.UserId,
                        Name = u.User.UserName
                    })
                    .FirstOrDefault(),
                Logo = meeting.Logo is null ? null : Convert.ToBase64String(meeting.Logo),
                PresentCount = meeting.UserMappings.Count(u => u.Role == MeetingRoles.Attendee && u.IsPresent),
                AbsentCount = meeting.UserMappings.Count(u => u.Role == MeetingRoles.Attendee && !u.IsPresent),
                TotalAttendees = meeting.UserMappings.Count(u => u.Role == MeetingRoles.Attendee),
                Attendees = meeting.UserMappings
                    .Where(u => u.Role == MeetingRoles.Attendee)
                    .Select(u => new MeetingAttendeeDto
                    {
                        MappingId = u.Id,
                        UserId = u.UserId,
                        UserName = u.User.UserName,
                        IsPresent = u.IsPresent
                    })
                    .ToList(),
                Agendas = meeting.Agendas.Select(a => new AgendaItemDto
                {
                    Id = a.Id,
                    MeetingId = a.MeetingId,
                    Topic = a.Topic,
                    Owner = a.OwnerUserId.HasValue && a.OwnerUser is not null
                        ? new LookupSelectionDto
                        {
                            Id = a.OwnerUserId.Value,
                            Name = a.OwnerUser.UserName
                        }
                        : null
                }).ToList(),
                ActionItems = meeting.ActionItems.Select(a => new ActionItemDto
                {
                    Id = a.Id,
                    MeetingId = a.MeetingId,
                    Task = a.Task,
                    Responsibility = a.ResponsibilityUserId.HasValue && a.ResponsibilityUser is not null
                        ? new LookupSelectionDto
                        {
                            Id = a.ResponsibilityUserId.Value,
                            Name = a.ResponsibilityUser.UserName
                        }
                        : null,
                    Deadline = a.Deadline
                }).ToList()
            };
        }

        public static async Task<Meeting> ToEntityAsync(this MeetingUpsertDto dto, MomContext context)
        {
            var meeting = new Meeting();
            await dto.ApplyToEntityAsync(meeting, context);
            return meeting;
        }

        public static async Task ApplyToEntityAsync(this MeetingUpsertDto dto, Meeting meeting, MomContext context)
        {
            meeting.Title = dto.Title.Trim();
            meeting.MeetingNumber = TrimOrNull(dto.MeetingNumber);
            meeting.MeetingType = TrimOrNull(dto.MeetingType);
            meeting.Subject = dto.Subject.Trim();
            meeting.Description = TrimOrNull(dto.Description);
            meeting.Date = dto.Date!.Value;
            meeting.Time = TrimOrNull(dto.Time);
            meeting.Logo = DecodeLogo(dto.Logo);

            await SyncSingleRoleAsync(meeting, dto.Facilitator, MeetingRoles.Facilitator, context);
            await SyncSingleRoleAsync(meeting, dto.Chairperson, MeetingRoles.Chairperson, context);
            await SyncSingleRoleAsync(meeting, dto.Secretary, MeetingRoles.Secretary, context);
            await SyncVenueAsync(meeting, dto.Venue, context);
            await SyncAttendeesAsync(meeting, dto.Attendees, context);

            await SyncAgendasAsync(dto, meeting, context);
            await SyncActionsAsync(dto, meeting, context);
        }

        private static async Task SyncSingleRoleAsync(Meeting meeting, LookupSelectionUpsertDto? selection, string role, MomContext context)
        {
            var existing = meeting.UserMappings.FirstOrDefault(u => u.Role == role);
            var resolvedUser = await ResolveUserAsync(selection, context);

            if (resolvedUser is null)
            {
                if (existing is not null)
                {
                    meeting.UserMappings.Remove(existing);
                }

                return;
            }

            if (existing is null)
            {
                meeting.UserMappings.Add(new MeetingUserMap
                {
                    UserId = resolvedUser.Id,
                    User = resolvedUser,
                    Role = role,
                    IsPresent = true
                });

                return;
            }

            existing.UserId = resolvedUser.Id;
            existing.User = resolvedUser;
            existing.IsPresent = true;
        }

        private static async Task SyncVenueAsync(Meeting meeting, LookupSelectionUpsertDto? selection, MomContext context)
        {
            var existing = meeting.VenueMappings.FirstOrDefault();
            var resolvedVenue = await ResolveVenueAsync(selection, context);

            if (resolvedVenue is null)
            {
                if (existing is not null)
                {
                    meeting.VenueMappings.Remove(existing);
                }

                return;
            }

            if (existing is null)
            {
                meeting.VenueMappings.Add(new MeetingVenueMap
                {
                    VenueId = resolvedVenue.Id,
                    Venue = resolvedVenue
                });

                return;
            }

            existing.VenueId = resolvedVenue.Id;
            existing.Venue = resolvedVenue;
        }

        private static async Task SyncAttendeesAsync(Meeting meeting, List<MeetingAttendeeUpsertDto> attendees, MomContext context)
        {
            var incoming = attendees.Where(a => !string.IsNullOrWhiteSpace(a.UserName) || a.UserId.HasValue).ToList();
            var incomingIds = incoming.Where(a => a.MappingId > 0).Select(a => a.MappingId).ToHashSet();

            meeting.UserMappings.RemoveAll(u => u.Role == MeetingRoles.Attendee && u.Id > 0 && !incomingIds.Contains(u.Id));

            foreach (var item in incoming)
            {
                var resolvedUser = await ResolveUserAsync(new LookupSelectionUpsertDto
                {
                    Id = item.UserId,
                    Name = item.UserName
                }, context);

                if (resolvedUser is null)
                {
                    continue;
                }

                var existing = meeting.UserMappings.FirstOrDefault(u => u.Role == MeetingRoles.Attendee && u.Id == item.MappingId && item.MappingId > 0);
                if (existing is null)
                {
                    var duplicate = meeting.UserMappings.FirstOrDefault(u => u.Role == MeetingRoles.Attendee && u.UserId == resolvedUser.Id);
                    if (duplicate is not null)
                    {
                        duplicate.IsPresent = item.IsPresent;
                        continue;
                    }

                    meeting.UserMappings.Add(new MeetingUserMap
                    {
                        UserId = resolvedUser.Id,
                        User = resolvedUser,
                        Role = MeetingRoles.Attendee,
                        IsPresent = item.IsPresent
                    });
                    continue;
                }

                existing.UserId = resolvedUser.Id;
                existing.User = resolvedUser;
                existing.IsPresent = item.IsPresent;
            }
        }

        private static async Task SyncAgendasAsync(MeetingUpsertDto dto, Meeting meeting, MomContext context)
        {
            var incoming = dto.Agendas.Where(a => !string.IsNullOrWhiteSpace(a.Topic)).ToList();
            var incomingIds = incoming.Where(a => a.Id > 0).Select(a => a.Id).ToHashSet();

            meeting.Agendas.RemoveAll(a => a.Id > 0 && !incomingIds.Contains(a.Id));

            foreach (var item in incoming)
            {
                var ownerUser = await ResolveUserAsync(item.Owner, context);
                var existing = meeting.Agendas.FirstOrDefault(a => a.Id == item.Id && item.Id > 0);
                if (existing is null)
                {
                    meeting.Agendas.Add(new AgendaItem
                    {
                        Topic = item.Topic.Trim(),
                        OwnerUserId = ownerUser?.Id,
                        OwnerUser = ownerUser
                    });
                    continue;
                }

                existing.Topic = item.Topic.Trim();
                existing.OwnerUserId = ownerUser?.Id;
                existing.OwnerUser = ownerUser;
            }
        }

        private static async Task SyncActionsAsync(MeetingUpsertDto dto, Meeting meeting, MomContext context)
        {
            var incoming = dto.ActionItems.Where(a => !string.IsNullOrWhiteSpace(a.Task)).ToList();
            var incomingIds = incoming.Where(a => a.Id > 0).Select(a => a.Id).ToHashSet();

            meeting.ActionItems.RemoveAll(a => a.Id > 0 && !incomingIds.Contains(a.Id));

            foreach (var item in incoming)
            {
                var responsibilityUser = await ResolveUserAsync(item.Responsibility, context);
                var existing = meeting.ActionItems.FirstOrDefault(a => a.Id == item.Id && item.Id > 0);
                if (existing is null)
                {
                    meeting.ActionItems.Add(new ActionItem
                    {
                        Task = item.Task.Trim(),
                        ResponsibilityUserId = responsibilityUser?.Id,
                        ResponsibilityUser = responsibilityUser,
                        Deadline = item.Deadline
                    });
                    continue;
                }

                existing.Task = item.Task.Trim();
                existing.ResponsibilityUserId = responsibilityUser?.Id;
                existing.ResponsibilityUser = responsibilityUser;
                existing.Deadline = item.Deadline;
            }
        }

        public static async Task<AppUser?> ResolveUserAsync(LookupSelectionUpsertDto? selection, MomContext context)
        {
            if (selection is null || (!selection.Id.HasValue && string.IsNullOrWhiteSpace(selection.Name)))
            {
                return null;
            }

            if (selection.Id.HasValue)
            {
                return await context.Users.FirstOrDefaultAsync(u => u.Id == selection.Id.Value);
            }

            var normalized = selection.Name!.Trim();
            var existing = await context.Users.FirstOrDefaultAsync(u => u.UserName.ToLower() == normalized.ToLower());
            if (existing is not null)
            {
                return existing;
            }

            var user = new AppUser { UserName = normalized };
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return user;
        }

        public static async Task<Venue?> ResolveVenueAsync(LookupSelectionUpsertDto? selection, MomContext context)
        {
            if (selection is null || (!selection.Id.HasValue && string.IsNullOrWhiteSpace(selection.Name)))
            {
                return null;
            }

            if (selection.Id.HasValue)
            {
                return await context.Venues.FirstOrDefaultAsync(v => v.Id == selection.Id.Value);
            }

            var normalized = selection.Name!.Trim();
            var existing = await context.Venues.FirstOrDefaultAsync(v => v.VenueName.ToLower() == normalized.ToLower());
            if (existing is not null)
            {
                return existing;
            }

            var venue = new Venue { VenueName = normalized };
            context.Venues.Add(venue);
            await context.SaveChangesAsync();
            return venue;
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
