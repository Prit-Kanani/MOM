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
                return CreateDocument(meeting).GeneratePdf();
            }
            catch
            {
                if (meeting.Logo is null)
                {
                    throw;
                }

                var fallbackMeeting = new Meeting
                {
                    Id = meeting.Id,
                    Title = meeting.Title,
                    MeetingNumber = meeting.MeetingNumber,
                    Subject = meeting.Subject,
                    Description = meeting.Description,
                    Date = meeting.Date,
                    Time = meeting.Time,
                    Venue = meeting.Venue,
                    Facilitator = meeting.Facilitator,
                    Chairperson = meeting.Chairperson,
                    Secretary = meeting.Secretary,
                    Logo = null,
                    Agendas = meeting.Agendas,
                    ActionItems = meeting.ActionItems
                };

                return CreateDocument(fallbackMeeting).GeneratePdf();
            }
        }

        private static IDocument CreateDocument(Meeting meeting)
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

                            if (meeting.Logo is not null)
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
            var presentCount = meeting.MeetingUsers.Count(u => u.IsPresent);
            var absentCount = meeting.MeetingUsers.Count(u => !u.IsPresent);

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
                table.Cell().Element(DetailValueCell).Column(c =>
                {
                    c.Item().Text($"{meeting.Date:dd/MM/yyyy} {meeting.Time}".Trim());
                });
                table.Cell().Element(DetailLabelCell).Text("Venue");
                table.Cell().Element(DetailColonCell).Text(":");
                table.Cell().Element(DetailValueCell).Text(meeting.Venue ?? string.Empty);

                table.Cell().Element(DetailLabelCell).Text("Meeting Subject");
                table.Cell().Element(DetailColonCell).Text(":");
                table.Cell().Element(DetailValueCell).Text(meeting.Subject);
                table.Cell().Element(DetailLabelCell).Text(string.Empty);
                table.Cell().Element(DetailColonCell).Text(string.Empty);
                table.Cell().Element(DetailValueCell).Text(string.Empty);

                table.Cell().Element(DetailLabelCell).Text("Meeting Facilitator");
                table.Cell().Element(DetailColonCell).Text(":");
                table.Cell().Element(DetailValueCell).Text(meeting.Facilitator ?? meeting.Chairperson ?? string.Empty);
                table.Cell().Element(DetailLabelCell).Text(string.Empty);
                table.Cell().Element(DetailColonCell).Text(string.Empty);
                table.Cell().Element(DetailValueCell).Text(string.Empty);

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
                    table.Cell().Element(BodyCell).AlignCenter().Text(agenda.Owner);
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
                    table.Cell().Element(BodyCell).AlignCenter().Text(action.Responsibility);
                    table.Cell().Element(BodyCell).AlignCenter().Text(action.Deadline?.ToString("dd-MM-yy") ?? string.Empty);
                }
            });
        }

        private static void RenderAttendeeTable(IContainer container, Meeting meeting)
        {
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

                if (meeting.MeetingUsers.Count == 0)
                {
                    table.Cell().Element(BodyCell).AlignCenter().Text("1");
                    table.Cell().Element(BodyCell).Text("No attendee data");
                    table.Cell().Element(BodyCell).Text(string.Empty);
                    return;
                }

                for (var index = 0; index < meeting.MeetingUsers.Count; index++)
                {
                    var attendee = meeting.MeetingUsers[index];
                    table.Cell().Element(BodyCell).AlignCenter().Text((index + 1).ToString());
                    table.Cell().Element(BodyCell).Text(attendee.UserName);
                    table.Cell().Element(BodyCell).Padding(3).AlignCenter().Element(statusContainer =>
                    {
                        var background = attendee.IsPresent ? "#D9F8EA" : "#FFE1EA";
                        var border = attendee.IsPresent ? "#0F9B6F" : "#E0527A";
                        var text = attendee.IsPresent ? "#0B6B4E" : "#9E1F4B";

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
