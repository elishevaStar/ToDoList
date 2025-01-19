# ToDo Application

This repository contains the source code for a ToDo application. It is a full-stack application that allows users to manage their tasks. The application uses React for the frontend and .NET Core for the backend API.

## Features

### Frontend
- User login with JWT authentication.
- Add, update, delete, and view tasks.
- Tasks are marked as complete or incomplete.
- Error handling for invalid login or API failures.

### Backend
- User authentication using JWT.
- CRUD operations for tasks.
- MySQL database integration with Entity Framework Core.
- Comprehensive error handling.
- CORS enabled for frontend-backend communication.

## Technology Stack

### Frontend
- React
- Axios (for HTTP requests)

### Backend
- .NET Core 6.0
- Entity Framework Core
- MySQL
- JWT Authentication
- Swagger (API documentation)

## Prerequisites
- Node.js (for running the frontend)
- .NET SDK (for running the backend)
- MySQL database
- Docker (optional, for containerization)

## Setup

### Frontend
1. Navigate to the `frontend` directory.
2. Install dependencies:
   ```bash
   npm install
   ```
3. Create a `.env` file in the `frontend` directory and add the following:
   ```env
   REACT_APP_API_URL=http://localhost:5000
   ```
4. Start the development server:
   ```bash
   npm start
   ```

### Backend
1. Navigate to the `backend` directory.
2. Install dependencies:
   ```bash
   dotnet restore
   ```
3. Create a MySQL database and update the connection string in `appsettings.json`:
   ```json
   "ConnectionStrings": {
       "ToDoDB": "server=localhost;user=root;password=yourpassword;database=ToDoDb"
   }
   ```
4. Apply database migrations:
   ```bash
   dotnet ef database update
   ```
5. Run the backend server:
   ```bash
   dotnet run
   ```

### Database
Run the following SQL commands to set up the database schema:
```sql
CREATE DATABASE ToDoDb;
USE ToDoDb;

CREATE TABLE Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(50) NOT NULL UNIQUE,
    PasswordHash VARCHAR(100) NOT NULL
);

CREATE TABLE Items (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    IsComplete BOOLEAN NOT NULL DEFAULT FALSE
);
```

## Usage

1. Open the application in your browser (default URL: `http://localhost:3000`).
2. Log in using the credentials stored in the `Users` table.
3. Manage tasks by adding, marking as complete, or deleting them.

## API Endpoints

### Authentication
- `POST /login`: Authenticate a user and return a JWT token.

### Tasks
- `GET /items`: Get all tasks (requires authentication).
- `POST /items`: Add a new task (requires authentication).
- `PUT /items/{id}`: Update a task (requires authentication).
- `DELETE /items/{id}`: Delete a task (requires authentication).

## Environment Variables

### Backend
- `Jwt:Key`: Secret key for JWT generation.
- `Jwt:Issuer`: JWT token issuer.
- `Jwt:Audience`: JWT token audience.

### Frontend
- `REACT_APP_API_URL`: The base URL for the backend API.

## Deployment

### Docker
1. Build Docker images for both the frontend and backend.
2. Use a `docker-compose.yml` file to manage services.
3. Start the services:
   ```bash
   docker-compose up
   ```

### Azure/AWS/Heroku
1. Deploy the frontend to a static web hosting service.
2. Deploy the backend to a cloud service with environment variables set accordingly.
3. Update the frontend `.env` file to point to the hosted backend API URL.

## Testing

### Backend
Run unit tests for the backend:
```bash
dotnet test
```

### Frontend
Run tests for the frontend:
```bash
npm test
```

## Known Issues
- Ensure that the JWT secret key is not exposed in the source code.
- Validate all user inputs to prevent SQL injection and XSS attacks.

## Contributing
Contributions are welcome! Please open an issue or submit a pull request.

## License
This project is licensed under the MIT License. See `LICENSE` for details.

---

Thank you for using the ToDo application. We hope it helps you manage your tasks efficiently!

