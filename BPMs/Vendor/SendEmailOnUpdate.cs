/*
This code can be used to send an email notification whenever a vendor's
information is updated in the system. It's meant to fire whenever whenever the
vendor table changes, check which vendor was updated, get the changelog entry
for that vendor for today, and then send an email notification with the
changelog details.

You might be able to do this slightly different with the ttVendor variable.

Also referenced in BPMs\ChangeLogs\SendChangelogOnUpdateVendor.cs
*/

// Initialize. Replace with your actual sender and recipient email addresses.
const string senderEmail = "no-reply@example.com";
const string recipientEmail = "recipient@example.com";
string emailSubject = string.Empty;
string emailBody = string.Empty;

// Create the message instance
var message = new Ice.Mail.SmtpMail();

// Find which vendor was updated.
var updatedVendor = ttVendor.Where(vendor => vendor.Updated()).FirstOrDefault();

emailSubject = $"VENDOR INFORMATION CHANGED FOR \"{updatedVendor.Name}\"";

// Find the associated changelog entry
var currentCompany = callContextClient.CurrentCompany;
var today = DateTime.Today;
var vendorNum = updatedVendor.VendorNum.ToString();

var logEntry = Db
    .ChgLog.Where(row =>
        row.Company == currentCompany
        && row.TableName == "Vendor"
        && row.DateStampedOn == today
        && row.Key1 == vendorNum
    )
    .Select(row => row.LogText)
    .FirstOrDefault();

// Set the body of the email to be the changelog message
emailBody = $"Changelog for vendor \"{updatedVendor.Name}\" \n\n{logEntry}";

// Set the properties of the email
message.SetFrom(senderEmail);
message.SetTo(recipientEmail);
message.SetSubject(emailSubject);
message.SetBody(emailBody);

// Create mailer instance and send the email
var mailer = this.GetMailer(async: true);
mailer.Send(message);
