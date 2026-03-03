# 📸 Photography Web App - Booking System Documentation

## ✅ What's Been Built

Your photography web app now has a fully functional booking system with the following features:

### 🎯 Core Features

#### 1. **Booking Page** (`/Booking`)
   - Professional booking form for clients
   - Fields: Full Name, Email, Phone Number, Service Type, Preferred Date, Special Notes
   - Real-time validation
   - Success/error messages
   - Auto-generated confirmation ID
   - Responsive design for mobile, tablet, and desktop

#### 2. **Packages Page** (`/Packages`)
   - Display all available photography services
   - Shows service name, description, and price
   - Direct "Book Now" buttons
   - Professional card layout with hover effects
   - Includes example packages:
     - 💒 Wedding Photography - $800
     - 📸 Portrait Session - $300
     - 💼 Corporate Photography - $500
     - 🎉 Event Photography - $600

#### 3. **Booking Management System** (`/AdminBookings`)
   - View all bookings in a professional table format
   - Status badges (Pending ⏳, Confirmed ✅, Cancelled ❌)
   - Statistics overview (Total, Pending, Confirmed bookings)
   - Actions to confirm, cancel, or delete bookings
   - View client contact information
   - Display special notes for each booking
   - Click-to-call phone numbers and mailto links

#### 4. **Contact Page** (`/Contact`)
   - Contact form for general inquiries
   - Contact information section
   - Payment deposit information
   - Professional layout matching the booking system

#### 5. **Updated Navigation**
   - New menu items: 📅 Book Now, 💰 Packages, 📞 Contact, ⚙️ Admin
   - Consistent branding: "🎨 MelvonPhotography"

---

## 🏗️ Technical Architecture

### Models
- **`Booking.cs`**: Contains booking data structure
- **`Service.cs`**: Represents photography service packages

### Services
- **`BookingService.cs`**: Core business logic
  - CRUD operations (Create, Read, Update, Delete)
  - Persistent storage using JSON file (`wwwroot/bookings.json`)
  - Service package management

### Pages (Razor Pages)
- **`Booking.cshtml` / `Booking.cshtml.cs`**: Client booking interface
- **`Packages.cshtml` / `Packages.cshtml.cs`**: Service showcase
- **`AdminBookings.cshtml` / `AdminBookings.cshtml.cs`**: Management dashboard
- **`Contact.cshtml` / `Contact.cshtml.cs`**: Contact form

### Data Storage
- **JSON-based storage**: Bookings are saved to `wwwroot/bookings.json`
- **No database required**: Perfect for small to medium projects
- **Easy to upgrade**: Can migrate to SQL Server/PostgreSQL later

---

## 🚀 How to Use

### For Clients:

1. **View Packages**
   - Navigate to `/Packages` to see available services and pricing
   - Click "Book This Package" on any service

2. **Book a Session**
   - Fill out the booking form with:
     - Full Name
     - Email Address
     - Phone Number
     - Service Type (dropdown)
     - Preferred Date (min. 7 days in advance recommended)
     - Special Notes (optional)
   - Submit the form
   - Receive confirmation ID

3. **Contact**
   - Use `/Contact` page for general inquiries
   - View contact information and payment details

### For Admin (You):

1. **View Bookings**
   - Navigate to `/AdminBookings` (password protection optional)
   - See all bookings with client details
   - Filter by status (Pending, Confirmed, Cancelled)

2. **Manage Bookings**
   - **Confirm**: Mark booking as confirmed
   - **Cancel**: Reject a booking
   - **Delete**: Remove booking from system
   - Click phone number to call, click email to send message

---

## 📁 File Structure

```
Pages/
├── Booking.cshtml                 # Client booking form
├── Booking.cshtml.cs              # Booking page model
├── Packages.cshtml                # Service showcase
├── Packages.cshtml.cs             # Packages page model
├── AdminBookings.cshtml           # Booking management
├── AdminBookings.cshtml.cs        # Admin page model
├── Contact.cshtml                 # Contact form
├── Contact.cshtml.cs              # Contact page model
└── Shared/
    └── _Layout.cshtml             # Updated navigation
Models/
├── Booking.cs                     # Booking model
└── Service.cs                     # Service model
Services/
└── BookingService.cs              # Business logic
wwwroot/
├── bookings.json                  # Booking storage (auto-created)
└── contacts/                      # Contact submissions (auto-created)
```

