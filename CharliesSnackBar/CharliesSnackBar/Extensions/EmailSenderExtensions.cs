using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using CharliesSnackBar.Services;
using CharliesSnackBar.Utility;

namespace CharliesSnackBar.Services
{
    public static class EmailSenderExtensions
    {
        public static Task SendEmailConfirmationAsync(this IEmailSender emailSender, string email, string link)
        {
            return emailSender.SendEmailAsync(email, "Confirm your email",
                $"Please confirm your account by clicking this link: &npsp; <a href='{HtmlEncoder.Default.Encode(link)}'>link</a>");
        }

        public static Task SecondOrderStatusAsync(this IEmailSender emailSender, string email, string orderNumber, string status)
        {
            var subject = "";
            var message = "";

            if (status==SD.StatusCancelled)
            {
                subject = "Order Cancelled";
                message = "Order Number " + orderNumber + "has been Cancelled. Please contact us if you have any query";
            }
            if (status == SD.StatusSubmitted)
            {
                subject = "Order Created Successfully";
                message = "Order Number " + orderNumber + "has been Created successfully";
            }
            if (status == SD.StatusReady)
            {
                subject = "Order Ready for Pickup";
                message = "Order Number " + orderNumber + "is ready for Pickup! Please contact us if you have any query";
            }

            return emailSender.SendEmailAsync(email, subject, message);
        }
    }
}
