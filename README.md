# 📚 Library Management System (LMS)

A full-stack library management application built with **.NET 8.0**, **React**, **Docker**, and **AWS** infrastructure-as-code.

## Features

- **Book Management** — Add, edit, delete books with rich metadata (title, author, ISBN, genre, description, publication year, publisher, copy count)
- **Check-out / Check-in** — Patrons can borrow and return books; tracks due dates and overdue status
- **Search** — Full-text search across title, author, ISBN, genre, and description with pagination
- **Authentication & Authorization** — JWT-based auth with three roles:
  - **Patron** — Browse catalog, checkout/return books, view own checkouts
  - **Librarian** — All patron abilities + add/edit books, view all checkouts, return books on behalf of users
  - **Admin** — All librarian abilities + delete books, manage user roles
- **Swagger API Docs** — Interactive API documentation at `/swagger`
- **AI-Powered Smart Search** — Natural language book search using OpenAI or Anthropic (provider-agnostic)

## Tech Stack

| Layer          | Technology              |
|----------------|------------------------|
| Backend API    | .NET 8.0, EF Core, SQLite |
| Frontend       | React 18, React Router, Axios |
| Auth           | JWT Bearer tokens, BCrypt password hashing |
| Containerization | Docker, Docker Compose |
| Infrastructure | Terraform, AWS (ECS Fargate, ALB, ECR, VPC) |
| AI Providers   | OpenAI (GPT-4o) or Anthropic (Claude), configurable |

## Quick Start (Docker Compose)

### Prerequisites
- Docker and Docker Compose installed

### Run
```bash
# Clone the repo and navigate to root
docker-compose up --build
```

### AI Search (Optional)

To enable the AI-powered Smart Search, set your API key via environment variables:

```bash
# For OpenAI
export AI_PROVIDER=OpenAi
export OPENAI_API_KEY=sk-your-key-here

# OR for Anthropic
export AI_PROVIDER=Anthropic
export ANTHROPIC_API_KEY=sk-ant-your-key-here
```

Then run `docker-compose up --build`. The Smart Search toggle will automatically appear on the Books page when an AI provider is configured. Without a key, the app works normally with standard keyword search only.

The app will be available at:
- **Frontend:** http://localhost (port 80)
- **Backend API:** http://localhost:8080
- **Swagger UI:** http://localhost:8080/swagger

### Demo Accounts
| Role      | Email                      | Password       |
|-----------|----------------------------|----------------|
| Admin     | admin@library.com          | Admin123!      |
| Librarian | librarian@library.com      | Librarian123!  |

New users who register via the UI are assigned the **Patron** role by default. Admins can promote users via the Users management page.

## Project Structure

```
├── backend/
│   ├── LibraryApi/
│   │   ├── Controllers/       # API endpoints (Auth, Books, Checkouts)
│   │   ├── Data/              # EF Core DbContext + seed data
│   │   ├── DTOs/              # Request/response data transfer objects
│   │   ├── Models/            # Entity models (Book, User, CheckoutRecord)
│   │   ├── Services/          # Business logic layer
│   │   ├── Program.cs         # App configuration & startup
│   │   └── appsettings.json   # Configuration
│   └── Dockerfile
├── frontend/
│   ├── public/
│   ├── src/
│   │   ├── components/        # Navbar
│   │   ├── context/           # AuthContext (global auth state)
│   │   ├── pages/             # Login, Books, MyCheckouts, ManageCheckouts, Users
│   │   ├── api.js             # Axios API client
│   │   ├── App.js             # Routes & protected route logic
│   │   └── index.css          # Styles
│   ├── Dockerfile
│   └── nginx.conf
├── infra/                     # Terraform IaC for AWS
│   ├── main.tf                # VPC, ECS, ALB, ECR, IAM, etc.
│   ├── variables.tf
│   └── outputs.tf
├── bonus/                     # Bonus writeups
│   ├── deploy-live.md         # Deployment steps & live URL plan
│   ├── demo-video.md          # Video demo recording plan
│   └── ai-features.md         # AI feature proposals (provider-agnostic)
├── docker-compose.yml
└── README.md
```

## API Endpoints

### Authentication
| Method | Endpoint              | Auth     | Description           |
|--------|-----------------------|----------|-----------------------|
| POST   | `/api/auth/register`  | Public   | Register new account  |
| POST   | `/api/auth/login`     | Public   | Login, returns JWT    |
| GET    | `/api/auth/me`        | Any role | Get current user info |
| GET    | `/api/auth/users`     | Admin    | List all users        |
| PUT    | `/api/auth/users/:id/role` | Admin | Update user role |

### Books
| Method | Endpoint           | Auth             | Description              |
|--------|--------------------|------------------|--------------------------|
| GET    | `/api/books`              | Public           | Search/list books            |
| GET    | `/api/books/:id`          | Public           | Get book details             |
| POST   | `/api/books`              | Librarian, Admin | Create a book                |
| PUT    | `/api/books/:id`          | Librarian, Admin | Update a book                |
| DELETE | `/api/books/:id`          | Admin            | Delete a book                |
| GET    | `/api/books/ai-search/status` | Public       | Check if AI search is available |
| POST   | `/api/books/ai-search`    | Public           | Natural language AI search   |

### Checkouts
| Method | Endpoint                    | Auth             | Description                |
|--------|-----------------------------|------------------|----------------------------|
| POST   | `/api/checkouts`            | Any role         | Checkout a book            |
| POST   | `/api/checkouts/:id/return` | Any role         | Return a book              |
| GET    | `/api/checkouts/my`         | Any role         | Get own checkouts          |
| GET    | `/api/checkouts`            | Librarian, Admin | Get all checkouts          |
| GET    | `/api/checkouts/book/:id`   | Librarian, Admin | Get checkouts for a book   |

## AWS Deployment

The `infra/` directory contains Terraform configuration to deploy on AWS using:
- **ECS Fargate** — Serverless container hosting for both frontend and backend
- **Application Load Balancer** — Routes `/api/*` to backend, everything else to frontend
- **ECR** — Container image repositories
- **VPC** — Isolated networking with public/private subnets

See [bonus/deploy-live.md](bonus/deploy-live.md) for step-by-step deployment instructions.

## Development (Without Docker)

### Backend
```bash
cd backend/LibraryApi
dotnet restore
dotnet run
# API runs on http://localhost:5000
```

### Frontend
```bash
cd frontend
npm install
REACT_APP_API_URL=http://localhost:5000 npm start
# App runs on http://localhost:3000
```

## License

MIT