---

## ⚙️ Configuration

### Update Service Packages

Edit `Services/BookingService.cs` to customize packages:

```csharp
_services = new List<Service>
{
    new Service { Id = 1, Name = "Your Service", Price = 500, Description = "Description" },
    // Add more services
};
```

### Update Contact Information

Edit `Pages/Contact.cshtml` to add your actual details:

```html
<p><a href="mailto:your-email@example.com">your-email@example.com</a></p>
<p><a href="tel:+1234567890">+1 (234) 567-890</a></p>
<p>Your City, Your State 12345</p>
```

---

## 🔒 Security Considerations

### Current Setup (Development):
- No authentication on `/AdminBookings` page
- Bookings stored in plain JSON file

### For Production, Add:
1. **Authentication**
   ```csharp
   app.UseAuthentication();
   app.UseAuthorization();
   ```

2. **Access Control** on AdminBookings page
   ```csharp
   [Authorize]
   public class AdminBookingsModel : PageModel { }
   ```

3. **Database** instead of JSON
   - Use Entity Framework Core with SQL Server
   - More scalable and secure

4. **Email Notifications**
   - Automatic confirmation emails to clients
   - Admin notifications for new bookings

---

## 📊 Data Format

### Bookings.json Example:
```json
[
  {
    "id": 1,
    "fullName": "John Doe",
    "email": "john@example.com",
    "phoneNumber": "555-0123",
    "serviceType": "Wedding Photography",
    "preferredDate": "2026-04-15T00:00:00",
    "specialNotes": "Early morning shoot preferred",
    "bookingDate": "2026-03-03T10:30:45.123456",
    "status": "Pending"
  }
]
```

---

## 🚀 Future Enhancements

### High Priority:
- [ ] Email notifications (new booking alerts & confirmations)
- [ ] Payment integration (Stripe/PayPal)
- [ ] Admin authentication (login required)
- [ ] Availability calendar (block booked dates)
- [ ] Photo gallery on home page

### Medium Priority:
- [ ] Database migration (SQL Server/PostgreSQL)
- [ ] Email reminders (5 days before session)
- [ ] SMS notifications
- [ ] Client portal (track booking status)
- [ ] Testimonials/reviews section

### Nice-to-Have:
- [ ] Instagram feed integration
- [ ] Google Analytics
- [ ] SEO optimization
- [ ] Blog section
- [ ] Newsletter signup

---

## 📝 URL Routes

| Page | URL | Purpose |
|------|-----|---------|
| Home | `/` or `/Index` | Portfolio showcase |
| Packages | `/Packages` | Service listing |
| Booking | `/Booking` | Client booking form |
| Contact | `/Contact` | Contact & inquiries |
| Admin Bookings | `/AdminBookings` | Manage all bookings |
| Privacy | `/Privacy` | Privacy policy |

---

## 🎨 Styling Notes

- All pages use **Bootstrap 5** for responsive design
- Custom CSS for photography-themed styling
- Emojis for intuitive icons
- Mobile-first responsive layout
- Hover effects for better UX

---

## 🐛 Troubleshooting

### Bookings not saving?
- Check if `wwwroot/` folder exists
- Ensure app has write permissions
- Check `bookings.json` file

### Form validation issues?
- Check browser console for errors
- Ensure `_ValidationScriptsPartial.cshtml` is included

### Styling looks off?
- Clear browser cache (Ctrl+Shift+Delete)
- Check Bootstrap CSS is loaded in `_Layout.cshtml`

---

## 📞 Support

For questions or customizations, refer to ASP.NET Core documentation:
- [Razor Pages Documentation](https://learn.microsoft.com/aspnet/core/razor-pages)
- [Bootstrap 5 Documentation](https://getbootstrap.com/docs/5.0/)

---

**Last Updated:** March 3, 2026
**Version:** 1.0.0
