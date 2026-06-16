# Record a Demo Video

## What
Use Loom (or similar screen recording tool) to create a walkthrough demo of the Library Management System, showcasing all features and user roles.

## How

### Recording Tool
- **Loom** (recommended) — free tier supports 5-minute recordings, easy to share via link
- Alternatives: OBS Studio, QuickTime (Mac), or Windows Game Bar

### Demo Script (Suggested Flow, ~4 minutes)

#### 1. Introduction (30s)
- Show the login page
- Briefly describe the LMS and its tech stack (.NET 8.0 + React + Docker + AWS)

#### 2. Authentication & Roles (45s)
- Log in as **Admin** (admin@library.com / Admin123!)
- Show the navbar with all navigation options (Books, My Checkouts, Manage Checkouts, Users)
- Navigate to Users page — show role management
- Log out, then **Register** a new patron account to demonstrate registration

#### 3. Book Management (60s)
- Log in as **Librarian**
- Browse the book catalog — show the card layout
- Use the **search bar** to find books by title, author, genre
- Click **Add Book** — fill out the form and submit
- Click **Edit** on an existing book — modify fields and save
- Log in as Admin and demonstrate **Delete**

#### 4. Check-out & Check-in (45s)
- As a Patron, click **Checkout** on an available book
- Navigate to **My Checkouts** — show the active checkout with due date
- Click **Return** to check the book back in
- Show the book's available copies updated

#### 5. Librarian/Admin View (30s)
- Log in as Librarian
- Navigate to **Manage Checkouts** — show all checkouts across users
- Toggle "Active only" filter
- Demonstrate returning a book on behalf of a user

#### 6. Wrap-up (15s)
- Mention Docker Compose for local dev
- Mention Terraform IaC for AWS deployment
- Thank the viewer

### Sharing
- Upload to Loom and share the public link
- Alternatively, upload to YouTube (unlisted) or attach the .mp4 file
