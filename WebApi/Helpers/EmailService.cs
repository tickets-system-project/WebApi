using System.Net.Mail;
using WebApi.Models.Entities;

namespace WebApi.Helpers;

public class EmailService(string smtpHost = "localhost", int smtpPort = 1025)
{
    public async Task SendConfirmationEmailAsync(Client client, Reservation reservation, CaseCategory category, string cancellationLink)
    {
        var from = new MailAddress("noreply@urzad.pl", "Urząd Miasta");
        var to = new MailAddress(client.Email, $"{client.FirstName} {client.LastName}");

        var message = new MailMessage(from, to)
        {
            Subject = "Potwierdzenie rezerwacji wizyty",
            IsBodyHtml = true,
            Body = $"""
                <div style="font-family: Arial, sans-serif; line-height: 1.5;">
                    <h1>Witaj {client.FirstName} {client.LastName},</h1>

                    <p>Twoja wizyta została zarezerwowana:</p>
                    <ul>
                        <li><strong>Data:</strong> {reservation.Date:yyyy-MM-dd}</li>
                        <li><strong>Godzina:</strong> {reservation.Time:HH\:mm}</li>
                        <li><strong>Kategoria sprawy:</strong> {category.Name}</li>
                        <li><strong>Kod potwierdzenia:</strong> {reservation.ConfirmationCode}</li>
                    </ul>
                    
                    <p>Jeśli chcesz anulować wizytę, kliknij 
                        <a href="{cancellationLink}">tutaj</a>.
                    </p>

                    <p>Pozdrawiamy,<br>Urząd Miasta</p>

                    <hr>

                    <p style="color: gray; font-size: 0.9em;">UWAGA: ta wiadomość została wygenerowana automatycznie. Prosimy na nią nie odpowiadać.</p>
                </div>
            """
        };

        using var smtpClient = new SmtpClient(smtpHost, smtpPort);
        smtpClient.UseDefaultCredentials = true;

        try
        {
            await smtpClient.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to send confirmation email.", ex);
        }
    }
    
    public async Task SendCancellationEmailAsync(Client client, Reservation reservation, CaseCategory category)
    {
        var from = new MailAddress("noreply@urzad.pl", "Urząd Miasta");
        var to = new MailAddress(client.Email, $"{client.FirstName} {client.LastName}");

        var message = new MailMessage(from, to)
        {
            Subject = "Potwierdzenie anulowania wizyty",
            IsBodyHtml = true,
            Body = $"""
                        <div style="font-family: Arial, sans-serif; line-height: 1.5;">
                            <h1>Witaj {client.FirstName} {client.LastName},</h1>
                    
                            <p>Twoja wizyta została anulowana:</p>
                            <ul>
                                <li><strong>Data:</strong> {reservation.Date:yyyy-MM-dd}</li>
                                <li><strong>Godzina:</strong> {reservation.Time:HH\:mm}</li>
                                <li><strong>Kategoria sprawy:</strong> {category.Name}</li>
                            </ul>
                    
                            <p>Pozdrawiamy,<br>Urząd Miasta</p>
                    
                            <hr>
                    
                            <p style="color: gray; font-size: 0.9em;">UWAGA: ta wiadomość została wygenerowana automatycznie. Prosimy na nią nie odpowiadać.</p>
                        </div>
                    """
        };

        using var smtpClient = new SmtpClient(smtpHost, smtpPort);
        smtpClient.UseDefaultCredentials = true;

        try
        {
            await smtpClient.SendMailAsync(message);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to send cancellation email.", ex);
        }
    }
}