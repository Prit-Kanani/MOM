using MoM.Api.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MoM.Api.Services
{
    public class PdfService
    {
        public byte[] GenerateMomPdf(Meeting meeting)
        {
            try
            {
                return CreateDocument(meeting, includeLogo: true).GeneratePdf();
            }
            catch
            {
                if (meeting.Logo is null)
                {
                    throw;
                }

                return CreateDocument(meeting, includeLogo: false).GeneratePdf();
            }
        }

        private static IDocument CreateDocument(Meeting meeting, bool includeLogo)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1.2f, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontFamily(Fonts.Arial).FontSize(10));

                    page.Header().Column(column =>
                    {
                        column.Item().Row(row =>
                        {
                            row.RelativeItem().AlignCenter().Text(meeting.Title)
                                .Bold()
                                .FontSize(16);

                            if (includeLogo && meeting.Logo is not null)
                            {
                                row.ConstantItem(60).Height(50).AlignRight().Image(meeting.Logo, ImageScaling.FitArea);
                            }
                        });

                        if (!string.IsNullOrWhiteSpace(meeting.Description))
                        {
                            column.Item().PaddingTop(4).Text(meeting.Description);
                        }
                    });

                    page.Content().PaddingTop(10).Column(column =>
                    {
                        column.Spacing(10);
                        column.Item().Element(c => RenderSectionTitle(c, "Meeting Detail"));
                        column.Item().Element(c => RenderMeetingDetails(c, meeting));
                        column.Item().Element(c => RenderSectionTitle(c, "Meeting Agenda :"));
                        column.Item().Element(c => RenderAgendaTable(c, meeting));
                        column.Item().Element(c => RenderSectionTitle(c, "Actionable Items :"));
                        column.Item().Element(c => RenderActionTable(c, meeting));
                        column.Item().Element(c => RenderSectionTitle(c, "Attendee Status :"));
                        column.Item().Element(c => RenderAttendeeTable(c, meeting));
                    });

                    page.Footer().AlignRight().Text(text =>
                    {
                        text.Span("Page ");
                        text.CurrentPageNumber();
                    });
                });
            });
        }

        private static void RenderSectionTitle(IContainer container, string title)
        {
            container.Text(title).Bold().FontSize(12);
        }

        private static void RenderMeetingDetails(IContainer container, Meeting meeting)
        {
            var attendees = GetAttendees(meeting);
            var venue = meeting.VenueMappings.Select(v => v.Venue.VenueName).FirstOrDefault();
            var facilitator = GetRoleUser(meeting, MeetingRoles.Facilitator);
            var chairperson = GetRoleUser(meeting, MeetingRoles.Chairperson);
            var secretary = GetRoleUser(meeting, MeetingRoles.Secretary);
            var presentCount = attendees.Count(u => u.IsPresent);
            var absentCount = attendees.Count - presentCount;

            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(110);
                    columns.ConstantColumn(16);
                    columns.RelativeColumn();
                    columns.ConstantColumn(110);
                    columns.ConstantColumn(16);
                    columns.RelativeColumn();
                });

                table.Cell().Element(DetailLabelCell).Text("Meeting No.");
                table.Cell().Element(DetailColonCell).Text(":");
                table.Cell().Element(DetailValueCell).Text(meeting.MeetingNumber ?? string.Empty);
                table.Cell().Element(DetailLabelCell).Text("Meeting Type");
                table.Cell().Element(DetailColonCell).Text(":");
                table.Cell().Element(DetailValueCell).Text(meeting.MeetingType ?? string.Empty);

                table.Cell().Element(DetailLabelCell).Text("Meeting Date & Time");
                table.Cell().Element(DetailColonCell).Text(":");
                table.Cell().Element(DetailValueCell).Text($"{meeting.Date:dd/MM/yyyy} {meeting.Time}".Trim());
                table.Cell().Element(DetailLabelCell).Text("Venue");
                table.Cell().Element(DetailColonCell).Text(":");
                table.Cell().Element(DetailValueCell).Text(venue ?? string.Empty);

                table.Cell().Element(DetailLabelCell).Text("Meeting Subject");
                table.Cell().Element(DetailColonCell).Text(":");
                table.Cell().Element(DetailValueCell).Text(meeting.Subject);
                table.Cell().Element(DetailLabelCell).Text("Facilitator");
                table.Cell().Element(DetailColonCell).Text(":");
                table.Cell().Element(DetailValueCell).Text(facilitator ?? string.Empty);

                table.Cell().Element(DetailLabelCell).Text("Chairperson");
                table.Cell().Element(DetailColonCell).Text(":");
                table.Cell().Element(DetailValueCell).Text(chairperson ?? string.Empty);
                table.Cell().Element(DetailLabelCell).Text("Secretary");
                table.Cell().Element(DetailColonCell).Text(":");
                table.Cell().Element(DetailValueCell).Text(secretary ?? string.Empty);

                table.Cell().Element(DetailLabelCell).Text("Present Count");
                table.Cell().Element(DetailColonCell).Text(":");
                table.Cell().Element(DetailValueCell).Text(presentCount.ToString());
                table.Cell().Element(DetailLabelCell).Text("Absent Count");
                table.Cell().Element(DetailColonCell).Text(":");
                table.Cell().Element(DetailValueCell).Text(absentCount.ToString());
            });
        }

        private static void RenderAgendaTable(IContainer container, Meeting meeting)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(35);
                    columns.RelativeColumn();
                    columns.ConstantColumn(90);
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCell).AlignCenter().Text("Sr.");
                    header.Cell().Element(HeaderCell).Text("Topic");
                    header.Cell().Element(HeaderCell).AlignCenter().Text("Owner");
                });

                if (meeting.Agendas.Count == 0)
                {
                    table.Cell().Element(BodyCell).AlignCenter().Text("1");
                    table.Cell().Element(BodyCell).Text(string.Empty);
                    table.Cell().Element(BodyCell).Text(string.Empty);
                    return;
                }

                for (var index = 0; index < meeting.Agendas.Count; index++)
                {
                    var agenda = meeting.Agendas[index];
                    table.Cell().Element(BodyCell).AlignCenter().Text((index + 1).ToString());
                    table.Cell().Element(BodyCell).Text(agenda.Topic);
                    table.Cell().Element(BodyCell).AlignCenter().Text(agenda.OwnerUser?.UserName ?? string.Empty);
                }
            });
        }

        private static void RenderActionTable(IContainer container, Meeting meeting)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(35);
                    columns.RelativeColumn();
                    columns.ConstantColumn(90);
                    columns.ConstantColumn(75);
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCell).AlignCenter().Text("Sr.");
                    header.Cell().Element(HeaderCell).Text("Action");
                    header.Cell().Element(HeaderCell).AlignCenter().Text("Owner");
                    header.Cell().Element(HeaderCell).AlignCenter().Text("Due Dt.");
                });

                if (meeting.ActionItems.Count == 0)
                {
                    table.Cell().Element(BodyCell).AlignCenter().Text("1");
                    table.Cell().Element(BodyCell).Text(string.Empty);
                    table.Cell().Element(BodyCell).Text(string.Empty);
                    table.Cell().Element(BodyCell).Text(string.Empty);
                    return;
                }

                for (var index = 0; index < meeting.ActionItems.Count; index++)
                {
                    var action = meeting.ActionItems[index];
                    table.Cell().Element(BodyCell).AlignCenter().Text((index + 1).ToString());
                    table.Cell().Element(BodyCell).Text(action.Task);
                    table.Cell().Element(BodyCell).AlignCenter().Text(action.ResponsibilityUser?.UserName ?? string.Empty);
                    table.Cell().Element(BodyCell).AlignCenter().Text(action.Deadline?.ToString("dd-MM-yy") ?? string.Empty);
                }
            });
        }

        private static void RenderAttendeeTable(IContainer container, Meeting meeting)
        {
            var attendees = GetAttendees(meeting);

            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(35);
                    columns.RelativeColumn();
                    columns.ConstantColumn(110);
                });

                table.Header(header =>
                {
                    header.Cell().Element(HeaderCell).AlignCenter().Text("Sr.");
                    header.Cell().Element(HeaderCell).Text("Member Name");
                    header.Cell().Element(HeaderCell).AlignCenter().Text("Status");
                });

                if (attendees.Count == 0)
                {
                    table.Cell().Element(BodyCell).AlignCenter().Text("1");
                    table.Cell().Element(BodyCell).Text("No attendee data");
                    table.Cell().Element(BodyCell).Text(string.Empty);
                    return;
                }

                for (var index = 0; index < attendees.Count; index++)
                {
                    var attendee = attendees[index];
                    table.Cell().Element(BodyCell).AlignCenter().Text((index + 1).ToString());
                    table.Cell().Element(BodyCell).Text(attendee.User.UserName);
                    table.Cell().Element(BodyCell).Padding(3).AlignCenter().Element(statusContainer =>
                    {
                        var background = attendee.IsPresent ? "#D7FFF0" : "#FFE0ED";
                        var border = attendee.IsPresent ? "#0CA678" : "#D6336C";
                        var text = attendee.IsPresent ? "#087F5B" : "#A61E4D";

                        return statusContainer
                            .Background(background)
                            .Border(1)
                            .BorderColor(border)
                            .PaddingVertical(4)
                            .PaddingHorizontal(8)
                            .DefaultTextStyle(x => x.FontColor(text).SemiBold());
                    }).Text(attendee.IsPresent ? "Present" : "Absent");
                }
            });
        }

        private static List<MeetingUserMap> GetAttendees(Meeting meeting)
        {
            return meeting.UserMappings
                .Where(u => u.Role == MeetingRoles.Attendee)
                .OrderBy(u => u.User.UserName)
                .ToList();
        }

        private static string? GetRoleUser(Meeting meeting, string role)
        {
            return meeting.UserMappings
                .Where(u => u.Role == role)
                .Select(u => u.User.UserName)
                .FirstOrDefault();
        }

        private static IContainer HeaderCell(IContainer container)
        {
            return container
                .Border(1)
                .BorderColor(Colors.Black)
                .Background(Colors.Black)
                .Padding(4)
                .DefaultTextStyle(x => x.FontColor(Colors.White).Bold());
        }

        private static IContainer BodyCell(IContainer container)
        {
            return container
                .Border(1)
                .BorderColor(Colors.Black)
                .Padding(4);
        }

        private static IContainer DetailLabelCell(IContainer container)
        {
            return BodyCell(container).DefaultTextStyle(x => x.Bold());
        }

        private static IContainer DetailColonCell(IContainer container)
        {
            return BodyCell(container).AlignCenter();
        }

        private static IContainer DetailValueCell(IContainer container)
        {
            return BodyCell(container);
        }
    }
}
