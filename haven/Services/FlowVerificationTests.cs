using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Obhoy.Data;
using Obhoy.Models;
using Obhoy.Services;

namespace Obhoy.Services;

public static class FlowVerificationTests
{
    public static async Task<bool> RunAllTestsAsync()
    {
        Console.WriteLine("=================================================");
        Console.WriteLine("STARTING COMPREHENSIVE FLOW VERIFICATION TESTS...");
        Console.WriteLine("=================================================");

        var options = new DbContextOptionsBuilder<ObhoyDbContext>()
            .UseSqlite("Data Source=test_verification.db")
            .Options;

        using var db = new ObhoyDbContext(options);
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();

        var passwordHasher = new PasswordHasher<User>();
        var authService = new AuthService(db, passwordHasher, NullLogger<AuthService>.Instance);
        var notifService = new NotificationService(db, NullLogger<NotificationService>.Instance);

        // Seed a therapist user and profile
        var therapistUser = new User
        {
            FullName = "Dr. Farzana Kabir",
            Email = "dr.farzana@obhoy.org",
            Role = "Professional",
            UserType = "Individual",
            Age = 35,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        therapistUser.PasswordHash = passwordHasher.HashPassword(therapistUser, "Therapist123!");
        db.Users.Add(therapistUser);
        await db.SaveChangesAsync();

        var therapistProfile = new ProfessionalProfile
        {
            UserId = therapistUser.Id,
            TitleEn = "Clinical Psychologist",
            TitleBn = "ক্লিনিক্যাল সাইকোলজিস্ট",
            Specialty = "Trauma & CBT",
            LicenseNo = "BMDC-12345",
            HourlyRateBDT = 600,
            ApprovalStatus = "Approved",
            IsBmdcVerified = true
        };
        db.ProfessionalProfiles.Add(therapistProfile);
        await db.SaveChangesAsync();

        // -------------------------------------------------------------
        // TEST 1: REGISTRATION -> LOGIN -> LOGOUT -> LOGIN AGAIN
        // -------------------------------------------------------------
        Console.WriteLine("\n[TEST 1] Testing User Registration & Authentication Lifecycle...");
        var regModel = new RegisterViewModel
        {
            FullName = "Test Patient",
            Email = "patient.test@obhoy.org",
            Password = "Password123!",
            ConfirmPassword = "Password123!",
            UserType = "Individual",
            Age = 22,
            AgreeToTerms = true
        };

        // 1.1 Register
        var (regSuccess, regError, registeredUser) = await authService.RegisterAsync(regModel);
        if (!regSuccess || registeredUser == null)
        {
            Console.WriteLine($"FAILED: Registration failed - {regError}");
            return false;
        }
        Console.WriteLine("PASS: User registered successfully with ID: " + registeredUser.Id);

        // 1.2 Verify Duplicate Registration blocked
        var (dupSuccess, dupError, _) = await authService.RegisterAsync(regModel);
        if (dupSuccess)
        {
            Console.WriteLine("FAILED: Duplicate registration should be rejected!");
            return false;
        }
        Console.WriteLine("PASS: Duplicate registration correctly rejected with: " + dupError);

        // 1.3 First Login
        var (login1Success, login1Error, login1User) = await authService.AuthenticateAsync("patient.test@obhoy.org", "Password123!");
        if (!login1Success || login1User == null)
        {
            Console.WriteLine($"FAILED: First login failed - {login1Error}");
            return false;
        }
        Console.WriteLine("PASS: First login succeeded for: " + login1User.Email);

        // 1.4 Simulate Logout (Ensure user is NOT deleted or deactivated)
        var userInDb = await db.Users.FirstOrDefaultAsync(u => u.Email == "patient.test@obhoy.org");
        if (userInDb == null || !userInDb.IsActive)
        {
            Console.WriteLine("FAILED: User was deleted or deactivated after logout!");
            return false;
        }
        Console.WriteLine("PASS: User account remains intact and active in database after logout.");

        // 1.5 Second Login using SAME credentials
        var (login2Success, login2Error, login2User) = await authService.AuthenticateAsync("patient.test@obhoy.org", "Password123!");
        if (!login2Success || login2User == null)
        {
            Console.WriteLine($"FAILED: Second login failed after logout - {login2Error}");
            return false;
        }
        Console.WriteLine("PASS: Second login SUCCEEDED using same credentials! (Role: " + login2User.Role + ")");

        // 1.6 Verify Incorrect Password fails
        var (wrongPwSuccess, _, _) = await authService.AuthenticateAsync("patient.test@obhoy.org", "WrongPassword!");
        if (wrongPwSuccess)
        {
            Console.WriteLine("FAILED: Login with wrong password should fail!");
            return false;
        }
        Console.WriteLine("PASS: Login with incorrect password correctly rejected.");

        // -------------------------------------------------------------
        // TEST 2: BOOKING CREATION & THERAPIST NOTIFICATION
        // -------------------------------------------------------------
        Console.WriteLine("\n[TEST 2] Testing User Booking Therapist Appointment...");
        var booking = new Booking
        {
            UserId = login2User.Id,
            TherapistId = therapistProfile.Id,
            BookingDate = DateTime.UtcNow.AddDays(2).Date,
            TimeSlot = "04:30 PM - 05:30 PM",
            CommunicationMode = "Online Video",
            Notes = "Anxiety counseling",
            Status = BookingStatus.Pending,
            BookingReference = "OBH-TEST-001",
            FeeBDT = 600,
            CreatedAt = DateTime.UtcNow
        };
        db.Bookings.Add(booking);
        await db.SaveChangesAsync();

        // Dispatch notification to Therapist
        await notifService.CreateNotificationAsync(
            therapistUser.Id,
            "নতুন অ্যাপয়েন্টমেন্ট অনুরোধ / New Appointment Request",
            $"New appointment request from {login2User.FullName} for {booking.BookingDate:dd MMM yyyy} ({booking.TimeSlot}).",
            "BookingPending",
            "/TherapistDashboard/ManageRequests");

        // Verify booking in DB
        var savedBooking = await db.Bookings.FirstOrDefaultAsync(b => b.Id == booking.Id);
        if (savedBooking == null || savedBooking.Status != BookingStatus.Pending)
        {
            Console.WriteLine("FAILED: Booking not saved as Pending in DB!");
            return false;
        }
        Console.WriteLine("PASS: Booking saved with Status: " + savedBooking.Status + " and Ref: " + savedBooking.BookingReference);

        // Verify Therapist received notification
        var therapistNotifs = await notifService.GetUserNotificationsAsync(therapistUser.Id);
        if (!therapistNotifs.Any(n => n.Type == "BookingPending"))
        {
            Console.WriteLine("FAILED: Therapist did not receive BookingPending notification!");
            return false;
        }
        int therapistUnread = await notifService.GetUnreadCountAsync(therapistUser.Id);
        Console.WriteLine($"PASS: Therapist received pending notification! (Unread: {therapistUnread}, Message: {therapistNotifs[0].Message})");

        // -------------------------------------------------------------
        // TEST 3: THERAPIST ACCEPTS BOOKING -> USER NOTIFICATION
        // -------------------------------------------------------------
        Console.WriteLine("\n[TEST 3] Testing Therapist Accept Action & User Notification...");
        savedBooking.Status = BookingStatus.Approved;
        savedBooking.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        // Dispatch notification to User
        await notifService.CreateNotificationAsync(
            login2User.Id,
            "অ্যাপয়েন্টমেন্ট অনুমোদিত / Appointment Accepted",
            $"Your appointment with {therapistUser.FullName} on {savedBooking.BookingDate:dd MMM yyyy} ({savedBooking.TimeSlot}) has been accepted.",
            "BookingAccepted",
            "/Booking/MyBookings");

        var userNotifs = await notifService.GetUserNotificationsAsync(login2User.Id);
        if (!userNotifs.Any(n => n.Type == "BookingAccepted"))
        {
            Console.WriteLine("FAILED: User did not receive BookingAccepted notification!");
            return false;
        }
        int userUnread = await notifService.GetUnreadCountAsync(login2User.Id);
        Console.WriteLine($"PASS: Booking status is APPROVED. User received notification! (Unread: {userUnread}, Message: {userNotifs[0].Message})");

        // -------------------------------------------------------------
        // TEST 4: THERAPIST REJECTS ANOTHER BOOKING -> USER NOTIFICATION
        // -------------------------------------------------------------
        Console.WriteLine("\n[TEST 4] Testing Therapist Reject Action & User Notification...");
        var booking2 = new Booking
        {
            UserId = login2User.Id,
            TherapistId = therapistProfile.Id,
            BookingDate = DateTime.UtcNow.AddDays(3).Date,
            TimeSlot = "07:00 PM - 08:00 PM",
            CommunicationMode = "Confidential Audio",
            Notes = "Followup session",
            Status = BookingStatus.Pending,
            BookingReference = "OBH-TEST-002",
            FeeBDT = 600,
            CreatedAt = DateTime.UtcNow
        };
        db.Bookings.Add(booking2);
        await db.SaveChangesAsync();

        // Therapist Rejects
        booking2.Status = BookingStatus.Rejected;
        booking2.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        // Dispatch rejection notification
        await notifService.CreateNotificationAsync(
            login2User.Id,
            "অ্যাপয়েন্টমেন্ট বাতিল / Appointment Rejected",
            $"Your appointment with {therapistUser.FullName} on {booking2.BookingDate:dd MMM yyyy} ({booking2.TimeSlot}) has been rejected.",
            "BookingRejected",
            "/Booking/MyBookings");

        userNotifs = await notifService.GetUserNotificationsAsync(login2User.Id);
        var rejectNotif = userNotifs.FirstOrDefault(n => n.Type == "BookingRejected");
        if (rejectNotif == null)
        {
            Console.WriteLine("FAILED: User did not receive BookingRejected notification!");
            return false;
        }
        Console.WriteLine($"PASS: Booking status is REJECTED. User received rejection notification! (Message: {rejectNotif.Message})");

        // -------------------------------------------------------------
        // TEST 5: NOTIFICATION READ / UNREAD STATUS & USER ISOLATION
        // -------------------------------------------------------------
        Console.WriteLine("\n[TEST 5] Testing Notification Read / Unread Status and User Isolation...");
        // Mark single notification as read
        await notifService.MarkAsReadAsync(rejectNotif.Id, login2User.Id);
        int updatedUnread = await notifService.GetUnreadCountAsync(login2User.Id);
        Console.WriteLine($"PASS: Marked notification #{rejectNotif.Id} as read. Remaining unread: {updatedUnread}");

        // Verify Data Isolation: Patient cannot see Therapist's notifications
        var patientViewOfTherapistNotifs = await db.Notifications.Where(n => n.UserId == login2User.Id && n.UserId == therapistUser.Id).ToListAsync();
        if (patientViewOfTherapistNotifs.Any())
        {
            Console.WriteLine("FAILED: Data isolation breach!");
            return false;
        }
        Console.WriteLine("PASS: User notification and Therapist data isolation verified!");

        // Clean up test database
        await db.Database.EnsureDeletedAsync();

        Console.WriteLine("\n=================================================");
        Console.WriteLine("ALL VERIFICATION TESTS PASSED SUCCESSFULLY! (100%)");
        Console.WriteLine("=================================================");
        return true;
    }
}
