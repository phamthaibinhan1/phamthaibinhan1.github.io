/**
 * Handles form submission from web.
 * Receives JSON payload with name, email, message, token.
 * Saves to Google Sheet, sends email to owner, and auto-reply to user.
 * Verifies reCAPTCHA v3 token server-side.
 */
function doPost(e) {
  try {
    const data = JSON.parse(e.postData.contents);

    // reCAPTCHA token verification
    if (!verifyRecaptcha(data.token)) {
      return ContentService.createTextOutput("reCAPTCHA failed").setMimeType(ContentService.MimeType.TEXT);
    }

    const name = data.name || 'No name';
    const email = data.email || 'No email';
    const message = data.message || 'No message';

    const sheet = SpreadsheetApp.getActiveSpreadsheet().getSheetByName("ContactForm");
    sheet.appendRow([new Date(), name, email, message]);

    // Send notification email to owner
    const ownerEmail = "hellokatplay@gmail.com";
    const subjectToOwner = `📩 New message from ${name}`;
    const bodyToOwner = `You received a new message from Katplay:\n\n` +
                        `👤 Name: ${name}\n` +
                        `📧 Email: ${email}\n\n` +
                        `💬 Message:\n${message}`;
    MailApp.sendEmail(ownerEmail, subjectToOwner, bodyToOwner);

    // Send auto-reply
    if (validateEmail(email)) {
      const subjectToUser = `Thanks for contacting Katplay, ${name}!`;
      const htmlBodyToUser = `
        <div style="font-family: Arial, sans-serif; font-size: 15px; color: #333; background-color: #fff; border: 1px solid #eee; border-radius: 8px; padding: 20px;">
          <div style="text-align: center;">
            <h2 style="color: #ff0051; margin-bottom: 5px;">Katplay</h2>
            <p style="font-size: 13px; color: #999; margin-top: 0;">Born to Kat, built to play.</p>
            <hr style="border: none; border-top: 1px solid #eee; margin: 20px 0;">
          </div>

          <p>Hi <strong>${name}</strong>,</p>

          <p>Thanks for contacting <strong>Katplay</strong>! We’ve received your message and will get back to you soon.</p>

          <p><strong>Your message:</strong></p>
          <blockquote style="margin: 1em 0; padding: 1em; background: #fff4f8; border-left: 4px solid #ff0051; color: #222;">
            ${message}
          </blockquote>

          <p style="margin-top: 2em;">Cheers,<br/><strong>Katplay Team</strong></p>

          <div style="text-align: center; margin-top: 30px;">
            <a href="https://katplay.fun" style="display: inline-block; padding: 10px 20px; background-color: #ff0051; color: white; text-decoration: none; border-radius: 4px; font-weight: bold;">Visit Katplay Website</a>
          </div>

          <hr style="border: none; border-top: 1px solid #eee; margin: 30px 0 10px;" />
          <p style="font-size: 12px; color: #999; text-align: center;">
            This is an automated message from <a href="https://katplay.fun" style="color: #ff0051; text-decoration: none;">katplay.fun</a>
          </p>
        </div>
      `;

      MailApp.sendEmail({
        to: email,
        subject: subjectToUser,
        htmlBody: htmlBodyToUser
      });
    }

    return ContentService
      .createTextOutput("Success")
      .setMimeType(ContentService.MimeType.TEXT);

  } catch (error) {
    return ContentService
      .createTextOutput("Error: " + error.message)
      .setMimeType(ContentService.MimeType.TEXT);
  }
}

/**
 * Validate email format
 */
function validateEmail(email) {
  const pattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return pattern.test(email);
}

/**
 * Verify reCAPTCHA token with Google server
 */
function verifyRecaptcha(token) {
  const secret = '6LcegE4rAAAAAFLt3XBc_oA-8K6lzInq070JJERa'; // Replace with your actual secret key
  const response = UrlFetchApp.fetch('https://www.google.com/recaptcha/api/siteverify', {
    method: 'post',
    payload: {
      secret: secret,
      response: token
    }
  });

  const result = JSON.parse(response.getContentText());
  return result.success && result.score >= 0.5;
}

/**
 * Test email manually
 */
function testSend() {
  const recipient = "hellokatplay@gmail.com";
  const subject = "✅ Test email from Katplay script";
  const body = "This is a test email sent via Google Apps Script.";

  MailApp.sendEmail(recipient, subject, body);
}
