using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace JPB.BorannRemapping.Server.Services.Mail
{
	public class AuthMessageSenderOptions
	{
		public string SendGridUser { get; set; }
		public string SendGridKey { get; set; }
	}

	public class TwilloMailService : IEmailSender
	{ 
		public TwilloMailService(IOptions<AuthMessageSenderOptions> optionsAccessor)
		{
			Options = optionsAccessor.Value;
		}

		public AuthMessageSenderOptions Options { get; } //set only via Secret Manager
		public string MailFrom { get; set; } = "no-reply@em7733.elitemapping.ltd";

		public async Task SendEmailAsync(string email, string subject, string message)
		{
			await Execute(Options.SendGridKey, subject, message, email);
		}

		public async Task Execute(string apiKey, string subject, string message, string email)
		{
			var client = new SendGridClient(apiKey);
			var msg = new SendGridMessage()
			{
				From = new EmailAddress(MailFrom, Options.SendGridUser),
				Subject = $"Borann Remapping Initiative - {subject}",
				PlainTextContent = message,
				HtmlContent = message
			};
			msg.AddTo(new EmailAddress(email));

			// Disable click tracking.
			// See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
			msg.SetClickTracking(false, false);

			var sendEmailAsync = await client.SendEmailAsync(msg);
			if (sendEmailAsync.StatusCode != HttpStatusCode.Accepted)
			{
				throw new InvalidOperationException("Could not send mail");
			}
		}
	}
}
